using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MaterialContainer
	{
		public MipContainerReference[] BaseMipContainers { get; }
		public MipContainerReference[] SecondaryMipContainers { get; }
		public MipContainerReference[] TertiaryMipContainers { get; }

		private MaterialContainer(MipContainerReference[] baseMipContainers, MipContainerReference[] secondaryMipContainers, MipContainerReference[] tertiaryMipContainers)
		{
			BaseMipContainers = baseMipContainers;
			SecondaryMipContainers = secondaryMipContainers;
			TertiaryMipContainers = tertiaryMipContainers;
		}

		public static MaterialContainer Read(BinaryReader r)
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

			return new MaterialContainer(baseMipContainers, secondaryMipContainers, tertiaryMipContainers);
		}
	}
}