using System.IO;

namespace RainbowForge.Core
{
	public class MetaLink
	{
		public ulong Uid { get; }
		public uint Size { get; }
		public ulong Un1 { get; }
		public ulong Un2 { get; }
		public bool Extra { get; }

		private MetaLink(ulong uid, uint size)
		{
			Uid = uid;
			Size = size;
			Un1 = 0;
			Un2 = 0;
			Extra = false;
		}

		private MetaLink(ulong uid, uint size, ulong un1, ulong un2)
		{
			Uid = uid;
			Size = size;
			Un1 = un1;
			Un2 = un2;
			Extra = true;
		}

		public static MetaLink Read(BinaryReader r, bool extra)
		{
			var uid = r.ReadUInt64();
			var size = r.ReadUInt32();

			if (!extra)
				return new MetaLink(uid, size);

			var un1 = r.ReadUInt64();
			var un2 = r.ReadUInt64();
			return new MetaLink(uid, size, un1, un2);
		}
	}
}