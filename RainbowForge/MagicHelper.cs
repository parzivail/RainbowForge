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
			0x9468B9E2, // TextureGui0
			0x05A61FAD, // TextureGui1
			0x427411A3, // WemSound
			// Totally unknown
			0x25B46ADB, 0x297D86AF, 0x2C62E10B, 0x2F732FDB, 0x307F9215, 0x30B11EF1, 0x33ECCB02, 0x36BE7C80, 0x38866309, 0x38D82699, 0x394B591A, 0x3BD86505, 0x431843B9,
			0x44929C4E, 0x44BC3648, 0x4802A946, 0x4B00F860, 0x4BDEDF64, 0x4E4C84DB, 0x522559E1, 0x55C79DF9, 0x5833D2FF, 0x5C68E246, 0x6009EB19, 0x6200CD51, 0x650F34CF, 0x669403B9, 0x6A52331D,
			0x6BE35F39, 0x6DDC8C93, 0x709BB47B, 0x7EFE24E6, 0x802CF26E, 0x805F5E74, 0x8331998F, 0x83E1A167, 0x840757DE, 0x854A3D0B, 0x8766EFE0, 0x87EEAF47, 0x8882306C, 0x8A4E6BA2, 0x91860285,
			0x91DFE9EC, 0x92005DF2, 0x942EAEDC, 0x97D8B660, 0x99434A89, 0x9A1C7FA6, 0x9B3BB0F4, 0x9BB2CDF2, 0x9CB7D50B, 0xA04ED9F8, 0xA43332F8, 0xA818CD8D, 0xA89F658D, 0xA96BA120, 0xAA97BA5F,
			0xADED9F2D, 0xB033548F, 0xB3FC5A70, 0xB4B2B9C2, 0xB648C1DF, 0xB660EA83, 0xB66B7701, 0xBAFB153E, 0xBB5542A0, 0xBFE86D68, 0xC8241D45, 0xC9BB7AEA, 0xCA7276F6, 0xCB12519E, 0xCBD4939A,
			0xD1082F1C, 0xD1339DA0, 0xD26D9DC9, 0xD9261E2F, 0xDAD658B0, 0xDB7A4E87, 0xDBAA67AC, 0xEA266162, 0xEB02A76C, 0xECD39854, 0xF4FF820A, 0xF5AC3340, 0xF68E6B43, 0xF8F4D2F9, 0xFABAE07A,
			0xFB364970, 0xFCE622E2, 0xFF138919,
			// Possible Names
			0x4EB4DD16, // BundleGameObject?
			0x61CDA86C, // ShotClassificationInfo?
			0x625EA9A5, // LimitedTextureDetail?
			0x7BCB6B7D, // SoundWorldComponent?
			0xB92FA8AC, // SiegeBootstrapLocator?
			0xFA32E3AF, // ManagedSoundBank?
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
				Magic.WemSound => AssetType.Sound,
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