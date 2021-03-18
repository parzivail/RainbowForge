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
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge";
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_bnk_mesh.forge";
			const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_playgo_bnk_guitextures0.forge";

			var outputDir = $@"R:\Siege Dumps\Unpacked\{Path.GetFileNameWithoutExtension(inputFile)}";
			Directory.CreateDirectory(outputDir);

			var forgeStream = new BinaryReader(File.Open(inputFile, FileMode.Open));

			var forge = Forge.Read(forgeStream);

			var sucessfulExports = 0;
			var failedExports = 0;
			var filterUids = new ulong[]
			{
				// 241888864993,
				// 241888865002,
				// 241888865013,
				// 264139769014,
				// 264139769027,
				// 264139769037,
				// 264139769156,
				// 264139769167,
				// 264139769176,
				// 264139769187,
				// 264139769198,
				// 264139769209,
				// 264139769218,
				// 264139769229,
				// 264139769240,
				// 264139769251,
				// 264139769260,
				// 264139769271,
				// 264139769282,
				// 264139769293,
				// 264139769302,
				// 264139769313,
				// 264139769335,
				// 264139769344,
				// 264139769355,
				// 264139769366,
				// 264139769377,
				// 264139769386,
				// 264139769397,
				// 264139769408,
				// 264139769419,
				// 264139769428,
				// 264139769439,
				// 264139769450,
				// 264139769461,
				// 264139769470,
				// 264139769481,
				// 67256658863,
				// 67256658867,
				// 67256658873
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