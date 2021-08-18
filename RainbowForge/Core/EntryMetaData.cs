using System.IO;

namespace RainbowForge.Core
{
	public class EntryMetaData
	{
		public byte[] Name { get; }
		public int NameLength { get; }
		public uint Timestamp { get; }
		public int PrevEntryIdx { get; }
		public int NextEntryIdx { get; }
		public uint FileType { get; }

		public EntryMetaData(byte[] name, int nameLength, uint timestamp, int prevEntryIdx, int nextEntryIdx, uint fileType)
		{
			Name = name;
			NameLength = nameLength;
			Timestamp = timestamp;
			PrevEntryIdx = prevEntryIdx;
			NextEntryIdx = nextEntryIdx;
			FileType = fileType;
		}

		public static EntryMetaData Read(BinaryReader r)
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
			var x134 = r.ReadBytes(12); // [0x134] looks like compressed data

			return new EntryMetaData(name, nameLength, timestamp, prevEntryIdx, nextEntryIdx, fileType);
		}
	}
}