using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using RainbowForge;
using RainbowForge.Forge;

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

			const string databaseFileA = @"R:\Siege Dumps\Asset Indexes\v15302504_y5s4.fidb";
			const string databaseFileB = @"R:\Siege Dumps\Asset Indexes\v15439724_y6s1_tts.fidb";
			const string assetDirectory = @"R:\Siege Dumps\Y5S4 v15302504";

			CompareIndexes(databaseFileA, databaseFileB);

			// CreateAssetIndex(args[1], args[0]);
		}

		private static void CompareIndexes(string databaseFileA, string databaseFileB)
		{
			using var dbA = new LiteDatabase(databaseFileA);
			using var dbB = new LiteDatabase(databaseFileB);

			Console.WriteLine("Loading entries...");

			var allEntriesA = GetAllEntries(dbA);
			Console.WriteLine($"Loaded entries from {databaseFileA}");

			var allEntriesB = GetAllEntries(dbB);
			Console.WriteLine($"Loaded entries from {databaseFileB}");

			var diffDir = Path.GetDirectoryName(databaseFileA);
			var nameA = Path.GetFileNameWithoutExtension(databaseFileA);
			var nameB = Path.GetFileNameWithoutExtension(databaseFileB);
			var diffDbPath = Path.Combine(diffDir, $"{nameA}_versus_{nameB}.fddb");

			using var diffDb = new LiteDatabase(diffDbPath);
			Console.WriteLine("Created diff database {diffDbPath}");

			var addedEntryCollection = diffDb.GetCollection<EntryDocumentWithSource>("added", BsonAutoId.Int64);
			addedEntryCollection.EnsureIndex(document => document.Uid);
			addedEntryCollection.EnsureIndex(document => document.FileType);

			var removedEntryCollection = diffDb.GetCollection<EntryDocumentWithSource>("removed", BsonAutoId.Int64);
			removedEntryCollection.EnsureIndex(document => document.Uid);
			removedEntryCollection.EnsureIndex(document => document.FileType);

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