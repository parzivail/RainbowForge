using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Dump;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;

namespace ForgeDiff
{
	internal class EntryDocument
	{
		public ulong Uid { get; init; }
		public uint Timestamp { get; init; }
		public uint FileType { get; init; }

		public static EntryDocument For(Entry entry)
		{
			return new()
			{
				Uid = entry.Uid,
				Timestamp = entry.Name.Timestamp,
				FileType = entry.Name.FileType
			};
		}
	}

	internal class EntryDocumentWithSource : EntryDocument
	{
		public string Source { get; init; }
	}

	internal record FilenameDocument(string Filename, string CollectionName);

	internal class Program
	{
		private static readonly char[] Characters = Enumerable.Range(0, 26).Select(i => (char) ('a' + i)).ToArray();

		private static string GetFilename(int i)
		{
			var len = Characters.Length;

			var s = "";
			do
			{
				s = Characters[i % len] + s;
				i /= len;
			} while (i > 0);

			return s;
		}

		private static void Main(string[] args)
		{
			// File formats naming scheme (all LiteDB):
			// *.FIDB: Forge asset index DB
			// *.FDDB: Forge asset diff DB, filename should specify what's diffed

			const string databaseFileDiffBase = @"R:\Siege Dumps\Asset Indexes\v15439724_y6s1_tts.fidb";
			const string databaseFileNewest = @"R:\Siege Dumps\Asset Indexes\v15447382_y6s1.fidb";
			const string assetDirectory = @"R:\Siege Dumps\Y5S4 v15302504";

			var diffDir = Path.GetDirectoryName(databaseFileDiffBase);
			var nameA = Path.GetFileNameWithoutExtension(databaseFileDiffBase);
			var nameB = Path.GetFileNameWithoutExtension(databaseFileNewest);
			var databaseFileDiff = Path.Combine(diffDir, $"{nameA}_versus_{nameB}.fddb");

			/*
				>	= referenced by
				<>	= self referenced by
				
				FA = flatarchive_id261653128116
				
				264139769111 = dokk elite body mesh
					> 264139769102 = FA/idx3 (pos 7508)
						> 261653128113 = FA/idx1 (pos 466)
							<> (pos 434)
							> 261653128116 = datapc64_ondemand/261653128116 (idx0) (pos 244)
							
				264139769302 = dokk elite body texture
					> 264139769298 = FA/idx104 (pos 104, 144)
						> 264139769297 = FA/idx50 (pos 12)
							> 261653127913 = FA/idx11 (pos 24)
								> 261653128115 = FA/idx2 (pos 2408)
									<> (pos 2224)
									> 261653128116 = datapc64_ondemand/261653128116 (idx0) (pos 268)
				
				FA2 = flatarchive_id261653128199
				
				264139768986 = dokk elite headgear mesh
					> 264139768982 = FA2/idx6 (pos 628)
						> 261653128201 = FA2/idx1 (pos 956)
							<> (pos 852)
							> 261653128199 = datapc64_ondemand/261653128199 (idx0) (pos 244)
							
				264139769014 = dokk elite headgear texture
					> 264139769010 = FA2/idx60 (pos 104, 144)
						> 264139769009 = FA2/idx36 (pos 12)
							> 261653127907 = FA2/idx11 (pos 24)
								> 261653128200 = FA2/idx2 (pos 1804)
									<> (pos 1576)
									> 261653128199 = datapc64_ondemand/261653128199 (idx0) (pos 268)
				
				idx0
					> 261653128199 (pos 212) -> ???
					> 261653128201 (pos 244) -> headgear mesh
					> 261653128200 (pos 268) -> headgear tex
			 */

			var searchNeedle = 4160749588u;
			// SearchAllFlatArchives(@"R:\Siege Dumps\Y6S1 v15447382\", searchNeedle);
			// SearchFlatArchives(@"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge", searchNeedle);
			// SearchBinFiles(@"R:\Siege Dumps\Unpacked\datapc64_ondemand\flatarchive_id261653128199", searchNeedle);
			SearchIndex(databaseFileNewest, searchNeedle);

			// DumpNewFiles(@"R:\Siege Dumps\Y6S1 v15447382", databaseFileDiff, @"R:\Siege Dumps\Asset Indexes\New in Y6S1");
			// CompareIndexes(databaseFileA, databaseFileB, databaseFileDiff);
			// CreateAssetIndex(databaseFileB, @"R:\Siege Dumps\Y6S1 v15447382");

			Console.WriteLine("Done");
		}

		private static void SearchIndex(string databaseFile, ulong needle)
		{
			var db = new LiteDatabase(databaseFile);

			var nameCollection = db.GetCollection<FilenameDocument>("filenames");

			foreach (var (filename, collectionName) in nameCollection.FindAll())
			{
				var collection = db.GetCollection<EntryDocument>(collectionName, BsonAutoId.Int64);
				collection.EnsureIndex(document => document.Uid);

				var result = collection.Query().Where(document => document.Uid == needle).SingleOrDefault();
				if (result != null)
					Console.WriteLine($"{filename}: {result.Uid}, type {MagicHelper.GetFiletype(result.FileType)} (0x{result.FileType:X})");
			}
		}

