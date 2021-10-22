using System.Runtime.InteropServices;

namespace RainbowScimitar.FileTypes
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Box
	{
		public readonly float MinX;
		public readonly float MinY;
		public readonly float MinZ;
		public readonly float MaxX;
		public readonly float MaxY;
		public readonly float MaxZ;
	}
}