using System.IO;

namespace RainbowForge.Core.DataBlock
{
	public class FlatDataBlock : IAssetBlock
	{
		public byte[] Meta { get; }
		public long Offset { get; }
		public int Length { get; }

		private FlatDataBlock(byte[] meta, long offset, int length)
		{
			Meta = meta;
			Offset = offset;
			Length = length;
		}

		public static FlatDataBlock Read(BinaryReader r, Entry entry)
		{
			var numMetaEntries = r.ReadByte();

			// smallest numMetaEntries is 1, so the smallest
			// header length is 15
			var metaLength = 12 * numMetaEntries + 3;
			var meta = r.ReadBytes(metaLength);

			var dataStart = r.BaseStream.Position;
			r.BaseStream.Seek(entry.End, SeekOrigin.Begin);

			return new FlatDataBlock(meta, dataStart, (int)(entry.End - dataStart));
		}

		public Stream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();
			r.BaseStream.Seek(Offset, SeekOrigin.Begin);
			r.BaseStream.CopyStream(ms, Length);

			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}
	}
}