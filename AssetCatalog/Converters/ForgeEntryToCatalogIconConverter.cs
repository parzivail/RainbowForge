using System;
using System.Globalization;
using System.Windows.Data;
using RainbowForge.Forge;

namespace AssetCatalog.Converters
{
	public class ForgeEntryToCatalogIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return "\uE7BA";

			// TODO: find catalog db entry for forge entry and see if it's complete or incomplete

			return "\uE711";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}