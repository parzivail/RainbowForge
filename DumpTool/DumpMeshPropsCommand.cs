using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using LiteDB;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Database;
using RainbowForge.Dump;

namespace DumpTool
{
	[Verb("dumpmeshprops", HelpText = "Dumps all MeshProperties containers in the given flat archive")]
	public class DumpMeshPropsCommand
	{
		[Value(0, HelpText = "The search index to use (see command: index)")]
		public string IndexFilename { get; set; }

		[Value(1, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		[Value(2, HelpText = "The archive UID to dump")]
		public ulong Uid { get; set; }

		public static void Run(DumpMeshPropsCommand args)
		{
			FileSystemUtil.AssertFileExists(args.IndexFilename);
			var forge = Forge.GetForge(args.ForgeFilename);

			try
			{
				var entry = forge.Entries.First(entry1 => entry1.Uid == args.Uid);

				var db = new LiteDatabase(args.IndexFilename);
				ProcessFlatArchive(db, forge, entry, Environment.CurrentDirectory, Path.GetDirectoryName(args.ForgeFilename));
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while dumping: {e}");
			}
		}

		public static void ProcessFlatArchive(ILiteDatabase db, Forge forge, Entry entry, string rootOutputDir, string rootForgeDir)
		{
			var container = forge.GetContainer(entry.Uid);
			if (container is not ForgeAsset forgeAsset) return;

			var assetStream = forgeAsset.GetDataStream(forge);
			var arc = FlatArchive.Read(assetStream, forge.Version);

			if (arc.Entries.All(archiveEntry => !MagicHelper.Equals(Magic.Mesh, archiveEntry.MetaData.FileType)))
			{
				Console.Error.WriteLine("No MeshProperties containers found");
				return;
			}

			foreach (var meshProp in arc.Entries)
			{
				var unresolvedExterns = new List<KeyValuePair<string, ulong>>();

				var outputDir = Path.Combine(rootOutputDir, $"model_flatarchive_id{entry.Uid}", $"{(Magic)meshProp.MetaData.FileType}_{meshProp.MetaData.Uid}");

				try
				{
					DumpHelper.DumpNonContainerChildren(outputDir, assetStream, arc, meshProp, unresolvedExterns);
				}
				catch
				{
					continue;
				}

				var resolvedExterns = new Dictionary<string, List<KeyValuePair<string, ulong>>>();

				var nameCollection = db.GetCollection<FilenameDocument>("filenames");

				foreach (var unresolvedExtern in unresolvedExterns)
				{
					var found = false;
					foreach (var (filename, collectionName) in nameCollection.FindAll())
					{
						var collection = db.GetCollection<EntryDocument>(collectionName, BsonAutoId.Int64);
						collection.EnsureIndex(document => document.Uid);

						if (!collection.Query().Where(document => document.Uid == unresolvedExtern.Value).Exists()) continue;

						if (!resolvedExterns.ContainsKey(filename))
							resolvedExterns[filename] = new List<KeyValuePair<string, ulong>>();

						found = true;
						resolvedExterns[filename].Add(unresolvedExtern);
						Console.WriteLine($"Resolved external reference {filename} => UID {unresolvedExtern.Value}");
					}

					if (!found)
						Console.WriteLine($"Unresolved external reference to UID {unresolvedExtern.Value}");
				}

				foreach (var resolvedForgeFile in resolvedExterns.Keys)
				{
					var filename = Path.Combine(rootForgeDir, resolvedForgeFile + ".forge");
					using var resolvedForgeStream = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
					var resolvedForge = Forge.Read(resolvedForgeStream);

					foreach (var resolvedUid in resolvedExterns[resolvedForgeFile])
					{
						var resolvedEntry = resolvedForge.Entries.First(entry1 => entry1.Uid == resolvedUid.Value);

						DumpHelper.Dump(resolvedForge, resolvedEntry, Path.Combine(outputDir, resolvedUid.Key));

						Console.WriteLine($"Dumped {resolvedForgeFile}/{resolvedUid.Value}");
					}
				}
			}
		}
	}
}