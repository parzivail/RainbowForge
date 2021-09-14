using System.Runtime.InteropServices;

namespace RainbowScimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ScimitarFile
	{
		public ulong Uid;
		public ulong Offset;
		public uint Size;
	}
}