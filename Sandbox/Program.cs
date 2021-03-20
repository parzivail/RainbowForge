using System;
using System.IO;
using System.Linq;
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
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_bnk_textures4.forge";

			var outputDir = $@"R:\Siege Dumps\Unpacked\{Path.GetFileNameWithoutExtension(inputFile)}";
			Directory.CreateDirectory(outputDir);

			var forgeStream = new BinaryReader(File.Open(inputFile, FileMode.Open));

			var forge = Forge.Read(forgeStream);

			var sucessfulExports = 0;
			var failedExports = 0;
			var filterUids = new ulong[]
			{
				261653128116
			};

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

				if (filterUids.Length > 0 && !filterUids.Contains(entry.Uid))
				{
					Console.WriteLine("Skipped (filter miss)");
					continue;
				}

				if (magic == AssetType.Unknown)
				{
					Console.WriteLine("Skipped (unknown asset type)");
					continue;
				}

				try
				{
					DumpHelper.Dump(forge, entry, outputDir);
					Console.WriteLine("Dumped");
					sucessfulExports++;
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed: {e.Message}");
					failedExports++;
				}
			}

			Console.WriteLine(
				$"Processed {forge.Entries.Length} entries, {sucessfulExports + failedExports} of {filterUids.Length} filter hits, {sucessfulExports} successful, {failedExports} failed");
		}
	}
}