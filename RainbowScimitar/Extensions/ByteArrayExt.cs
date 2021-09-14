using System.Runtime.InteropServices;

namespace RainbowScimitar.Extensions
{
	public static class ByteArrayExt
	{
		public static void ToStruct<T>(this byte[] buffer, ref T dest) where T : struct
		{
			var ptr = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, ptr, buffer.Length);
			dest = (T)Marshal.PtrToStructure(ptr, dest.GetType());
			Marshal.FreeHGlobal(ptr);
		}
	}
}