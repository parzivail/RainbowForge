using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarFileHeader
	{
		public readonly short Unknown1;
		public readonly ScimitarFilePackMethod PackMethod; // TODO: is this two int16s?
		public readonly byte Unknown2;
		public readonly short Unknown3;

		public ScimitarFileHeader(ScimitarFilePackMethod packMethod)
		{
			PackMethod = packMethod;
			Unknown1 = 0;
			Unknown2 = 0;
			Unknown3 = 0;
		}
	}
}