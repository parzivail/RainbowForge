using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RainbowForge.Core;

namespace RainbowScimitar.Scimitar
{
	public class ScimitarBuilder
	{
		public Dictionary<ScimitarId, UnbakedScimitarEntry> Entries = new();
		public ScimitarId GlobalMetaFileId { get; set; }

		public void Write(Stream fileStream)
		{
			var w = new BinaryWriter(fileStream);
			var formatId = Encoding.ASCII.GetBytes("scimitar\x00");

			w.Write(formatId);

			const int version = 30;
			w.Write(version);

			var fatLocationPos = fileStream.Position;
			w.Write(0); // Fat Location

			w.Write(0); // 0

			w.Write(GlobalMetaFileId);

			w.Write((byte)1); // 1

			var tables = CreateTables();
			var directories = CreateDirectories();

			var numEntries = tables.Sum(table => table.Count);
			w.Write(numEntries);
			w.Write(directories.Count); // TODO: directories

			w.Write(2); // unk3 == 2
			w.Write(2); // unk4 == 2
			w.Write(0); // unk4b == 0

			w.Write(-1); // FirstFreeFile == -1
			w.Write(-1); // FirstFreeDir == -1

			w.Write(numEntries + directories.Count);
			w.Write(tables.Count);

			w.Write(fileStream.Position + sizeof(ulong)); // FirstTablePosition -- pack tightly against header

			foreach (var t in tables)
				WriteTable(w, t);
		}

		private void WriteTable(BinaryWriter w, Dictionary<ScimitarId, UnbakedScimitarEntry> table)
		{
		}

		private List<ScimitarDirectory> CreateDirectories()
		{
			// No directories
			return new List<ScimitarDirectory>();
		}

		private List<Dictionary<ScimitarId, UnbakedScimitarEntry>> CreateTables()
		{
			// current strategy for creating tables is to put all entries into the same table
			return new List<Dictionary<ScimitarId, UnbakedScimitarEntry>>
			{
				Entries
			};
		}
	}

	public record UnbakedScimitarEntry(EntryMetaData MetaData, Stream DataStream);
}