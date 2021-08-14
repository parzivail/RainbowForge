using System.IO;

namespace RainbowForge.Model
{
	/// <summary>
	///     This is just a guess based on structure. It might turn out being something else.
	/// </summary>
	public class MeshObjectSkinMapping
	{
		private const int Size = 0x10C;

		public byte BonesUsed { get; }
		public byte MatId { get; }
		public ushort VertBufLen { get; }
		public byte[] Indices { get; }

		public MeshObjectSkinMapping(byte bonesUsed, byte matId, ushort vertBufLen, byte[] indices)
		{
			BonesUsed = bonesUsed;
			MatId = matId;
			VertBufLen = vertBufLen;
			Indices = indices;
		}

		public static MeshObjectSkinMapping Read(BinaryReader r)
		{
			var x00 = r.ReadUInt16();
			var bonesUsed = r.ReadByte();
			var matId = r.ReadByte();
			var x04 = r.ReadUInt16();
			var vertBufLen = r.ReadUInt16();
			var lenIndices = r.ReadByte();
			var indices = r.ReadBytes(lenIndices);

			if (lenIndices > 0)
			{
				r.BaseStream.Seek(Size - 2 - 4 - 2 - 1 - lenIndices - 4, SeekOrigin.Current);
				var x108 = r.ReadUInt32();
			}

			return new MeshObjectSkinMapping(bonesUsed, matId, vertBufLen, indices);
		}
	}
}