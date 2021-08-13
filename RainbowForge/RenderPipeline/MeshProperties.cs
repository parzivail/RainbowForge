using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MeshProperties
	{
		public uint Var1 { get; }
		public ulong InternalUid { get; }
		public byte[] Data { get; }
		public MeshProperty[] Props { get; }
		public uint Magic2 { get; }
		public uint Var2 { get; }
		public ulong MeshUid { get; }
		public ulong[] MaterialContainers { get; }
		public ulong InternalUid2 { get; }
		public byte[] Data2 { get; }

		private MeshProperties(uint var1, ulong internalUid, byte[] data, MeshProperty[] props, uint magic2, uint var2, ulong meshUid, ulong[] materialContainers, ulong internalUid2, byte[] data2)
		{
			Var1 = var1;
			InternalUid = internalUid;
			Data = data;
			Props = props;
			Magic2 = magic2;
			Var2 = var2;
			MeshUid = meshUid;
			MaterialContainers = materialContainers;
			InternalUid2 = internalUid2;
			Data2 = data2;
		}

		public static MeshProperties Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.Mesh, magic);

			var var1 = r.ReadUInt32();

			var internalUid = r.ReadUInt64();

			var data = r.ReadBytes(32);

			var numProperties = r.ReadUInt32();
			var props = new MeshProperty[numProperties];
			for (var i = 0; i < props.Length; i++)
				props[i] = MeshProperty.Read(r);

			var magic2 = r.ReadUInt32();
			var var2 = r.ReadUInt32();

			var meshUid = r.ReadUInt64();

			var numMaterialContainers = r.ReadUInt32();
			var materialContainers = new ulong[numMaterialContainers];
			for (var i = 0; i < materialContainers.Length; i++)
				materialContainers[i] = r.ReadUInt64();

			var internalUid2 = r.ReadUInt64();
			var data2 = r.ReadBytes(25);

			return new MeshProperties(var1, internalUid, data, props, magic2, var2, meshUid, materialContainers, internalUid2, data2);
		}
	}

	public class MeshProperty
	{
		public uint Magic { get; }
		public byte[] Data { get; }

		private MeshProperty(uint magic, byte[] data)
		{
			Magic = magic;
			Data = data;
		}

		public static MeshProperty Read(BinaryReader r)
		{
			var padding = r.ReadBytes(8); // padding?
			var magic = r.ReadUInt32();

			var data = r.ReadBytes(68);

			return new MeshProperty(magic, data);
		}
	}
}