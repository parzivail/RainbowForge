using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AssetCatalog.Model;
using AssetCatalog.Render;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using RainbowForge;
using RainbowForge.Forge;
using RainbowForge.Mesh;
using RainbowForge.Texture;

namespace AssetCatalog
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly ModelRenderer _modelRenderer;

		private ulong _loadedMeshUid;

		public MainWindow()
		{
			InitializeComponent();

			var settings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 1,
				RenderContinuously = true
			};
			ModelViewport.Start(settings);

			_modelRenderer = new ModelRenderer(ModelViewport);
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			ForgeCatalog.Instance.CatalogDb.Connect();
		}

		private void OpenForge_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Filter = "Forge Databanks|*.forge"
			};

			var result = dialog.ShowDialog();

			if (!result.HasValue || !result.Value)
				return;

			ForgeCatalog.Instance.OpenForge(dialog.OpenFile());
			ForgeCatalog.Instance.Status = dialog.FileName;
		}

		private void OnFilterChanged(object sender, RoutedEventArgs e)
		{
			ulong.TryParse(AssetUidFilterTextbox.Text, out var uid);
			ForgeCatalog.Instance.SetFilter(StatusFilterList.SelectedItems.Cast<CatalogEntryStatus>().ToArray(), CategoryFilterList.SelectedItems.Cast<CatalogAssetCategory>().ToArray(), uid,
				AssetNameFilterTextbox.Text);
		}

		private void OnCopyUid_Click(object sender, RoutedEventArgs e)
		{
			if (ForgeCatalog.Instance.SelectedEntry == null)
				return;

			Clipboard.SetText(ForgeCatalog.Instance.SelectedEntry.Uid.ToString());
		}

		private void OnSaveEntry_Click(object sender, RoutedEventArgs e)
		{
			var entry = ForgeCatalog.Instance.SelectedCatalogEntry;
			ForgeCatalog.Instance.CatalogDb.Put(ForgeCatalog.Instance.SelectedEntry.Uid, entry);
		}

		private void ModelViewport_OnRender(TimeSpan obj)
		{
			var entry = ForgeCatalog.Instance.SelectedEntry;
			if (entry != null && _loadedMeshUid != entry.Uid)
			{
				var filetype = MagicHelper.GetFiletype(entry.Name.FileType);

				switch (filetype)
				{
					case AssetType.Mesh:
					{
						using var stream = GetAssetStream(entry);
						var header = MeshHeader.Read(stream);
						var mesh = Mesh.Read(stream, header);

						_modelRenderer.BuildModelQuads(mesh);
						_modelRenderer.SetTexture(null);

						_modelRenderer.SetPartBounds(header.ObjectBoundingBoxes.Take((int) (header.ObjectBoundingBoxes.Length / header.NumLods)).ToArray());

						break;
					}
					case AssetType.Texture:
					{
						using var stream = GetAssetStream(entry);

						var texture = Texture.Read(stream);

						var bmp = DdsHelper.GetBitmap(DdsHelper.GetDds(texture, texture.ReadSurfaceBytes(stream)));

						_modelRenderer.BuildTextureMesh(texture);
						_modelRenderer.SetTexture(bmp);

						_modelRenderer.SetPartBounds(Array.Empty<BoundingBox>());
						break;
					}
				}

				_loadedMeshUid = entry.Uid;
			}

			_modelRenderer.Render();

			var err = GL.GetError();
			if (err != ErrorCode.NoError)
				Debug.WriteLine(err);

			GL.Finish();
		}

		private static BinaryReader GetAssetStream(Entry entry)
		{
			var forge = ForgeCatalog.Instance.OpenedForge;
			var container = forge.GetContainer(entry.Uid);

			if (container is not ForgeAsset file)
				throw new InvalidDataException("Entry with asset header was not file");

			if (file.FileBlock == null)
				throw new InvalidDataException("Asset file contained no file block");

			return new BinaryReader(file.FileBlock.GetDecompressedStream(forge.Stream));
		}

		private void ModelViewport_OnMouseMove(object sender, MouseEventArgs e)
		{
			_modelRenderer.OnMouseMove(e);
		}

		private void ModelViewport_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			_modelRenderer.OnMouseDown(e);

			// var devPt = e.MouseDevice.GetPosition(ModelViewport);
			// var id = _modelRenderer.GetObjectIdAt((int) devPt.X, (int) devPt.Y);
		}

		private void ModelViewport_OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			_modelRenderer.OnMouseWheel(e);
		}
	}
}