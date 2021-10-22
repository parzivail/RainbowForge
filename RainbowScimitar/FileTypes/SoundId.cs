using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record SoundId(uint Unknown1, ScimitarId Id)
	{
		public static SoundId Read(BinaryReader r)
		{
			r.ReadMagic(Magic.SoundID);

			var prop1 = r.ReadUInt32();
			var uid = r.ReadUid();

			return new SoundId(prop1, uid);
		}
	}
}