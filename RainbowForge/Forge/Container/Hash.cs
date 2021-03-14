using System.IO;
using System.Text;

namespace RainbowForge.Forge.Container
{
	public class Hash : ForgeContainer
	{
		public ulong Hash1 { get; }
		public ulong Hash2 { get; }
		public string Name { get; }

		private Hash(ulong hash1, ulong hash2 = 0, string name = "")
		{
			Hash1 = hash1;
			Hash2 = hash2;
			Name = name;
		}

		public static Hash Read(BinaryReader r)
		{
			var hash1 = r.ReadUInt64(); // [0x0]

			var continueFlag = r.ReadUInt32(); // [0x8]
			if (continueFlag == 0) // switch value, controls whether there is more metadata
				return new Hash(hash1);

			var hash2 = r.ReadUInt64(); // [0xC]

			continueFlag = r.ReadUInt32(); // [0x14]
			if (continueFlag == 0)
				return new Hash(hash1, hash2);

			var name = Encoding.UTF8.GetString(r.ReadBytes(0x40)); // [0x18]
			var x58 = r.ReadUInt64(); // [0x58]
			var x60 = r.ReadUInt64(); // [0x60]

			continueFlag = r.ReadUInt32(); // [0x68]
			if (continueFlag == 0)
				return new Hash(hash1, hash2, name);

			var x6C = r.ReadUInt64(); // [0x6C]
			var x74 = r.ReadUInt64(); // [0x74]

			continueFlag = r.ReadUInt32(); // [0x7C]
			if (continueFlag == 0)
				return new Hash(hash1, hash2, name);

			var x80 = r.ReadUInt64(); // [0x80]

			return new Hash(hash1, hash2, name);
		}
	}
}