using System.IO;
using System.IO.Compression;
using RainbowScimitar.Extensions;
using RainbowScimitar.Helper;
using Zstandard.Net;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarBlockPackedData(ushort Unknown1, ScimitarChunkSizeInfo[] SizeInfo, ScimitarChunkDataInfo[] DataInfo) : IScimitarFileData
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
					using var dctx = new ZstandardStream(bundleStream, CompressionMode.Decompress, true);
					dctx.CopyStreamTo(ms, size.PayloadSize);
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

		public static ScimitarBlockPackedData Read(Stream bundleStream)
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

			return new ScimitarBlockPackedData(unknown1, sizeData, chunkData);
		}
	}
}