using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using RainbowForge.Forge;
using Zstandard.Net;

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

		public static MemoryStream GetDecompressedStream(this Datablock block, BinaryReader r)
		{
			return GetDecompressedStream(block.Chunks, r);
		}

		public static MemoryStream GetDecompressedStream(this DatablockChunk chunk, BinaryReader r)
		{
			return GetDecompressedStream(new[] {chunk}, r);
		}

		public static MemoryStream GetDecompressedStream(this IEnumerable<DatablockChunk> chunks, BinaryReader r)
		{
			var ms = new MemoryStream();

			foreach (var chunk in chunks)
			{
				r.BaseStream.Seek(chunk.Offset, SeekOrigin.Begin);

				if (chunk.IsPacked)
				{
					var dctx = new ZstandardStream(r.BaseStream, CompressionMode.Decompress, true);
					// TODO: make sure this reads exactly {chunk.OnDiskLength} bytes -- it should,
					// but reading {chunk.DecompressedLength} bytes from a decompression stream is
					// a weird way to do it
					dctx.CopyStream(ms, (int) chunk.DecompressedLength);
				}
				else
				{
					r.BaseStream.CopyStream(ms, (int) chunk.OnDiskLength);
				}
			}

			ms.Seek(0, SeekOrigin.Begin);
			return ms;
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