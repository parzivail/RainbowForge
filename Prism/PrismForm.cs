using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using BrightIdeasSoftware;
using JeremyAnsel.Media.WavefrontObj;
using OpenTK;
using OpenTK.Graphics;
using Pfim;
using Prism.Controls;
using Prism.Extensions;
using Prism.Render;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Dump;
using RainbowForge.Image;
using RainbowForge.Model;
using RainbowForge.RenderPipeline;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Color4 = RainbowForge.Model.Color4;

namespace Prism
{
	public class PrismForm : Form
	{
		private readonly ToolStripLabel _statusForgeInfo;
		
		private readonly ToolStripMenuItem _bOpenForge;
		private readonly ToolStripMenuItem _bResetViewport;

		private readonly ToolStripMenuItem _bDumpAsBin;
		private readonly ToolStripMenuItem _bDumpAsDds;
		private readonly ToolStripMenuItem _bDumpAsObj;

		private readonly TreeListView _assetList;
		private readonly MinimalSplitContainer _splitContainer;
		private readonly GLControl _glControl;
		private readonly SKControl _imageControl;
		private readonly TreeListView _infoControl;

		private ModelRenderer _renderer3d;
		private SurfaceRenderer _renderer2d;

		private Forge _openedForge;
		private Dictionary<ulong, ulong> _flatArchiveEntryMap;

		public Forge OpenedForge
		{
			get => _openedForge;
			set
			{
				_openedForge = value;
				_flatArchiveEntryMap = new Dictionary<ulong, ulong>();
				_assetList.SetObjects(_openedForge.Entries);
				UpdateAbility(null);

				_assetList.SelectedIndex = 0;

				var sb = new StringBuilder();

				sb.Append($"{_openedForge.NumEntries:N0} entries");

				if (_openedForge.NumEntries > 0)
				{
					sb.Append(" (");

					var types = _openedForge.Entries
						.GroupBy(entry => (Magic)entry.Name.FileType)
						.OrderByDescending(entries => entries.Count())
						.Select(entries => $"{entries.Count():N0} {(Enum.IsDefined(typeof(Magic), entries.Key) ? entries.Key : $"{(uint)entries.Key:X8}")}")
						.ToList();
					sb.Append(string.Join(", ", types.Take(5)));

					if (types.Count > 5)
						sb.Append(", ...");

					sb.Append(')');
				}

				_statusForgeInfo.Text = sb.ToString();
			}
		}

		public PrismForm()
		{
			using (this.SuspendPainting())
			{
				AutoScaleMode = AutoScaleMode.Font;
				ClientSize = new Size(800, 450);
				Text = "Prism";

				SuspendLayout();

				Controls.Add(_splitContainer = new MinimalSplitContainer
				{
					Dock = DockStyle.Fill,
					SplitterDistance = 350,
					Panel1 =
					{
						Controls =
						{
							(_assetList = new TreeListView
							{
								Dock = DockStyle.Fill,
								View = View.Details,
								ShowGroups = false,
								FullRowSelect = true
							})
						}
					}
				});

				Controls.Add(new MenuStrip
				{
					Dock = DockStyle.Top,
					Renderer = new FlatToolStripRenderer(),

					Items =
					{
						new ToolStripDropDownButton
						{
							Text = "&File",
							DropDownItems =
							{
								(_bOpenForge = new ToolStripMenuItem("&Open Forge")
								{
									ShortcutKeys = Keys.Control | Keys.O
								})
							}
						},
						new ToolStripDropDownButton
						{
							Text = "&Dump",
							DropDownItems =
							{
								(_bDumpAsBin = new ToolStripMenuItem("&Binary file")),
								new ToolStripSeparator(),
								(_bDumpAsDds = new ToolStripMenuItem("&DirectDraw Surface")),
								new ToolStripSeparator(),
								(_bDumpAsObj = new ToolStripMenuItem("&Wavefront OBJ"))
							}
						},
						new ToolStripDropDownButton
						{
							Text = "&View",
							DropDownItems =
							{
								(_bResetViewport = new ToolStripMenuItem("&Reset 3D Viewport"))
							}
						}
					}
				});

				Controls.Add(new StatusStrip
				{
					Dock = DockStyle.Bottom,
					Items =
					{
						(_statusForgeInfo = new ToolStripLabel("Ready"))
					}
				});

				_glControl = new GLControl(new GraphicsMode(new ColorFormat(8), 24, 8, 1))
				{
					Dock = DockStyle.Fill,
					VSync = true
				};

				_imageControl = new SKControl
				{
					Dock = DockStyle.Fill
				};

				_infoControl = new TreeListView
				{
					Dock = DockStyle.Fill,
					View = View.Details,
					ShowGroups = false,
					FullRowSelect = true
				};

				SetPreviewPanel(new Label
				{
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter,
					Text = "Open a Forge to get started."
				});

				ResumeLayout(true);
			}

			_bOpenForge.Click += (sender, args) =>
			{
				var ofd = new OpenFileDialog
				{
					Filter = "Forge Files|*.forge"
				};

				if (ofd.ShowDialog() != DialogResult.OK)
					return;

				OpenForge(ofd.FileName);
			};

			_bResetViewport.Click += (sender, args) => _renderer3d.ResetView();

			_bDumpAsBin.Click += CreateDumpEventHandler("Binary Files|*.bin", DumpSelectionAsBin);
			_bDumpAsDds.Click += CreateDumpEventHandler("DirectDraw Surfaces|*.dds", DumpSelectionAsDds);
			_bDumpAsObj.Click += CreateDumpEventHandler("Wavefront OBJs|*.obj", DumpSelectionAsObj);

			SetupRenderer();
			SetupAssetList();

			UpdateAbility(null);
		}

