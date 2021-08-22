using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using var sw = new StreamWriter("out.txt");
			foreach (var forgeFilename in Directory.GetFiles("R:\\Siege Dumps\\Y6S1 v15500403", "*.forge"))
			{
				Console.WriteLine(forgeFilename);

				var newForge = Forge.GetForge(forgeFilename);

				foreach (var entry in newForge.Entries)
				{
					TestMagic(entry.MetaData.FileType);

					if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
						continue;

					var container = newForge.GetContainer(entry.Uid);
					if (container is not ForgeAsset fa)
						continue;

					var arc = FlatArchive.Read(fa.GetDataStream(newForge));

					foreach (var arcEntry in arc.Entries) TestMagic(arcEntry.MetaData.FileType);
				}
			}

			foreach (var (magic, count) in MagicTable.OrderByDescending(pair => pair.Value))
				if (Enum.IsDefined(typeof(Magic), (ulong)magic))
					sw.WriteLine($"0x{magic:X8} ({(Magic)magic}) - {count}");
				else
					sw.WriteLine($"0x{magic:X8} - {count}");

			Console.WriteLine("Done.");
		}

		private static readonly Dictionary<uint, int> MagicTable = new();

		private static void TestMagic(uint fileType)
		{
			if (!MagicTable.ContainsKey(fileType))
				MagicTable[fileType] = 0;

			MagicTable[fileType]++;
		}
	}
}