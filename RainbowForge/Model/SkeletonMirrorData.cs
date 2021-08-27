using System.IO;

namespace RainbowForge.Model
{
	public class SkeletonMirrorData
	{
		public ulong Uid { get; }
		public byte[] Data { get; }

		private SkeletonMirrorData(ulong uid, byte[] data)
		{
			Uid = uid;
			Data = data;
		}

		public static SkeletonMirrorData Read(BinaryReader r)
		{
			var uid = r.ReadUInt64();

			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.SkeletonMirrorData, magic);

			var data = r.ReadBytes(8);

			return new SkeletonMirrorData(uid, data);
		}
	}
}