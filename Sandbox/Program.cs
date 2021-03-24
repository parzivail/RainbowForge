using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Database;
using RainbowForge.Dump;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge";
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_bnk_mesh.forge";
			// const string inputFile = @"R:\Siege Dumps\Y6S1 v15447382\datapc64_merged_bnk_textures4.forge";

			var referenceDb = new LiteDatabase(@"R:\Siege Dumps\Asset Indexes\v15447382_y6s1.fidb");

			var outputDir = $@"R:\Siege Dumps\Unpacked\{Path.GetFileNameWithoutExtension(inputFile)}";
			Directory.CreateDirectory(outputDir);

			var forgeStream = new BinaryReader(File.Open(inputFile, FileMode.Open));

			var forge = Forge.Read(forgeStream);

			var sucessfulExports = 0;
			var failedExports = 0;
			var filterUids = new ulong[]
			{
				// 79280146794,
				// 79280146796,
				// 79280146828,
				// 79280146835
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

				if (magic == AssetType.Unknown || entry.Uid <= 78754982433)
				{
					Console.WriteLine("Skipped (unknown asset type)");
					continue;
				}

				try
				{
					if (magic == AssetType.FlatArchive)
					{
						if (filterUids.Contains(entry.Uid))
							DumpHelper.Dump(forge, entry, outputDir);
						else
							ProcessFlatArchive(referenceDb, forge, entry, outputDir, Path.GetDirectoryName(inputFile));

						sucessfulExports++;
						continue;
					}

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

		private static void ProcessFlatArchive(ILiteDatabase db, Forge forge, Entry entry, string rootOutputDir, string rootForgeDir)
		{
			var container = forge.GetContainer(entry.Uid);
			if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

			var assetStream = forgeAsset.GetDataStream(forge);
			var arc = FlatArchive.Read(assetStream);

			if (arc.Entries[0].Meta.Var1 != 278)
			{
				Console.WriteLine($"Archive was not model archive (expected var1=278, got {arc.Entries[0].Meta.Var1})");
				return;
			}

			var rootEntry = arc.Entries[0];
			var unresolvedExterns = new List<ulong>();
			DumpHelper.DumpNonContainerChildren(Path.Combine(rootOutputDir, $"flatarchive_id{entry.Uid}"), assetStream, arc, rootEntry, unresolvedExterns);

			var resolvedExterns = new Dictionary<string, List<ulong>>();

			var nameCollection = db.GetCollection<FilenameDocument>("filenames");

			foreach (var unresolvedExtern in unresolvedExterns)
			{
				var found = false;
				foreach (var (filename, collectionName) in nameCollection.FindAll())
				{
					var collection = db.GetCollection<EntryDocument>(collectionName, BsonAutoId.Int64);
					collection.EnsureIndex(document => document.Uid);

					if (!collection.Query().Where(document => document.Uid == unresolvedExtern).Exists()) continue;

					if (!resolvedExterns.ContainsKey(filename))
						resolvedExterns[filename] = new List<ulong>();

					found = true;
					resolvedExterns[filename].Add(unresolvedExtern);
					Console.WriteLine($"Resolved external reference {filename} => UID {unresolvedExtern}");
				}

				if (!found)
					Console.WriteLine($"Unresolved external reference to UID {unresolvedExtern}");
			}

			var outputDir = Path.Combine(rootOutputDir, $"model_flatarchive_{entry.Uid}");
			Directory.CreateDirectory(outputDir);

			foreach (var resolvedForgeFile in resolvedExterns.Keys)
			{
				var filename = Path.Combine(rootForgeDir, resolvedForgeFile + ".forge");
				using var resolvedForgeStream = new BinaryReader(File.Open(filename, FileMode.Open));
				var resolvedForge = Forge.Read(resolvedForgeStream);

				foreach (var resolvedUid in resolvedExterns[resolvedForgeFile])
				{
					var resolvedEntry = resolvedForge.Entries.First(entry1 => entry1.Uid == resolvedUid);
					DumpHelper.Dump(resolvedForge, resolvedEntry, outputDir);

					Console.WriteLine($"{resolvedForgeFile}/{resolvedUid} Dumped");
				}
			}
		}
	}
}