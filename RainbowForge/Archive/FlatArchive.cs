using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Archive
{
	public class FlatArchive
	{
		public FlatArchiveEntry[] Entries { get; }

		private FlatArchive(FlatArchiveEntry[] entries)
		{
			Entries = entries;
		}

		public static FlatArchive Read(BinaryReader r)
		{
			var entries = new List<FlatArchiveEntry>();

			while (r.BaseStream.Position != r.BaseStream.Length)
				entries.Add(FlatArchiveEntry.Read(r));

			return new FlatArchive(entries.ToArray());
		}
	}
}