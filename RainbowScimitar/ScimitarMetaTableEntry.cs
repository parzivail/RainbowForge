using System.Runtime.InteropServices;
using System.Text;
using RainbowForge.Core;

namespace RainbowScimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ScimitarMetaTableEntry
	{
		public uint Unknown1;
		public uint Unknown2;
		public ulong Unknown3;
		public uint Unknown4;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
		public byte[] NameData;

		public byte NameLength;
		public uint Timestamp;
		public uint Unknown5;
		public uint PreviousEntryIndex;
		public uint NextEntryIndex;
		public ulong Unknown6;
		public uint TypeHash;
		public uint Unknown7;

		public ulong Unknown8;
		public uint FileSize;

		public string DecodeName(ScimitarFile file)
		{
			var nameBytes = NameEncoding.DecodeName(NameData[..NameLength], TypeHash, file.Uid, file.Offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
			return Encoding.ASCII.GetString(nameBytes);
		}
	}
}