using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MipContainerReference
	{
		public ulong EntryUid { get; }
		public uint Magic { get; }
		public ulong MipContainerUid { get; }
		public uint Var1 { get; }
		public uint MipTarget { get; }

		private MipContainerReference(ulong entryUid, uint magic, ulong mipContainerUid, uint var1, uint mipTarget)
		{
			EntryUid = entryUid;
			Magic = magic;
			MipContainerUid = mipContainerUid;
			Var1 = var1;
			MipTarget = mipTarget;
		}

		public static MipContainerReference Read(BinaryReader r)
		{
			var entryUid = r.ReadUInt64();

			var magic = r.ReadUInt32();
			var mipContainerUid = r.ReadUInt64();

			var var1 = r.ReadUInt32();
			var mipTarget = r.ReadUInt32();

			return new MipContainerReference(entryUid, magic, mipContainerUid, var1, mipTarget);
		}
	}
}