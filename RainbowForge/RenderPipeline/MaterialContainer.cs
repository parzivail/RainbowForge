using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MaterialContainer
	{
		public MipContainerEntry[] BaseMipContainers { get; }
		public MipContainerEntry[] SecondaryMipContainers { get; }
		public MipContainerEntry[] TertiaryMipContainers { get; }

		private MaterialContainer(MipContainerEntry[] baseMipContainers, MipContainerEntry[] secondaryMipContainers, MipContainerEntry[] tertiaryMipContainers)
		{
			BaseMipContainers = baseMipContainers;
			SecondaryMipContainers = secondaryMipContainers;
			TertiaryMipContainers = tertiaryMipContainers;
		}

		public static MaterialContainer Read(BinaryReader r)
		{
			var magic1 = r.ReadUInt32();

			var baseMipContainers = new MipContainerEntry[5];
			for (var i = 0; i < baseMipContainers.Length; i++)
				baseMipContainers[i] = MipContainerEntry.Read(r);

			var padding = r.ReadUInt64();

			var magic2 = r.ReadUInt32();
			var data2 = r.ReadBytes(43);

			var secondaryMipContainers = new MipContainerEntry[3];
			for (var i = 0; i < secondaryMipContainers.Length; i++)
				secondaryMipContainers[i] = MipContainerEntry.Read(r);

			var padding2 = r.ReadUInt64();

			var magic3 = r.ReadUInt32();
			var data3 = r.ReadBytes(43);

			var internalUid1 = r.ReadUInt64();
			var magic4 = r.ReadUInt32();

			var data4 = r.ReadBytes(16);

			var internalUid2 = r.ReadUInt64();
			var magic5 = r.ReadUInt32();

			var data5 = r.ReadBytes(16);

			var tertiaryMipContainers = new MipContainerEntry[2];
			for (var i = 0; i < tertiaryMipContainers.Length; i++)
				tertiaryMipContainers[i] = MipContainerEntry.Read(r);

			return new MaterialContainer(baseMipContainers, secondaryMipContainers, tertiaryMipContainers);
		}
	}
}