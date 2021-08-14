using System.IO;

namespace RainbowForge.Core
{
	public class Entry
	{
		public ulong Uid { get; }
		public long Offset { get; }
		public uint Size { get; }
		public long End { get; }

		public NameEntry Name { get; set; }

		public Entry(ulong uid, long offset, uint size, long end)
		{
			Uid = uid;
			Offset = offset;
			Size = size;
			End = end;
		}

		public static Entry Read(BinaryReader r)
		{
			var offset = r.ReadInt64();
			var uid = r.ReadUInt64();
			var size = r.ReadUInt32();
			var end = offset + size;

			return new Entry(uid, offset, size, end);
		}
	}
}