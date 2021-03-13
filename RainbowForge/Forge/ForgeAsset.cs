using System;
using System.IO;

namespace RainbowForge.Forge
{
	public class ForgeAsset : Container
	{
		public IAssetBlock MetaBlock { get; }
		public IAssetBlock AssetBlock { get; }

		public bool HasMeta => MetaBlock != null;

		private ForgeAsset(IAssetBlock metaBlock, IAssetBlock assetBlock)
		{
			MetaBlock = metaBlock;
			AssetBlock = assetBlock;
		}

		public static ForgeAsset Read(BinaryReader r, Entry entry)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.FileContainer, magic);

			var end = entry.End;

			var blockA = GetAssetBlock(r);

			if (r.BaseStream.Position >= end)
				return new ForgeAsset(null, blockA);

			var assetBlockMagic = r.ReadUInt64(); // this might actually be 8 shorts and 16 0x00 bytes

			var blockB = GetAssetBlock(r);

			return new ForgeAsset(blockA, blockB);
		}

		private static IAssetBlock GetAssetBlock(BinaryReader r)
		{
			var x = r.ReadUInt16(); // 2 (changed to 3 in Y5) <-- container deserializer type
			var assetDeserializerType = r.ReadUInt16(); // 3 for chunked, 7 for linear (flags?)
			var y = r.ReadByte(); // 0
			var z = r.ReadUInt16();

			return assetDeserializerType switch
			{
				3 => ChunkedDataBlock.Read(r),
				7 => LinearDataBlock.Read(r),
				_ => throw new NotImplementedException()
			};
		}

		public BinaryReader GetDataStream(Forge forge)
		{
			return new(AssetBlock.GetDataStream(forge.Stream));
		}
	}
}