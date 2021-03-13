using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RainbowForge
{
	public static class BufferUtil
	{
		public static T ToStruct<T>(this byte[] buffer) where T : struct
		{
			var temp = new T();
			var size = Marshal.SizeOf(temp);
			var ptr = Marshal.AllocHGlobal(size);

			Marshal.Copy(buffer, 0, ptr, size);

			var ret = (T) Marshal.PtrToStructure(ptr, temp.GetType());
			Marshal.FreeHGlobal(ptr);

			return ret;
		}

		public static void CopyStream(this Stream input, Stream output, int length, int buffSize = 8192)
		{
			var buffer = new byte[buffSize];
			int read;
			while (length > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, length))) > 0)
			{
				output.Write(buffer, 0, read);
				length -= read;
			}
		}
	}
}