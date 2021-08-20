using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RainbowForge.Core.Container;

namespace RainbowForge.Core
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
				throw new InvalidDataException("Input file not SCIMITAR archive");

			var version = r.ReadUInt32();
			var headerOffset = r.ReadUInt32();
			
			var x11 = r.ReadUInt32(); // [0x11] = 0
			var x15 = r.ReadUInt32(); // [(0x15] = 0x10
			var x19 = r.ReadUInt32(); // [0x19] = 0
			var x1d = r.ReadByte(); // literally no correlation found to any data so far

			var numEntries = r.ReadUInt32(); // files + hash entry + descriptor entry
			var numDirectories = r.ReadUInt32(); // [0x22] = 2
			var unk2 = r.ReadUInt32();
			var unk3 = r.ReadUInt32();
			if (version >= 27)
			{
				var unk3b = r.ReadUInt32();
			}

			var unk4 = r.ReadUInt32();
			var unk5 = r.ReadUInt32();

			var maxEntriesPerTable = r.ReadUInt32();
			var numTables = r.ReadUInt32();

			var firstTablePosition = r.ReadUInt64();

			var totalEntries = new List<Entry>();

			r.BaseStream.Seek((long)firstTablePosition, SeekOrigin.Begin);
			for (var i = 0; i < numTables; i++)
			{
				var numTEntries = r.ReadInt32();
				var numTDirectories = r.ReadInt32();
				var firstEntryOffset = r.ReadInt64();
				var nextTableOffset = r.ReadInt64();
				var startIndex = r.ReadInt32();
				var endIndex = r.ReadInt32();
				var metaTableOffset = r.ReadInt64();
				var directoryOffset = r.ReadInt64();

				r.BaseStream.Seek(firstEntryOffset, SeekOrigin.Begin);
				var entries = new Entry[numTEntries];
				for (var j = 0; j < numTEntries; j++)
					entries[j] = Entry.Read(r);

				r.BaseStream.Seek(metaTableOffset, SeekOrigin.Begin);
				for (var j = 0; j < numTEntries; j++)
					entries[j].MetaData = EntryMetaData.Read(r, entries[j].Uid, (ulong)entries[j].Offset);

				totalEntries.AddRange(entries);

				// TODO: directories
				// r.BaseStream.Seek(directoryOffset, SeekOrigin.Begin);
				// for (var j = 0; j < numTDirectories; j++)
				// {
				// 	var numFiles = r.ReadInt32();
				// 	var unk2a = r.ReadInt32();
				// 	var unk3a = r.ReadInt32();
				// 	var unk4a = r.ReadInt32();
				// 	var unk5a = r.ReadInt32();
				// 	var name = r.ReadBytes(128);
				// 	var unk6 = r.ReadInt32();
				// 	var unk7 = r.ReadInt32();
				// 	var unk8 = r.ReadInt32();
				// 	var unk9 = r.ReadInt32();
				// }

				if (nextTableOffset != -1)
					r.BaseStream.Seek(nextTableOffset, SeekOrigin.Begin);
			}

			// TODO: organize Forge into tables instead of one big entry list
			return new Forge(version, headerOffset, numEntries, totalEntries.ToArray(), r);
		}

		public static Forge GetForge(string filename)
		{
			FileSystemUtil.AssertFileExists(filename);
			var forgeStream = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
			return Read(forgeStream);
		}

		public ForgeContainer GetContainer(ulong entryUid)
		{
			return GetContainer(_uidToEntryIndexMap[entryUid]);
		}

		public BinaryReader GetEntryStream(Entry entry)
		{
			var ms = new MemoryStream();
			Stream.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
			Stream.BaseStream.CopyStream(ms, (int)entry.Size);

			ms.Seek(0, SeekOrigin.Begin);

			return new BinaryReader(ms);
		}

		public ForgeContainer GetContainer(int i)
		{
			var entry = Entries[i];
			var start = entry.Offset;
			var end = entry.End;

			Stream.BaseStream.Seek(start, SeekOrigin.Begin);

			var containerMagic = Stream.ReadUInt32();
			var magic = (ContainerMagic)containerMagic;

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
	}
}