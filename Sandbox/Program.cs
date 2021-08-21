using System;

namespace Sandbox
{
	internal class Program
	{
		private static void Xor(byte[] data, byte[] needle)
		{
			for (var i = 0; i < data.Length; i++)
				data[i] = (byte)(data[i] ^ needle[i % needle.Length]);
		}

		public static byte[] StringToByteArray(string hex)
		{
			if (hex.Length % 2 == 1)
				throw new Exception("The binary key cannot have an odd number of digits");

			var arr = new byte[hex.Length >> 1];

			for (var i = 0; i < hex.Length >> 1; ++i) arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));

			return arr;
		}

		public static int GetHexVal(char hex)
		{
			int val = hex;
			//For uppercase A-F letters:
			//return val - (val < 58 ? 48 : 55);
			//For lowercase a-f letters:
			//return val - (val < 58 ? 48 : 87);
			//Or the two combined, but a bit slower:
			return val - (val < 58 ? 48 : val < 97 ? 55 : 87);
		}

		private static void Main(string[] args)
		{
			// var newForge = Forge.GetForge("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_ondemand.forge");
			//
			// foreach (var entry in newForge.Entries)
			// {
			// 	if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
			// 		continue;
			//
			// 	var container = newForge.GetContainer(entry.Uid);
			// 	if (container is not ForgeAsset fa)
			// 		continue;
			//
			// 	Console.WriteLine($">>> {entry.MetaData.FileName}");
			//
			// 	var arc = FlatArchive.Read(fa.GetDataStream(newForge));
			// }

			// var encoded = new ulong[] { 0x5A1C70FE01297209, 0x4D4E82DBAAC7C4DE, 0xB9C4DC3B164D74B3 };
			//
			// // correct = "AccuracyData_SMG_MP5FMLI"
			// var decoded = new ulong[] { 0x4163637572616379, 0x446174615f534d47, 0x5f4d5035464d4c49 };
			//
			// Console.WriteLine($"{encoded[1] ^ decoded[1]:X16}");
			
			Console.WriteLine("Done.");
		}
	}

	public sealed class Crc32
	{
		public const uint DefaultPolynomial = 0xedb88320u;
		public const uint DefaultSeed = 0xffffffffu;
		private static readonly uint[] DefaultTable;

		static Crc32()
		{
			DefaultTable = new uint[256];
			for (var i = 0; i < 256; i++)
			{
				var entry = (uint)i;
				for (var j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ DefaultPolynomial;
					else
						entry >>= 1;
				DefaultTable[i] = entry;
			}
		}

		public static uint Compute(byte[] buffer)
		{
			return Compute(DefaultSeed, buffer);
		}

		public static uint Compute(uint seed, byte[] buffer)
		{
			return ~CalculateHash(seed, buffer, 0, buffer.Length);
		}

		public static uint CalculateHash(uint seed, byte[] buffer, int start, int size)
		{
			var hash = seed;
			for (var i = start; i < start + size; i++)
				hash = (hash >> 8) ^ DefaultTable[buffer[i] ^ (hash & 0xff)];
			return hash;
		}
	}
}