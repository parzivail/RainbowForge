using System.IO;
using System.Text;

namespace RainbowForge.Info
{
	public record R6AIRoom(string Name, ulong[] Uids);

	public class R6AIWorldComponent
	{
		public R6AIRoom[] Rooms { get; }

		private R6AIWorldComponent(R6AIRoom[] rooms)
		{
			Rooms = rooms;
		}

		public static R6AIWorldComponent Read(BinaryReader r)
		{
			var magic = r.ReadUInt32();
			MagicHelper.AssertEquals(Magic.R6AIWorldComponent, magic);

			var data = r.ReadBytes(34);

			var numAreas = r.ReadInt32();

			var areas = new R6AIRoom[numAreas];

			for (var i = 0; i < numAreas; i++)
			{
				var data2 = r.ReadBytes(9);

				var entryMagic = r.ReadUInt32();
				MagicHelper.AssertEquals(Magic.R6AIRoom, entryMagic);

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

				areas[i] = new R6AIRoom(entry, uids);
			}

			// TODO: there's more data at the end of this file, possibly UIDs?

			return new R6AIWorldComponent(areas);
		}
	}
}