		private static EventHandler CreateDumpEventHandler(string filter, Action<string> action)
		{
			return (_, _) =>
			{
				var sfd = new SaveFileDialog
				{
					Filter = filter
				};

				if (sfd.ShowDialog() == DialogResult.OK)
					action(sfd.FileName);
			};
		}

		private void DumpSelectionAsBin(string fileName)
		{
			var streamData = GetAssetStream(_assetList.SelectedObject);
			using var stream = streamData.Stream;
			DumpHelper.DumpBin(fileName, stream.BaseStream);
		}

		private void DumpSelectionAsObj(string fileName)
		{
			var streamData = GetAssetStream(_assetList.SelectedObject);
			using var stream = streamData.Stream;

			var header = MeshHeader.Read(stream);

			var compiledMeshObject = CompiledMeshObject.Read(stream, header);

			var obj = new ObjFile();

			for (var objId = 0; objId < compiledMeshObject.Objects.Count; objId++)
			{
				var lod = objId / compiledMeshObject.MeshHeader.NumLods;

				var o = compiledMeshObject.Objects[objId];
				foreach (var face in o)
				{
					var objFace = new ObjFace
					{
						ObjectName = $"lod{lod}_object{objId % compiledMeshObject.MeshHeader.NumLods}"
					};

					objFace.Vertices.Add(new ObjTriplet(face.A + 1, face.A + 1, face.A + 1));
					objFace.Vertices.Add(new ObjTriplet(face.B + 1, face.B + 1, face.B + 1));
					objFace.Vertices.Add(new ObjTriplet(face.C + 1, face.C + 1, face.C + 1));

					obj.Faces.Add(objFace);
				}
			}

			var container = compiledMeshObject.Container;
			for (var i = 0; i < container.Vertices.Length; i++)
			{
				var vert = container.Vertices[i];
				var color = container.Colors?[0, i] ?? new Color4(1, 1, 1, 1);

				obj.Vertices.Add(new ObjVertex(vert.X, vert.Y, vert.Z, color.R, color.G, color.B, color.A));
			}

			foreach (var v in container.Normals)
				obj.VertexNormals.Add(new ObjVector3(v.X, v.Y, v.Z));

			foreach (var v in container.TexCoords)
				obj.TextureVertices.Add(new ObjVector3(v.X, v.Y));

			obj.WriteTo(fileName);
		}

		private void DumpSelectionAsDds(string fileName)
		{
			var streamData = GetAssetStream(_assetList.SelectedObject);
			using var stream = streamData.Stream;

			var texture = Texture.Read(stream);
			var surface = texture.ReadSurfaceBytes(stream);
			using var ddsStream = DdsHelper.GetDdsStream(texture, surface);

			DumpHelper.DumpBin(fileName, ddsStream);
		}

