using System.IO;
using RainbowForge.Model;
using RainbowForge.Structs;

namespace RainbowForge.RenderPipeline
{
	public class Mesh
	{
		public uint Var1 { get; }
		public ulong InternalUid { get; }
		public byte[] Data { get; }
		public MeshBone[] Bones { get; }
		public uint Magic2 { get; }
		public uint Var2 { get; }
		public ulong CompiledMeshObjectUid { get; }
		public ulong[] Materials { get; }
		public ulong InternalUid2 { get; }
		public byte[] Data2 { get; }

		private Mesh(uint var1, ulong internalUid, byte[] data, MeshBone[] bones, uint magic2, uint var2, ulong compiledMeshObjectUid, ulong[] materials, ulong internalUid2, byte[] data2)
		{
			Var1 = var1;
			InternalUid = internalUid;
			Data = data;
			Bones = bones;
			Magic2 = magic2;
			Var2 = var2;
			CompiledMeshObjectUid = compiledMeshObjectUid;
			Materials = materials;
			InternalUid2 = internalUid2;
			Data2 = data2;
		}

		public static Mesh Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.Mesh, magic);

			var var1 = r.ReadUInt32();

			var internalUid = r.ReadUInt64();

			var data = r.ReadBytes(32);

			var numBones = r.ReadUInt32();
			var bones = new MeshBone[numBones];
			for (var i = 0; i < bones.Length; i++)
				bones[i] = MeshBone.Read(r);

			var magic2 = r.ReadUInt32();
			var var2 = r.ReadUInt32();

			var meshUid = r.ReadUInt64();

			var numMaterialContainers = r.ReadUInt32();
			var materialContainers = new ulong[numMaterialContainers];
			for (var i = 0; i < materialContainers.Length; i++)
				materialContainers[i] = r.ReadUInt64();

			var internalUid2 = r.ReadUInt64();
			var data2 = r.ReadBytes(25);

			return new Mesh(var1, internalUid, data, bones, magic2, var2, meshUid, materialContainers, internalUid2, data2);
		}
	}

	public class MeshBone
	{
		public Matrix4F Transformation { get; }
		public BoneId Id { get; }

		private MeshBone(Matrix4F transformation, BoneId id)
		{
			Transformation = transformation;
			Id = id;
		}

		public static MeshBone Read(BinaryReader r)
		{
			var padding = r.ReadBytes(8); // padding?
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.MeshBone, magic);

			var transformation = r.ReadStruct<Matrix4F>(Matrix4F.SizeInBytes);
			var id = (BoneId)r.ReadUInt32();

			return new MeshBone(transformation, id);
		}
	}
}