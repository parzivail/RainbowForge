using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.DataTypes
{
	public record GISettings(float Unknown1, float Unknown2)
	{
		public static GISettings Read(BinaryReader r)
		{
			r.ReadMagic(Magic.GISettings);

			var unk1 = r.ReadSingle();
			var unk2 = r.ReadSingle();

			return new GISettings(unk1, unk2);
		}
	}
}