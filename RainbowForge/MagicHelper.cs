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
			0x56C88C7F, // GDOUIDescription2
			// Totally unknown
			0x21943A63, 0x23BF9925, 0x23CD3131, 0x24D19860, 0x25B46ADB, 0x27966F42, 0x297D86AF, 0x2C62E10B, 0x2F732FDB, 0x305DAAF0,
			0x307F9215, 0x30B11EF1, 0x31B492A9, 0x334B31A2, 0x337544B7, 0x33A3568D, 0x33ECCB02, 0x340378D4, 0x345D8BAB, 0x36BE7C80,
			0x38866309, 0x38D82699, 0x394B591A, 0x3BD86505, 0x415C097D, 0x431843B9, 0x43CEDB18, 0x44929C4E, 0x44BC3648, 0x47B5DECD,
			0x4802A946, 0x489C5552, 0x48B37D86, 0x4A798445, 0x4B00F860, 0x4BDEDF64, 0x4E4C84DB, 0x4EB4DD16, 0x522559E1, 0x55C79DF9,
			0x56C88C7F, 0x5833D2FF, 0x5C53C116, 0x5C68E246, 0x5EC04AA3, 0x6009EB19, 0x61517D87, 0x61CDA86C, 0x6200CD51, 0x625EA9A5,
			0x650F34CF, 0x6619D57A, 0x669403B9, 0x6A52331D, 0x6B231FF1, 0x6BE35F39, 0x6C95705C, 0x6DDC8C93, 0x709BB47B, 0x71288716,
			0x759B2F0C, 0x76D5826F, 0x7BCB6B7D, 0x7DC0C72E, 0x7EFE24E6, 0x802CF26E, 0x805F5E74, 0x82CA892F, 0x8331998F, 0x83E1A167,
			0x840757DE, 0x854A3D0B, 0x85BE0AE0, 0x8766EFE0, 0x87EEAF47, 0x8882306C, 0x8A4E6BA2, 0x91860285, 0x91DFE9EC, 0x92005DF2,
			0x936B86B6, 0x942EAEDC, 0x97D8B660, 0x99434A89, 0x9A1C7FA6, 0x9B3BB0F4, 0x9B4018D4, 0x9B8921EF, 0x9BB2CDF2, 0x9C3CF247,
			0x9CB7D50B, 0x9DEB0049, 0xA04ED9F8, 0xA1111DBC, 0xA43332F8, 0xA818CD8D, 0xA89F658D, 0xA96BA120, 0xA9A2526B, 0xAA97BA5F,
			0xADED9F2D, 0xAE148772, 0xAF296DB7, 0xB033548F, 0xB3E0418E, 0xB3FC5A70, 0xB4B2B9C2, 0xB648C1DF, 0xB660EA83, 0xB66B7701,
			0xB92FA8AC, 0xBAFB153E, 0xBB5542A0, 0xBFE86D68, 0xC3692044, 0xC8241D45, 0xC9BB7AEA, 0xC9C108A7, 0xCA7276F6, 0xCB12519E,
			0xCBD4939A, 0xCBF34700, 0xCE4A0B93, 0xCEA4CA0E, 0xD1082F1C, 0xD1339DA0, 0xD26D9DC9, 0xD74F36D1, 0xD9261E2F, 0xDAD658B0,
			0xDB7A4E87, 0xDBAA67AC, 0xDD535A95, 0xDD743C79, 0xDE8C31D8, 0xE2047AF0, 0xE46C4EFE, 0xE5929EEB, 0xEA266162, 0xEA9FA35E,
			0xEB02A76C, 0xECD39854, 0xF2B7EF38, 0xF40A6466, 0xF4FF820A, 0xF504EECC, 0xF5AC3340, 0xF622AE46, 0xF68E6B43, 0xF776ED2A,
			0xF8E187EA, 0xF8F4D2F9, 0xFA32E3AF, 0xFABAE07A, 0xFB364970, 0xFCE622E2, 0xFDA4B79A, 0xFE15F469, 0xFF138919,
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