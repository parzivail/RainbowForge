using System.IO;
using System.Text;

namespace RainbowForge.Info
{
	public record Area(uint Magic, string Name, ulong[] Uids);
	
	public class AreaMap
	{
		public Area[] Areas { get; }

		private AreaMap(Area[] areas)
		{
			Areas = areas;
		}

		public static AreaMap Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.AreaMap, magic);

			var data = r.ReadBytes(34);

			var numAreas = r.ReadInt32();

			var areas = new Area[numAreas];

			for (var i = 0; i < numAreas; i++)
			{
				var data2 = r.ReadBytes(9);

				var entryMagic = r.ReadUInt32();
				var entryLength = r.ReadInt32();
				var entry = Encoding.ASCII.GetString(r.ReadBytes(entryLength));

				var data3 = r.ReadBytes(17);

				var numThings = r.ReadInt32();
				var uids = new ulong[numThings];
				for (var j = 0; j < numThings; j++)
				{
					uids[j] = r.ReadUInt64();
				}

				var data4 = r.ReadBytes(17);

				areas[i] = new Area(entryMagic, entry, uids);
			}

			// TODO: there's more data at the end of this file, possibly UIDs?

			return new AreaMap(areas);
		}
	}
}