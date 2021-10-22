using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record World()
	{
		public static World Read(BinaryReader r)
		{
			r.ReadMagic(Magic.World);

			var spaceManagerUid = r.ReadUid();
			var spaceManager = SpaceManager.Read(r);

			var soundStateUid = r.ReadUid();
			var soundState = SoundState.Read(r);

			var worldDivisionContainerUid = r.ReadUid();
			var zero = r.ReadByte();

			var worldLoaderUid = r.ReadUid();
			var worldLoader = WorldLoader.Read(r);

			var defaultScenarioUid = r.ReadUid();
			var zero2 = r.ReadByte();

			var graphicDataUid = r.ReadUid();
			var graphicData = WorldGraphicData.Read(r);

			var boundingVolumeUid = r.ReadUid();
			var boundingVolume = BoundingVolume.Read(r);

			// TODO: only seems to be 0 or 3, 0 on all maps except Tower, Italy, Morocco, HerefordRework, Austrailia, and ThemePark_V2
			var giBoundingVolumeFlags = r.ReadByte();
			if (giBoundingVolumeFlags == 0)
			{
				var giBoundingVolumeUid = r.ReadUid();
				var giBoundingVolume = GIBoundingVolume.Read(r);
			}

			var giBoundingVolumeFlags2 = r.ReadByte();
			if (giBoundingVolumeFlags2 == 0)
			{
				var giBoundingVolumeUid = r.ReadUid();
				var giBoundingVolume = GIBoundingVolume.Read(r);
			}

			var iColorUid = r.ReadUid();
			var iColor = IColor.Read(r);

			var windDefinitionUid = r.ReadUid();
			var windDefinition = WindDefinition.Read(r);

			var extraUids = r.ReadLengthPrefixedStructs<ScimitarId>();

			var soundPropogationMapUid = r.ReadUid();

			var unk2 = r.ReadByte();

			var floats = r.ReadStructs<float>(23);

			var pos = r.BaseStream.Position;
			return null;
		}
	}
}