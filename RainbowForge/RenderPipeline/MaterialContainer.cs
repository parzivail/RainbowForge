using System;
using System.IO;

namespace RainbowForge.RenderPipeline
{
	public class MaterialContainer
	{
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

			throw new NotImplementedException();
		}
	}
}