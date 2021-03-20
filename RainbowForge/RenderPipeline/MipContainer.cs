using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MipContainer
	{
		public uint Magic { get; }
		public ulong MipUid { get; }
		public uint TextureType { get; }
		public byte[] ExtraData { get; }

		private MipContainer(uint magic, ulong mipUid, uint textureType, byte[] extraData)
		{
			Magic = magic;
			MipUid = mipUid;
			TextureType = textureType;
			ExtraData = extraData;
		}

		public static MipContainer Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();

			var mipUid = r.ReadUInt64();
			var textureType = r.ReadUInt32();

			var extraData = r.ReadBytes(3);

			return new MipContainer(magic, mipUid, textureType, extraData);
		}
	}
}