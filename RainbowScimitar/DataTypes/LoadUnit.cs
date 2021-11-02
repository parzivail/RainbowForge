using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record LoadUnit(ScimitarId[] Units)
	{
		public static LoadUnit Read(BinaryReader r)
		{
			r.ReadMagic(Magic.LoadUnit);
			var units = r.ReadLengthPrefixedStructs<ScimitarId>();
			return new LoadUnit(units);
		}
	}
}