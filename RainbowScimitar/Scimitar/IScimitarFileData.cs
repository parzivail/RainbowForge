using System;
using System.IO;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public interface IScimitarFileData
	{
		private const ulong MAGIC = 0x1015FA9957FBAA36;

		public Stream GetStream(Stream bundleStream);

		public static IScimitarFileData Read(BinaryReader r)
		{
			var magic = r.ReadUInt64();
			if (magic != MAGIC)
				throw new InvalidDataException($"Expected file magic 0x{MAGIC:X16}, got 0x{magic:X16}");

			var header = r.ReadStruct<ScimitarFileHeader>();

			return header.PackMethod switch
			{
				ScimitarFilePackMethod.Block => ScimitarBlockPackedData.Read(r),
				ScimitarFilePackMethod.Streaming => ScimitarStreamingPackedData.Read(r),
				_ => throw new ArgumentOutOfRangeException(nameof(header.PackMethod), $"Unknown pack method 0x{(uint)header.PackMethod:X}")
			};
		}
	}
}