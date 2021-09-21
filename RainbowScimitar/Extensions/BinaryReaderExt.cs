using System.IO;
using System.Runtime.InteropServices;

namespace RainbowScimitar.Extensions
{
	public static class BinaryReaderExt
	{
		public static T ReadStruct<T>(this BinaryReader stream) where T : struct
		{
			var t = new T();
			var size = Marshal.SizeOf(t);

			var buf = new byte[size];
			var amountRead = stream.Read(buf, 0, size);
			if (amountRead != size)
				throw new EndOfStreamException();

			buf.ToStruct(ref t);

			return t;
		}

		public static T[] ReadStructs<T>(this BinaryReader r, int count) where T : struct
		{
			var structs = new T[count];

			for (var i = 0; i < count; i++)
				structs[i] = r.ReadStruct<T>();

			return structs;
		}

		public static T[] ReadLengthPrefixedStructs<T>(this BinaryReader r) where T : struct
		{
			var count = r.ReadInt32();
			return r.ReadStructs<T>(count);
		}
	}
}