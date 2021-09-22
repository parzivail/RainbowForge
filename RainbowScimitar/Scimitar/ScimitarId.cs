using System.Runtime.InteropServices;

namespace RainbowScimitar.Scimitar
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public readonly struct ScimitarId
	{
		private const ulong RELATIVE_ID_MASK = 0xFFFFFFFFFF000000;
		private const ulong RELATIVE_ID_BITMAP = 0x00000000F8000000;
		private const ulong RELATIVE_INDEX_MASK = 0x0000000000FFFFFF;

		public readonly ulong Id;

		public bool IsRelative => (Id & RELATIVE_ID_MASK) == RELATIVE_ID_BITMAP;
		public int RelativeIndex => (int)(Id & RELATIVE_INDEX_MASK);

		public ScimitarId(ulong id)
		{
			Id = id;
		}

		public static implicit operator ulong(ScimitarId id) => id.Id;
		public static implicit operator ScimitarId(ulong id) => new(id);
	}
}