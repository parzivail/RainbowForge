using System;
using System.IO;
using CommandLine;

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
			Program.AssertDirectoryExists(args.ForgeDirectory);

			foreach (var file in Directory.GetFiles(args.ForgeDirectory, "*.forge"))
				try
				{
					var forge = Program.GetForge(file);
					for (var i = 0; i < forge.Entries.Length; i++)
					{
						var entry = forge.Entries[i];

						if (FindAllMeshPropsCommand.SearchFlatArchive(forge, entry, args.Uid))
							Console.WriteLine($"{Path.GetFileName(file)}: {entry.Uid}");
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"Error while dumping: {e}");
				}
		}
	}
}