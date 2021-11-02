using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Model;

namespace RainbowScimitar.DataTypes
{
	public record BoundingVolume(int Unknown1, Box Volume)
	{
		public static BoundingVolume Read(BinaryReader r)
		{
			r.ReadMagic(Magic.BoundingVolume);

			var unk1 = r.ReadInt32();
			var box = r.ReadStruct<Box>();

			return new BoundingVolume(unk1, box);
		}
	}
}