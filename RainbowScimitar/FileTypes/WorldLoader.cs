using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record WorldLoader(WorldLoaderSubNode[] Children)
	{
		public static WorldLoader Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WorldLoader);

			var childCount = r.ReadInt32();

			var children = new WorldLoaderSubNode[childCount];

			// var zero = r.ReadByte();
			var uid1 = r.ReadUid();

			for (var i = 0; i < childCount; i++)
			{
				r.ReadBytes(1);

				var lookup = WorldDivisionToTagLoadUnitLookup.Read(r);

				var numWorldDivisions = r.ReadInt32();
				var divisions = new WorldLoadUnitWorldDivision[numWorldDivisions];
				for (var j = 0; j < numWorldDivisions; j++)
				{
					var zero2 = r.ReadByte();
					var childInternalUid = r.ReadUid();
					divisions[j] = WorldLoadUnitWorldDivision.Read(r);
				}

				/* TODO: It doesn't seem right to have scenarios bundled into WorldLoader, especially
				 * because it's only ever the last sub node that has one. Should these actually be after
				 * WorldLoader is read completely?
				 */
				var scenarios = r.ReadLengthPrefixedStructs<ScimitarId>();

				children[i] = new WorldLoaderSubNode(divisions, scenarios, lookup);
			}

			return new WorldLoader(children);
		}
	}
}