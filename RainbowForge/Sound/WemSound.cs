using System.IO;

namespace RainbowForge.Sound
{
	public class WemSound
	{
		public byte[] Meta { get; }
		public byte[] Meta2 { get; }
		public long PayloadOffset { get; }
		public int PayloadLength { get; }

		private WemSound(byte[] meta, byte[] meta2, long payloadOffset, int payloadLength)
		{
			Meta = meta;
			Meta2 = meta2;
			PayloadOffset = payloadOffset;
			PayloadLength = payloadLength;
		}

		public static WemSound Read(BinaryReader r)
		{
			var metaLength = r.ReadInt32();
			var meta = r.ReadBytes(metaLength);

			int meta2Length;

			switch (meta[4])
			{
				case 0x81:
				case 0x82:
				case 0x83:
				case 0x88:
				case 0x89:
				case 0x94:
				case 0x96:
				case 0x98:
				case 0x9C:
					meta2Length = 28;
					break;
				default:
					meta2Length = 44;
					break;
			}

			var meta2 = r.ReadBytes(meta2Length);

			var payloadLength = r.ReadInt32();
			var payloadOffset = r.BaseStream.Position;

			var b4 = r.ReadBytes(4);
			r.BaseStream.Seek(-4, SeekOrigin.Current);

			if (!(b4[0] == 'R' && b4[1] == 'I' && b4[2] == 'F' && b4[3] == 'F'))
				;

			return new WemSound(meta, meta2, payloadOffset, payloadLength);
		}
	}
}