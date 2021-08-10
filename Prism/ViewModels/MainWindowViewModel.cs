using System.Collections.ObjectModel;

namespace Prism.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		public ObservableCollection<AssetTab> OpenedAssets { get; set; } = new();

		public MainWindowViewModel()
		{
			OpenedAssets.Add(new AssetTab
			{
				Content = "Bbbbbb",
				Header = "Aaaaaa"
			});
			OpenedAssets.Add(new AssetTab
			{
				Content = "Tab 2",
				Header = "Tab 2"
			});
		}
	}

	public class AssetTab
	{
		public string Header { get; set; }
		public string Content { get; set; }
	}
}