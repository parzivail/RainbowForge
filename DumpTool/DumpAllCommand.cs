using System;
using CommandLine;
using RainbowForge.Dump;

namespace DumpTool
{
	[Verb("dumpall", HelpText = "Dumps all assets in the given forge file")]
	public class DumpAllCommand
	{
		[Value(0, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		public static void Run(DumpAllCommand args)
		{
			var forge = Program.GetForge(args.ForgeFilename);

			foreach (var entry in forge.Entries)
			{
				try
				{
					DumpHelper.Dump(forge, entry, Environment.CurrentDirectory);
					Console.Error.WriteLine($"Dumped UID {entry.Uid}");
				}
				catch (Exception e)
				{
					Console.Error.WriteLine($"Error while dumping: UID {entry.Uid}\n{e}");
				}
			}
		}
	}
}