using System;
using System.IO;

namespace RainbowScimitar.Extensions
{
	public static class StreamExt
	{
		public static void CopyStreamTo(this Stream src, Stream dest, int length, int buffSize = 8192)
		{
			var buffer = new byte[buffSize];
			int read;
			while (length > 0 && (read = src.Read(buffer, 0, Math.Min(buffer.Length, length))) > 0)
			{
				dest.Write(buffer, 0, read);
				length -= read;
			}
		}
	}
}