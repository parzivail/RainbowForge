using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class TextureMapSpec
	{
		public ulong TextureMapUid { get; }
		public uint TextureType { get; }
		public byte[] ExtraData { get; }

		private TextureMapSpec(ulong textureMapUid, uint textureType, byte[] extraData)
		{
			TextureMapUid = textureMapUid;
			TextureType = textureType;
			ExtraData = extraData;
		}

		public static TextureMapSpec Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			// MagicHelper.AssertEquals(Magic.TextureMapSpec, magic);

			var mipUid = r.ReadUInt64();
			var textureType = r.ReadUInt32();

			var extraData = r.ReadBytes(3);

			return new TextureMapSpec(mipUid, textureType, extraData);
		}
	}
}