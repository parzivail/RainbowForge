using System.IO;

namespace RainbowForge.Forge
{
	public class ForgeAsset : Container
	{
		public Datablock MetaBlock { get; }
		public Datablock FileBlock { get; }

		public bool HasMeta => MetaBlock != null;

		private ForgeAsset(Datablock metaBlock, Datablock fileBlock)
		{
			MetaBlock = metaBlock;
			FileBlock = fileBlock;
		}

		public static ForgeAsset Read(BinaryReader r, Entry entry)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FileContainer, magic);

			var end = entry.End;
			var block = Datablock.Read(r);

			if (r.BaseStream.Position >= end)
				return new ForgeAsset(null, block);

			var containerMagic = r.ReadUInt64();

			if (!MagicHelper.Equals(Magic.FileContainerKnownType, containerMagic))
			{
				r.BaseStream.Seek(end, SeekOrigin.Begin);
				return new ForgeAsset(block, null);
			}

			var file = Datablock.Read(r);
			return new ForgeAsset(block, file);
		}
	}
}