using System.IO;

namespace RainbowForge.Archive
{
	public class FlatArchiveEntry
	{
		public FileMetaData MetaData { get; }
		public int Index { get; }
		public long PayloadOffset { get; }
		public int PayloadLength { get; }

		private FlatArchiveEntry(FileMetaData metaData, int index, long payloadOffset, int payloadLength)
		{
			MetaData = metaData;
			Index = index;
			PayloadOffset = payloadOffset;
			PayloadLength = payloadLength;
		}

		public static FlatArchiveEntry Read(BinaryReader r, uint version, int index)
		{
			var meta = FileMetaData.Read(r, version);

			var length = meta.ContainerType - 8; // length - int64(uid)

			var payloadOffset = r.BaseStream.Position;

			if (r.BaseStream.Length < r.BaseStream.Position + length)
				throw new EndOfStreamException();

			r.BaseStream.Seek(length, SeekOrigin.Current);

			return new FlatArchiveEntry(meta, index, payloadOffset, (int)length);
		}
	}
}