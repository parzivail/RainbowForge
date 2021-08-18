using System.IO;

namespace RainbowForge
{
	public class FileMetaData
	{
		public byte[] EncodedMeta { get; }
		public uint Var1 { get; }
		public uint FileType { get; }
		public ulong Uid { get; }

		private FileMetaData(byte[] encodedMeta, uint var1, uint fileType, ulong uid)
		{
			EncodedMeta = encodedMeta;
			Var1 = var1;
			FileType = fileType;
			Uid = uid;
		}

		public static FileMetaData Read(BinaryReader r)
		{
			var metaLen = r.ReadUInt32();

			var encodedMeta = r.ReadBytes((int) metaLen);
			var var1 = r.ReadUInt32();
			var fileType = r.ReadUInt32();
			var uid = r.ReadUInt64();

			return new FileMetaData(encodedMeta, var1, fileType, uid);
		}
	}
}