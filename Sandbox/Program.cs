using System;
using System.Collections.Generic;
using System.Text;
using RainbowForge;
using RainbowForge.Archive;
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
			var newForge = Forge.GetForge("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_ondemand.forge");

			foreach (var entry in newForge.Entries)
			{
				TestMagic(entry.MetaData.FileName, entry.MetaData.FileType);

				if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive)
					continue;

				var container = newForge.GetContainer(entry.Uid);
				if (container is not ForgeAsset fa)
					continue;

				var arc = FlatArchive.Read(fa.GetDataStream(newForge));

				foreach (var arcEntry in arc.Entries) TestMagic(arcEntry.MetaData.FileName, arcEntry.MetaData.FileType);
			}

			Console.WriteLine("Done.");
		}

		private static readonly Dictionary<uint, HashSet<string>> FoundMagics = new();

		private static void TestMagic(string name, uint fileType)
		{
			if (Enum.IsDefined(typeof(Magic), (ulong)fileType))
				return;

			var substrings = GetAllSubstrings(name);

			foreach (var substr in substrings)
			{
				var computedCrc = Crc32.Compute(Encoding.ASCII.GetBytes(substr));
				if (computedCrc == fileType)
				{
					if (!FoundMagics.ContainsKey(fileType))
						FoundMagics[fileType] = new HashSet<string>();

					if (!FoundMagics[fileType].Contains(substr))
					{
						FoundMagics[fileType].Add(substr);
						Console.WriteLine($"{substr} = 0x{fileType:X8},");
					}
				}
			}
		}

		private static string[] GetAllSubstrings(string s)
		{
			var n = s.Length;
			var strings = new string[n * (n + 1) / 2];
			var strIdx = 0;

			for (var i = 0; i < s.Length; i++)
				for (var j = i; j < s.Length; j++)
					strings[strIdx++] = s.Substring(i, j - i + 1);

			return strings;
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