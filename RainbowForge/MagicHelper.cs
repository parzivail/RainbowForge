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
			// Possible Names
			0x61CDA86C, // ShotClassificationInfo?
			// Totally unknown
			0x11C5A979, 0xDBAA67AC, 0x91DFE9EC, 0xCBD4939A, 0x01DD1BAE, 0x07E763B3, 0x44929C4E, 0xB66B7701, 0x6A52331D, 0x297D86AF, 0x87EEAF47, 0xA43332F8, 0x431843B9, 0xC8241D45, 0x6009EB19,
			0x0D8CA779, 0x669403B9, 0xD26D9DC9, 0x9CB7D50B, 0x8BB308EA, 0xC3645505, 0xA818CD8D, 0x854A3D0B, 0x91860285, 0x0745E69C, 0xFABAE07A, 0x101AF874, 0x4802A946, 0xF5AC3340, 0xB648C1DF,
			0x307F9215, 0xA89F658D, 0x650F34CF, 0xBB5542A0, 0x2FEF6C7F, 0x253D8D16
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