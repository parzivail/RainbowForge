using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record WorldLoaderSubNode(WorldLoadUnitWorldDivision[] WorldDivisions, ScimitarId[] Scenarios, WorldDivisionToTagLoadUnitLookup Lookup);
}