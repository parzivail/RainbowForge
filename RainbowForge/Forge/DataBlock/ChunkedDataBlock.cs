using System.IO;
using System.IO.Compression;
using Zstandard.Net;

namespace RainbowForge.Forge.DataBlock
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

		public MemoryStream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();

			foreach (var chunk in Chunks)
			{
				r.BaseStream.Seek(chunk.Offset, SeekOrigin.Begin);

				if (chunk.IsCompressed)
				{
					using var dctx = new ZstandardStream(r.BaseStream, CompressionMode.Decompress, true);
					// TODO: make sure this reads exactly {chunk.SerializedLength} bytes -- it should,
					// but reading {chunk.DataLength} bytes from a decompression stream is
					// a weird way to do it
					dctx.CopyStream(ms, (int) chunk.DataLength);
				}
				else
				{
					r.BaseStream.CopyStream(ms, (int) chunk.SerializedLength);
				}
			}

			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}

		public static ChunkedDataBlock Read(BinaryReader r)
		{
			// numChunks used to be an int32, but it caused read errors
			// TODO: is it actually an int16?
			var numChunks = r.ReadUInt16();
			var u2 = r.ReadUInt16();

			var serializedLength = 0u;
			var dataLength = 0u;
			var isCompressed = false;

			var chunks = new ChunkedData[numChunks];

			for (var i = 0; i < numChunks; i++)
			{
				var chunk = ChunkedData.Read(r);

				serializedLength += chunk.SerializedLength;
				dataLength += chunk.DataLength;

				isCompressed |= chunk.IsCompressed;

				chunks[i] = chunk;
			}

			foreach (var chunk in chunks)
			{
				chunk.Finalize(r);
				r.BaseStream.Seek(chunk.SerializedLength, SeekOrigin.Current);
			}

			return new ChunkedDataBlock(chunks, isCompressed, serializedLength, dataLength);
		}
	}
}