using RainbowForge.Forge;

namespace RainbowForge.Database
{
	public class EntryDocument
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
}