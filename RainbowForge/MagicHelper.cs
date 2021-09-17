using System;
using System.Collections.Generic;
using System.IO;

namespace RainbowForge
{
	public static class MagicHelper
	{
		public static readonly HashSet<ulong> UnknownMagics = new()
		{
			// Named but not correctly
			0x1014FA99, // FileContainer
			0x57FBAA34, // File container magic
			0x9468B9E2, // TextureGui0
			0x05A61FAD, // TextureGui1
			// Notable magics
			0x61CDA86C, // Not ShotClassificationInfo even though the CRC fits, this magic is unicode font-related
			// Totally unknown
			0xBB5542A0, 0xDBAA67AC, 0x2FEF6C7F, 0xF5AC3340, 0x01DD1BAE, 0x07E763B3, 0x854A3D0B, 0x650F34CF, 0x6A52331D, 0x297D86AF, 0x87EEAF47, 0xA89F658D, 0x431843B9, 0xC8241D45, 0x61CDA86C
		};

		public static void AssertEquals(Magic magic, ulong value)
		{
			if (!Equals(magic, value))
				throw new InvalidDataException();
		}

		public static bool Equals(Magic magic, ulong value)
		{
			return (Magic)value == magic;
		}

		public static AssetType GetFiletype(ulong magic)
		{
			if (!Enum.IsDefined(typeof(Magic), magic))
				return AssetType.Unknown;

			return (Magic)magic switch
			{
				Magic.CompiledMeshObject => AssetType.Mesh,
				Magic.CompiledLowResolutionTextureMap => AssetType.Texture,
				Magic.CompiledMediumResolutionTextureMap => AssetType.Texture,
				Magic.CompiledHighResolutionTextureMap => AssetType.Texture,
				Magic.CompiledUltraResolutionTextureMap => AssetType.Texture,
				Magic.CompiledFutureResolutionTextureMap => AssetType.Texture,
				Magic.TextureGui0 => AssetType.Texture,
				Magic.TextureGui1 => AssetType.Texture,
				Magic.CompiledSoundMedia => AssetType.Sound,
				Magic.EntityBuilder => AssetType.FlatArchive,
				Magic.WeaponData => AssetType.FlatArchive,
				Magic.GameBootstrap => AssetType.FlatArchive,
				Magic.PlatformManager => AssetType.FlatArchive,
				Magic.World => AssetType.FlatArchive,
				Magic.LoadUnit => AssetType.FlatArchive,
				Magic.CompiledSoundBank => AssetType.FlatArchive,
				Magic.BuildTable => AssetType.FlatArchive,
				Magic.CompiledMeshShapeDataObject => AssetType.FlatArchive,
				Magic.GIStream => AssetType.FlatArchive,
				Magic.WorldMetaData => AssetType.FlatArchive,
				_ => AssetType.Unknown
			};
		}
	}
}