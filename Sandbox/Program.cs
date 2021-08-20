using System;
using System.IO;
using System.Linq;
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
			var oldForge = Forge.GetForge("R:\\Siege Dumps\\Y1S3\\datapc64_merged_bnk_000000002_mesh.forge");
			var newForge = Forge.GetForge("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_merged_bnk_mesh.forge");

			using var sw = new StreamWriter("out.txt");
			foreach (var entryOld in oldForge.Entries)
			{
				var entryNew = newForge.Entries.FirstOrDefault(entry => entry.Uid == entryOld.Uid);

				if (entryNew == null)
					continue;

				var oldExtraData = string.Join(' ', entryOld.MetaData.ExtraData.Select(b => $"{b:X2}"));
				var newExtraData = string.Join(' ', entryNew.MetaData.ExtraData.Select(b => $"{b:X2}"));

				var oldNameBytes = entryOld.MetaData.Name.Skip(24).TakeWhile(b => b != 0).ToArray();
				var oldName = string.Join(' ', Encoding.ASCII.GetString(oldNameBytes));

				if (oldNameBytes.Length != entryNew.MetaData.NameLength)
					continue;

				var newName = string.Join(' ', entryNew.MetaData.Name.Take(entryNew.MetaData.NameLength).Select(b => $"{b:X2}"));


				sw.WriteLine($"Y1S3           - Uid: 0x{entryOld.Uid:X16}" +
				             $", Filetype: 0x{entryOld.MetaData.FileType:X8}" +
				             $", Timestamp: 0x{entryOld.MetaData.Timestamp:X8}" +
				             $", Previous Index: 0x{entryOld.MetaData.PrevEntryIdx:X8}" +
				             $", Next Index: 0x{entryOld.MetaData.NextEntryIdx:X8}" +
				             $", ExtraData: [{oldExtraData}]" +
				             $", Name: [{oldName}]");

				sw.WriteLine($"Y6S1 v15500403 - Uid: 0x{entryNew.Uid:X16}" +
				             $", Filetype: 0x{entryNew.MetaData.FileType:X8}" +
				             $", Timestamp: 0x{entryNew.MetaData.Timestamp:X8}" +
				             $", Previous Index: 0x{entryNew.MetaData.PrevEntryIdx:X8}" +
				             $", Next Index: 0x{entryNew.MetaData.NextEntryIdx:X8}" +
				             $", ExtraData: [{newExtraData}]" +
				             $", Name: [{newName}]");

				sw.WriteLine();

				Console.WriteLine($"0x{entryOld.Uid:X16}");
			}

			// var files = Directory.GetFiles("R:\\Siege Dumps\\Y6S1 v15500403", "*.forge");
			// files = new[] { "R:\\Siege Dumps\\Y0\\datapc64_pvp05_plane.forge" };

			// var oldSeason = "Y5S4 v15302504";
			// var newSeason = "Y6S1 v15500403";
			//
			// var forge = "datapc64_merged_bnk_textures3";
			//
			// var fOld = Forge.GetForge($"R:\\Siege Dumps\\{oldSeason}\\{forge}.forge");
			// var fNew = Forge.GetForge($"R:\\Siege Dumps\\{newSeason}\\{forge}.forge");
			//
			// int i = 0;
			//
			// using var sw = new StreamWriter("out.txt");
			//
			// foreach (var entryNew in fNew.Entries)
			// {
			// 	var entryOld = fOld.Entries.FirstOrDefault(entry => entry.Uid == entryNew.Uid);
			// 	
			// 	if (entryOld == null || entryOld.MetaData.Timestamp == entryNew.MetaData.Timestamp)
			// 		continue;
			//
			// 	var oldExtraData = string.Join(' ', entryOld.MetaData.ExtraData.Take(entryOld.MetaData.NameLength).Select(b => $"{b:X2}"));
			// 	var newExtraData = string.Join(' ', entryNew.MetaData.ExtraData.Take(entryNew.MetaData.NameLength).Select(b => $"{b:X2}"));
			//
			// 	var oldName = string.Join(' ', entryOld.MetaData.Name.Take(entryOld.MetaData.NameLength).Select(b => $"{b:X2}"));
			// 	var newName = string.Join(' ', entryNew.MetaData.Name.Take(entryNew.MetaData.NameLength).Select(b => $"{b:X2}"));
			// 	
			// 	if (i++ > 50)
			// 		break;
			// 	
			// 	sw.WriteLine($"{oldSeason} - Uid: 0x{entryOld.Uid:X16}" +
			// 	             $", Filetype: 0x{entryOld.MetaData.FileType:X8}" +
			// 	             $", Timestamp: 0x{entryOld.MetaData.Timestamp:X8}" +
			// 	             $", ExtraData: [{oldExtraData}]" +
			// 	             $", Name: [{oldName}]");
			// 		
			// 	sw.WriteLine($"{newSeason} - Uid: 0x{entryNew.Uid:X16}" +
			// 	             $", Filetype: 0x{entryNew.MetaData.FileType:X8}" +
			// 	             $", Timestamp: 0x{entryNew.MetaData.Timestamp:X8}" +
			// 	             $", ExtraData: [{newExtraData}]" +
			// 	             $", Name: [{newName}]");
			// 		
			// 	sw.WriteLine();
			// }

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