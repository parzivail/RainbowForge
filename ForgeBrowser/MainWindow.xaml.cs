using System;
using System.Windows;
using ForgeBrowser.Command;
using Microsoft.Win32;

namespace ForgeBrowser
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DelegateCommand<object> _openCommand;

		public DelegateCommand<object> OpenCommand => _openCommand ??= new DelegateCommand<object>(OpenExecutable);

		public MainWindow()
		{
			InitializeComponent();
		}

		private void OpenExecutable(object obj)
		{
			var ofd = new OpenFileDialog
			{
				Filter = "Game Executable|RainbowSix.exe"
			};

			var res = ofd.ShowDialog();
			if (res.HasValue && res.Value) throw new NotImplementedException();
		}
	}
}