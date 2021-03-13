using System.IO;
using System.IO.Compression;
using Zstandard.Net;

namespace RainbowForge.Forge
{
	public class ChunkedDataBlock : IAssetBlock
	{
		public ChunkedData[] Chunks { get; }
		public bool IsPacked { get; }
		public uint PackedLength { get; }
		public uint UnpackedLength { get; }

		public ChunkedDataBlock(ChunkedData[] chunks, bool isPacked, uint packedLength, uint unpackedLength)
		{
			Chunks = chunks;
			IsPacked = isPacked;
			PackedLength = packedLength;
			UnpackedLength = unpackedLength;
		}

		public static ChunkedDataBlock Read(BinaryReader r)
		{
			// numChunks used to be an int32, but it caused read errors
			// TODO: is it actually an int16?
			var numChunks = r.ReadUInt16();
			var u2 = r.ReadUInt16();

			var packedLength = 0u;
			var unpackedLength = 0u;
			var isPacked = false;

			var chunks = new ChunkedData[numChunks];

			for (var i = 0; i < numChunks; i++)
			{
				var chunk = ChunkedData.Read(r);

				packedLength += chunk.OnDiskLength;
				unpackedLength += chunk.DecompressedLength;

				isPacked |= chunk.IsPacked;

				chunks[i] = chunk;
			}

			for (var i = 0; i < numChunks; i++)
			{
				chunks[i].Finalize(r);
				r.BaseStream.Seek(chunks[i].OnDiskLength, SeekOrigin.Current);
			}

			return new ChunkedDataBlock(chunks, isPacked, packedLength, unpackedLength);
		}


		public MemoryStream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();

			foreach (var chunk in Chunks)
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
	}
}