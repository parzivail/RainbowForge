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

			if (magic - 0x1015FA9957FBAA35u >= 2 && magic != 0x1004FA9957FBAA33 && magic != 0x1014FA9957FBAA34)
			{
				// Not sure how this check works in practice but it's the one the game uses
				throw new InvalidDataException($"Expected file magic, got 0x{magic:X16}");
			}

			// if ((magic & 0xFF00FFFFFFFFFF00) != 0x1000FA9957FBAA00)
			// 	throw new InvalidDataException($"Expected file magic, got 0x{magic:X16}");

			var header = r.ReadStruct<ScimitarFileHeader>();

			return header.PackMethod switch
			{
				ScimitarFilePackMethod.Block => ScimitarBlockPackedData.Read(bundleStream),
				ScimitarFilePackMethod.Streaming => ScimitarStreamingPackedData.Read(bundleStream),
				_ => throw new ArgumentOutOfRangeException(nameof(header.PackMethod), $"Unknown pack method 0x{(uint)header.PackMethod:X}")
			};
		}

		public static void Write(Stream dataStream, Stream bundleStream, ScimitarFilePackMethod packMethod)
		{
			var w = new BinaryWriter(bundleStream);

			w.Write(0x1014FA9957FBAA34uL);
			w.WriteStruct(new ScimitarFileHeader(packMethod));

			switch (packMethod)
			{
				case ScimitarFilePackMethod.Block:
					ScimitarBlockPackedData.Write(dataStream, bundleStream);
					break;
				case ScimitarFilePackMethod.Streaming:
					ScimitarStreamingPackedData.Write(dataStream, bundleStream);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(packMethod), packMethod, null);
			}
		}
	}
}