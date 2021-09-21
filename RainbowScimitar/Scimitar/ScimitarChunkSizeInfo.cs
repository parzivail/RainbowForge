using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarChunkSizeInfo
	{
		public readonly int PayloadSize;
		public readonly int SerializedSize;
	}
}