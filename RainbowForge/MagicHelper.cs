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
				Magic.CompiledMeshObject => AssetType.Mesh,
				Magic.CompiledLowResolutionTextureMap => AssetType.Texture,
				Magic.CompiledMediumResolutionTextureMap => AssetType.Texture,
				Magic.CompiledHighResolutionTextureMap => AssetType.Texture,
				Magic.CompiledUltraResolutionTextureMap => AssetType.Texture,
				Magic.CompiledFutureResolutionTextureMap => AssetType.Texture,
				Magic.TextureGui1 => AssetType.Texture,
				Magic.TextureGui2 => AssetType.Texture,
				Magic.WemSound => AssetType.Sound,
				Magic.BuildTable => AssetType.FlatArchive,
				Magic.EntityBuilder => AssetType.FlatArchive,
				Magic.WeaponData => AssetType.FlatArchive,
				Magic.GameBootstrap => AssetType.FlatArchive,
				Magic.LocalizationPackage => AssetType.FlatArchive,
				Magic.FlatArchive6 => AssetType.FlatArchive,
				Magic.World => AssetType.FlatArchive,
				Magic.LoadUnit => AssetType.FlatArchive,
				Magic.WorldMetaData => AssetType.FlatArchive,
				Magic.GIStream => AssetType.FlatArchive,
				Magic.CompiledMeshShapeDataObject => AssetType.FlatArchive,
				Magic.FlatArchive12 => AssetType.FlatArchive,
				_ => AssetType.Unknown
			};
		}
	}
}