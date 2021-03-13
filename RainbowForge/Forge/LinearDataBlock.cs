using System;
using System.IO;

namespace RainbowForge.Forge
{
	public class LinearDataBlock : IAssetBlock
	{
		public byte[] SomeData { get; }
		public byte[] Meta { get; }
		public byte[] Meta2 { get; }
		public long Offset { get; }
		public int Length { get; }

		private LinearDataBlock(byte[] someData, byte[] meta, byte[] meta2, long offset, int length)
		{
			SomeData = someData;
			Meta = meta;
			Meta2 = meta2;
			Offset = offset;
			Length = length;
		}

		public static LinearDataBlock Read(BinaryReader r)
		{
			var numSomeData = r.ReadByte();

			// smallest numSomeData is 1, so the smallest
			// header length is 15
			var someDataBytes = 12 * numSomeData + 3;
			var someData = r.ReadBytes(someDataBytes);

			// TODO: These lengths in rare cases read too much or too little
			// failures seem to occur when meta[4] == 156 && meta[5] == 68, meta[7] == 181,
			// meta[13] == 176, 
			var metaLength = r.ReadInt32();
			var meta = r.ReadBytes(metaLength);

			var length = r.ReadInt32() - 0x2c;
			var meta2 = r.ReadBytes(0x2c);

			var fail = (char) r.PeekChar() != 'R';
			var failIndicatorA = meta[4] == 156;
			var failIndicatorB = meta[5] == 68; // likely not root cause
			var failIndicatorC = meta[7] == 181; // likely not root cause
			var failIndicatorD = meta[13] == 176; // likely not root cause

			if (fail != failIndicatorA)
			{
				Console.Write("Not A"); // is possible but far less likely

				if (fail != failIndicatorC)
					Console.Write("Not A or C"); // not possible?
			}

			// format notes: see https://github.com/vgmstream/vgmstream/blob/master/src/meta/wwise.c
			// vgmstream should be able to convert all of the WEM files spit out by this to WAV without any issues

			var offset = r.BaseStream.Position;
			r.BaseStream.Seek(length, SeekOrigin.Current);

			return new LinearDataBlock(someData, meta, meta2, offset, length);
		}

		public MemoryStream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();
			r.BaseStream.Seek(Offset, SeekOrigin.Begin);
			r.BaseStream.CopyStream(ms, Length);

			return ms;
		}
	}
}