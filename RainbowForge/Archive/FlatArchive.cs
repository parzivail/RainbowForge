using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public BinaryReader GetEntryStream(Stream archiveStream, ulong entryUid)
		{
			var entry = Entries.FirstOrDefault(entry => entry.Meta.Uid == entryUid);
			if (entry == null)
				return null;

			var ms = new MemoryStream();
			archiveStream.Seek(entry.PayloadOffset, SeekOrigin.Begin);
			archiveStream.CopyStream(ms, entry.PayloadLength);
			ms.Seek(0, SeekOrigin.Begin);
			return new BinaryReader(ms);
		}
	}
}