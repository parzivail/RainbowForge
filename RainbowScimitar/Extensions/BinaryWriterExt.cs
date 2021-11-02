using System.Collections.Generic;
using System.IO;

namespace RainbowScimitar.Extensions
{
	public static class BinaryWriterExt
	{
		public static void WriteStructs<T>(this BinaryWriter w, IEnumerable<T> structs) where T : struct
		{
			foreach (var s in structs) w.WriteStruct(s);
		}

		public static void WriteStruct<T>(this BinaryWriter w, T s) where T : struct
		{
			w.Write(s.ToBytes());
		}
	}
}