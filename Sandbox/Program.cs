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
			};

			// Forge file naming scheme:
			// - mesh: model assets
			// - textures: texture assets
			// - soundmedia: sound assets
			// - gidata: global illumination maps

			// General notes:
			//	- Textures
			//		- "Extra Texture": Red = metallic map, green = 1 - roughness 

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

			var knownOtherArchives = new uint[]
			{
				227, // one entry
				288, // one entry
				294, // one entry
				416, // one entry
				440, // one entry
				530, // one entry
				562, // ** known but broken model archive
				573, // one entry
				591, // sound
				597, // sound
				600, // sound
				634, // always 3 entries, link container also contains a Matrix4F
				659, // sound
				661, // sound
				663, // sound
				665, // sound
				667, // sound
				669, // sound
				670, // possibly model
				671, // sound
				673, // sound
				675, // sound
				677, // sound
				679, // sound
				681, // sound
				685, // sound
				695, // sound
				930, // one entry
				1086, // possibly model
				1166, // possibly model, unique repeating structure
				1182, // possibly model, unique repeating structure
				1198, // possibly model, unique repeating structure
				1214, // possibly model, unique repeating structure
				1222, // possibly model, unique repeating structure
				1238, // possibly model, unique repeating structure
				1254, // possibly model, unique repeating structure
				1270, // possibly model, unique repeating structure
				1278, // possibly model, unique repeating structure
				1282, // possibly model
				1302, // possibly model, unique repeating structure
				1750, // one entry, contain strings like "RushAllBoostMoveModeModifier"
				1832, // possibly model
				1852, // possibly model, has slightly different entry lengths
				1950, // possibly model
				2064, // possibly model, few entries
				2300 // possibly model, few entries
			};

			// TODO: Known but failing model link archive types
			// 562: one extra entry in first entry set

			// TODO: Known but failing model link entry container types
			// 1382: some containers have double entries, some don't
			// 1852: has 4 extra entries, longer footer entries

			var modelLinkArchiveTypes = new uint[]
			{
				670, // composite operator bodies + heads
				572, // operator bodies
				548, // composite operator bodies
				450, // one unidentified map prop?
				376, // composite operator bodies with holster models, operator bodies, some gadgets, some chibis
				363, // gun skins, some entries produce no output
				342, // only one model, mesh UID unresolved
				298, // chibi map props
				180, // charms, some operator bodies
				278 // operator heads, operator bodies, some map props
			};

			if (arc.Entries[0].Meta.Var1 != 670)
				return;

			if (!modelLinkArchiveTypes.Contains(arc.Entries[0].Meta.Var1))
			{
				Console.WriteLine($"Archive was not model archive (expected var1=278, got {arc.Entries[0].Meta.Var1})");
				DumpHelper.Dump(forge, entry, rootOutputDir);
				return;
			}

			if (arc.Entries.Any(archiveEntry => archiveEntry.Meta.Var1 == 1382))
				throw new NotSupportedException();

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