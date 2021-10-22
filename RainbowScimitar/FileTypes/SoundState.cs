using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record SoundState(ScimitarId Uid, SoundId Sound1, SoundId Sound2)
	{
		public static SoundState Read(BinaryReader r)
		{
			r.ReadMagic(Magic.SoundState);

			var uid = r.ReadUid();

			var soundId1 = SoundId.Read(r);
			var soundId2 = SoundId.Read(r);

			return new SoundState(uid, soundId1, soundId2);
		}
	}
}