using System;
using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Helper;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarStreamingPackedData(ushort Unknown1, ScimitarChunkSizeInfo[] SizeInfo, uint[] ChecksumInfo, long[] Offsets) : IScimitarFileData
	{
		/// <inheritdoc />
		public Stream GetStream(Stream bundleStream)
		{
			var ms = StreamHelper.MemoryStreamManager.GetStream("ScimitarBlockPackedData.GetStream");

			for (var i = 0; i < SizeInfo.Length; i++)
			{
				var size = SizeInfo[i];

				bundleStream.Seek(Offsets[i], SeekOrigin.Begin);

				if (size.PayloadSize > size.SerializedSize)
				{
					// Is this supported in the wild?
					throw new NotSupportedException("Compressed streaming blocks are not supported!");
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

		public static void Write(Stream dataStream, Stream bundleStream)
		{
			var w = new BinaryWriter(bundleStream);

			// 256kb chunks
			const int chunkSize = 256 * 1024;
			var numChunks = dataStream.Length / chunkSize + 1;

			w.Write((short)numChunks);

			// w.Write(Unknown1);
			w.Write((short)0);

			using var msChunks = StreamHelper.MemoryStreamManager.GetStream("ScimitarStreamingPackedData.Write/Chunks");

			using var msChecksums = StreamHelper.MemoryStreamManager.GetStream("ScimitarStreamingPackedData.Write/Checksums");
			var wChecksums = new BinaryWriter(msChecksums);

			while (dataStream.Position < dataStream.Length)
			{
				using var msChunk = StreamHelper.MemoryStreamManager.GetStream("ScimitarStreamingPackedData.Write/TempChunk");
				var payloadSize = dataStream.CopyStreamTo(msChunk, chunkSize);

				msChunk.Position = 0;
				wChecksums.Write(Crc32.Compute(msChunk, msChunk.Length));

				var startPos = msChunks.Position;

				msChunk.Position = 0;
				msChunk.CopyTo(msChunks);

				var serializedSize = (int)(msChunks.Position - startPos);

				w.Write(payloadSize);
				w.Write(serializedSize);
			}

			msChecksums.Position = 0;
			msChecksums.CopyTo(bundleStream);

			msChunks.Position = 0;
			msChunks.CopyTo(bundleStream);
		}

		public static ScimitarStreamingPackedData Read(Stream bundleStream)
		{
			var r = new BinaryReader(bundleStream);

			var numChunks = r.ReadUInt16();
			var unk1 = r.ReadUInt16();

			var sizeData = r.ReadStructs<ScimitarChunkSizeInfo>(numChunks);
			var checksumData = new uint[numChunks];

			for (var i = 0; i < numChunks; i++)
				checksumData[i] = r.ReadUInt32();

			var offsets = new long[numChunks];
			for (var i = 0; i < sizeData.Length; i++)
			{
				var sizeInfo = sizeData[i];

				offsets[i] = r.BaseStream.Position;
				r.BaseStream.Seek(sizeInfo.SerializedSize, SeekOrigin.Current);
			}

			return new ScimitarStreamingPackedData(unk1, sizeData, checksumData, offsets);
		}
	}
}