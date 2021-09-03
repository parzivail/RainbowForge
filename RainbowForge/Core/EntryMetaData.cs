using System;
using System.IO;
using System.Text;

namespace RainbowForge.Core
{
	public class EntryMetaData
	{
		private const ulong FILENAME_ENCODING_KEY_STEP = 0x357267C76FFB9EB2;
		private const ulong FILENAME_ENCODING_BASE_KEY = 0x72EE89256E379B49 + FILENAME_ENCODING_KEY_STEP;

		public string FileName { get; }
		public byte[] Name { get; }
		public byte NameLength { get; }
		public uint Timestamp { get; }
		public int PrevEntryIdx { get; }
		public int NextEntryIdx { get; }
		public uint FileType { get; }
		public uint DataSize { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public ulong Unk3 { get; }
		public uint Unk4 { get; }

		public EntryMetaData(string fileName, byte[] name, byte nameLength, uint timestamp, int prevEntryIdx, int nextEntryIdx, uint fileType, uint dataSize, uint unk1, uint unk2, ulong unk3,
			uint unk4)
		{
			FileName = fileName;
			Name = name;
			NameLength = nameLength;
			Timestamp = timestamp;
			PrevEntryIdx = prevEntryIdx;
			NextEntryIdx = nextEntryIdx;
			FileType = fileType;
			DataSize = dataSize; // should equal with Entry::Size
			Unk1 = unk1;
			Unk2 = unk2;
			Unk3 = unk3;
			Unk4 = unk4;
		}

		public static EntryMetaData Read(BinaryReader r, ulong uid, ulong offset, uint version)
		{
			switch (version)
			{
				case >= 30:
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
					// var extraData = r.ReadBytes(12); // [0x134] looks like compressed data
					var x134 = r.ReadUInt64(); // [0x134]
					var dataSize = r.ReadUInt32(); // [0x13c]

					var nameBytes = NameEncoding.DecodeName(name[..nameLength], fileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
					return new EntryMetaData(Encoding.ASCII.GetString(nameBytes), name, nameLength, timestamp, prevEntryIdx, nextEntryIdx, fileType, dataSize, x00, x04, x08, x10);
				}
				case <= 29:
				{
					var dataSize = r.ReadUInt32(); // [0x00]
					var x04 = r.ReadUInt64(); // [0x04]
					var x0c = r.ReadUInt32(); // [0x0c]
					var fileType = r.ReadUInt32(); // [0x10]
					var x18 = r.ReadUInt64(); // [0x18]
					var nextEntryIdx = r.ReadInt32(); // [0x1c] next entry index
					var prevEntryIdx = r.ReadInt32(); // [0x20] previous entry index
					var x24 = r.ReadUInt32(); // [0x24]
					var timestamp = r.ReadUInt32(); // [0x28]
					var name = r.ReadBytes(0xFF); // [0x2c] entry metadata
					var nameLength = r.ReadByte(); // [0x12b] some byte
					var x12c = r.ReadUInt32(); // [0x12c]
					var x130 = r.ReadUInt32(); // [0x130]
					var x134 = r.ReadUInt32(); // [0x134]
					var x138 = r.ReadUInt32(); // [0x138]
					var x13c = r.ReadUInt32(); // [0x13c]

					var nameBytes = NameEncoding.DecodeName(name[..nameLength], fileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
					return new EntryMetaData(Encoding.ASCII.GetString(nameBytes), name, nameLength, timestamp, prevEntryIdx, nextEntryIdx, fileType, dataSize, x0c, x24, x04, x12c);
				}
				default:
					throw new NotImplementedException($"Unsupported version {version}");
			}
		}
	}
}