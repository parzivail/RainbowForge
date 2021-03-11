using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RainbowForge.Forge
{
	public class Forge
	{
		private readonly Dictionary<ulong, int> _uidToEntryIndexMap;
		public uint Version { get; }
		public uint HeaderOffset { get; }
		public uint NumEntries { get; }
		public Entry[] Entries { get; }

		public BinaryReader Stream { get; }

		private Forge(uint version, uint headerOffset, uint numEntries, Entry[] entries, BinaryReader stream)
		{
			Version = version;
			HeaderOffset = headerOffset;
			NumEntries = numEntries;
			Entries = entries;
			Stream = stream;

			_uidToEntryIndexMap = new Dictionary<ulong, int>();

			for (var i = 0; i < entries.Length; i++)
				_uidToEntryIndexMap[entries[i].Uid] = i;
		}

		public static Forge Read(BinaryReader r)
		{
			var formatId = Encoding.ASCII.GetBytes("scimitar\x00");

			var magic = r.ReadBytes(formatId.Length);
			if (!magic.SequenceEqual(formatId))
				throw new InvalidDataException("Wrong format id");

			var version = r.ReadUInt32();
			var headerOffset = r.ReadUInt32();
			var x11 = r.ReadUInt32(); // [0x11] = 0
			var x15 = r.ReadUInt32(); // [(0x15] = 0x10
			var x19 = r.ReadUInt32(); // [0x19] = 0
			var x1d = r.ReadByte(); // literally no correlation found to any data so far

			var numEntries = r.ReadUInt32(); // files + hash entry + descriptor entry
			var x22 = r.ReadUInt32(); // [0x22] = 2
			var x26 = r.ReadUInt32(); // [0x26] = 0
			var x2A = r.ReadUInt32(); // [0x2a] = 0
			var x2E = r.ReadUInt32(); // [0x2e] = 0
			var x32 = r.ReadInt32(); // [0x32] = -1
			var x36 = r.ReadInt32(); // [0x36] = -1

			var numPlus2 = r.ReadUInt32(); // num_entries+2 (what for?..)
			var x3E = r.ReadUInt32(); // [0x3e] = 1
			var x4A = r.ReadUInt32(); // [0x42] = 0x4a
			var x46 = r.ReadUInt32(); // [0x46] = 0

			var num2 = r.ReadUInt32(); // num_entries again
			var x4E = r.ReadUInt32(); // [0x4e] = 2
			var x52 = r.ReadUInt32(); // [0x52] = 0x7a
			var x56 = r.ReadUInt32(); // [0x56] = 0
			var x5A = r.ReadInt32(); // [0x5a] = -1
			var x5E = r.ReadInt32(); // [0x5e] = -1
			var x62 = r.ReadUInt32(); // [0x62] = 0

			var numPlus1 = r.ReadUInt32(); // [0x66] num_entries+1 (what for?..)
			var namesOffset = r.ReadUInt64(); // [0x6a]
			var lostfound = r.ReadUInt64(); // [0x72]

			var entries = new Entry[numEntries];
			for (var i = 0; i < numEntries; i++)
				entries[i] = Entry.Read(r);

			r.BaseStream.Seek((long) namesOffset, SeekOrigin.Begin);
			for (var i = 0; i < numEntries; i++)
				entries[i].Name = NameEntry.Read(r);

			// TODO: LOSTFOUND

			return new Forge(version, headerOffset, numEntries, entries, r);
		}

		public Container GetContainer(ulong entryUid)
		{
			return GetContainer(_uidToEntryIndexMap[entryUid]);
		}

		public Container GetContainer(int i)
		{
			var entry = Entries[i];
			var start = entry.Offset;
			var end = entry.End;

			Stream.BaseStream.Seek(start, SeekOrigin.Begin);

			var containerMagic = Stream.ReadUInt32();
			var magic = (ContainerMagic) containerMagic;

			switch (magic)
			{
				case ContainerMagic.Descriptor:
					return Descriptor.Read(Stream, entry);
				case ContainerMagic.Hash:
					return Hash.Read(Stream);
				case ContainerMagic.File:
					return ForgeAsset.Read(Stream, entry);
				default:
					throw new InvalidDataException($"No constructor for container {i} of style 0x{containerMagic:X} (from 0x{start:X} to 0x{end:X}), skipping");
			}
		}

		public BinaryReader GetAssetStream(Entry entry)
		{
			var container = GetContainer(entry.Uid);

			if (container is not ForgeAsset file)
				throw new InvalidDataException("Entry with asset header was not file");

			if (file.FileBlock == null)
				throw new InvalidDataException("Asset file contained no file block");

			return new BinaryReader(file.FileBlock.GetDecompressedStream(Stream));
		}
	}
}