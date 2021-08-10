using System.IO;

namespace RainbowForge.Archive
{
	public class FlatArchiveEntry
	{
		public FileMeta Meta { get; }
		public int Index { get; }
		public long PayloadOffset { get; }
		public int PayloadLength { get; }

		private FlatArchiveEntry(FileMeta meta, int index, long payloadOffset, int payloadLength)
		{
			Meta = meta;
			Index = index;
			PayloadOffset = payloadOffset;
			PayloadLength = payloadLength;
		}

		public static FlatArchiveEntry Read(BinaryReader r, int index)
		{
			var meta = FileMeta.Read(r);

			var length = meta.Var1 - 8; // length - int64(uid)

			var payloadOffset = r.BaseStream.Position;

			if (r.BaseStream.Length < r.BaseStream.Position + length)
				throw new EndOfStreamException();

			r.BaseStream.Seek(length, SeekOrigin.Current);

			return new FlatArchiveEntry(meta, index, payloadOffset, (int) length);
		}
	}
}