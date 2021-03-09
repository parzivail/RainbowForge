using System.IO;

namespace RainbowForge.Forge
{
	public class Datablock
	{
		public DatablockChunk[] Chunks { get; }
		public bool IsPacked { get; }
		public uint PackedLength { get; }
		public uint UnpackedLength { get; }

		public Datablock(DatablockChunk[] chunks, bool isPacked, uint packedLength, uint unpackedLength)
		{
			Chunks = chunks;
			IsPacked = isPacked;
			PackedLength = packedLength;
			UnpackedLength = unpackedLength;
		}

		public static Datablock Read(BinaryReader r)
		{
			r.ReadUInt16(); // [0x8] = 2 (changed to 3 in Y5) <-- container deserializer type
			r.ReadUInt16(); // [0xA] = 3
			r.ReadByte(); // [0xC] = 0

			var xd = r.ReadUInt16();
			var numChunks = r.ReadUInt32();

			var packedLength = 0u;
			var unpackedLength = 0u;
			var isPacked = false;

			var chunks = new DatablockChunk[numChunks];

			for (var i = 0; i < numChunks; i++)
			{
				var chunk = DatablockChunk.Read(r);

				packedLength += chunk.OnDiskLength;
				unpackedLength += chunk.DecompressedLength;

				isPacked |= chunk.IsPacked;

				chunks[i] = chunk;
			}

			for (var i = 0; i < numChunks; i++)
			{
				chunks[i].Finalize(r);
				r.BaseStream.Seek(chunks[i].OnDiskLength, SeekOrigin.Current);
			}

			return new Datablock(chunks, isPacked, packedLength, unpackedLength);
		}
	}
}