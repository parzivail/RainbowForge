using System.IO;

namespace RainbowForge.Components
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

		public static LocalizationPackage Read(BinaryReader r, uint version)
		{
			var fileMeta = FileMetaData.Read(r, version);

			var secondMagic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.LocalizationPackage, secondMagic);

			r.ReadBytes(8); // padding

			var cld = CompressedLocalizationData.Read(r);

			return new LocalizationPackage(fileMeta, cld);
		}
	}
}