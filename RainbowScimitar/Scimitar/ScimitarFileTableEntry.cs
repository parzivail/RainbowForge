using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarFileTableEntry
	{
		public readonly ulong Uid;
		public readonly long Offset;
		public readonly int Size;
	}
}