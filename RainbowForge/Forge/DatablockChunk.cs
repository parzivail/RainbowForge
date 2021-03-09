using System.IO;

namespace RainbowForge.Forge
{
	/// <summary>
	///     Describes where to find particular data chunk, whether
	///     it is compressed/raw and where to find it within data
	///     stream. It's serialized form is split in parts. First
	///     we have ``[unpacked, packed]`` for each chunk, then we
	///     have ``[hash, data]`` for each chunk. ``offset`` is not
	///     a serialized variable, it's added merely for reverse
	///     engineering ease.
	/// </summary>
	public class DatablockChunk
	{
		public uint DecompressedLength { get; }
		public uint OnDiskLength { get; }
		public uint Hash { get; private set; }
		public long Offset { get; private set; }

		public bool IsPacked => DecompressedLength > OnDiskLength;

		private DatablockChunk(uint decompressedLength, uint onDiskLength)
		{
			DecompressedLength = decompressedLength;
			OnDiskLength = onDiskLength;
		}

		public void Finalize(BinaryReader r)
		{
			Hash = r.ReadUInt32();
			Offset = r.BaseStream.Position;
		}

		public static DatablockChunk Read(BinaryReader r)
		{
			var unpacked = r.ReadUInt32();
			var packed = r.ReadUInt32();

			return new DatablockChunk(unpacked, packed);
		}
	}
}