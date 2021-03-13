using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RainbowForge.Forge
{
	public class Descriptor : Container
	{
		public Dictionary<uint, uint> UIntData { get; }
		public Dictionary<uint, string> StringData { get; }
		public Dictionary<uint, ulong> ULongData { get; }

		private Descriptor(Dictionary<uint, uint> uIntData, Dictionary<uint, string> stringData, Dictionary<uint, ulong> uLongData)
		{
			UIntData = uIntData;
			StringData = stringData;
			ULongData = uLongData;
		}

		public static Descriptor Read(BinaryReader r, Entry entry)
		{
			var uintData = new Dictionary<uint, uint>();
			var stringData = new Dictionary<uint, string>();
			var ulongData = new Dictionary<uint, ulong>();

			while (r.BaseStream.Position < entry.End)
			{
				var dataId = r.ReadUInt32();
				var dataType = r.ReadUInt32();

				switch (dataType)
				{
					case 0x0: // uint
						uintData[dataId] = r.ReadUInt32();
						break;
					case 0x1: // string
						var strLen = r.ReadInt32();
						stringData[dataId] = Encoding.UTF8.GetString(r.ReadBytes(strLen));
						r.ReadByte(); // 0x00
						break;
					case 0x2: // ulong
						ulongData[dataId] = r.ReadUInt64();
						break;
					case 0x5: // ???
						r.ReadBytes(8);
						break;
					default:
						throw new InvalidDataException($"Unknown data type! Data ID: 0x{dataId:X}, type 0x{dataType:X}");
				}
			}

			return new Descriptor(uintData, stringData, ulongData);
		}
	}
}