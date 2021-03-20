﻿using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MipContainerEntry
	{
		public ulong EntryUid { get; }
		public uint Magic { get; }
		public ulong MipContainerUid { get; }
		public uint Var1 { get; }
		public uint MipTarget { get; }

		private MipContainerEntry(ulong entryUid, uint magic, ulong mipContainerUid, uint var1, uint mipTarget)
		{
			EntryUid = entryUid;
			Magic = magic;
			MipContainerUid = mipContainerUid;
			Var1 = var1;
			MipTarget = mipTarget;
		}

		public static MipContainerEntry Read(BinaryReader r)
		{
			var entryUid = r.ReadUInt64();

			var magic = r.ReadUInt32();
			var mipContainerUid = r.ReadUInt64();

			var var1 = r.ReadUInt32();
			var mipTarget = r.ReadUInt32();

			return new MipContainerEntry(entryUid, magic, mipContainerUid, var1, mipTarget);
		}
	}
}