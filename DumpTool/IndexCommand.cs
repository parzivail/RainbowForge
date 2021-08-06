using System;
using System.IO;
using System.Linq;
using CommandLine;
using LiteDB;
using RainbowForge;
using RainbowForge.Database;
using RainbowForge.Forge;

namespace DumpTool
{
	[Verb("index", HelpText = "Create a search index of all of the forge files in a given directory. Required for some commands.")]
	public class IndexCommand
	{
		private static readonly char[] Characters = Enumerable.Range(0, 26).Select(i => (char) ('a' + i)).ToArray();

		[Value(0, HelpText = "The directory of forge files to search")]
		public string SearchDirectory { get; set; }

		[Value(1, HelpText = "The output index file")]
		public string IndexFilename { get; set; }

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

		public static void Run(IndexCommand args)
		{
			FileSystemUtil.AssertDirectoryExists(args.SearchDirectory);

			using var db = new LiteDatabase(args.IndexFilename);

			var nameCollection = db.GetCollection<FilenameDocument>("filenames");
			nameCollection.EnsureIndex(document => document.CollectionName);
			nameCollection.EnsureIndex(document => document.Filename);

			var files = Directory.GetFiles(args.SearchDirectory, "*.forge");
			for (var fileIdx = 0; fileIdx < files.Length; fileIdx++)
			{
				var forgeFile = files[fileIdx];

				using var forgeStream = new BinaryReader(File.Open(forgeFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
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