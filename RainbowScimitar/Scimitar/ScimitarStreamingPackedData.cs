using System;
using System.IO;
using System.Linq;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarStreamingPackedData(ushort Unknown1, ScimitarChunkSizeInfo[] SizeInfo, uint[] ChecksumInfo, long Offset) : IScimitarFileData
	{
		/// <inheritdoc />
		public Stream GetStream(Stream bundleStream)
		{
			return new SubStream(bundleStream, Offset, SizeInfo.Sum(info => info.PayloadSize));
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

			if (sizeData.Any(info => info.PayloadSize != info.SerializedSize))
				throw new NotSupportedException("Streaming data blocks with compression are not yet supported!");

			return new ScimitarStreamingPackedData(unk1, sizeData, checksumData, bundleStream.Position);
		}
	}
}