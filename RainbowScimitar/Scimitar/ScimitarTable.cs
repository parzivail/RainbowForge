using System.IO;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarTable(int NumFiles, int NumDirs, long PosFat, long NextPosFat, int FirstIndex, int LastIndex, long MetaTableOffset, long DirectoryOffset, ScimitarFileTableEntry[] Files,
		ScimitarAssetMetadata[] MetaTableEntries)
	{
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
			var files = r.ReadStructs<ScimitarFileTableEntry>(numFiles);

			r.BaseStream.Seek(metaTableOffset, SeekOrigin.Begin);
			var metaTableEntries = r.ReadStructs<ScimitarAssetMetadata>(numFiles);

			return new ScimitarTable(numFiles, numDirs, posFat, nextPosFat, firstIndex, lastIndex, metaTableOffset, directoryOffset, files, metaTableEntries);
		}
	}
}