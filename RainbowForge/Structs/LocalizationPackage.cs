﻿using System.IO;

namespace RainbowForge.Structs
{
	public class LocalizationPackage
	{
		public FileMetaData FileMeta { get; }
		public CompressedLocalizationData Data { get; }

		private LocalizationPackage(FileMetaData fileMeta, CompressedLocalizationData data)
		{
			FileMeta = fileMeta;
			Data = data;
		}

		public static LocalizationPackage Read(BinaryReader r)
		{
			var fileMeta = FileMetaData.Read(r);

			var secondMagic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.LocalizationPackage, secondMagic);

			r.ReadBytes(8); // padding

			var cld = CompressedLocalizationData.Read(r);

			return new LocalizationPackage(fileMeta, cld);
		}
	}

	public class CompressedLocalizationData
	{
		public static CompressedLocalizationData Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.CompressedLocalizationData, magic);

			return new CompressedLocalizationData();
		}
	}
}