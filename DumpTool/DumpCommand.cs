using System;
using System.Linq;
using CommandLine;
using RainbowForge.Dump;

namespace DumpTool
{
	[Verb("dump", HelpText = "Dumps the given asset")]
	public class DumpCommand
	{
		[Value(0, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		[Value(1, HelpText = "The UID to dump")]
		public ulong Uid { get; set; }

		public static void Run(DumpCommand args)
		{
			var forge = Program.GetForge(args.ForgeFilename);

			try
			{
				var metaEntry = forge.Entries.First(entry1 => entry1.Uid == args.Uid);
				DumpHelper.Dump(forge, metaEntry, Environment.CurrentDirectory);

				Console.Error.WriteLine($"Dumped UID {args.Uid}");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while dumping: {e}");
			}
		}
	}
}