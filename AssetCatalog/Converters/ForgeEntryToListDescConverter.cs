using System;
using System.Globalization;
using System.Windows.Data;
using RainbowForge.Forge;

namespace AssetCatalog.Converters
{
	public class ForgeEntryToListDescConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return "Invalid Entry";

			var catalogEntry = ForgeCatalog.Instance.CatalogDb.Get(forgeEntry.Uid);

			return catalogEntry.Name;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}