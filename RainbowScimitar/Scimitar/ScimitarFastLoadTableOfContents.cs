using System;
using System.IO;
using System.Text;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarFastLoadTableOfContents(int Unknown1, ulong Unknown2, int Unknown3, ulong Unknown4, string Name)
	{
		public static ScimitarFastLoadTableOfContents Read(Stream bundleStream)
		{
			var r = new BinaryReader(bundleStream);

			var unk1 = r.ReadInt32(); // 6
			var unk2 = r.ReadUInt64(); // varies, hash?
			var unk3 = r.ReadInt32(); // 1
			var unk4 = r.ReadUInt64(); // varies, hash?
			var hasNameData = r.ReadInt32(); // 1 if contains name data, 0 otherwise

			string name = null;
			if (hasNameData == 1)
			{
				var nameBytes = r.ReadBytes(64);
				var zeroTerminatorPos = Array.IndexOf(nameBytes, (byte)0);
				name = Encoding.ASCII.GetString(nameBytes, 0, zeroTerminatorPos == -1 ? nameBytes.Length : zeroTerminatorPos);
			}

			return new ScimitarFastLoadTableOfContents(unk1, unk2, unk3, unk4, name);
		}
	}
}