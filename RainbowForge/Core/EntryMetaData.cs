using System;
using System.IO;
using System.Text;

namespace RainbowForge.Core
{
	public class EntryMetaData
	{
		public string Name { get; }
		public uint Timestamp { get; }
		public int PrevEntryIdx { get; }
		public int NextEntryIdx { get; }
		public uint FileType { get; }
		public byte[] ExtraData { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public ulong Unk3 { get; }
		public uint Unk4 { get; }

		public EntryMetaData(string name, uint timestamp, int prevEntryIdx, int nextEntryIdx, uint fileType, byte[] extraData, uint unk1, uint unk2, ulong unk3, uint unk4)
		{
			Name = name;
			Timestamp = timestamp;
			PrevEntryIdx = prevEntryIdx;
			NextEntryIdx = nextEntryIdx;
			FileType = fileType;
			ExtraData = extraData;
			Unk1 = unk1;
			Unk2 = unk2;
			Unk3 = unk3;
			Unk4 = unk4;
		}

		public static EntryMetaData Read(BinaryReader r, Entry entry)
		{
			var x00 = r.ReadUInt32(); // [0x00] 0
			var x04 = r.ReadUInt32(); // [0x04] 4
			var x08 = r.ReadUInt64(); // [0x08] 0
			var x10 = r.ReadUInt32(); // [0x10] 4
			var name = r.ReadBytes(0xFF); // [0x14] entry metadata
			var nameLength = r.ReadByte(); // [0x113] some byte
			var timestamp = r.ReadUInt32(); // [0x114]
			var x118 = r.ReadUInt32(); // [0x118] 0
			var prevEntryIdx = r.ReadInt32(); // [0x11c] previous entry index
			var nextEntryIdx = r.ReadInt32(); // [0x120] next entry index
			var x124 = r.ReadUInt64(); // [0x124] 0
			var fileType = r.ReadUInt32(); // [0x12c]
			var x130 = r.ReadUInt32(); // [0x130] 0
			var extraData = r.ReadBytes(12); // [0x134] looks like compressed data

			var nameStr = DecodeName(name, nameLength, entry.Uid, (ulong)entry.Offset);

			return new EntryMetaData(nameStr, timestamp, prevEntryIdx, nextEntryIdx, fileType, extraData, x00, x04, x08, x10);
		}

		private static string DecodeName(byte[] name, byte nameLength, ulong uid, ulong dataOffset)
		{
			const ulong baseKey = 0x1ED9B7211A22C944;
			const ulong keyStep = 0x357267C76FFB9EB2;

			var key = baseKey + uid + dataOffset;

			var decoded = "";

			using var ms = new BinaryReader(new MemoryStream(name));
			var blocks = (nameLength + 8) / 8;

			for (var i = 0; i < blocks; i++)
			{
				key += keyStep;

				ulong block = 0;

				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();
				block <<= 8;
				block |= ms.ReadByte();

				block ^= key;

				decoded += Encoding.ASCII.GetString(BitConverter.GetBytes(block));
			}

			Console.WriteLine(decoded);

			return decoded;
		}
	}
}