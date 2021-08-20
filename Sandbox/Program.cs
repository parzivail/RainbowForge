using System;
using RainbowForge.Core;
using RainbowForge.Core.Container;

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
			var newForge = Forge.GetForge("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_merged_bnk_mesh.forge");

			for (var i = 1; i < newForge.Entries.Length; i++)
			{
				if (newForge.Entries[i].Uid == 3040)
				{
					var container = newForge.GetContainer(newForge.Entries[i].Uid);
					if (container is Hash h)
					{
						Console.WriteLine($"FILENAME_ENCODING_BASE_KEY = {EntryMetaData.FILENAME_ENCODING_BASE_KEY:X16}");
						Console.WriteLine($"FILENAME_ENCODING_KEY_STEP = {EntryMetaData.FILENAME_ENCODING_KEY_STEP:X16}");

						Console.WriteLine($"STEP1 = {EntryMetaData.FILENAME_ENCODING_BASE_KEY + EntryMetaData.FILENAME_ENCODING_KEY_STEP:X16}");

						Console.WriteLine($"{h.Hash1:X16}");
						Console.WriteLine($"{h.Hash2:X16}");
						Console.WriteLine($"{h.Hash1 ^ h.Hash2:X16}");
						break;
					}
				}

				Console.WriteLine(newForge.Entries[i].MetaData.FileName);

				if (i > 20)
					break;
			}

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