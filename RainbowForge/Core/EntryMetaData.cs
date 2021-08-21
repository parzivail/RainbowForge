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
		public byte[] ExtraData { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public ulong Unk3 { get; }
		public uint Unk4 { get; }

		public EntryMetaData(string fileName, byte[] name, byte nameLength, uint timestamp, int prevEntryIdx, int nextEntryIdx, uint fileType, byte[] extraData, uint unk1, uint unk2, ulong unk3,
			uint unk4)
		{
			FileName = fileName;
			Name = name;
			NameLength = nameLength;
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

		public static EntryMetaData Read(BinaryReader r, ulong uid, ulong offset)
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

			var nameBytes = NameEncoding.DecodeName(name[..nameLength], fileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);

			return new EntryMetaData(Encoding.ASCII.GetString(nameBytes), name, nameLength, timestamp, prevEntryIdx, nextEntryIdx, fileType, extraData, x00, x04, x08, x10);
		}
	}
}