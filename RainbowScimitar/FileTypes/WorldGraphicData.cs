using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record WorldGraphicData(ScimitarId Uid, GISettings GiSettings)
	{
		public static WorldGraphicData Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WorldGraphicData);

			var uid = r.ReadUid();

			var giSettings = GISettings.Read(r);

			return new WorldGraphicData(uid, giSettings);
		}
	}
}