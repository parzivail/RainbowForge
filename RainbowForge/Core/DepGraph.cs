using System.IO;
using System.Runtime.InteropServices;
using RainbowForge.Core.DataBlock;

namespace RainbowForge.Core
{
	public class DepGraph
	{
		public DepGraphEntry[] Structs { get; private set; }

		private DepGraph(DepGraphEntry[] structs)
		{
			Structs = structs;
		}

		public static DepGraph Read(BinaryReader r)
		{
			var containerMagic = (ContainerMagic)r.ReadUInt32();
			if (containerMagic != ContainerMagic.File)
				throw new InvalidDataException($"Expected container magic, found 0x{containerMagic:X8}");

			var fileContainerMagic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FileContainer, fileContainerMagic);

			var x = r.ReadUInt16(); // 2 (changed to 3 in Y5) <-- container deserializer type
			var assetDeserializerType = r.ReadUInt16(); // 3 for chunked, 7 for linear (flags?)
			var y = r.ReadByte(); // 0
			var z = r.ReadUInt16();

			if (assetDeserializerType != 3)
				throw new InvalidDataException($"Expected chunked asset deserializer magic, found 0x{assetDeserializerType:X4}");

			var dataBlock = ChunkedDataBlock.Read(r);
			var stream = dataBlock.GetDataStream(r);

			using var depGraphStream = new BinaryReader(stream);
			var x00 = depGraphStream.ReadByte(); // == 2

			var numStructs = stream.Length / DepGraphEntry.SizeInBytes;

			var structs = new DepGraphEntry[numStructs];

			for (var i = 0; i < numStructs; i++)
				structs[i] = depGraphStream.ReadStruct<DepGraphEntry>(DepGraphEntry.SizeInBytes);

			return new DepGraph(structs);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct DepGraphEntry
	{
		public const int SizeInBytes = 24;

		[FieldOffset(0)] public ulong ParentUid;
		[FieldOffset(8)] public ulong ChildUid;
		[FieldOffset(16)] public uint ChildType;
		[FieldOffset(20)] public ushort Unk1;
		[FieldOffset(22)] public byte Unk2;
		[FieldOffset(23)] public byte Unk3;
	}
}