using System.IO;
using System.Text;
using RainbowForge.Core;

namespace RainbowForge
{
	public class FileMetaData
	{
		public string FileName { get; }
		public byte[] EncodedMeta { get; }
		public uint ContainerType { get; }
		public uint FileType { get; }
		public ulong Uid { get; }

		private FileMetaData(string fileName, byte[] encodedMeta, uint containerType, uint fileType, ulong uid)
		{
			FileName = fileName;
			EncodedMeta = encodedMeta;
			ContainerType = containerType;
			FileType = fileType;
			Uid = uid;
		}

		public static FileMetaData Read(BinaryReader r)
		{
			var metaLen = r.ReadUInt32();

			var encodedMeta = r.ReadBytes((int)metaLen);
			var var1 = r.ReadUInt32();
			var fileType = r.ReadUInt32();
			var uid = r.ReadUInt64();

			var name = NameEncoding.DecodeName(encodedMeta, fileType, uid, 0, NameEncoding.FILENAME_ENCODING_FILE_KEY_STEP);

			return new FileMetaData(Encoding.ASCII.GetString(name), encodedMeta, var1, fileType, uid);
		}
	}
}