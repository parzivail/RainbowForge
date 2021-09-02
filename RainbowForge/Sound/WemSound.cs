using System;
using System.IO;

namespace RainbowForge.Sound
{
	public class WemSound
	{
		public FileMetaData MetaData { get; }
		public byte[] Meta2 { get; }
		public long PayloadOffset { get; }
		public int PayloadLength { get; }

		private WemSound(FileMetaData metaData, byte[] meta2, long payloadOffset, int payloadLength)
		{
			MetaData = metaData;
			Meta2 = meta2;
			PayloadOffset = payloadOffset;
			PayloadLength = payloadLength;
		}

		// private static BinaryWriter bw = new BinaryWriter(File.Open(@"R:\Siege Dumps\Asset Indexes\New in Y6S1\sound\headers.bin", FileMode.Append));
		// private static int i = 0;

		public static WemSound Read(BinaryReader r, uint version)
		{
			var fmeta = FileMetaData.Read(r, version);

			var secondMagic = r.ReadUInt32();
			var var2 = r.ReadUInt32();
			var var3 = r.ReadUInt32();

			// var metaLength = r.ReadInt32();
			// var meta = r.ReadBytes(metaLength);

			int meta2Length;

			switch (fmeta.EncodedMeta[4])
			{
				case 0x80:
				case 0x81:
				case 0x82:
				case 0x83:
				case 0x88:
				case 0x89:
				case 0x8C:
				case 0x8D:
				case 0x8E:
				case 0x92:
				case 0x94:
				case 0x96:
				case 0x98:
				case 0x99:
				case 0xA4:
				case 0xA5:
				case 0xA7:
				case 0xA9:
				case 0xAE:
				case 0xAF:
				case 0xB6:
				case 0xB7:
					meta2Length = 28;
					break;
				case 0xA2:
				case 0xA3:
				case 0xB0:
				case 0xB2:
				case 0x9C:
					// TODO: these WEM files seem to have both header lengths (i.e. meta[4] isn't the only discriminator)
					throw new NotSupportedException();
					// meta2Length = -1;
					break;
				case 0xB1:
					// TODO: these WEM files seem to get rid of the WAV header and keep only the data right after the WAV `data` block
					throw new NotSupportedException();
					// meta2Length = -2;
					break;
				default:
					meta2Length = 44;
					break;
			}

			// var pos = r.BaseStream.Position;
			//
			// while (r.BaseStream.Position < r.BaseStream.Length - 4)
			// {
			// 	r.BaseStream.Seek(pos + meta2Length, SeekOrigin.Begin);
			// 	var bR1 = (char)r.ReadByte();
			// 	var bI1 = (char)r.ReadByte();
			// 	var bF1 = (char)r.ReadByte();
			// 	var bF2 = (char)r.ReadByte();
			// 	
			// 	if (bR1 == 'R' && bI1 == 'I' && bF1 == 'F' && bF2 == 'F')
			// 		break;
			// 	
			// 	if (meta2Length >= 255)
			// 		break;
			//
			// 	meta2Length++;
			// }
			//
			// bw.Write((byte) meta2Length);
			//
			// foreach (var b in fmeta.EncodedMeta.Take(3))
			// 	bw.Write(b);
			//
			// // var pos = r.BaseStream.Position;
			// // r.BaseStream.Seek(0, SeekOrigin.Begin);
			// // using (var fs = File.Open(@$"R:\Siege Dumps\Asset Indexes\New in Y6S1\sound\{i++}.bin", FileMode.Create))
			// // 	r.BaseStream.CopyTo(fs);
			// throw new Exception($"{meta2Length:X2} = {string.Join(',', fmeta.EncodedMeta.Take(3).Select(b => $"{b:X2}"))}");

			var meta2 = r.ReadBytes(meta2Length);

			var payloadLength = r.ReadInt32();
			var payloadOffset = r.BaseStream.Position;

			var b4 = r.ReadBytes(4);
			r.BaseStream.Seek(-4, SeekOrigin.Current);

			if (!(b4[0] == 'R' && b4[1] == 'I' && b4[2] == 'F' && b4[3] == 'F'))
				;

			return new WemSound(fmeta, meta2, payloadOffset, payloadLength);
		}
	}
}