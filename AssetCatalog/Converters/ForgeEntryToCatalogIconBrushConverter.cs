using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AssetCatalog.Model;
using RainbowForge.Forge;

namespace AssetCatalog.Converters
{
	public class ForgeEntryToCatalogIconBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return Brushes.DarkGray;

			var catalogEntry = App.Catalog.CatalogDb.Get(forgeEntry.Uid);

			switch (catalogEntry.Status)
			{
				case CatalogEntryStatus.Incomplete:
					return Brushes.Red;
				case CatalogEntryStatus.PartiallyComplete:
					return Brushes.Orange;
				case CatalogEntryStatus.Complete:
					return Brushes.LimeGreen;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class ForgeEntryToListDescConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return "Invalid Entry";

			var catalogEntry = App.Catalog.CatalogDb.Get(forgeEntry.Uid);

			return catalogEntry.Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}