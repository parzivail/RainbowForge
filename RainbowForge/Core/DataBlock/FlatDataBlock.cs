using System.IO;

namespace RainbowForge.Core.DataBlock
{
	public class FlatDataBlock : IAssetBlock
	{
		public long Offset { get; }
		public int Length { get; }

		private FlatDataBlock(long offset, int length)
		{
			Offset = offset;
			Length = length;
		}

		public static FlatDataBlock Read(BinaryReader r, Entry entry)
		{
			var numChunks = r.ReadUInt16();
			var unk1 = r.ReadUInt16();

			var payloadSizes = new int[numChunks];
			var serializedSizes = new int[numChunks];
			for (var i = 0; i < numChunks; i++)
			{
				payloadSizes[i] = r.ReadInt32();
				serializedSizes[i] = r.ReadInt32();
			}

			var checksumData = new uint[numChunks];

			for (var i = 0; i < numChunks; i++)
				checksumData[i] = r.ReadUInt32();

			var dataStart = r.BaseStream.Position;
			r.BaseStream.Seek(entry.End, SeekOrigin.Begin);

			return new FlatDataBlock(dataStart, (int)(entry.End - dataStart));
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