		private static void SearchAllFlatArchives(string dir, ulong needle)
		{
			foreach (var forgeFile in Directory.GetFiles(dir, "*.forge"))
				SearchFlatArchives(forgeFile, needle);
		}

		private static void SearchFlatArchives(string forgeFile, ulong needle)
		{
			var forgeStream = new BinaryReader(File.Open(forgeFile, FileMode.Open));

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
				if (magic != AssetType.FlatArchive)
					continue;

				var container = forge.GetContainer(entry.Uid);
				if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

				var assetStream = forgeAsset.GetDataStream(forge);

				var arc = FlatArchive.Read(assetStream);

				for (var arcEntryIdx = 0; arcEntryIdx < arc.Entries.Length; arcEntryIdx++)
				{
					var arcEntry = arc.Entries[arcEntryIdx];

					for (var pos = arcEntry.PayloadOffset; pos < arcEntry.PayloadOffset + arcEntry.PayloadLength - sizeof(ulong); pos++)
					{
						assetStream.BaseStream.Seek(pos, SeekOrigin.Begin);

						var ul = assetStream.ReadUInt64();
						if (ul != needle)
							continue;

						Console.WriteLine($"flatarchive_id{entry.Uid}/idx{arcEntryIdx}_filetype{arcEntry.Meta.Magic}: {needle:X} at pos {pos - arcEntry.PayloadOffset}");
					}
				}
			}
		}

		private static void SearchBinFiles(string rootSearchDir, ulong needle)
		{
			foreach (var directory in Directory.GetDirectories(rootSearchDir))
				SearchBinFiles(directory, needle);

			foreach (var file in Directory.GetFiles(rootSearchDir, "*.bin"))
				SearchBinFile(file, needle);
		}

		private static void SearchBinFile(string file, ulong needle)
		{
			using var br = new BinaryReader(File.Open(file, FileMode.Open));

			for (var i = 0; i < br.BaseStream.Length - sizeof(ulong); i++)
			{
				br.BaseStream.Seek(i, SeekOrigin.Begin);

				var ul = br.ReadUInt64();
				if (ul != needle)
					continue;

				Console.WriteLine($"{file}: {needle:X} at pos {br.BaseStream.Position - sizeof(ulong)}");
			}
		}

		private static void DumpNewFiles(string forgeSourceDir, string databaseDiff, string outputBaseDir)
		{
			using var diffDb = new LiteDatabase(databaseDiff);
			Console.WriteLine($"Loaded diff database {databaseDiff}");

			var addedEntryCollection = GetAddedEntryCollection(diffDb);

			var forgeCache = new Dictionary<string, Forge>();

			foreach (var grouping in addedEntryCollection.FindAll().GroupBy(source => source.Uid))
			{
				var uid = grouping.Key;
				var instances = grouping.ToArray();
				var firstInstance = instances[0];
				var src = firstInstance.Source;

				if (MagicHelper.GetFiletype(firstInstance.FileType) != AssetType.Texture || !src.StartsWith("datapc64_merged_events_bnk_texture"))
					continue;

				// if (!forgeCache.ContainsKey(src))
				// {
				// 	var forgeStream = new BinaryReader(File.Open(Path.Combine(forgeSourceDir, $"{src}.forge"), FileMode.Open));
				// 	forgeCache[src] = Forge.Read(forgeStream);
				// }

				if (forgeCache[src].Entries.All(entry1 => entry1.Uid != uid))
				{
					Console.WriteLine($"No entry matching {uid} in {src}");
					continue;
				}

				var outputDir = Path.Combine(outputBaseDir, src);
				Directory.CreateDirectory(outputDir);

				var entry = forgeCache[src].Entries.First(entry1 => entry1.Uid == uid);
				Console.Write($"Entry: UID {entry.Uid}, {MagicHelper.GetFiletype(entry.Name.FileType)} (0x{entry.Name.FileType:X}) ");

				try
				{
					DumpHelper.Dump(forgeCache[src], entry, outputDir);
					Console.WriteLine("Dumped");
				}
				catch (Exception e)
				{
					if (e.Message.Contains("GDI"))
						;
					Console.WriteLine($"Failed: {e.Message}");
				}
			}
		}

		private static ILiteCollection<EntryDocumentWithSource> GetAddedEntryCollection(LiteDatabase diffDb)
		{
			var addedEntryCollection = diffDb.GetCollection<EntryDocumentWithSource>("added", BsonAutoId.Int64);
			addedEntryCollection.EnsureIndex(document => document.Uid);
			addedEntryCollection.EnsureIndex(document => document.FileType);
			return addedEntryCollection;
		}

