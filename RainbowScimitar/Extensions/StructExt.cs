using System.Runtime.InteropServices;

namespace RainbowScimitar.Extensions
{
	public static class StructExt
	{
		public static byte[] ToBytes<T>(this T s) where T : struct
		{
			var size = Marshal.SizeOf(s);
			var arr = new byte[size];

			var ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(s, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return arr;
		}
	}
}