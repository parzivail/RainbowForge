using System;
using System.Globalization;
using System.Windows.Data;

namespace AssetCatalog.Converters
{
	public class CatalogStatusToTitleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is string status ? $"AssetCatalog - {status}" : "AssetCatalog";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}