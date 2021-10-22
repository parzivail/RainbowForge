using System.IO;
using RainbowForge;
using RainbowScimitar.Extensions;
using RainbowScimitar.Scimitar;

namespace RainbowScimitar.FileTypes
{
	public record WorldLoadUnitLoadByTag(ScimitarId Uid, ScimitarId LoadUnitUid)
	{
		public static WorldLoadUnitLoadByTag Read(BinaryReader r)
		{
			r.ReadMagic(Magic.WorldLoadUnit_LoadByTag);

			var uid = r.ReadUid();
			var loadUnitUid = r.ReadUid();

			return new WorldLoadUnitLoadByTag(uid, loadUnitUid);
		}
	}
}