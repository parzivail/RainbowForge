using System.IO;

namespace RainbowForge.Forge
{
	public class Meta
	{
		public MetaLink[] Links { get; }

		public Meta(MetaLink[] links)
		{
			Links = links;
		}

		public static Meta Read(BinaryReader r)
		{
			var length = r.BaseStream.Length;

			var numLinks = r.ReadUInt16();

			var extra = (length - 2) / numLinks != 12 || (length - 2) % numLinks != 0;

			var links = new MetaLink[numLinks];
			for (var i = 0; i < numLinks; i++)
				links[i] = MetaLink.Read(r, extra);

			return new Meta(links);
		}
	}
}