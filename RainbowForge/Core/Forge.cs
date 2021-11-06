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
			/*
				"scimitar"
			    u32 _version = _binaryReader.readInt32();
			    u64 fatLocation = _binaryReader.readUInt64();
			    m_GlobalMetaFileKey = readResourceId(&_binaryReader); // 4 bytes or 8 depending of game (8 after AC3). It's always 0x10 as far as I can tell

			    // Fat header
			    int MaxFile = _binaryReader.readUInt32();
			    int MaxDir = _binaryReader.readInt32();
			    u64 MaxKey = readResourceId(&_binaryReader); (4 or 8 bytes)
			    u32 Root = _binaryReader.readInt32();
			    int FirstFreeFile = _binaryReader.readInt32();
			    int FirstFreeDir = _binaryReader.readInt32();
			    int SizeOfFat = _binaryReader.readInt32();
			    int NumFat = _binaryReader.readInt32();

			For each fat:
			    m_MaxFile = _binaryReader->readInt32();
			    m_MaxDir = _binaryReader->readInt32();
			    m_PosFat = _binaryReader->readInt64();
			    m_NextPosFat = _binaryReader->readInt64();
			    m_FirstIndex = _binaryReader->readInt32();
			    m_LastIndex = _binaryReader->readInt32();
			    _metaTableOffset = _binaryReader->readInt64();
			    _directoryOffset = _binaryReader->readInt64();
			    
			If metadata is present (not the case in valhalla and later)
			    m_LengthOnDisk = _binaryReader->readInt32(); //0
			    m_UMACHash = _binaryReader->readUInt64(); //8
			    m_EngineVersion = _binaryReader->readInt32(); //0x10
			    m_ClassID = _binaryReader->readUInt32(); //0x14
			    m_RevisionNumberData = _binaryReader->readInt32(); //0x18
			    m_RevisionNumberAttributes = _binaryReader->readInt32(); //0x1C
			    m_Prev = _binaryReader->readInt32(); //0x38
			    m_Next = _binaryReader->readInt32(); //0x3C
			    m_Parent = _binaryReader->readInt32(); //0x40
			    m_Time = (time_t)_binaryReader->readInt32(); //0x44
			    _binaryReader->readBuffer(_name, 128); // 0x48
			    _SCCStatusData = _binaryReader->readInt32(); // 0x24
			    m_MetaFileKey = readResourceId(_binaryReader); // 0x30
			    _SCCStatusAttributes = _binaryReader->readInt32(); // 0x28
			    _isHidden = _binaryReader->readUInt32(); //0x20 (is &1, the file is hidden)
			 */
			var formatId = Encoding.ASCII.GetBytes("scimitar\x00");

			var magic = r.ReadBytes(formatId.Length);
			if (!magic.SequenceEqual(formatId))
				throw new InvalidDataException("Input file not SCIMITAR archive");

			var version = r.ReadUInt32();
			var fatLocation = r.ReadUInt32();
			var gloablMetaFileKey = r.ReadUInt64();

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

			var firstFreeFile = r.ReadUInt32();
			var firstFreeDir = r.ReadUInt32();

			var sizeOfFat = r.ReadUInt32();
			var numTables = r.ReadUInt32();

			var firstTablePosition = r.ReadUInt64();

			var totalEntries = new List<Entry>();

			r.BaseStream.Seek((long)firstTablePosition, SeekOrigin.Begin);
			for (var i = 0; i < numTables; i++)
			{
				var maxFile = r.ReadInt32();
				var maxDir = r.ReadInt32();
				var posFat = r.ReadInt64();
				var nextPosFat = r.ReadInt64();
				var firstIndex = r.ReadInt32();
				var lastIndex = r.ReadInt32();
				var metaTableOffset = r.ReadInt64();
				var directoryOffset = r.ReadInt64();

				r.BaseStream.Seek(posFat, SeekOrigin.Begin);
				var entries = new Entry[maxFile];
				for (var j = 0; j < maxFile; j++)
					entries[j] = Entry.Read(r);

				r.BaseStream.Seek(metaTableOffset, SeekOrigin.Begin);
				for (var j = 0; j < maxFile; j++)
					entries[j].MetaData = EntryMetaData.Read(r, entries[j].Uid, (ulong)entries[j].Offset, version);

				totalEntries.AddRange(entries);

				if (nextPosFat != -1)
					r.BaseStream.Seek(nextPosFat, SeekOrigin.Begin);
			}

			// TODO: organize Forge into tables instead of one big entry list
			return new Forge(version, fatLocation, numEntries, totalEntries.ToArray(), r);
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
				case ContainerMagic.File2:
					return ForgeAsset.Read(Stream, entry);
				default:
					throw new InvalidDataException($"No constructor for container {i} of style 0x{containerMagic:X} (from 0x{start:X} to 0x{end:X}), skipping");
			}
		}
	}
}