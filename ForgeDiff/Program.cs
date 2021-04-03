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

namespace ForgeDiff
{
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

			var searchNeedle = 22439849214u;
			// PrintRawReferences(SearchAllFlatArchives(@"R:\Siege Dumps\Y6S1 v15447382\", searchNeedle));
			PrintRawReferences(SearchFlatArchives(@"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge", searchNeedle));
			// SearchBinFiles(@"R:\Siege Dumps\Unpacked\datapc64_ondemand\", searchNeedle);
			// SearchIndex(databaseFileNewest, searchNeedle);

			var filterUids = new ulong[]
			{
			};

			// var refs = new Dictionary<ArchiveReference, List<UidReference>>();
			// foreach (var filterUid in filterUids) BuildReferenceList(@"R:\Siege Dumps\Y6S1 v15447382\datapc64_ondemand.forge", filterUid, refs, 261653128116);
			//
			// PrintReferenceTree(refs, 261653128116);

			// DumpNewFiles(@"R:\Siege Dumps\Y6S1 v15447382", databaseFileDiff, @"R:\Siege Dumps\Asset Indexes\New in Y6S1");
			// CompareIndexes(databaseFileA, databaseFileB, databaseFileDiff);
			// CreateAssetIndex(databaseFileB, @"R:\Siege Dumps\Y6S1 v15447382");

			Console.WriteLine("Done");
		}

		private static void PrintRawReferences(Dictionary<ArchiveReference, List<UidReference>> refs)
		{
			foreach (var (arcRef, uidRefs) in refs)
			{
				Console.WriteLine($"{arcRef.ArchiveEntryUid} = flatarchive_{arcRef.FlatArchiveUid}/idx{arcRef.ArchiveEntryIdx}");

				foreach (var (referencedUid, pos) in uidRefs)
					Console.WriteLine($"\t{referencedUid} (pos {pos})");
			}
		}

		private static void PrintReferenceTree(Dictionary<ArchiveReference, List<UidReference>> refs, ulong root, long localPos = 0, int indentLevel = 0)
		{
			var archiveRef = refs.First(pair => pair.Key.ArchiveEntryUid == root); // there should only be one
			Console.WriteLine($"{IndentString(indentLevel)}> {archiveRef.Key.ArchiveEntryUid} = FA/idx{archiveRef.Key.ArchiveEntryIdx} (pos {localPos})");

			foreach (var (refUid, pos) in archiveRef.Value)
			{
				if (refUid == root)
					continue;

				if (refs.Any(pair => pair.Key.ArchiveEntryUid == refUid))
					PrintReferenceTree(refs, refUid, pos, indentLevel + 1);
				else
					Console.WriteLine($"{IndentString(indentLevel + 1)}> {refUid} = asset (pos {pos})");
			}
		}

		private static string IndentString(int level)
		{
			return new('\t', level);
		}

		private static void BuildReferenceList(string forgeFile, ulong needle, Dictionary<ArchiveReference, List<UidReference>> refs, ulong archiveUid = 0)
		{
			Console.Write($"{needle}: ");
			var foundRefs = SearchFlatArchives(forgeFile, needle, archiveUid);
			Console.WriteLine($"{foundRefs.Count} references");

			foreach (var refArchiveUid in foundRefs.Keys)
			{
				if (refs.ContainsKey(refArchiveUid))
					refs[refArchiveUid].AddRange(foundRefs[refArchiveUid].Where(r => !refs[refArchiveUid].Contains(r)));
				else
					refs[refArchiveUid] = foundRefs[refArchiveUid];

				if (refArchiveUid.ArchiveEntryUid == needle)
					continue;

				BuildReferenceList(forgeFile, refArchiveUid.ArchiveEntryUid, refs, archiveUid);
			}
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

		private static Dictionary<ArchiveReference, List<UidReference>> SearchAllFlatArchives(string dir, ulong needle)
		{
			var deps = new Dictionary<ArchiveReference, List<UidReference>>();

			foreach (var forgeFile in Directory.GetFiles(dir, "*.forge"))
			{
				var refs = SearchFlatArchives(forgeFile, needle);

				foreach (var archiveUid in refs.Keys)
					if (deps.ContainsKey(archiveUid))
						deps[archiveUid].AddRange(refs[archiveUid].Where(r => !deps[archiveUid].Contains(r)));
					else
						deps[archiveUid] = refs[archiveUid];
			}

			return deps;
		}

		private static Dictionary<ArchiveReference, List<UidReference>> SearchFlatArchives(string forgeFile, ulong needle, ulong archiveUid = 0)
		{
			var deps = new Dictionary<ArchiveReference, List<UidReference>>();

			using var forgeStream = new BinaryReader(File.Open(forgeFile, FileMode.Open));

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

				if (archiveUid != 0 && entry.Uid != archiveUid)
					continue;

				var container = forge.GetContainer(entry.Uid);
				if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

				var assetStream = forgeAsset.GetDataStream(forge);

				var arc = FlatArchive.Read(assetStream);

				for (var arcEntryIdx = 0; arcEntryIdx < arc.Entries.Length; arcEntryIdx++)
				{
					var arcEntry = arc.Entries[arcEntryIdx];

					var archiveRef = new ArchiveReference(entry.Uid, arcEntry.Meta.Uid, arcEntryIdx);

					if (arcEntry.Meta.Uid == needle)
					{
						if (!deps.ContainsKey(archiveRef))
							deps[archiveRef] = new List<UidReference>();

						deps[archiveRef].Add(new UidReference(needle, -1));
					}

					for (var pos = arcEntry.PayloadOffset; pos <= arcEntry.PayloadOffset + arcEntry.PayloadLength - sizeof(ulong); pos++)
					{
						assetStream.BaseStream.Seek(pos, SeekOrigin.Begin);

						var ul = assetStream.ReadUInt64();
						if (ul != needle)
							continue;

						if (!deps.ContainsKey(archiveRef))
							deps[archiveRef] = new List<UidReference>();

						deps[archiveRef].Add(new UidReference(needle, pos - arcEntry.PayloadOffset));
					}
				}
			}

			return deps;
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

				using var forgeStream = new BinaryReader(File.Open(forgeFile, FileMode.Open));
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

		private class TreeNode<T>
		{
			public T Value { get; }
			public List<TreeNode<T>> Children { get; }

			public TreeNode(T value)
			{
				Value = value;
				Children = new List<TreeNode<T>>();
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return Value.ToString();
			}
		}

		public record ArchiveReference(ulong FlatArchiveUid, ulong ArchiveEntryUid, int ArchiveEntryIdx);

		public record UidReference(ulong ReferencedUid, long Pos);
	}
}