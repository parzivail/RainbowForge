using System.IO;
using System.IO.Compression;
using RainbowForge.Compression;
using Zstandard.Net;

namespace RainbowForge.Core.DataBlock
{
	public class ChunkedDataBlock : IAssetBlock
	{
		public ChunkedData[] Chunks { get; }
		public bool IsPacked { get; }
		public uint PackedLength { get; }
		public uint UnpackedLength { get; }
		public bool UseOodle { get; }

		public ChunkedDataBlock(ChunkedData[] chunks, bool isPacked, uint packedLength, uint unpackedLength, bool useOodle)
		{
			Chunks = chunks;
			IsPacked = isPacked;
			PackedLength = packedLength;
			UnpackedLength = unpackedLength;
			UseOodle = useOodle;
		}

		public Stream GetDataStream(BinaryReader r)
		{
			var ms = new MemoryStream();

			foreach (var chunk in Chunks)
			{
				r.BaseStream.Seek(chunk.Offset, SeekOrigin.Begin);

				if (chunk.IsCompressed)
				{
					if (UseOodle)
					{
						OodleHelper.EnsureOodleLoaded();

						// Contents are compressed
						var compressed = r.ReadBytes((int)chunk.SerializedLength);
						var decompressed = Oodle2Core8.Decompress(compressed, (int)chunk.DataLength);
						ms.Write(decompressed, 0, decompressed.Length);
					}
					else
					{
						using var dctx = new ZstandardStream(r.BaseStream, CompressionMode.Decompress, true);
						// TODO: make sure this reads exactly {chunk.SerializedLength} bytes -- it should,
						// but reading {chunk.DataLength} bytes from a decompression stream is
						// a weird way to do it
						dctx.CopyStream(ms, (int)chunk.DataLength);
					}
				}
				else
				{
					r.BaseStream.CopyStream(ms, (int)chunk.SerializedLength);
				}
			}

			ms.Seek(0, SeekOrigin.Begin);
			return ms;
		}

		public static ChunkedDataBlock Read(BinaryReader r, bool useOodle = false)
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

			return new ChunkedDataBlock(chunks, isCompressed, serializedLength, dataLength, useOodle);
		}
	}
}