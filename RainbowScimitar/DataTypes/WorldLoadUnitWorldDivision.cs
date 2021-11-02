using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record WorldLoadUnitWorldDivision(ScimitarId WorldSectionDivisionLookupUid, ScimitarId WorldSectionUid)
	{
		public static WorldLoadUnitWorldDivision Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WorldLoadUnit_WorldDivision);

			var worldSectionDivisionLookupUid = r.ReadUid();
			var worldSectionUid = r.ReadUid();

			return new WorldLoadUnitWorldDivision(worldSectionDivisionLookupUid, worldSectionUid);
		}
	}
}