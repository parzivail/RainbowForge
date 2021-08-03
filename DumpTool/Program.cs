using System;
using System.IO;
using CommandLine;
using RainbowForge.Forge;

namespace DumpTool
{
	internal class Program
	{
		public static void AssertFileExists(string filename)
		{
			if (File.Exists(filename)) return;

			Console.Error.WriteLine($"File not found: {filename}");
			Environment.Exit(-1);
		}

		public static void AssertDirectoryExists(string dir)
		{
			if (Directory.Exists(dir)) return;

			Console.Error.WriteLine($"Directory not found: {dir}");
			Environment.Exit(-1);
		}

		public static Forge GetForge(string filename)
		{
			AssertFileExists(filename);
			var forgeStream = new BinaryReader(File.Open(filename, FileMode.Open));
			return Forge.Read(forgeStream);
		}

		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<ListCommand, FindCommand, InspectCommand, DumpCommand, DumpMeshPropsCommand, IndexCommand,
					DumpAllCommand, DumpAllMeshPropsCommand, FindAllMeshPropsCommand, FindAllMeshPropsGlobalCommand>(args)
				.WithParsed<ListCommand>(ListCommand.Run)
				.WithParsed<FindCommand>(FindCommand.Run)
				.WithParsed<InspectCommand>(InspectCommand.Run)
				.WithParsed<DumpCommand>(DumpCommand.Run)
				.WithParsed<DumpAllCommand>(DumpAllCommand.Run)
				.WithParsed<DumpMeshPropsCommand>(DumpMeshPropsCommand.Run)
				.WithParsed<DumpAllMeshPropsCommand>(DumpAllMeshPropsCommand.Run)
				.WithParsed<FindAllMeshPropsCommand>(FindAllMeshPropsCommand.Run)
				.WithParsed<FindAllMeshPropsGlobalCommand>(FindAllMeshPropsGlobalCommand.Run)
				.WithParsed<IndexCommand>(IndexCommand.Run);
		}
	}
}