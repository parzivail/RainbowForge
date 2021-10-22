using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;

namespace RainbowScimitar.FileTypes
{
	public record GIBoundingVolume(Box Bounds, Vec3f Unknown1, int Unknown2, int Unknown3, int Unknown4, float Unknown5, int Unknown6)
	{
		public static GIBoundingVolume Read(BinaryReader r)
		{
			r.ReadMagic(Magic.GIBoundingVolume);

			var box = r.ReadStruct<Box>();
			var vec = r.ReadStruct<Vec3f>();

			var a = r.ReadInt32();
			var b = r.ReadInt32();
			var c = r.ReadInt32();
			var d = r.ReadSingle();
			var e = r.ReadInt32();

			return new GIBoundingVolume(box, vec, a, b, c, d, e);
		}
	}
}