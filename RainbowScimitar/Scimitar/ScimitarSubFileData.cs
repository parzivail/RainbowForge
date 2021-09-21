using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarSubFileData
	{
		public readonly ScimitarId Uid;
		public readonly int Unknown1;
	}
}