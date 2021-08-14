using System;
using System.Globalization;
using System.Windows.Data;
using RainbowForge;
using RainbowForge.Core;

namespace AssetCatalog.Converters
{
	public class ForgeEntryToIconConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Entry forgeEntry)
				return "\uE7BA";

			var magic = MagicHelper.GetFiletype(forgeEntry.Name.FileType);

			switch (magic)
			{
				case AssetType.Mesh:
					return "\uF158";
				case AssetType.Texture:
					return "\uEB9F";
				case AssetType.Sound:
					return "\uE767";
				default:
					return "\uE9CE";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}