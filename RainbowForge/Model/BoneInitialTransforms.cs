using System.IO;
using RainbowForge.Structs;

namespace RainbowForge.Model
{
	public class BoneInitialTransforms
	{
		public Matrix4F Transformation { get; private set; }

		private BoneInitialTransforms(Matrix4F transformation)
		{
			Transformation = transformation;
		}

		public static BoneInitialTransforms Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.BoneInitialTransforms, magic);

			var transformation = r.ReadStruct<Matrix4F>(Matrix4F.SizeInBytes);

			return new BoneInitialTransforms(transformation);
		}
	}
}