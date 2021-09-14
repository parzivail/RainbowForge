using System.IO;
using RainbowScimitar.Extensions;

namespace RainbowScimitar
{
	public class ScimitarTable
	{
		public int NumFiles { get; }
		public int NumDirs { get; }
		public long PosFat { get; }
		public long NextPosFat { get; }
		public int FirstIndex { get; }
		public int LastIndex { get; }
		public long MetaTableOffset { get; }
		public long DirectoryOffset { get; }
		public ScimitarFile[] Files { get; }
		public ScimitarMetaTableEntry[] MetaTableEntries { get; }

		private ScimitarTable(int numFiles, int numDirs, long posFat, long nextPosFat, int firstIndex, int lastIndex, long metaTableOffset, long directoryOffset, ScimitarFile[] files,
			ScimitarMetaTableEntry[] metaTableEntries)
		{
			NumFiles = numFiles;
			NumDirs = numDirs;
			PosFat = posFat;
			NextPosFat = nextPosFat;
			FirstIndex = firstIndex;
			LastIndex = lastIndex;
			MetaTableOffset = metaTableOffset;
			DirectoryOffset = directoryOffset;
			Files = files;
			MetaTableEntries = metaTableEntries;
		}

		public static ScimitarTable Read(BinaryReader r)
		{
			var numFiles = r.ReadInt32();
			var numDirs = r.ReadInt32();
			var posFat = r.ReadInt64();
			var nextPosFat = r.ReadInt64();
			var firstIndex = r.ReadInt32();
			var lastIndex = r.ReadInt32();
			var metaTableOffset = r.ReadInt64();
			var directoryOffset = r.ReadInt64();

			r.BaseStream.Seek(posFat, SeekOrigin.Begin);
			var files = r.ReadStructs<ScimitarFile>(numFiles);

			r.BaseStream.Seek(metaTableOffset, SeekOrigin.Begin);
			var metaTableEntries = r.ReadStructs<ScimitarMetaTableEntry>(numFiles);

			return new ScimitarTable(numFiles, numDirs, posFat, nextPosFat, firstIndex, lastIndex, metaTableOffset, directoryOffset, files, metaTableEntries);
		}
	}
}