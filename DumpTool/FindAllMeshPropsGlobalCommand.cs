using System;
using System.IO;
using CommandLine;
using RainbowForge;
using RainbowForge.Core;

namespace DumpTool
{
	[Verb("findallmeshpropsglobal", HelpText = "Find all MeshProperties containers which reference the given UID in all flat archives in the given forge file")]
	public class FindAllMeshPropsGlobalCommand
	{
		[Value(0, HelpText = "The directory of forge files to search")]
		public string ForgeDirectory { get; set; }

		[Value(1, HelpText = "The UID to search for")]
		public ulong Uid { get; set; }

		public static void Run(FindAllMeshPropsGlobalCommand args)
		{
			FileSystemUtil.AssertDirectoryExists(args.ForgeDirectory);

			foreach (var file in Directory.GetFiles(args.ForgeDirectory, "*.forge"))
			{
				var forge = Forge.GetForge(file);
				for (var i = 0; i < forge.Entries.Length; i++)
				{
					try
					{
						var entry = forge.Entries[i];
						if (FindAllMeshPropsCommand.SearchFlatArchive(forge, entry, args.Uid))
							Console.WriteLine($"{Path.GetFileName(file)}: {entry.Uid}");
					}
					catch
					{
						// ignored
					}
				}
			}
		}
	}
}