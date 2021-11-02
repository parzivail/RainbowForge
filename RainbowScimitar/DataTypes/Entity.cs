using System;
using System.IO;
using System.Linq;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Model;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.DataTypes
{
	public record Entity()
	{
		public static Entity Read(BinaryReader r)
		{
			r.ReadMagic(Magic.Entity);

			var unk1 = r.ReadByte();

			var matrix = r.ReadStruct<Matrix4f>();

			var boundingVolumeUid = r.ReadUid();
			var boundingVolume = BoundingVolume.Read(r);

			var unk2 = r.ReadBytes(31);

			var unk3 = r.ReadByte(); // either 1 or 3
			ScimitarId worldSectionUid;
			if (unk3 == 1) worldSectionUid = r.ReadUid();

			var zero1 = r.ReadByte();

			var tagClientUid = r.ReadUid();
			var tagClient = TagClient.Read(r);

			var unk4 = r.ReadBytes(14);

			var next = r.ReadUInt32();

			Console.WriteLine($"{next:X8} - {string.Join(' ', unk2.Select(b => b == 0 ? ".." : b.ToString("X2")))} - {string.Join(' ', unk4.Select(b => b == 0 ? ".." : b.ToString("X2")))}");

			var pos = r.BaseStream.Position;
			return new Entity();
		}
	}
}