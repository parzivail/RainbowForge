using System;
using System.IO;
using System.Linq;
using CommandLine;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;

namespace DumpTool
{
	[Verb("find", HelpText = "Find the forge file(s) that contain the given UID")]
	public class FindCommand
	{
		[Value(0, HelpText = "The directory of forge files to search")]
		public string SearchDirectory { get; set; }

		[Value(1, HelpText = "The UID to search for")]
		public ulong Uid { get; set; }

		public static void Run(FindCommand args)
		{
			FileSystemUtil.AssertDirectoryExists(args.SearchDirectory);

			foreach (var file in Directory.GetFiles(args.SearchDirectory, "*.forge"))
			{
				var forge = Forge.GetForge(file);

				foreach (var entry in forge.Entries)
				{
					if (entry.Uid == args.Uid)
						Console.WriteLine(file);

					var container = forge.GetContainer(entry.Uid);
					if (container is not ForgeAsset forgeAsset) return;

					var assetStream = forgeAsset.GetDataStream(forge);
					var arc = FlatArchive.Read(assetStream);

					if (arc.Entries.Any(archiveEntry => archiveEntry.Meta.Uid == args.Uid))
						Console.WriteLine($"{file} -> within flat archive {entry.Uid}");
				}
			}
		}
	}
}