		private AssetStream GetAssetStream(object o)
		{
			switch (o)
			{
				case Entry entry:
				{
					var container = _openedForge.GetContainer(entry.Uid);

					return container switch
					{
						ForgeAsset forgeAsset => new AssetStream(entry.Uid, entry.Name.FileType, forgeAsset.GetDataStream(_openedForge)),
						_ => new AssetStream(entry.Uid, entry.Name.FileType, _openedForge.GetEntryStream(entry))
					};
				}
				case FlatArchiveEntry flatArchiveEntry:
				{
					var container = _openedForge.GetContainer(_flatArchiveEntryMap[flatArchiveEntry.Meta.Uid]);
					if (container is not ForgeAsset forgeAsset)
						return null;

					using var assetStream = forgeAsset.GetDataStream(_openedForge);
					var arc = FlatArchive.Read(assetStream);
					return new AssetStream(flatArchiveEntry.Meta.Uid, flatArchiveEntry.Meta.Magic, arc.GetEntryStream(assetStream.BaseStream, flatArchiveEntry.Meta.Uid));
				}
			}

			return null;
		}

		private void SetPreviewPanel(Control control)
		{
			using (_splitContainer.Panel2.SuspendPainting())
			{
				_splitContainer.Panel2.Controls.Clear();
				_splitContainer.Panel2.Controls.Add(control);
			}
		}

		private void SetupRenderer()
		{
			_renderer2d = new SurfaceRenderer(_imageControl);
			_imageControl.MouseMove += (sender, args) => _renderer2d.OnMouseMove(args.Location, (args.Button & MouseButtons.Left) != 0);
			_imageControl.MouseWheel += (sender, args) => _renderer2d.OnMouseWheel(args.Location, args.Delta);

			_imageControl.PaintSurface += (sender, args) => { _renderer2d.Render(args); };

			_renderer3d = new ModelRenderer(new GlControlContext(_glControl));
			_glControl.MouseDown += (sender, args) => _renderer3d.OnMouseDown(args.Location);
			_glControl.MouseMove += (sender, args) => _renderer3d.OnMouseMove(args.Location, (args.Button & MouseButtons.Left) != 0, (args.Button & MouseButtons.Right) != 0);
			_glControl.MouseWheel += (sender, args) => _renderer3d.OnMouseWheel(args.Delta);

			_glControl.Paint += (sender, args) =>
			{
				_glControl.MakeCurrent();
				_renderer3d.Render();
				_glControl.SwapBuffers();
			};

			_infoControl.Columns.Add(new OLVColumn("Key", nameof(TreeListViewEntry.Key))
			{
				Width = 200
			});
			_infoControl.Columns.Add(new OLVColumn("Value", nameof(TreeListViewEntry.Value))
			{
				FillsFreeSpace = true
			});
			_infoControl.CanExpandGetter = model => model is TreeListViewEntry tlve && tlve.Children.Length > 0;
			_infoControl.ChildrenGetter = model => model is TreeListViewEntry tlve ? tlve.Children : null;
		}

		private void SetupAssetList()
		{
			_assetList.Columns.Add(new OLVColumn("Type", null)
			{
				Width = 100,
				AspectGetter = rowObject =>
				{
					ulong fileType = rowObject switch
					{
						Entry e => e.Name.FileType,
						FlatArchiveEntry fae => fae.Meta.Magic,
						_ => 0
					};

					return fileType;
				},
				AspectToStringConverter = value =>
				{
					var fileType = (ulong)value;
					return Enum.IsDefined(typeof(Magic), fileType) ? ((Magic)fileType).ToString() : fileType.ToString("X");
				}
			});

			_assetList.Columns.Add(new OLVColumn("Size", null)
			{
				Width = 70,
				AspectGetter = rowObject =>
				{
					uint size = rowObject switch
					{
						Entry e => e.Size,
						FlatArchiveEntry fae => (uint)fae.PayloadLength,
						_ => 0
					};

					return size;
				},
				AspectToStringConverter = value => ((uint)value).ToFileSizeString()
			});

			_assetList.Columns.Add(new OLVColumn("UID", null)
			{
				FillsFreeSpace = true,
				AspectGetter = rowObject =>
				{
					return rowObject switch
					{
						Entry e => e.Uid,
						FlatArchiveEntry fae => fae.Meta.Uid,
						_ => null
					};
				}
			});

			_assetList.SelectedIndexChanged += OnAssetListOnSelectionChanged;

			_assetList.CanExpandGetter = model => { return model is Entry e && MagicHelper.GetFiletype(e.Name.FileType) == AssetType.FlatArchive; };

			_assetList.ChildrenGetter = model =>
			{
				if (model is not Entry e || MagicHelper.GetFiletype(e.Name.FileType) != AssetType.FlatArchive)
					return null;

				var container = _openedForge.GetContainer(e.Uid);
				if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

				var assetStream = forgeAsset.GetDataStream(_openedForge);
				var fa = FlatArchive.Read(assetStream);

				foreach (var entry in fa.Entries) _flatArchiveEntryMap[entry.Meta.Uid] = e.Uid;

				return fa.Entries;
			};
		}

