using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using LiteDB;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Database;
using RainbowForge.Dump;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;

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
			Program.AssertFileExists(args.IndexFilename);
			var forge = Program.GetForge(args.ForgeFilename);

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
			var arc = FlatArchive.Read(assetStream);

			if (arc.Entries.All(archiveEntry => !MagicHelper.Equals(Magic.MeshProperties, archiveEntry.Meta.Magic)))
			{
				Console.Error.WriteLine("No MeshProperties containers found");
				return;
			}

			foreach (var meshProp in arc.Entries)
			{
				var unresolvedExterns = new List<ulong>();

				var outputDir = Path.Combine(rootOutputDir, $"model_flatarchive_id{entry.Uid}", $"{(Magic) meshProp.Meta.Magic}_{meshProp.Meta.Uid}");
				Directory.CreateDirectory(outputDir);

				try
				{
					DumpHelper.DumpNonContainerChildren(outputDir, assetStream, arc, meshProp, unresolvedExterns);
				}
				catch
				{
					continue;
				}

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

				foreach (var resolvedForgeFile in resolvedExterns.Keys)
				{
					var filename = Path.Combine(rootForgeDir, resolvedForgeFile + ".forge");
					using var resolvedForgeStream = new BinaryReader(File.Open(filename, FileMode.Open));
					var resolvedForge = Forge.Read(resolvedForgeStream);

					foreach (var resolvedUid in resolvedExterns[resolvedForgeFile])
					{
						var resolvedEntry = resolvedForge.Entries.First(entry1 => entry1.Uid == resolvedUid);
						DumpHelper.Dump(resolvedForge, resolvedEntry, outputDir);

						Console.WriteLine($"Dumped {resolvedForgeFile}/{resolvedUid}");
					}
				}
			}
		}
	}
}