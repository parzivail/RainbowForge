using System.IO;

namespace RainbowForge.Forge
{
	public class LinearDataBlock : IAssetBlock
	{
		public byte[] SomeData { get; }
		public long Offset { get; }
		public int Length { get; }

		private LinearDataBlock(byte[] someData, long offset, int length)
		{
			SomeData = someData;
			Offset = offset;
			Length = length;
		}

		public static LinearDataBlock Read(BinaryReader r, Entry entry)
		{
			var ldbStart = r.BaseStream.Position;
			var numSomeData = r.ReadByte();

			// smallest numSomeData is 1, so the smallest
			// header length is 15
			var someDataBytes = 12 * numSomeData + 3;
			var someData = r.ReadBytes(someDataBytes);

			var dataStart = r.BaseStream.Position;
			r.BaseStream.Seek(entry.End, SeekOrigin.Begin);

			return new LinearDataBlock(someData, dataStart, (int) (entry.End - dataStart));
		}

		public MemoryStream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();
			r.BaseStream.Seek(Offset, SeekOrigin.Begin);
			r.BaseStream.CopyStream(ms, Length);

			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}
	}
}