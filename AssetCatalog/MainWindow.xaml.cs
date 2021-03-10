using System.Windows;
using Microsoft.Win32;

namespace AssetCatalog
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			App.Catalog.CatalogDb.Connect();
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

			App.Catalog.OpenForge(dialog.OpenFile());
			App.Catalog.Status = dialog.FileName;
		}
	}
}