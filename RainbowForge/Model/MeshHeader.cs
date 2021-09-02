using System.IO;

namespace RainbowForge.Model
{
	public class MeshHeader
	{
		public FileMetaData MetaData { get; }
		
		public uint Revision { get; }
		public int MeshType { get; }
		public uint NumVerts { get; }
		public uint VertLen { get; }
		public uint NumLods { get; }
		public MeshObjectHeader[] ObjectHeaders { get; }
		public BoundingBox[] ObjectBoundingBoxes { get; }
		public MeshObjectSkinMapping[] ObjectSkinMappings { get; }
		public long VertBlockOffset { get; }
		public long TrisBlockStart { get; }
		public uint VertsDataLen { get; }
		public uint VertmapsDataLen { get; }
		public long ExtraDataStart { get; }
		public uint FaceDataLen { get; }
		public uint UnknownBlock1Len { get; }
		public uint FaceUnknownDataLen { get; }

		public MeshHeader(FileMetaData metaData, uint revision, int meshType, uint numVerts, uint vertLen, uint numLods, MeshObjectHeader[] objectHeaders, BoundingBox[] objectBoundingBoxes,
			MeshObjectSkinMapping[] objectSkinMappings, long vertBlockOffset, long trisBlockStart, uint vertsDataLen, uint vertmapsDataLen, long extraDataStart, uint faceDataLen,
			uint unknownBlock1Len, uint faceUnknownDataLen)
		{
			MetaData = metaData;
			Revision = revision;
			MeshType = meshType;
			NumVerts = numVerts;
			VertLen = vertLen;
			NumLods = numLods;
			ObjectHeaders = objectHeaders;
			ObjectBoundingBoxes = objectBoundingBoxes;
			ObjectSkinMappings = objectSkinMappings;
			VertBlockOffset = vertBlockOffset;
			TrisBlockStart = trisBlockStart;
			VertsDataLen = vertsDataLen;
			VertmapsDataLen = vertmapsDataLen;
			ExtraDataStart = extraDataStart;
			FaceDataLen = faceDataLen;
			UnknownBlock1Len = unknownBlock1Len;
			FaceUnknownDataLen = faceUnknownDataLen;
		}

		public static MeshHeader Read(BinaryReader r, uint version)
		{
			// file header (0x5C - zeroes till verts)
			var metaHeader = FileMetaData.Read(r, version);

			var secondMagic = r.ReadUInt32();
			var var2 = r.ReadUInt32();
			var var3 = r.ReadUInt32();

			// model header
			var innerModelStructMagic = r.ReadUInt32(); // [-0x8], inner model struct type
			MagicHelper.AssertEquals(Magic.CompiledMesh, innerModelStructMagic);

			var sizeUntilFooter = r.ReadUInt32(); // [-0x4]
			var dataStart = r.BaseStream.Position; // inner model zero byte

			var x00 = r.ReadUInt32(); // [0x0] = 0x14
			var revision = r.ReadUInt32(); // [0x4] = 0, 1, 2
			var vertLen = r.ReadUInt32(); // [0x8] how much bytes are allocated

			// per vertex
			if (!IsKnownVertexLength(vertLen))
				throw new InvalidDataException($"Unknown vertLen 0x{vertLen:X}");

			var vertsDataLen = r.ReadUInt32(); // [0x0C]
			var faceDataLen = r.ReadUInt32(); // [0x10]

			// these hold lengths of data blocks following immediately after trisblock
			var vertmapsDataLen = r.ReadUInt32(); // [0x14]  num_verts*12

			var unknownBlock1Len = r.ReadUInt32(); // [0x18] unreversed data
			var faceStatDataLen = r.ReadUInt32(); // [0x1C] size of array that

			// contains 11 floats per each tris chunk
			var faceUnknownDataLen = r.ReadUInt32(); // [0x20]  size of array that

			// contains packed 4-byte value per each triangle (including invalid tris)
			var x24 = r.ReadUInt32(); // [0x24] = 0

			var x28 = r.ReadUInt32(); // [0x28] = 0
			var x2C = r.ReadUInt32(); // [0x2C] = 1(animated/interactive?),

			var numLods = r.ReadUInt32(); // 2(map props?), 8(some bosses, some hands), 9, 10(some weapons)

			var meshType = r.ReadInt32(); // flags?.. = -1(bboxes???), 2 (assets?), 269 (weapons/gadgets)
			var numIslands = r.ReadUInt32(); // [0x38]

			var x3C = r.ReadUInt32(); // [0x3C] = 0
			var x40 = r.ReadSingle(); // float
			var x44 = r.ReadSingle(); // float

			// EXAMINE
			var rng3Len = r.ReadUInt32(); // [0x48] len from vertblock till valuable

			// data end (till end of floats section)

			var numVerts = vertsDataLen / vertLen;
			var vertBlockOffset = r.BaseStream.Position;
			var trisBlockOffset = vertBlockOffset + vertsDataLen;
			var extraDataOffset = trisBlockOffset + faceDataLen;

			var tailOffset = extraDataOffset + vertmapsDataLen + unknownBlock1Len + faceStatDataLen + faceUnknownDataLen;

			r.BaseStream.Seek(tailOffset, SeekOrigin.Begin);

			var islandMetas = new MeshObjectHeader[numIslands * numLods];
			for (var i = 0; i < islandMetas.Length; i++)
				islandMetas[i] = MeshObjectHeader.Read(r);

			var islandBboxes = new BoundingBox[numIslands];
			for (var i = 0; i < islandBboxes.Length; i++)
				islandBboxes[i] = BoundingBox.Read(r);

			var islandSkinMapping = new MeshObjectSkinMapping[numIslands];
			for (var i = 0; i < islandSkinMapping.Length; i++)
				islandSkinMapping[i] = MeshObjectSkinMapping.Read(r);

			return new MeshHeader(metaHeader, revision, meshType, numVerts, vertLen, numLods, islandMetas, islandBboxes, islandSkinMapping, vertBlockOffset, trisBlockOffset, vertsDataLen,
				vertmapsDataLen, extraDataOffset, faceDataLen, unknownBlock1Len, faceUnknownDataLen);
		}

		private static bool IsKnownVertexLength(uint vertLen)
		{
			return vertLen switch
			{
				0x18 => true, // 24 bytes
				0x1C => true, // 28 bytes
				0x24 => true, // 36 bytes
				0x28 => true, // 40 bytes
				0x2C => true, // 44 bytes
				0x5C => true, // 92 bytes
				_ => false
			};
		}
	}
}