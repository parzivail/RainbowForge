using System;
using System.IO;

namespace RainbowScimitar.Extensions
{
	public static class StreamExt
	{
		public static int CopyStreamTo(this Stream src, Stream dest, int length, int buffSize = 8192)
		{
			var buffer = new byte[buffSize];
			int read;

			var totalRead = 0;
			while (length > 0 && (read = src.Read(buffer, 0, Math.Min(buffer.Length, length))) > 0)
			{
				dest.Write(buffer, 0, read);
				length -= read;
				totalRead += read;
			}

			return totalRead;
		}
	}
}