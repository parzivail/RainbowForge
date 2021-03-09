using System.IO;

namespace RainbowForge.Mesh
{
	/// <summary>
	///     Holds data needed to construct a given island.
	///     What a tris_chunk is:
	///     triangles are stored in chunks. Each chunk is 0x180 bytes long.
	///     If given chunk is the last one in island ant is not filled till the
	///     end, it gets filled with last vert's id, forming invalid triangles.
	///     Example of an end of such chunk:
	///     ...: ...
	///     0x160: 0F AB 1C AB 1B AB 1A AB 1B AB 1f AB 10 AB 11 AB
	///     0x170: 12 AB 11 AB|11 AB 11 AB 11 AB 11 AB 11 AB 11 AB
	///     ^    |^
	///     last valid tri's id|buffer filled with last id
	/// </summary>
	public class MeshObjectHeader
	{
		public uint OffsetVerts { get; }
		public uint NumVerts { get; }
		public uint OffsetFaceChunks { get; }
		public uint NumFaceChunks { get; }
		public uint MatId { get; }

		public MeshObjectHeader(uint offsetVerts, uint numVerts, uint offsetFaceChunks, uint numFaceChunks, uint matId)
		{
			OffsetVerts = offsetVerts;
			NumVerts = numVerts;
			OffsetFaceChunks = offsetFaceChunks;
			NumFaceChunks = numFaceChunks;
			MatId = matId;
		}

		public static MeshObjectHeader Read(BinaryReader r)
		{
			var x00 = r.ReadUInt32();
			var offsetVerts = r.ReadUInt32();
			var numVerts = r.ReadUInt32();
			var offsetFaceChunks = r.ReadUInt32(); // tris chunks offset
			var numFaceChunks = r.ReadUInt32(); // tris chunks num per this island
			var matId = r.ReadUInt32();
			var x18 = r.ReadUInt32();
			var x1C = r.ReadUInt32();
			var x20 = r.ReadUInt32();

			return new MeshObjectHeader(offsetVerts, numVerts, offsetFaceChunks, numFaceChunks, matId);
		}
	}
}