using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.FileTypes
{
	public record WindDefinition(float[] Unknown1)
	{
		public static WindDefinition Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WindDefinition);

			var unk1 = r.ReadStructs<float>(25);

			return new WindDefinition(unk1);
		}
	}
}