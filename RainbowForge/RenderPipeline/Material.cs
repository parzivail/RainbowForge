using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class Material
	{
		public MipContainerReference[] BaseTextureMapSpecs { get; }
		public MipContainerReference[] SecondaryTextureMapSpecs { get; }
		public MipContainerReference[] TertiaryTextureMapSpecs { get; }

		private Material(MipContainerReference[] baseTextureMapSpecs, MipContainerReference[] secondaryTextureMapSpecs, MipContainerReference[] tertiaryTextureMapSpecs)
		{
			BaseTextureMapSpecs = baseTextureMapSpecs;
			SecondaryTextureMapSpecs = secondaryTextureMapSpecs;
			TertiaryTextureMapSpecs = tertiaryTextureMapSpecs;
		}

		public static Material Read(BinaryReader r)
		{
			var magic1 = r.ReadUInt32();

			var baseMipContainers = new MipContainerReference[5];
			for (var i = 0; i < baseMipContainers.Length; i++)
				baseMipContainers[i] = MipContainerReference.Read(r);

			var padding = r.ReadUInt64();

			var magic2 = r.ReadUInt32();
			var data2 = r.ReadBytes(43);

			var secondaryMipContainers = new MipContainerReference[3];
			for (var i = 0; i < secondaryMipContainers.Length; i++)
				secondaryMipContainers[i] = MipContainerReference.Read(r);

			var padding2 = r.ReadUInt64();

			var magic3 = r.ReadUInt32();
			var data3 = r.ReadBytes(43);

			var internalUid1 = r.ReadUInt64();
			var magic4 = r.ReadUInt32();

			var data4 = r.ReadBytes(16);

			var internalUid2 = r.ReadUInt64();
			var magic5 = r.ReadUInt32();

			var data5 = r.ReadBytes(16);

			var tertiaryMipContainers = new MipContainerReference[2];
			for (var i = 0; i < tertiaryMipContainers.Length; i++)
				tertiaryMipContainers[i] = MipContainerReference.Read(r);

			return new Material(baseMipContainers, secondaryMipContainers, tertiaryMipContainers);
		}
	}
}