using System;
using System.Globalization;
using System.Windows.Data;
using AssetCatalog.Model;
using RainbowForge.Forge;

namespace AssetCatalog.Converters
{
	public class ForgeEntryToCatalogIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return "\uE7BA";

			var catalogEntry = ForgeCatalog.Instance.CatalogDb.Get(forgeEntry.Uid);

			switch (catalogEntry.Status)
			{
				case CatalogEntryStatus.Incomplete:
					return "";
				case CatalogEntryStatus.PartiallyComplete:
					return "\uE73C";
				case CatalogEntryStatus.Complete:
					return "\uE73E";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}