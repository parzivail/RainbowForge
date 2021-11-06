using System.IO;
using System.Runtime.InteropServices;
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

			r.BaseStream.Seek(directoryOffset, SeekOrigin.Begin);
			var directories = r.ReadStructs<ScimitarDirectory>(numDirs);

			return new ScimitarTable(numFiles, numDirs, posFat, nextPosFat, firstIndex, lastIndex, metaTableOffset, directoryOffset, files, metaTableEntries);
		}

		public void Write(BinaryWriter w)
		{
			w.Write(NumFiles);
			w.Write(NumDirs);
			w.Write(PosFat);
			w.Write(NextPosFat);
			w.Write(FirstIndex);
			w.Write(LastIndex);
			w.Write(MetaTableOffset);
			w.Write(DirectoryOffset);

			w.BaseStream.Seek(PosFat, SeekOrigin.Begin);
			w.WriteStructs(Files);

			w.BaseStream.Seek(MetaTableOffset, SeekOrigin.Begin);
			w.WriteStructs(MetaTableEntries);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct ScimitarDirectory
	{
		public readonly int NumFiles;
		public readonly int Unknown1;
		public readonly int Unknown2;
		public readonly int Unknown3;
		public readonly int Unknown4;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public readonly byte[] Name;

		public readonly int Unknown5;
		public readonly int Unknown6;
		public readonly int Unknown7;
		public readonly int Unknown8;
		public readonly int Unknown9;
		public readonly int Unknown10;
		public readonly int Unknown11;
	}
}