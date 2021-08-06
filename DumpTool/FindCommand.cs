using System;
using System.IO;
using System.Linq;
using CommandLine;
using RainbowForge;
using RainbowForge.Forge;

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

				if (forge.Entries.Any(entry => entry.Uid == args.Uid))
					Console.WriteLine(file);
			}
		}
	}
}