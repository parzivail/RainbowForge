using System.Runtime.InteropServices;
using System.Text;
using RainbowForge.Core;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarAssetMetadata
	{
		public readonly uint Unknown1;
		public readonly uint Unknown2;
		public readonly ulong Unknown3;
		public readonly uint Unknown4;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
		public readonly byte[] NameData;

		public readonly byte NameLength;
		public readonly uint Timestamp;
		public readonly uint Unknown5;
		public readonly int PreviousEntryIndex;
		public readonly int NextEntryIndex;
		public readonly ulong Unknown6;
		public readonly uint TypeHash;
		public readonly uint Unknown7;

		public readonly ulong Unknown8;
		public readonly int FileSize;

		public string DecodeName(ScimitarFileTableEntry file)
		{
			// TODO: move this function into RainbowScimitar
			var nameBytes = NameEncoding.DecodeName(NameData[..NameLength], TypeHash, file.Uid, (ulong)file.Offset, NameEncoding.FILENAME_ENCODING_ENTRY_KEY_STEP);
			return Encoding.ASCII.GetString(nameBytes);
		}
	}
}