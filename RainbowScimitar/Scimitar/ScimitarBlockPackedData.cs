using System;
using System.IO;
using RainbowForge;
using RainbowScimitar.Compression;
using RainbowScimitar.Extensions;
using RainbowScimitar.Helper;
using ZstdNet;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarBlockPackedData(ushort Unknown1, ScimitarChunkSizeInfo[] SizeInfo, ScimitarChunkDataInfo[] DataInfo, CompressionMethod CompressionMethod) : IScimitarFileData
	{
		/// <inheritdoc />
		public Stream GetStream(Stream bundleStream)
		{
			var ms = StreamHelper.MemoryStreamManager.GetStream("ScimitarBlockPackedData.GetStream");

			for (var i = 0; i < SizeInfo.Length; i++)
			{
				var size = SizeInfo[i];
				var chunk = DataInfo[i];

				bundleStream.Seek(chunk.Offset, SeekOrigin.Begin);

				if (size.PayloadSize > size.SerializedSize)
				{
					// Contents are compressed
					switch (CompressionMethod)
					{
						case CompressionMethod.Zstd:
						{
							using var dctx = new DecompressionStream(bundleStream, 1024);
							dctx.CopyStreamTo(ms, size.PayloadSize);
							break;
						}
						case CompressionMethod.Oodle:
						{
							OodleHelper.EnsureOodleLoaded();

							var compressed = new byte[size.SerializedSize];
							bundleStream.Read(compressed, 0, compressed.Length);
							var decompressed = Oodle2Core8.Decompress(compressed, size.PayloadSize);
							ms.Write(decompressed, 0, decompressed.Length);
							break;
						}
						default:
							throw new ArgumentOutOfRangeException(nameof(CompressionMethod), CompressionMethod, null);
					}
				}
				else
				{
					// Contents are not compressed
					bundleStream.CopyStreamTo(ms, size.PayloadSize);
				}
			}

			ms.Position = 0;
			return ms;
		}

		public static void Write(Stream dataStream, Stream bundleStream, CompressionMethod compressionMethod)
		{
			var w = new BinaryWriter(bundleStream);

			// 256kb chunks
			const int chunkSize = 256 * 1024;
			var numChunks = dataStream.Length / chunkSize + 1;

			w.Write((short)numChunks);

			// w.Write(Unknown1);
			w.Write((short)0);

			using var msChunks = StreamHelper.MemoryStreamManager.GetStream("ScimitarBlockPackedData.Write/Chunks");
			var wChunk = new BinaryWriter(msChunks);

			while (dataStream.Position < dataStream.Length)
			{
				using var msChunk = StreamHelper.MemoryStreamManager.GetStream("ScimitarBlockPackedData.Write/TempChunk");
				var payloadSize = dataStream.CopyStreamTo(msChunk, chunkSize);

				msChunk.Position = 0;
				wChunk.Write(Crc32.Compute(msChunk, msChunk.Length));

				var startPos = msChunks.Position;

				switch (compressionMethod)
				{
					case CompressionMethod.Zstd:
					{
						var cs = new CompressionStream(msChunks);
						msChunk.Position = 0;
						msChunk.CopyTo(cs);
						cs.Close();
						break;
					}
					case CompressionMethod.Oodle:
					{
						OodleHelper.EnsureOodleLoaded();

						var uncompressedData = new byte[msChunk.Length];
						msChunk.Read(uncompressedData, 0, uncompressedData.Length);
						var compressed = Oodle2Core8.Compress(uncompressedData, Oodle2Core8.CompressionLevel.Normal);
						msChunks.Write(compressed, 0, compressed.Length);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null);
				}

				var serializedSize = (int)(msChunks.Position - startPos);

				w.Write(payloadSize);
				w.Write(serializedSize);
			}

			msChunks.Position = 0;
			msChunks.CopyTo(bundleStream);
		}

		public static ScimitarBlockPackedData Read(Stream bundleStream, CompressionMethod compressionMethod)
		{
			var r = new BinaryReader(bundleStream);

			var numChunks = r.ReadUInt16();
			var unknown1 = r.ReadUInt16();

			var sizeData = r.ReadStructs<ScimitarChunkSizeInfo>(numChunks);
			var chunkData = new ScimitarChunkDataInfo[numChunks];

			for (var i = 0; i < numChunks; i++)
			{
				var size = sizeData[i];

				var checksum = r.ReadUInt32();
				chunkData[i] = new ScimitarChunkDataInfo(checksum, r.BaseStream.Position);

				bundleStream.Seek(size.SerializedSize, SeekOrigin.Current);
			}

			return new ScimitarBlockPackedData(unknown1, sizeData, chunkData, compressionMethod);
		}
	}
}