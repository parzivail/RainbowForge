using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.FileTypes
{
	public record DepGraph
	{
		public static DepGraph Read(Stream s)
		{
			// var fileData = IScimitarFileData.Read(s);
			// var dataStream = fileData.GetStream(s);

			var r = new BinaryReader(s);

			// var unk1 = r.ReadByte(); // 2

			while (r.BaseStream.Position < r.BaseStream.Length)
			{
				var root = DepGraphNode.Read(r);
			}

			foreach (var (i, count) in DepGraphNode.Data.OrderByDescending(pair => pair.Value))
				Console.WriteLine($"{i:X6}: {count}");

			return null;
		}
	}

	public record DepGraphNode
	{
		public static Dictionary<int, int> Data = new Dictionary<int, int>();

		public static DepGraphNode Read(BinaryReader r)
		{
			// datapc64_ui_playgo.depgraphbin.unpacked.bin: (zero entries with sub-entries)
			// - unk3 EF: 39929

			// datapc64_pvp20_italy.depgraphbin.unpacked: (57 entries with sub-entries)
			// - unk3 EF: 23950
			// - unk3 FF: 3702

			var parentUid = r.ReadUid();
			var childUid = r.ReadUid();

			var unk2 = r.ReadInt32(); // -1

			var unk3 = r.ReadByte(); // 0xEF, 0xFF
			var unk4 = r.ReadByte(); // 0x1
			var unk5 = r.ReadByte();
			/*
			 unk5 ==
			 0 00000000
			 1 00000001
			 2 00000010
			 3 00000011
			 4 00000100
			 5 00000101
			 8 00001000
			 9 00001001
			 A 00001010
			 B 00001011
			 */

			var unk6 = r.ReadByte();
			/*
			 unk6 ==
			 0 00000000
			 1 00000001
			 5 00000101
			 7 00000111
			 */

			var unk56 = unk5; //(unk3 << 16) | (unk5 << 8) | unk6;
			if (!Data.ContainsKey(unk56))
				Data[unk56] = 0;
			Data[unk56]++;

			return null;
		}
	}
}