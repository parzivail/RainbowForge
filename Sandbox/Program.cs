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
					Console.Write($"Skipped (unknown asset type 0x{entry.Name.FileType:X8}) ");

					try
					{
						DumpHelper.Dump(forge, entry, outputDir);
						Console.WriteLine("(Asset, dumped)");
					}
					catch
					{
						Console.WriteLine("(Not asset)");
					}

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
				227, // [1] one entry
				288, // [1] one entry
				294, // [1] one entry
				416, // [1] one entry
				440, // [1] one entry
				530, // [1] one entry
				562, // [1] ** known but broken model archive
				573, // [1] one entry
				591, // [1] sound
				597, // [1] sound
				600, // [1] sound
				634, // [1] {MipContainer, MipSet} link container also contains a Matrix4F
				659, // [1] sound
				661, // [1] sound
				663, // [1] sound
				665, // [1] sound
				667, // [1] sound
				669, // [1] sound
				671, // [1] sound
				673, // [1] sound
				675, // [1] sound
				677, // [1] sound
				679, // [1] sound
				681, // [1] sound
				685, // [1] sound
				695, // [1] sound
				930, // [1] one entry
				1750, // [1]  one entry, contain strings like "RushAllBoostMoveModeModifier"
				1832, // [1]  ** known but broken model archive
				1852, // [1]  ** known but broken model archive
				1950, // [1]  ** known but broken model archive
				2064, // [1]  ** known but broken model archive
				2300, // [1]  ** known but broken model archive
				// ---
				62, // [2] {MipContainer, MipSet} sound
				70, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				78, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} contain references to physics stuff 
				86, // [2] very few entries
				94, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				102, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} sound
				110, // [2] possibly model, some contain >13k type-2629293323 entries
				126, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				134, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				136, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} 
				142, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} sound
				150, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} sound
				168, // [2] {MaterialContainer, MipContainer, MipSet} no mesh props
				192, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				200, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet} sound
				210, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				218, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				224, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				226, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				234, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				248, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				284, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				280, // [2] {MaterialContainer, MipContainer, MipSet} sound? no mesh props
				290, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				322, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				346, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				372, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				392, // [2] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				//---
				757, // [3] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				755, // [3] {MeshProperties, MaterialContainer, MipContainer, MipSet}
				729 // [3] {MaterialContainer, MipContainer, MipSet}
			};

			// TODO: some entries have Matrix4F

			// TODO: Known but failing model link archive types
			// 562: one extra entry in first entry set

			// TODO: Known but failing model link entry container types
			// 1382: some containers have double entries, some don't
			// 1832: has extra entries, longer footer entries
			// 1852: has extra entries, longer footer entries
			// 1950: has extra entries, longer footer entries
			// 2064: has extra entries, longer footer entries
			// 2300

			var modelLinkArchiveTypes = new uint[]
			{
				1302, // operator bodies, link container has tons of TypeC UID entries
				1282, // operator heads + headgear
				1278, // operator bodies, link container has tons of TypeC UID entries
				1270, // operator bodies, link container has tons of TypeC UID entries
				1254, // operator bodies, link container has tons of TypeC UID entries
				1238, // operator bodies, link container has tons of TypeC UID entries
				1222, // operator bodies, link container has tons of TypeC UID entries
				1214, // operator bodies, link container has tons of TypeC UID entries
				1198, // operator bodies, link container has tons of TypeC UID entries
				1182, // operator bodies, link container has tons of TypeC UID entries
				1166, // operator bodies, link container has tons of TypeC UID entries
				1086, // operator heads + headgear
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

			if (!MagicHelper.Equals(Magic.FlatArchive3, entry.Name.FileType))
				return;

			if (!modelLinkArchiveTypes.Contains(arc.Entries[0].Meta.Var1))
			{
				Console.WriteLine($"Archive was not model archive (got {arc.Entries[0].Meta.Var1})");

				if (!knownOtherArchives.Contains(arc.Entries[0].Meta.Var1))
				{
					DumpHelper.Dump(forge, entry, rootOutputDir);
					return;
				}

				return;
			}

			return;

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