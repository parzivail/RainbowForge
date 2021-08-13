using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using OpenTK;
using OpenTK.Graphics;
using Pfim;
using Prism.Controls;
using Prism.Extensions;
using Prism.Render;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Dump;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;
using RainbowForge.Mesh;
using RainbowForge.RenderPipeline;
using RainbowForge.Texture;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Prism
{
	public class PrismForm : Form
	{
		private readonly OpenFileDialog _ofdForges;

		private readonly ToolStripMenuItem _bOpenForge;
		private readonly ToolStripMenuItem _bResetViewport;

		private readonly ToolStripMenuItem _bDumpAsBin;

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
								(_bDumpAsBin = new ToolStripMenuItem("&Bin"))
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

			_ofdForges = new OpenFileDialog
			{
				Filter = "Forge Files|*.forge"
			};

			_bOpenForge.Click += (sender, args) =>
			{
				if (_ofdForges.ShowDialog() != DialogResult.OK)
					return;

				OpenForge(_ofdForges.FileName);
			};

			_bResetViewport.Click += (sender, args) => _renderer3d.ResetView();

			_bDumpAsBin.Click += (sender, args) =>
			{
				var sfd = new SaveFileDialog
				{
					Filter = "Binary Files|*.bin"
				};

				if (sfd.ShowDialog() == DialogResult.OK)
					DumpSelectionAsBin(sfd.FileName);
			};

			SetupRenderer();
			SetupAssetList();
		}

		private void DumpSelectionAsBin(string fileName)
		{
			var streamData = GetAssetStream(_assetList.SelectedObject);
			using var stream = streamData.Stream;
			DumpHelper.DumpBin(fileName, stream.BaseStream);
		}

		private AssetStream GetAssetStream(object o)
		{
			switch (o)
			{
				case Entry entry:
				{
					var container = _openedForge.GetContainer(entry.Uid);
					if (container is not ForgeAsset forgeAsset)
						return null;

					return new AssetStream(entry.Uid, entry.Name.FileType, forgeAsset.GetDataStream(_openedForge));
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

		private async void OnAssetListOnSelectionChanged(object sender, EventArgs args)
		{
			var selectedEntry = _assetList.SelectedObject;
			lock (_openedForge)
			{
				var stream = GetAssetStream(selectedEntry);
				if (stream != null)
					PreviewAsset(stream);
			}
		}

		private void PreviewAsset(AssetStream assetStream)
		{
			using var stream = assetStream.Stream;
			switch (MagicHelper.GetFiletype(assetStream.Magic))
			{
				case AssetType.Mesh:
				{
					var header = MeshHeader.Read(stream);
					var mesh = Mesh.Read(stream, header);

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
						case ImageFormat.Rgba32:
							colorType = SKColorType.Bgra8888;
							break;
						default:
							throw new ArgumentException($"Skia unable to interpret pfim format: {image.Format}");
					}

					var imageInfo = new SKImageInfo(image.Width, image.Height, colorType);
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
						case Magic.MeshProperties:
						{
							var mp = MeshProperties.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new("MeshProperties", null,
									new TreeListViewEntry("Var1", mp.Var1),
									new TreeListViewEntry("Var2", mp.Var2),
									new TreeListViewEntry("Mesh UID", mp.MeshUid),
									new TreeListViewEntry("MaterialContainers", null, mp.MaterialContainers.Select(arg => new TreeListViewEntry("UID", arg)).ToArray())
								)
							};
							break;
						}
						case Magic.MaterialContainer:
						{
							var mc = MaterialContainer.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new("MaterialContainer", null,
									new TreeListViewEntry("BaseMipContainers", null, mc.BaseMipContainers.Select(CreateMipContainerReferenceEntry).ToArray()),
									new TreeListViewEntry("SecondaryMipContainers", null, mc.SecondaryMipContainers.Select(CreateMipContainerReferenceEntry).ToArray()),
									new TreeListViewEntry("TertiaryMipContainers", null, mc.TertiaryMipContainers.Select(CreateMipContainerReferenceEntry).ToArray())
								)
							};
							break;
						}
						case Magic.MipContainer:
						{
							var mc = MipContainer.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new("MipContainer", null,
									new TreeListViewEntry("MipUid", mc.MipUid),
									new TreeListViewEntry("TextureType", mc.TextureType)
								)
							};
							break;
						}
						case Magic.MipSet:
						{
							var mc = MipSet.Read(stream);

							entries = new List<TreeListViewEntry>
							{
								GetMetadataInfoEntry(assetStream.Uid, assetStream.Magic),
								new("MipContainer", null,
									new TreeListViewEntry("Var1", mc.Var1),
									new TreeListViewEntry("Var2", mc.Var2),
									new TreeListViewEntry("Var3", mc.Var3),
									new TreeListViewEntry("Var4", mc.Var4),
									new TreeListViewEntry("TexUidMipSet1", null, mc.TexUidMipSet1.Select(arg => new TreeListViewEntry("UID", arg)).ToArray()),
									new TreeListViewEntry("TexUidMipSet1", null, mc.TexUidMipSet1.Select(arg => new TreeListViewEntry("UID", arg)).ToArray())
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
				new TreeListViewEntry("MipContainerUid", mcr.MipContainerUid)
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