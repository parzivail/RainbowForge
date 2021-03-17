using System.IO;

namespace RainbowForge
{
	public class FileMeta
	{
		public byte[] EncodedMeta { get; }
		public uint Var1 { get; }
		public uint Magic { get; }
		public ulong Uid { get; }

		private FileMeta(byte[] encodedMeta, uint var1, uint magic, ulong uid)
		{
			EncodedMeta = encodedMeta;
			Var1 = var1;
			Magic = magic;
			Uid = uid;
		}

		public static FileMeta Read(BinaryReader r)
		{
			var metaLen = r.ReadUInt32();

			var encodedMeta = r.ReadBytes((int) metaLen);
			var var1 = r.ReadUInt32();
			var magic = r.ReadUInt32();
			var uid = r.ReadUInt64();

			return new FileMeta(encodedMeta, var1, magic, uid);
		}
	}
}