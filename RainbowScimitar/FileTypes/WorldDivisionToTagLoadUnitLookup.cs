using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record WorldDivisionToTagLoadUnitLookup(ScimitarId WorldSectionUid, WorldLoadUnitLoadByTag WorldLoadUnitLoadByTag)
	{
		public static WorldDivisionToTagLoadUnitLookup Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WorldDivisionToTagLoadUnitLookup);

			var worldSectionUid = r.ReadUid();

			var zero = r.ReadByte();
			var worldLoadUnitLoadByTagUid = r.ReadUid();
			var worldLoadUnitLoadByTag = WorldLoadUnitLoadByTag.Read(r);

			return new WorldDivisionToTagLoadUnitLookup(worldSectionUid, worldLoadUnitLoadByTag);
		}
	}
}