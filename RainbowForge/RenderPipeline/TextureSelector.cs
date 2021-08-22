using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class TextureSelector
	{
		public ulong EntryUid { get; }
		public ulong TextureMapSpecUid { get; }
		public uint Var1 { get; }
		public uint MipTarget { get; }

		private TextureSelector(ulong entryUid, ulong textureMapSpecUid, uint var1, uint mipTarget)
		{
			EntryUid = entryUid;
			TextureMapSpecUid = textureMapSpecUid;
			Var1 = var1;
			MipTarget = mipTarget;
		}

		public static TextureSelector Read(BinaryReader r)
		{
			var entryUid = r.ReadUInt64();

			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.TextureSelector, magic);

			var mipContainerUid = r.ReadUInt64();

			var var1 = r.ReadUInt32();
			var mipTarget = r.ReadUInt32();

			return new TextureSelector(entryUid, mipContainerUid, var1, mipTarget);
		}
	}
}