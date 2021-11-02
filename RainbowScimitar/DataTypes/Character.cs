using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record Character()
	{
		public static Character Read(BinaryReader r)
		{
			r.ReadMagic(Magic.Character);

			var spawningCharacterSlot = r.ReadUid();
			var subUids = r.ReadLengthPrefixedStructs<ScimitarId>();

			var unk1 = r.ReadBytes(12);
			var selfUid = r.ReadUid();

			var unk2 = r.ReadBytes(12);


			return null;
		}
	}
}