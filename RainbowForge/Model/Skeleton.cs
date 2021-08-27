using System.IO;
using RainbowForge.Structs;

namespace RainbowForge.Model
{
	public class Skeleton
	{
		public static Skeleton Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.Skeleton, magic);

			var skeletonMirrorData = SkeletonMirrorData.Read(r);

			var numBones = r.ReadInt32();

			var uid = r.ReadUInt64();

			var unk1 = r.ReadByte();

			var bones = new Bone[numBones];
			for (var i = 0; i < numBones; i++)
			{
				bones[i] = Bone.Read(r);
			}

			return null;
		}
	}

	public class Bone
	{
		public static Bone Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.Bone, magic);

			var unk2 = r.ReadInt32();
			var unk3 = r.ReadInt16();

			var internalUid = r.ReadUInt64();

			var initialTransform = BoneInitialTransforms.Read(r);

			var numModifiers = r.ReadInt32();

			var unk4 = r.ReadByte();

			for (var i = 0; i < numModifiers; i++)
			{
				var mod = BoneModifier.Read(r);
			}

			var unk5 = r.ReadBytes(3);

			var unk6 = r.ReadUInt32();
			var unk7 = r.ReadUInt32();

			var boneId = (BoneId)r.ReadUInt32();

			var unk8 = r.ReadBytes(19);

			return null;
		}
	}

	public class BoneModifier
	{
		public static BoneModifier Read(BinaryReader r)
		{
			var magic = (Magic)r.ReadUInt32();

			return magic switch
			{
				Magic.BallJointBoneModifier => BallJointBoneModifier.ReadData(r),
				_ => null
			};
		}
	}

	public class BallJointBoneModifier : BoneModifier
	{
		public ulong Uid { get; }
		public byte[] Unk { get; }
		public ulong Uid2 { get; }
		public byte[] Unk2 { get; }
		public Matrix4F Matrix { get; }
		public byte[] Unk3 { get; }

		private BallJointBoneModifier(ulong uid, byte[] unk, ulong uid2, byte[] unk2, Matrix4F matrix, byte[] unk3)
		{
			Uid = uid;
			Unk = unk;
			Uid2 = uid2;
			Unk2 = unk2;
			Matrix = matrix;
			Unk3 = unk3;
		}

		public static BallJointBoneModifier ReadData(BinaryReader r)
		{
			var uid = r.ReadUInt64();

			var unk = r.ReadBytes(10);

			var uid2 = r.ReadUInt64();

			var unk2 = r.ReadBytes(2);

			var matrix = r.ReadStruct<Matrix4F>(Matrix4F.SizeInBytes);

			var unk3 = r.ReadBytes(13);

			return new BallJointBoneModifier(uid, unk, uid2, unk2, matrix, unk3);
		}
	}
}