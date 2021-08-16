using System;
using System.IO;
using System.Text;

namespace RainbowForge.Info
{
	public class AreaMap
	{
		public static AreaMap Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.AreaMap, magic);

			var uid1 = r.ReadUInt64();
			var uid2 = r.ReadUInt64();
			var x00_1 = r.ReadByte();

			var uid3 = r.ReadUInt64();
			var uid4 = r.ReadUInt64();
			var x00_2 = r.ReadByte();

			var numAreas = r.ReadInt32();

			for (var i = 0; i < numAreas; i++)
			{
				var uid5 = r.ReadUInt64();
				var x00_3 = r.ReadByte();

				var entryMagic = r.ReadUInt32();
				var entryLength = r.ReadInt32();
				var entry = Encoding.ASCII.GetString(r.ReadBytes(entryLength));
				Console.WriteLine(entry);

				var uid6 = r.ReadUInt64();
				var uid7 = r.ReadUInt64();
				var x00_4 = r.ReadByte();

				var numThings = r.ReadInt32();
				for (var j = 0; j < numThings; j++)
				{
					var uid8 = r.ReadUInt64();
				}

				var x00_5 = r.ReadByte();
				var uid9 = r.ReadUInt64();
				var uid10 = r.ReadUInt64();
			}

			return null;
		}
	}
}