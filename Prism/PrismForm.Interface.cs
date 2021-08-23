using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
		private const string SETTINGS_FILENAME = "settings.json";

		private readonly ToolStripLabel _statusForgeInfo;

		private readonly ToolStripMenuItem _bOpenForge;
		private readonly ToolStripMenuItem _bResetViewport;
		private readonly ToolStripMenuItem _bExportPath;

		private readonly ToolStripMenuItem _bDumpAsBinHeader;
		private readonly ToolStripMenuItem _bDumpAsBinQuick;
		private readonly ToolStripMenuItem _bDumpAsBinAs;

		private readonly ToolStripMenuItem _bDumpAsDdsHeader;
		private readonly ToolStripMenuItem _bDumpAsDdsQuick;
		private readonly ToolStripMenuItem _bDumpAsDdsAs;

		private readonly ToolStripMenuItem _bDumpAsPngHeader;
		private readonly ToolStripMenuItem _bDumpAsPngQuick;
		private readonly ToolStripMenuItem _bDumpAsPngAs;

		private readonly ToolStripMenuItem _bDumpAsObjHeader;
		private readonly ToolStripMenuItem _bDumpAsObjQuick;
		private readonly ToolStripMenuItem _bDumpAsObjAs;

		private readonly TextBox _searchTextBox;
		private readonly TreeListView _assetList;

		private readonly MinimalSplitContainer _splitContainer;
		private readonly GLControl _glControl;
		private readonly SKControl _imageControl;
		private readonly TreeListView _infoControl;
		private readonly TextBox _errorInfoControl;

		private ToolStripMenuItem _bEditSettings;

		private PrismSettings _settings;

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
							Dock = DockStyle.Top,
							PlaceholderText = "Search groups (i.e. keyword, ?type, #uid, seperated by commas)"
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
							}),
							new ToolStripSeparator(),
							(_bEditSettings = new ToolStripMenuItem("&Settings")),
						}
					},
					new ToolStripDropDownButton
					{
						Text = "&Export",
						DropDownItems =
						{
							(_bDumpAsBinHeader = new ToolStripMenuItem("&Binary File (*.bin)")
							{
								DropDownItems =
								{
									(_bDumpAsBinQuick = new ToolStripMenuItem("&Quick Export")),
									(_bDumpAsBinAs = new ToolStripMenuItem("&Export as...")),
								}
							}),
							new ToolStripSeparator(),
							(_bDumpAsDdsHeader = new ToolStripMenuItem("&DirectDraw Surface (*.dds)")
							{
								DropDownItems =
								{
									(_bDumpAsDdsQuick = new ToolStripMenuItem("&Quick Export")),
									(_bDumpAsDdsAs = new ToolStripMenuItem("&Export as...")),
								}
							}),
							(_bDumpAsPngHeader = new ToolStripMenuItem("&PNG (*.png)")
							{
								DropDownItems =
								{
									(_bDumpAsPngQuick = new ToolStripMenuItem("&Quick Export")),
									(_bDumpAsPngAs = new ToolStripMenuItem("&Export as...")),
								}
							}),
							new ToolStripSeparator(),
							(_bDumpAsObjHeader = new ToolStripMenuItem("&Wavefront OBJ (*.obj)")
							{
								DropDownItems =
								{
									(_bDumpAsObjQuick = new ToolStripMenuItem("&.obj | Quick Export")),
									(_bDumpAsObjAs = new ToolStripMenuItem("&.obj | Export as..."))
								}
							})
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

			_bEditSettings.Click += (sender, args) =>
			{
				if (new SettingsForm(SETTINGS_FILENAME).ShowDialog(this) == DialogResult.OK)
					_settings = PrismSettings.Load(SETTINGS_FILENAME);
			};

			_bDumpAsBinQuick.Click += CreateDumpEventHandler(DumpSelectionAsBin);
			_bDumpAsBinAs.Click += CreateDumpEventHandler(DumpSelectionAsBin, true);

			_bDumpAsDdsQuick.Click += CreateDumpEventHandler(DumpSelectionAsDds);
			_bDumpAsDdsAs.Click += CreateDumpEventHandler(DumpSelectionAsDds, true);

			_bDumpAsPngQuick.Click += CreateDumpEventHandler(DumpSelectionAsPng);
			_bDumpAsPngAs.Click += CreateDumpEventHandler(DumpSelectionAsPng, true);

			_bDumpAsObjQuick.Click += CreateDumpEventHandler(DumpSelectionAsObj);
			_bDumpAsObjAs.Click += CreateDumpEventHandler(DumpSelectionAsObj, true);

			_bResetViewport.Click += (sender, args) => _renderer3d.ResetView();

			SetupRenderer();
			SetupAssetList();

			UpdateAbility(null);

			_settings = PrismSettings.Load(SETTINGS_FILENAME);
		}

		private EventHandler CreateDumpEventHandler(Action<string, object> action, bool saveAs = false)
		{
			return (_, _) =>
			{
				var outputPath = _settings.QuickExportLocation;

				if (saveAs)
				{
					var folderBrowserDialog = new FolderBrowserDialog();
					if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
						return;

					outputPath = folderBrowserDialog.SelectedPath;
				}

				Directory.CreateDirectory(outputPath);

				foreach (var selectedObject in _assetList.SelectedObjects)
				{
					action(outputPath, selectedObject);
				}
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
			var searchGroups = filter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var groupMatches = new List<bool>();

			var meta = GetAssetMetaData(entry);

			foreach (var t in searchGroups)
			{
				if (string.IsNullOrWhiteSpace(t))
				{
					groupMatches.Add(true);
					continue;
				}

				if (t.StartsWith('#') && ulong.TryParse(t[1..], NumberStyles.HexNumber, Thread.CurrentThread.CurrentCulture, out var filterUid) &&
				    filterUid == meta.Uid)
				{
					groupMatches.Add(true);
					continue;
				}

				if (t.StartsWith('?'))
				{
					groupMatches.Add(((Magic)meta.Magic).ToString().Contains(t[1..], StringComparison.OrdinalIgnoreCase));
					continue;
				}

				groupMatches.Add(meta.Filename.Contains(t, StringComparison.OrdinalIgnoreCase));
			}

			return groupMatches.All(x => x);
		}

		private void OnAssetListOnSelectionChanged(object sender, EventArgs args)
		{
			if (_assetList.SelectedObjects.Count < 1) return;
			var selectedEntry = _assetList.SelectedObjects[0]; // TODO: find a way to get the last selected index to preview the latest selected asset
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

			_bDumpAsBinHeader.Enabled = assetStream != null;
			_bDumpAsDdsHeader.Enabled = _bDumpAsPngHeader.Enabled = type == AssetType.Texture;
			_bDumpAsObjHeader.Enabled = type == AssetType.Mesh;
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