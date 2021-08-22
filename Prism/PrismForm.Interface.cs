using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using OpenTK;
using OpenTK.Graphics;
using Prism.Controls;
using Prism.Extensions;
using Prism.Render;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using SkiaSharp.Views.Desktop;

namespace Prism
{
	public partial class PrismForm
	{
		private readonly ToolStripLabel _statusForgeInfo;

		private readonly ToolStripMenuItem _bOpenForge;
		private readonly ToolStripMenuItem _bResetViewport;

		private readonly ToolStripMenuItem _bDumpAsBin;
		private readonly ToolStripMenuItem _bDumpAsDds;
		private readonly ToolStripMenuItem _bDumpAsObj;

		private readonly TextBox _searchTextBox;
		private readonly TreeListView _assetList;

		private readonly MinimalSplitContainer _splitContainer;
		private readonly GLControl _glControl;
		private readonly SKControl _imageControl;
		private readonly TreeListView _infoControl;
		private readonly TextBox _errorInfoControl;

		public PrismForm()
		{
			SuspendLayout();

			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Text = "Prism";

			Controls.Add(_splitContainer = new MinimalSplitContainer
			{
				Dock = DockStyle.Fill,
				SplitterDistance = 450,
				Panel1 =
				{
					Controls =
					{
						(_assetList = new TreeListView
						{
							Dock = DockStyle.Fill,
							View = View.Details,
							ShowGroups = false,
							FullRowSelect = true,
							UseFiltering = true,
							UseHotItem = false,
							UseHyperlinks = false,
							UseHotControls = false
						}),
						(_searchTextBox = new TextBox
						{
							Dock = DockStyle.Top
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

			_errorInfoControl = new TextBox
			{
				Multiline = true,
				Dock = DockStyle.Fill,
				ReadOnly = true,
				BackColor = Color.White,
				ForeColor = Color.Red
			};

			SetPreviewPanel(new Label
			{
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Text = "Open a Forge to get started."
			});

			ResumeLayout(true);

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
			_assetList.Columns.Add(new OLVColumn("Filename", null)
			{
				Width = 100,
				AspectGetter = rowObject =>
				{
					return rowObject switch
					{
						Entry e => e.MetaData.FileName,
						FlatArchiveEntry fae => fae.MetaData.FileName,
						_ => string.Empty
					};
				}
			});

			_assetList.Columns.Add(new OLVColumn("Type", null)
			{
				Width = 100,
				AspectGetter = rowObject =>
				{
					(ulong type, ulong uid) fileType = rowObject switch
					{
						Entry e => (e.MetaData.FileType, e.Uid),
						FlatArchiveEntry fae => (fae.MetaData.FileType, fae.MetaData.Uid),
						_ => (0xFFFFFFFFFFFFFFFF, 0)
					};

					return fileType;
				},
				AspectToStringConverter = value =>
				{
					var (type, uid) = (ValueTuple<ulong, ulong>)value;

					if (type == 0xFFFFFFFFFFFFFFFF)
						return string.Empty;

					if (Enum.IsDefined(typeof(Magic), type))
					{
						var m = (Magic)type;
						if (m == Magic.Metadata)
						{
							var container = _openedForge.GetContainer(uid);
							switch (container)
							{
								case Hash:
									return $"[{nameof(Hash)}]";
								case Descriptor:
									return $"[{nameof(Descriptor)}]";
							}
						}

						return m.ToString();
					}

					return type.ToString("X");
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
				Width = 210,
				AspectGetter = rowObject =>
				{
					return rowObject switch
					{
						Entry e => e.Uid,
						FlatArchiveEntry fae => fae.MetaData.Uid,
						_ => null
					};
				},
				AspectToStringConverter = value => $"{value:X16}"
			});

			_assetList.SelectedIndexChanged += OnAssetListOnSelectionChanged;

			_assetList.CanExpandGetter = model => { return model is Entry e && MagicHelper.GetFiletype(e.MetaData.FileType) == AssetType.FlatArchive; };

			_assetList.ChildrenGetter = model =>
			{
				if (model is not Entry e || MagicHelper.GetFiletype(e.MetaData.FileType) != AssetType.FlatArchive)
					return null;

				var container = _openedForge.GetContainer(e.Uid);
				if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

				var assetStream = forgeAsset.GetDataStream(_openedForge);
				var fa = FlatArchive.Read(assetStream);

				foreach (var entry in fa.Entries) _flatArchiveEntryMap[entry.MetaData.Uid] = e.Uid;

				return fa.Entries;
			};

			_assetList.CellRightClick += (sender, args) =>
			{
				var tlv = (TreeListView)sender;
				var meta = GetAssetMetaData(tlv.SelectedObject);

				var bCopyName = new ToolStripMenuItem("&Copy Name");
				bCopyName.Click += (o, eventArgs) => Clipboard.SetText(meta.Filename);

				var bCopyUid = new ToolStripMenuItem("&Copy UID");
				bCopyUid.Click += (o, eventArgs) => Clipboard.SetText($"0x{meta.Uid:X16}");

				var bCopyFiletype = new ToolStripMenuItem("&Copy Filetype");
				bCopyFiletype.Click += (o, eventArgs) => Clipboard.SetText($"0x{meta.Magic:X8}");

				args.MenuStrip = new ContextMenuStrip
				{
					Location = Cursor.Position,
					Items =
					{
						bCopyName,
						bCopyUid,
						bCopyFiletype
					}
				};
			};

			_searchTextBox.TextChanged += (sender, args) =>
			{
				var filterStr = _searchTextBox.Text;
				_assetList.SelectedIndex = -1;
				_assetList.ModelFilter = new ModelFilter(o => DoesEntryMatchFilter(o, filterStr));
			};
		}


		private static bool DoesEntryMatchFilter(object entry, string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
				return true;

			var meta = GetAssetMetaData(entry);

			if (ulong.TryParse(filter, NumberStyles.HexNumber, Thread.CurrentThread.CurrentCulture, out var filterUid) && filterUid == meta.Uid)
				return true;

			return meta.Filename.Contains(filter, StringComparison.OrdinalIgnoreCase) || (((Magic)meta.Magic).ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));
		}

		private void OnAssetListOnSelectionChanged(object sender, EventArgs args)
		{
			var selectedEntry = _assetList.SelectedObject;
			lock (_openedForge)
			{
				var stream = GetAssetStream(selectedEntry);
				if (stream != null)
					try
					{
						PreviewAsset(stream);
					}
					catch (Exception e)
					{
						OnUiThread(() =>
						{
							_errorInfoControl.Text = e.ToString();
							SetPreviewPanel(_errorInfoControl);
						});
					}

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
				type = MagicHelper.GetFiletype(assetStream.MetaData.Magic);
				magic = (Magic)assetStream.MetaData.Magic;
			}

			_bDumpAsBin.Enabled = assetStream != null;
			_bDumpAsDds.Enabled = type == AssetType.Texture;
			_bDumpAsObj.Enabled = type == AssetType.Mesh;
		}

		private void OnUiThread(Action action)
		{
			if (!InvokeRequired)
				action();
			else
				BeginInvoke(action);
		}
	}
}