		private void OnAssetListOnSelectionChanged(object sender, EventArgs args)
		{
			var selectedEntry = _assetList.SelectedObject;
			lock (_openedForge)
			{
				var stream = GetAssetStream(selectedEntry);
				if (stream != null)
					PreviewAsset(stream);

				UpdateAbility(stream);
			}
		}

		private void UpdateAbility(AssetStream assetStream)
		{
			AssetType type;
			Magic magic;

			if (assetStream == null)
			{
				type = AssetType.Unknown;
				magic = 0;
			}
			else
			{
				type = MagicHelper.GetFiletype(assetStream.Magic);
				magic = (Magic)assetStream.Magic;
			}

			_bDumpAsBin.Enabled = assetStream != null;
			_bDumpAsDds.Enabled = type == AssetType.Texture;
			_bDumpAsObj.Enabled = type == AssetType.Mesh;
		}

		private void PreviewAsset(AssetStream assetStream)
		{
			using var stream = assetStream.Stream;
			switch (MagicHelper.GetFiletype(assetStream.Magic))
			{
				case AssetType.Mesh:
				{
					var header = MeshHeader.Read(stream);
					var mesh = CompiledMeshObject.Read(stream, header);

					OnUiThread(() =>
					{
						SetPreviewPanel(_glControl);
						using (_glControl.SuspendPainting())
						{
							_renderer3d.BuildModelQuads(mesh);
							_renderer3d.SetTexture(null);
							_renderer3d.SetPartBounds(header.ObjectBoundingBoxes.Take((int)(header.ObjectBoundingBoxes.Length / header.NumLods)).ToArray());
						}
					});

					break;
				}
				case AssetType.Texture:
				{
					var texture = Texture.Read(stream);
					using var image = Pfim.Pfim.FromStream(DdsHelper.GetDdsStream(texture, texture.ReadSurfaceBytes(stream)));

					var newData = image.Data;
					var newDataLen = image.DataLen;
					var stride = image.Stride;
					SKColorType colorType;
					switch (image.Format)
					{
						case ImageFormat.Rgb8:
							colorType = SKColorType.Gray8;
							break;
						case ImageFormat.R5g6b5:
							// color channels still need to be swapped
							colorType = SKColorType.Rgb565;
							break;
						case ImageFormat.Rgba16:
							// color channels still need to be swapped
							colorType = SKColorType.Argb4444;
							break;
						case ImageFormat.Rgb24:
						{
							// Skia has no 24bit pixels, so we upscale to 32bit
							var pixels = image.DataLen / 3;
							newDataLen = pixels * 4;
							newData = new byte[newDataLen];
							for (var i = 0; i < pixels; i++)
							{
								newData[i * 4] = image.Data[i * 3];
								newData[i * 4 + 1] = image.Data[i * 3 + 1];
								newData[i * 4 + 2] = image.Data[i * 3 + 2];
								newData[i * 4 + 3] = 255;
							}

							stride = image.Width * 4;
							colorType = SKColorType.Bgra8888;
							break;
						}
						case ImageFormat.Rgba32:
							colorType = SKColorType.Bgra8888;
							break;
						default:
							throw new ArgumentException($"Skia unable to interpret pfim format: {image.Format}");
					}

					var imageInfo = new SKImageInfo(image.Width, image.Height, colorType, SKAlphaType.Unpremul);
					var handle = GCHandle.Alloc(newData, GCHandleType.Pinned);
					var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(newData, 0);

					using (var data = SKData.Create(ptr, newDataLen, (address, context) => handle.Free()))
					using (var skImage = SKImage.FromPixels(imageInfo, data, stride))
					{
						_renderer2d.SetTexture(SKBitmap.FromImage(skImage));
					}

					OnUiThread(() => { SetPreviewPanel(_imageControl); });

					break;
				}
				default:
				{
					List<TreeListViewEntry> entries;

					switch ((Magic)assetStream.Magic)
					{
						case Magic.Mesh:
						{
							var mp = Mesh.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new(nameof(Mesh), null,
									new TreeListViewEntry("Var1", mp.Var1),
									new TreeListViewEntry("Var2", mp.Var2),
									new TreeListViewEntry("Mesh UID", mp.CompiledMeshObjectUid),
									new TreeListViewEntry(nameof(Mesh.Materials), null, mp.Materials.Select(arg => new TreeListViewEntry("UID", arg)).ToArray())
								)
							};
							break;
						}
						case Magic.Material:
						{
							var mc = Material.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new(nameof(Material), null,
									new TreeListViewEntry(nameof(Material.BaseTextureMapSpecs), null, mc.BaseTextureMapSpecs.Select(CreateMipContainerReferenceEntry).ToArray()),
									new TreeListViewEntry(nameof(Material.SecondaryTextureMapSpecs), null, mc.SecondaryTextureMapSpecs.Select(CreateMipContainerReferenceEntry).ToArray()),
									new TreeListViewEntry(nameof(Material.TertiaryTextureMapSpecs), null, mc.TertiaryTextureMapSpecs.Select(CreateMipContainerReferenceEntry).ToArray())
								)
							};
							break;
						}
						case Magic.TextureMapSpec:
						{
							var mc = TextureMapSpec.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new(nameof(TextureMapSpec), null,
									new TreeListViewEntry(nameof(TextureMapSpec.TextureMapUid), mc.TextureMapUid),
									new TreeListViewEntry("TextureType", mc.TextureType)
								)
							};
							break;
						}
						case Magic.TextureMap:
						{
							var mc = TextureMap.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new(nameof(TextureMap), null,
									new TreeListViewEntry("Var1", mc.Var1),
									new TreeListViewEntry("Var2", mc.Var2),
									new TreeListViewEntry("Var3", mc.Var3),
									new TreeListViewEntry("Var4", mc.Var4),
									new TreeListViewEntry(nameof(TextureMap.TexUidMipSet1), null, mc.TexUidMipSet1.Select(arg => new TreeListViewEntry("UID", arg)).ToArray()),
									new TreeListViewEntry(nameof(TextureMap.TexUidMipSet2), null, mc.TexUidMipSet2.Select(arg => new TreeListViewEntry("UID", arg)).ToArray())
								)
							};
							break;
						}
						default:
						{
							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic)
							};
							break;
						}
					}

					OnUiThread(() =>
					{
						_infoControl.SetObjects(entries);
						_infoControl.ExpandAll();
						SetPreviewPanel(_infoControl);
					});

					break;
				}
			}
		}

		private void OnUiThread(Action action)
		{
			if (!InvokeRequired)
				action();
			else
				BeginInvoke(action);
		}

		private TreeListViewEntry CreateMipContainerReferenceEntry(MipContainerReference mcr)
		{
			return new TreeListViewEntry("MipContainerReference", null,
				new TreeListViewEntry("Var1", mcr.Var1),
				new TreeListViewEntry("MipTarget", mcr.MipTarget),
				new TreeListViewEntry("MipContainerUid", mcr.TextureMapSpecUid)
			);
		}

		private static TreeListViewEntry GetMetadataInfoEntry(ulong uid, ulong fileType)
		{
			return new TreeListViewEntry("Metadata", null,
				new TreeListViewEntry("UID", uid),
				new TreeListViewEntry("FileType", uid.ToString("X")),
				new TreeListViewEntry("Magic", (Magic)fileType),
				new TreeListViewEntry("AssetType", MagicHelper.GetFiletype(fileType))
			);
		}

		private void OpenForge(string filename)
		{
			OpenedForge = Forge.GetForge(filename);
			Text = $"Prism - {filename}";
		}
	}

	internal record AssetStream(ulong Uid, ulong Magic, BinaryReader Stream);

	internal record TreeListViewEntry(string Key, object Value, params TreeListViewEntry[] Children);
}