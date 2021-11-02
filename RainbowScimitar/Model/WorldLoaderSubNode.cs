using RainbowScimitar.DataTypes;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.Model
{
	public record WorldLoaderSubNode(WorldLoadUnitWorldDivision[] WorldDivisions, ScimitarId[] Scenarios, WorldDivisionToTagLoadUnitLookup Lookup);
}