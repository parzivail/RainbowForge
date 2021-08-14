using System;
using System.Globalization;
using System.Windows.Data;

namespace AssetCatalog.Converters
{
	public class ForgeToTitleMsgConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Forge forge)
				return "";

			return $"{forge.NumEntries} entries, v{forge.Version}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}