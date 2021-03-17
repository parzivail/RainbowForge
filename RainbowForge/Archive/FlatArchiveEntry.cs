using System.IO;

namespace RainbowForge.Archive
{
	public class FlatArchiveEntry
	{
		public FileMeta Meta { get; }
		public long PayloadOffset { get; }
		public int PayloadLength { get; }

		private FlatArchiveEntry(FileMeta meta, long payloadOffset, int payloadLength)
		{
			Meta = meta;
			PayloadOffset = payloadOffset;
			PayloadLength = payloadLength;
		}

		public static FlatArchiveEntry Read(BinaryReader r)
		{
			var meta = FileMeta.Read(r);

			var length = meta.Var1 - 8; // length - int64(uid)

			var payloadOffset = r.BaseStream.Position;
			r.BaseStream.Seek(length, SeekOrigin.Current);

			return new FlatArchiveEntry(meta, payloadOffset, (int) length);
		}
	}
}