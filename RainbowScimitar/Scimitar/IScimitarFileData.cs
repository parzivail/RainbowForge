using System;
using System.Collections.Generic;
using System.IO;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.Scimitar
{
	public interface IScimitarFileData
	{
		private static readonly HashSet<ulong> KnownMagics = new()
		{
			0x1014FA9957FBAA34, 0x1015FA9957FBAA36
		};

		public Stream GetStream(Stream bundleStream);

		public static IScimitarFileData Read(Stream bundleStream)
		{
			var r = new BinaryReader(bundleStream);

			var magic = r.ReadUInt64();
			if (!KnownMagics.Contains(magic))
				throw new InvalidDataException($"Expected file magic, got 0x{magic:X16}");

			var header = r.ReadStruct<ScimitarFileHeader>();

			return header.PackMethod switch
			{
				ScimitarFilePackMethod.Block => ScimitarBlockPackedData.Read(bundleStream),
				ScimitarFilePackMethod.Streaming => ScimitarStreamingPackedData.Read(bundleStream),
				_ => throw new ArgumentOutOfRangeException(nameof(header.PackMethod), $"Unknown pack method 0x{(uint)header.PackMethod:X}")
			};
		}
	}
}