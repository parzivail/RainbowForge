using System;
using System.IO;

namespace RainbowForge
{
	public static class MagicHelper
	{
		public static void AssertEquals(Magic magic, ulong value)
		{
			if (!Equals(magic, value))
				throw new InvalidDataException();
		}

		public static bool Equals(Magic magic, ulong value)
		{
			return (Magic) value == magic;
		}

		public static AssetType GetFiletype(ulong magic)
		{
			if (!Enum.IsDefined(typeof(Magic), magic))
				return AssetType.Unknown;

			return (Magic) magic switch
			{
				Magic.Mesh => AssetType.Mesh,
				Magic.TextureA => AssetType.Texture,
				Magic.TextureB => AssetType.Texture,
				Magic.TextureC => AssetType.Texture,
				Magic.TextureD => AssetType.Texture,
				Magic.TextureE => AssetType.Texture,
				Magic.TextureGui1 => AssetType.Texture,
				Magic.TextureGui2 => AssetType.Texture,
				Magic.WemSound => AssetType.Sound,
				Magic.FlatArchive1 => AssetType.FlatArchive,
				Magic.FlatArchive2 => AssetType.FlatArchive,
				Magic.FlatArchive3 => AssetType.FlatArchive,
				Magic.FlatArchive4 => AssetType.FlatArchive,
				Magic.FlatArchive5 => AssetType.FlatArchive,
				Magic.FlatArchive6 => AssetType.FlatArchive,
				Magic.FlatArchive7 => AssetType.FlatArchive,
				Magic.FlatArchive8 => AssetType.FlatArchive,
				Magic.FlatArchive9 => AssetType.FlatArchive,
				Magic.FlatArchive10 => AssetType.FlatArchive,
				Magic.FlatArchive11 => AssetType.FlatArchive,
				Magic.FlatArchive12 => AssetType.FlatArchive,
				_ => AssetType.Unknown
			};
		}
	}
}