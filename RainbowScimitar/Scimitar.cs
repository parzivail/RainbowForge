using System.IO;
using System.Linq;
using System.Text;

namespace RainbowScimitar
{
	public class Scimitar
	{
		public uint Version { get; }
		public uint FatLocation { get; }
		public ulong GloablMetaFileKey { get; }
		public uint Unk1 { get; }
		public uint Unk2 { get; }
		public uint Unk3 { get; }
		public uint Unk4 { get; }
		public uint Unk4B { get; }
		public uint FirstFreeFile { get; }
		public uint FirstFreeDir { get; }
		public uint SizeOfFat { get; }
		public ulong FirstTablePosition { get; }
		public ScimitarTable[] Tables { get; }

		private Scimitar(uint version, uint fatLocation, ulong gloablMetaFileKey, uint unk1, uint unk2, uint unk3, uint unk4, uint unk4B, uint firstFreeFile, uint firstFreeDir, uint sizeOfFat,
			ulong firstTablePosition, ScimitarTable[] tables)
		{
			Version = version;
			FatLocation = fatLocation;
			GloablMetaFileKey = gloablMetaFileKey;
			Unk1 = unk1;
			Unk2 = unk2;
			Unk3 = unk3;
			Unk4 = unk4;
			Unk4B = unk4B;
			FirstFreeFile = firstFreeFile;
			FirstFreeDir = firstFreeDir;
			SizeOfFat = sizeOfFat;
			FirstTablePosition = firstTablePosition;
			Tables = tables;
		}

		public static Scimitar Read(BinaryReader r)
		{
			var formatId = Encoding.ASCII.GetBytes("scimitar\x00");

			var magic = r.ReadBytes(formatId.Length);
			if (!magic.SequenceEqual(formatId))
				throw new InvalidDataException("Input file not SCIMITAR bundle");

			var version = r.ReadUInt32();
			var fatLocation = r.ReadUInt32();
			var gloablMetaFileKey = r.ReadUInt64();

			var unk1 = r.ReadUInt32();
			var unk2 = r.ReadByte();

			var numEntries = r.ReadUInt32(); // files + hash entry + descriptor entry
			var numDirectories = r.ReadUInt32();
			var unk3 = r.ReadUInt32();
			var unk4 = r.ReadUInt32();
			var unk4b = 0u;
			if (version >= 27)
			{
				unk4b = r.ReadUInt32();
			}

			var firstFreeFile = r.ReadUInt32();
			var firstFreeDir = r.ReadUInt32();

			var sizeOfFat = r.ReadUInt32();
			var numTables = r.ReadUInt32();

			var firstTablePosition = r.ReadUInt64();

			var tables = new ScimitarTable[numTables];

			r.BaseStream.Seek((long)firstTablePosition, SeekOrigin.Begin);
			for (var i = 0; i < numTables; i++)
			{
				tables[i] = ScimitarTable.Read(r);

				if (tables[i].NextPosFat != -1)
					r.BaseStream.Seek(tables[i].NextPosFat, SeekOrigin.Begin);
			}

			// TODO: directories

			return new Scimitar(version, fatLocation, gloablMetaFileKey, unk1, unk2, unk3, unk4, unk4b, firstFreeFile, firstFreeDir, sizeOfFat, firstTablePosition, tables);
		}
	}
}