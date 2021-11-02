using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record TagClient(ScimitarId[] Units)
	{
		public static TagClient Read(BinaryReader r)
		{
			r.ReadMagic(Magic.TagClient);
			var units = r.ReadLengthPrefixedStructs<ScimitarId>();
			return new TagClient(units);
		}
	}
}