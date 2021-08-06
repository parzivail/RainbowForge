using System;
using CommandLine;
using RainbowForge;
using RainbowForge.Forge;

namespace DumpTool
{
	[Verb("list", HelpText = "List all of the UIDs in the given container")]
	internal class ListCommand
	{
		[Value(0, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		public static void Run(ListCommand args)
		{
			var forge = Forge.GetForge(args.ForgeFilename);

			foreach (var forgeEntry in forge.Entries)
			{
				var magic = MagicHelper.GetFiletype(forgeEntry.Name.FileType);
				Console.WriteLine($"{forgeEntry.Uid}: {magic}");
			}
		}
	}
}