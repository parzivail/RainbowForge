using System;
using System.IO;
using RainbowForge;
using RainbowForge.Dump;
using RainbowForge.Forge;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge";
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_bnk_mesh.forge";	

			var outputDir = $@"R:\Siege Dumps\Unpacked\{Path.GetFileNameWithoutExtension(inputFile)}";
			Directory.CreateDirectory(outputDir);

			var forgeStream = new BinaryReader(File.Open(inputFile, FileMode.Open));

			var forge = Forge.Read(forgeStream);

			// Forge file naming scheme:
			// - mesh: model assets
			// - textures: texture assets
			// - soundmedia: sound assets
			// - gidata: global illumination maps

			for (var i = 0; i < forge.NumEntries; i++)
			{
				var entry = forge.Entries[i];

				var magic = MagicHelper.GetFiletype(entry.Name.FileType);

				Console.Write($"Entry {i}: UID {entry.Uid}, {magic} (0x{entry.Name.FileType:X}) ");

				if (magic == AssetType.Unknown)
				{
					Console.WriteLine("Skipped");
					continue;
				}

				try
				{
					DumpHelper.Dump(forge, entry, outputDir);
					Console.WriteLine("Dumped");
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed: {e.Message}");
				}
			}

			Console.WriteLine($"Processed {forge.Entries.Length} entries");
		}
	}
}