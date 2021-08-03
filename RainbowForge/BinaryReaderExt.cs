using System.IO;
using OpenTK.Mathematics;

namespace RainbowForge
{
	public static class BinaryReaderExt
	{
		public static Vector3 ReadVector3(this BinaryReader r)
		{
			var x = r.ReadSingle();
			var y = r.ReadSingle();
			var z = r.ReadSingle();

			return new Vector3(x, y, z);
		}

		public static Vector3 ReadUInt64AsPos(this BinaryReader r)
		{
			const float bias = 0x7FFF;

			var x = r.ReadUInt16();
			var y = r.ReadUInt16();
			var z = r.ReadUInt16();
			var s = r.ReadUInt16();

			return new Vector3(x * s / bias, y * s / bias, z * s / bias);
		}

		public static Vector3 ReadUInt32AsVec(this BinaryReader r)
		{
			const float bias = 0x7F;

			var x = r.ReadByte();
			var y = r.ReadByte();
			var z = r.ReadByte();
			var l = r.ReadByte();

			return new Vector3(x / bias - 1, y / bias - 1, z / bias - 1);
		}

		public static Color4 ReadUInt32AsColor(this BinaryReader r)
		{
			const float bias = 0xFF;

			var red = r.ReadByte();
			var green = r.ReadByte();
			var blue = r.ReadByte();
			var alpha = r.ReadByte();

			return new Color4(red / bias, green / bias, blue / bias, alpha / bias);
		}

		public static Vector2 ReadUInt32AsUv(this BinaryReader r)
		{
			var packedCoords = r.ReadBytes(4);

			return new Vector2(Half.FromBytes(packedCoords, 0), Half.FromBytes(packedCoords, 2));
		}

		public static T ReadStruct<T>(this BinaryReader stream, int size) where T : struct
		{
			var buf = new byte[size];
			if (stream.Read(buf, 0, size) != size)
				throw new InvalidDataException("Unable to read struct, insufficient data");

			return buf.ToStruct<T>();
		}
	}
}