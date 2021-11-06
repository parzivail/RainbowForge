using System;
using System.IO;
using System.Text;

namespace RainbowForge.Core
{
	public class EntryMetaData
	{
		public string FileName { get; }
		public uint Timestamp { get; }
		public int PrevEntryIdx { get; }
		public int NextEntryIdx { get; }
		public uint FileType { get; }
		public uint DataSize { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public ulong Unk3 { get; }
		public uint Unk4 { get; }

		public EntryMetaData(string fileName, uint timestamp, int prevEntryIdx, int nextEntryIdx, uint fileType, uint dataSize, uint unk1, uint unk2, ulong unk3, uint unk4)
		{
			FileName = fileName;
			Timestamp = timestamp;
			PrevEntryIdx = prevEntryIdx;
			NextEntryIdx = nextEntryIdx;
			FileType = fileType;
			DataSize = dataSize; // should equal Entry::Size
			Unk1 = unk1;
			Unk2 = unk2;
			Unk3 = unk3;
			Unk4 = unk4;
		}

		public void Write(BinaryWriter w, ulong uid, ulong offset)
		{
			w.Write(Unk1);
			w.Write(Unk2);
			w.Write(Unk3);
			w.Write(Unk4);

			var nameBytes = NameEncoding.DecodeName(Encoding.ASCII.GetBytes(FileName), FileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
			var nameBlock = new byte[255];

			var nameLength = (byte)Math.Min(nameBytes.Length, nameBlock.Length);
			Array.Copy(nameBytes, nameBlock, nameLength);

			w.Write(nameBlock);
			w.Write(nameLength);
			w.Write(0);
			w.Write(PrevEntryIdx);
			w.Write(NextEntryIdx);
			w.Write((ulong)0);
			w.Write(FileType);
			w.Write(0);

			w.Write((ulong)0); // TODO: x134, what is it?
			w.Write(DataSize);
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
					var name = r.ReadBytes(0xFF);
					var nameLength = r.ReadByte();
					var timestamp = r.ReadUInt32();
					var x118 = r.ReadUInt32(); // [0x118] 0
					var prevEntryIdx = r.ReadInt32();
					var nextEntryIdx = r.ReadInt32();
					var x124 = r.ReadUInt64(); // [0x124] 0
					var fileType = r.ReadUInt32();
					var x130 = r.ReadUInt32(); // [0x130] 0

					var x134 = r.ReadUInt64();
					var dataSize = r.ReadUInt32();

					var nameBytes = NameEncoding.DecodeName(name[..nameLength], fileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
					return new EntryMetaData(Encoding.ASCII.GetString(nameBytes), timestamp, prevEntryIdx, nextEntryIdx, fileType, dataSize, x00, x04, x08, x10);
				}
				case <= 29:
				{
					var dataSize = r.ReadUInt32();
					var x04 = r.ReadUInt64();
					var x0c = r.ReadUInt32();
					var fileType = r.ReadUInt32();
					var x18 = r.ReadUInt64();
					var nextEntryIdx = r.ReadInt32(); // [0x1c] next entry index
					var prevEntryIdx = r.ReadInt32(); // [0x20] previous entry index
					var x24 = r.ReadUInt32();
					var timestamp = r.ReadUInt32();
					var name = r.ReadBytes(0xFF);
					var nameLength = r.ReadByte();
					var x12c = r.ReadUInt32();
					var x130 = r.ReadUInt32();
					var x134 = r.ReadUInt32();
					var x138 = r.ReadUInt32();
					var x13c = r.ReadUInt32();

					var nameBytes = NameEncoding.DecodeName(name[..nameLength], fileType, uid, offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
					return new EntryMetaData(Encoding.ASCII.GetString(nameBytes), timestamp, prevEntryIdx, nextEntryIdx, fileType, dataSize, x0c, x24, x04, x12c);
				}
				default:
					throw new NotImplementedException($"Unsupported version {version}");
			}
		}
	}
}