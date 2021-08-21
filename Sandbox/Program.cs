using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using RainbowForge.Core;

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
			var oldForge = Forge.GetForge("R:\\Siege Dumps\\Y0\\datapc64_merged_playgo_bnk_textures2.forge");
			var newForge = Forge.GetForge("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_merged_playgo_bnk_textures2.forge");

			using var sw = new StreamWriter("out.txt");
			for (var i = 0; i < newForge.Entries.Length; i++)
			{
				var newEntry = newForge.Entries[i];
				var oldEntry = oldForge.Entries.FirstOrDefault(entry => entry.Uid == newEntry.Uid);

				if (oldEntry == null)
					continue;

				sw.WriteLine(
					$"Y0 - UID: 0x{oldEntry.Uid:X16}, Offset: 0x{oldEntry.Offset:X16}, FileType: 0x{oldEntry.MetaData.FileType:X8}, Timestamp: 0x{oldEntry.MetaData.Timestamp:X8}, Name: [{Encoding.ASCII.GetString(oldEntry.MetaData.Name.Skip(24).TakeWhile(b => b != 0).ToArray())}]");
				sw.WriteLine(
					$"Y6 - UID: 0x{newEntry.Uid:X16}, Offset: 0x{newEntry.Offset:X16}, FileType: 0x{newEntry.MetaData.FileType:X8}, Timestamp: 0x{newEntry.MetaData.Timestamp:X8}, Name: [{string.Join(' ', newEntry.MetaData.Name.Take(newEntry.MetaData.NameLength).Select(b => $"{b:X2}"))}]");

				sw.WriteLine();
			}

			// for (var i = 0; i < newForge.Entries.Length; i++)
			// {
			// 	var entry = newForge.Entries[i];
			// 	Console.WriteLine(entry.MetaData.FileName);
			// 	
			// 	if (i > 5)
			// 		break;
			// }

			// var nameBytes = new byte[] { 76, 150, 47, 188, 141, 156, 45, 205, 201, 249, 122, 39, 216, 61 };
			// var outputBytes = Encoding.ASCII.GetBytes("GlobalMetaFile");
			//
			// long i = 0;
			// Parallel.For(0x35726700_00000000, 0x357267FF_FFFFFFFF + 1, l =>
			// {
			// 	if (Interlocked.Increment(ref i) % 0x0_10000000 == 0)
			// 		Console.WriteLine($"{i:X16}");
			//
			// 	var name = EntryMetaData.DecodeName(nameBytes, 0, 0, 0, (ulong)l);
			// 	if (ByteArrayCompare(name, outputBytes))
			// 	{
			// 		Console.WriteLine($"!! {l:X16}");
			// 		Environment.Exit(0);
			// 	}
			// });

			Console.WriteLine("Done.");
		}

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int memcmp(byte[] b1, byte[] b2, long count);

		private static bool ByteArrayCompare(byte[] b1, byte[] b2)
		{
			// Validate buffers are the same length.
			// This also ensures that the count does not exceed the length of either buffer.  
			return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
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