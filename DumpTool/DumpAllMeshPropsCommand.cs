using System;
using System.IO;
using CommandLine;
using LiteDB;
using RainbowForge;
using RainbowForge.Forge;

namespace DumpTool
{
	[Verb("dumpallmeshprops", HelpText = "Dumps all MeshProperties containers in all flat archives in the given forge file")]
	public class DumpAllMeshPropsCommand
	{
		[Value(0, HelpText = "The search index to use (see command: index)")]
		public string IndexFilename { get; set; }

		[Value(1, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		public static void Run(DumpAllMeshPropsCommand args)
		{
			FileSystemUtil.AssertFileExists(args.IndexFilename);
			var forge = Forge.GetForge(args.ForgeFilename);

			try
			{
				var db = new LiteDatabase(args.IndexFilename);

				foreach (var entry in forge.Entries) DumpMeshPropsCommand.ProcessFlatArchive(db, forge, entry, Environment.CurrentDirectory, Path.GetDirectoryName(args.ForgeFilename));
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while dumping: {e}");
			}
		}
	}
}