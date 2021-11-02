using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarGlobalMeta(int Unknown1, Dictionary<int, ScimitarGlobalMeta.Entry> Entries)
	{
		public enum DataType
		{
			Null = 0x0,
			LengthPrefixedAsciiString = 0x1,
			Int64A = 0x2,
			Int64B = 0x5
		}

		public record Entry(DataType DataType, object Value);

		public static ScimitarGlobalMeta Read(Stream bundleStream)
		{
			var r = new BinaryReader(bundleStream);

			var unk1 = r.ReadInt32();

			var entries = new Dictionary<int, Entry>();

			while (true)
			{
				var id = r.ReadInt32();
				var type = (DataType)r.ReadInt32();

				if (!Enum.IsDefined(typeof(DataType), type))
					throw new NotSupportedException();

				if (type == DataType.Null)
					break;

				switch (type)
				{
					case DataType.LengthPrefixedAsciiString:
					{
						var strLength = r.ReadInt32();
						var strBytes = r.ReadBytes(strLength);
						var str = Encoding.ASCII.GetString(strBytes);

						r.ReadByte(); // null terminator

						entries[id] = new Entry(type, str);
						break;
					}
					case DataType.Int64A:
					{
						var i = r.ReadInt64();
						entries[id] = new Entry(type, i);
						break;
					}
					case DataType.Int64B:
					{
						var i = r.ReadInt64();
						entries[id] = new Entry(type, i);
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return new ScimitarGlobalMeta(unk1, entries);
		}
	}
}