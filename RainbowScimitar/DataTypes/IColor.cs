using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.DataTypes
{
	public record IColor(int Unknown1, float[] Unknown2, ushort Unknown3)
	{
		public static IColor Read(BinaryReader r)
		{
			r.ReadMagic(Magic.IColor);

			var unk1 = r.ReadInt32();
			var unk2 = r.ReadStructs<float>(16);
			var unk3 = r.ReadUInt16(); // could just be bytes?

			return new IColor(unk1, unk2, unk3);
		}
	}
}