		private static ILiteCollection<EntryDocumentWithSource> GetRemovedEntryCollection(LiteDatabase diffDb)
		{
			var removedEntryCollection = diffDb.GetCollection<EntryDocumentWithSource>("removed", BsonAutoId.Int64);
			removedEntryCollection.EnsureIndex(document => document.Uid);
			removedEntryCollection.EnsureIndex(document => document.FileType);
			return removedEntryCollection;
		}

		private static void CompareIndexes(string databaseFileA, string databaseFileB, string databaseDiff)
		{
			using var dbA = new LiteDatabase(databaseFileA);
			using var dbB = new LiteDatabase(databaseFileB);

			Console.WriteLine("Loading entries...");

			var allEntriesA = GetAllEntries(dbA);
			Console.WriteLine($"Loaded entries from {databaseFileA}");

			var allEntriesB = GetAllEntries(dbB);
			Console.WriteLine($"Loaded entries from {databaseFileB}");

			using var diffDb = new LiteDatabase(databaseDiff);
			Console.WriteLine($"Created diff database {databaseDiff}");

			var addedEntryCollection = GetAddedEntryCollection(diffDb);
			var removedEntryCollection = GetRemovedEntryCollection(diffDb);

			Console.WriteLine("Finding added entries...");

			foreach (var (uid, instances) in allEntriesB)
			{
				if (allEntriesA.ContainsKey(uid))
					continue;

				addedEntryCollection.Insert(instances);
			}

			Console.WriteLine("Finding removed entries...");

			foreach (var (uid, instances) in allEntriesA)
			{
				if (allEntriesB.ContainsKey(uid))
					continue;

				removedEntryCollection.Insert(instances);
			}

			Console.WriteLine("Catagorizing new entries...");

			var assetTypeHistogram = new Dictionary<AssetType, int>();
			foreach (var grouping in addedEntryCollection.FindAll().GroupBy(source => source.Uid))
			{
				var uid = grouping.Key;
				var instances = grouping.ToArray();

				var assetType = MagicHelper.GetFiletype(instances[0].FileType);

				if (!assetTypeHistogram.ContainsKey(assetType))
					assetTypeHistogram[assetType] = 0;

				assetTypeHistogram[assetType]++;
			}

			Console.WriteLine("New entries:");

			foreach (var (assetType, count) in assetTypeHistogram) Console.WriteLine($"\t{assetType}: {count}");
		}

		private static Dictionary<ulong, List<EntryDocumentWithSource>> GetAllEntries(ILiteDatabase db)
		{
			var allEntries = new Dictionary<ulong, List<EntryDocumentWithSource>>();
			var nameCollection = db.GetCollection<FilenameDocument>("filenames");

			foreach (var (filename, collectionName) in nameCollection.FindAll())
			{
				var collection = db.GetCollection<EntryDocument>(collectionName, BsonAutoId.Int64);

				foreach (var entryDoc in collection.FindAll())
				{
					if (entryDoc.FileType == 0)
						continue; // Forge metadata

					if (!allEntries.ContainsKey(entryDoc.Uid))
						allEntries.Add(entryDoc.Uid, new List<EntryDocumentWithSource>());

					allEntries[entryDoc.Uid].Add(new EntryDocumentWithSource
					{
						Uid = entryDoc.Uid,
						Timestamp = entryDoc.Timestamp,
						FileType = entryDoc.FileType,
						Source = filename
					});
				}
			}

			return allEntries;
		}

		private static void CreateAssetIndex(string databaseFile, string assetDirectory)
		{
			using var db = new LiteDatabase(databaseFile);

			var nameCollection = db.GetCollection<FilenameDocument>("filenames");
			nameCollection.EnsureIndex(document => document.CollectionName);
			nameCollection.EnsureIndex(document => document.Filename);

			var files = Directory.GetFiles(assetDirectory, "*.forge");
			for (var fileIdx = 0; fileIdx < files.Length; fileIdx++)
			{
				var forgeFile = files[fileIdx];

				var forgeStream = new BinaryReader(File.Open(forgeFile, FileMode.Open));
				var forge = Forge.Read(forgeStream);

				var forgeFileName = Path.GetFileNameWithoutExtension(forgeFile);
				var forgeFileIdent = GetFilename(fileIdx);

				Console.WriteLine($"{forgeFileName} => {forgeFileIdent}");

				nameCollection.Insert(new FilenameDocument(forgeFileName, forgeFileIdent));

				var collection = db.GetCollection<EntryDocument>(forgeFileIdent, BsonAutoId.Int64);

				collection.EnsureIndex(document => document.Uid);
				collection.EnsureIndex(document => document.FileType);

				for (var entryIdx = 0; entryIdx < forge.NumEntries; entryIdx++)
				{
					var entry = forge.Entries[entryIdx];

					collection.Insert(EntryDocument.For(entry));
				}
			}
		}
	}
}