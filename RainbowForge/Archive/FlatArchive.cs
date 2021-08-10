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

			var index = 0;
			while (r.BaseStream.Position != r.BaseStream.Length)
				entries.Add(FlatArchiveEntry.Read(r, index++));

			if (r.BaseStream.Length != r.BaseStream.Position)
				throw new InvalidDataException();

			return new FlatArchive(entries.ToArray());
		}
	}
}