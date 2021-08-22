using System;
using System.IO;
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
					TestMagic(sw, entry.MetaData.FileType);

					if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
						continue;

					var container = newForge.GetContainer(entry.Uid);
					if (container is not ForgeAsset fa)
						continue;

					var arc = FlatArchive.Read(fa.GetDataStream(newForge));

					foreach (var arcEntry in arc.Entries) TestMagic(sw, arcEntry.MetaData.FileType);
				}
			}
			
			Console.WriteLine("Done.");
		}

		private static void TestMagic(StreamWriter sw, uint fileType)
		{
		}
	}
}