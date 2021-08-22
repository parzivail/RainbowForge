using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class Material
	{
		public TextureSelector[] BaseTextureMapSpecs { get; }
		public TextureSelector[] SecondaryTextureMapSpecs { get; }
		public TextureSelector[] TertiaryTextureMapSpecs { get; }

		private Material(TextureSelector[] baseTextureMapSpecs, TextureSelector[] secondaryTextureMapSpecs, TextureSelector[] tertiaryTextureMapSpecs)
		{
			BaseTextureMapSpecs = baseTextureMapSpecs;
			SecondaryTextureMapSpecs = secondaryTextureMapSpecs;
			TertiaryTextureMapSpecs = tertiaryTextureMapSpecs;
		}

		public static Material Read(BinaryReader r)
		{
			var magic1 = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.Material, magic1);

			var baseMipContainers = new TextureSelector[5];
			for (var i = 0; i < baseMipContainers.Length; i++)
				baseMipContainers[i] = TextureSelector.Read(r);

			var padding = r.ReadUInt64();

			var magic2 = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.UVTransform, magic2);

			var uvTransform1 = r.ReadBytes(43);

			var secondaryMipContainers = new TextureSelector[3];
			for (var i = 0; i < secondaryMipContainers.Length; i++)
				secondaryMipContainers[i] = TextureSelector.Read(r);

			var padding2 = r.ReadUInt64();

			var magic3 = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.UVTransform, magic3);

			var uvTransform2 = r.ReadBytes(43);

			var internalUid1 = r.ReadUInt64();
			var magic4 = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.DetailMapDescriptor, magic4);

			var detailMapDesc1 = r.ReadBytes(16);

			var internalUid2 = r.ReadUInt64();
			var magic5 = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.DetailMapDescriptor, magic5);

			var detailMapDesc2 = r.ReadBytes(16);

			var tertiaryMipContainers = new TextureSelector[2];
			for (var i = 0; i < tertiaryMipContainers.Length; i++)
				tertiaryMipContainers[i] = TextureSelector.Read(r);

			return new Material(baseMipContainers, secondaryMipContainers, tertiaryMipContainers);
		}
	}
}