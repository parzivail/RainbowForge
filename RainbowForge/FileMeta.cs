using System.IO;

namespace RainbowForge
{
	public class FileMeta
	{
		public byte[] EncodedMeta { get; }
		public uint Var1 { get; }
		public uint Magic { get; }
		public ulong Uid { get; }
		public uint Var2 { get; }
		public uint Var3 { get; }

		private FileMeta(byte[] encodedMeta, uint var1, uint magic, ulong uid, uint var2, uint var3)
		{
			EncodedMeta = encodedMeta;
			Var1 = var1;
			Magic = magic;
			Uid = uid;
			Var2 = var2;
			Var3 = var3;
		}

		public static FileMeta Read(BinaryReader r)
		{
			var metaLen = r.ReadUInt32();

			var encodedMeta = r.ReadBytes((int) metaLen);
			var var1 = r.ReadUInt32();
			var magic = r.ReadUInt32();
			var uid = r.ReadUInt64();
			var secondMagic = r.ReadUInt32();

			if (magic != secondMagic)
				throw new InvalidDataException($"Second magic mismatch at 0x{r.BaseStream.Position - 4:X}");

			var var2 = r.ReadUInt32();
			var var3 = r.ReadUInt32();

			return new FileMeta(encodedMeta, var1, magic, uid, var2, var3);
		}
	}
}