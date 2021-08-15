using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Dump;

namespace Sandbox
{
	internal class Program
	{
		private static readonly HashSet<uint> UnknownMagics = new()
		{
			0x1EB4394F, 0x1FB57CD5, 0x2013DBD0, 0x20315192, 0x20A7F8DB, 0x21943A63, 0x23356DD3, 0x235C0560, 0x2372F68D, 0x239EC16A, 0x23BF9925, 0x23CD3131, 0x24D19860, 0x25246116, 0x252D9654,
			0x25B46ADB, 0x25E81D4E, 0x27966F42, 0x28435F00, 0x28941138, 0x28C63628, 0x297D86AF, 0x2A1CD834, 0x2AFED9F5, 0x2B1E772F, 0x2C1329D2, 0x2C62E10B, 0x2DEA8F56, 0x2EE13FC3, 0x2F181F2A,
			0x2F732FDB, 0x2F8211BE, 0x2F9A4C63, 0x30199DD2, 0x3033C2F5, 0x305DAAF0, 0x307F9215, 0x30B11EF1, 0x314AECEE, 0x319D3B23, 0x31B492A9, 0x31EDEB8F, 0x3201FD47, 0x328A37EF, 0x329B6C35,
			0x331885FB, 0x334B31A2, 0x3362A659, 0x337544B7, 0x33A3568D, 0x33E7DDEF, 0x33ECCB02, 0x340378D4, 0x345D8BAB, 0x34834281, 0x3527D1AE, 0x35A43414, 0x35CB395B, 0x360AF241,
			0x362E52EF, 0x36BE7C80, 0x36DD627F, 0x3766A74D, 0x37DC8B92, 0x37FDE164, 0x38866309, 0x38D82699, 0x38D9651C, 0x38E25AF8, 0x394B591A, 0x39895C86, 0x39B65F1C, 0x3A2E6B70, 0x3A8EBDC4,
			0x3BC05E8A, 0x3BD86505, 0x3ECFFBE9, 0x3F132C04, 0x3F49D5C9, 0x3FCCEF27, 0x415C097D, 0x421B4E2C, 0x4221C364, 0x42711A6B, 0x42A270E2, 0x42A73527, 0x431843B9,
			0x431F8D59, 0x43CDE9D1, 0x43CEDB18, 0x43D7EC0D, 0x44929C4E, 0x44932C9B, 0x44BC3648, 0x44CC846A, 0x44F584B2, 0x4663080B, 0x46863A3E, 0x47266B85, 0x47B5DECD, 0x4802A946,
			0x489C5552, 0x48B37D86, 0x492C212A, 0x4A2B1E69, 0x4A798445, 0x4A8D785E, 0x4B00F860, 0x4B2C6170, 0x4BDEDF64, 0x4CAED6B4, 0x4DD323EF, 0x4E4C84DB, 0x4EB4DD16, 0x4EC26571, 0x500E5AA7,
			0x50E8BCD5, 0x50E9F706, 0x51066FDD, 0x5199247E, 0x522559E1, 0x525B5254, 0x53510945, 0x5361FE09, 0x55C79DF9, 0x55FED664, 0x56C88C7F, 0x57E53D80,
			0x580CB97B, 0x5833D2FF, 0x584879B2, 0x587515A2, 0x58A5FA99, 0x58B67BB9, 0x5A2916ED, 0x5B4434F9, 0x5B66A12C, 0x5B99D7EE, 0x5C53C116, 0x5C68E246, 0x5C6DB8EE,
			0x5CB07EA0, 0x5CC5B62D, 0x5CD67520, 0x5E287E30, 0x5E748F79, 0x5E78DF4D, 0x5EC04AA3, 0x5EF32472, 0x5FB5B6AC, 0x6009EB19, 0x601620B7, 0x61517D87, 0x61CDA86C, 0x61DBE0D4, 0x61E700A1,
			0x6200CD51, 0x6221143B, 0x62521AFA, 0x625EA9A5, 0x62940EDE, 0x633C50A5, 0x6365DDB5, 0x63DE24AF, 0x650BC5F7, 0x650F34CF, 0x654B677E, 0x66170928, 0x6619D57A, 0x669403B9, 0x6741C390,
			0x677C81B5, 0x67B7180C, 0x67D4BB33, 0x68D14966, 0x69294D15, 0x6996E7AE, 0x69FDD4B0, 0x6A3CCB87, 0x6A52331D, 0x6A7D34A9, 0x6AB6B15A, 0x6AFDC2A9, 0x6B065046, 0x6B231FF1, 0x6BE35F39,
			0x6BEC97E0, 0x6C95705C, 0x6C95ED4B, 0x6C98B26A, 0x6CCAF448, 0x6D5ABB38, 0x6DD2D2B2, 0x6DDC8C93, 0x6EC51CE8, 0x6EDA4278, 0x6F74DDB4, 0x6F809E8D, 0x701AAFD9, 0x7094C8C0,
			0x709BB47B, 0x71288716, 0x72492CC9, 0x72FCB11D, 0x73D5049A, 0x74DF1F46, 0x7534775B, 0x757A9B6A, 0x759B2F0C, 0x76461ED4, 0x768983D5, 0x769234F3, 0x76D5826F, 0x772F30F9,
			0x77EEEBB4, 0x78153A75, 0x78826E3F, 0x79D5DC5D, 0x7A57CFE8, 0x7A9E1890, 0x7B1E209F, 0x7B956A71, 0x7BCB6B7D, 0x7D61B237, 0x7DC0C72E, 0x7DC13543, 0x7E071365, 0x7E90DAF3, 0x7ED1E02F,
			0x7EFC7BBC, 0x7EFE24E6, 0x7F33044E, 0x802CF26E, 0x805F5E74, 0x807B632C, 0x81416D84, 0x823B0214, 0x82CA892F, 0x8331998F, 0x83B706A1, 0x83E1A167, 0x83EF4293, 0x840757DE, 0x84EC653F,
			0x854A3D0B, 0x85BE0AE0, 0x867272AD, 0x870333E7, 0x8766EFE0, 0x8781BA45, 0x87DC6BA2, 0x87EA637F, 0x87EEAF47, 0x88619F2B, 0x88723072, 0x8882306C, 0x88AEAEB2, 0x8A4E6BA2,
			0x8AD757D8, 0x8CD460EF, 0x8D1D966B, 0x8D255754, 0x8DA57685, 0x8DC233A0, 0x8E566044, 0x8EA0B8DA, 0x8EC8EFA3, 0x8F5197B5, 0x8FC83A83, 0x8FFC66AF, 0x906F8AFA, 0x90C46F67,
			0x91860285, 0x919BC0C1, 0x91A51884, 0x91DFE9EC, 0x92005DF2, 0x92487281, 0x9259E092, 0x927ED8BA, 0x92B7B9C4, 0x92F31FA2, 0x936B86B6, 0x93F555A4, 0x942EAEDC, 0x9448465F, 0x9493DAE5,
			0x950CCA15, 0x9537DEF3, 0x960ADC3B, 0x963C6585, 0x965F7A70, 0x968EB336, 0x96CACB2E, 0x96EADACB, 0x97164A5A, 0x97D8B660, 0x98941647, 0x98AB6F5F,
			0x99434A89, 0x997493CF, 0x99BEC7AC, 0x9A1C7FA6, 0x9B19F386, 0x9B3BB0F4, 0x9B4018D4, 0x9B40DEE7, 0x9B5A2DDD, 0x9B7A3615, 0x9B8921EF, 0x9BB2CDF2, 0x9C3CF247, 0x9C746BA2, 0x9CB7D50B,
			0x9CD86D47, 0x9DB1FD8E, 0x9DEB0049, 0x9EEEEE64, 0x9F7CC8DE, 0x9FCCFC0F, 0x9FCDC4C4, 0xA04ED9F8, 0xA1063448, 0xA1111DBC, 0xA1CA4B23, 0xA28319BE, 0xA2D01604, 0xA3ED4D0E, 0xA43332F8,
			0xA4729FC9, 0xA56C8CC5, 0xA5863E1C, 0xA59508E9, 0xA64AA8EA, 0xA6EA7232, 0xA7408587, 0xA818CD8D, 0xA89F658D, 0xA96BA120, 0xA9892F4D, 0xA9A2526B, 0xA9DF6179, 0xA9F08594, 0xAA97BA5F,
			0xAAC174F3, 0xAB65D3FB, 0xABD50170, 0xABF1126E, 0xAC448642, 0xACB24881, 0xAD2D511F, 0xAD65B916, 0xAD823C99, 0xADED9F2D, 0xAE148772, 0xAEC77F9A, 0xAEF1B01E, 0xAF296DB7, 0xAF43CA83,
			0xAF5106DC, 0xAF53B819, 0xAF7EAE1C, 0xAFBFA30A, 0xB033548F, 0xB03D110F, 0xB09DBCA5, 0xB0DFE3A2, 0xB11FC381, 0xB1385123, 0xB1420AD1, 0xB19D4FCD,
			0xB2314E6A, 0xB25C10A6, 0xB2A34FD6, 0xB384D6BA, 0xB3E0418E, 0xB3E69826, 0xB3FC5A70, 0xB437504C, 0xB4B2B9C2, 0xB53E8731, 0xB578ED58, 0xB648C1DF, 0xB660EA83, 0xB66B7701,
			0xB6AA66C5, 0xB8D30640, 0xB8D39F2C, 0xB92FA8AC, 0xB93410FB, 0xBAF2B765, 0xBAFB153E, 0xBB5542A0, 0xBBA2898E, 0xBBD29395, 0xBCC33D21, 0xBD036711, 0xBDE2D3B6, 0xBE5DE60D,
			0xBEA18D0F, 0xBF95D157, 0xBFE86D68, 0xBFF0DE83, 0xC10BB5D1, 0xC112A19D, 0xC164129B, 0xC2BB2B7B, 0xC3692044, 0xC38BA632, 0xC41C7553, 0xC56394EC, 0xC56BD2E2, 0xC56C24BF,
			0xC62947F6, 0xC65B4C82, 0xC6C17A7E, 0xC6D9C255, 0xC73269C3, 0xC75792F9, 0xC7623831, 0xC7D8141C, 0xC7EA0284, 0xC8241D45, 0xC8A9EA4D, 0xC8C42C4A, 0xC9208D05,
			0xC9BB7AEA, 0xC9C108A7, 0xC9D94E7B, 0xCA7276F6, 0xCB12519E, 0xCBD05F03, 0xCBD4939A, 0xCBF34700, 0xCC0308BC, 0xCD3A69BB, 0xCE4A0B93, 0xCEA4CA0E, 0xD097D3CC, 0xD0BF3717, 0xD1082F1C,
			0xD11602E4, 0xD1339DA0, 0xD26A7C20, 0xD26D9DC9, 0xD28AECC8, 0xD3264858, 0xD38F00F7, 0xD4400C98, 0xD4FF0A39, 0xD523C20F, 0xD5B8BCCF, 0xD5DD2E78, 0xD62CBF1B, 0xD6511707, 0xD74F36D1,
			0xD832DDF2, 0xD869438B, 0xD8D4A3B7, 0xD9261E2F, 0xDAD658B0, 0xDB673D78, 0xDB7A4E87, 0xDBAA67AC, 0xDC2518CD, 0xDD3175E0, 0xDD535A95, 0xDD743C79, 0xDE19A21F, 0xDE8C31D8, 0xDFA222C0,
			0xE0DC44A6, 0xE11D37B0, 0xE2047AF0, 0xE23D8547, 0xE2B1DA63, 0xE429F928, 0xE46C4EFE, 0xE4C879CB, 0xE4DA998A, 0xE5929EEB, 0xE6EA7903, 0xE6F88917, 0xE785230F, 0xE7EB3EA2, 0xE802B9DA,
			0xE8301D07, 0xE9B7B975, 0xEA266162, 0xEA9FA35E, 0xEAA269F9, 0xEB02A76C, 0xEBEE4F97, 0xEC1AE5CE, 0xEC3826FF, 0xEC5E34FC, 0xEC84C5A8, 0xECBCB42A, 0xECD39854, 0xECEB61FF, 0xF080B73C,
			0xF08FF8E1, 0xF0B801F4, 0xF0D30C80, 0xF0FC923B, 0xF1A4FFE8, 0xF2909128, 0xF299B284, 0xF2B7EF38, 0xF38AD0D3, 0xF3E453E6, 0xF40A6466, 0xF46703EF, 0xF4A4EA3C, 0xF4FF820A, 0xF504EECC,
			0xF5957839, 0xF5AC3340, 0xF622AE46, 0xF68E6B43, 0xF69F0F3A, 0xF776ED2A, 0xF827AF5A, 0xF83984F4, 0xF868521C, 0xF894F45C, 0xF8C91259, 0xF8E187EA, 0xF8F4D2F9, 0xF9547F6E, 0xF97A20D9,
			0xFA32E3AF, 0xFA53A1B9, 0xFA961D1B, 0xFABAE07A, 0xFB1080A9, 0xFB364970, 0xFB75DFB2, 0xFB911AC9, 0xFC6CDAC0, 0xFCE622E2, 0xFD73E329, 0xFDA4B79A, 0xFE15F469, 0xFE41FE3A,
			0xFF138919, 0xFFC937C2
		};

		private static void Main(string[] args)
		{
			var magics = new Dictionary<ulong, int>();

			foreach (var filename in Directory.GetFiles("R:\\Siege Dumps\\Y6S1 v15500403", "*.forge"))
			{
				Console.WriteLine(filename);

				var forge = Forge.GetForge(filename);

				foreach (var entry in forge.Entries)
				{
					if (!magics.ContainsKey(entry.Name.FileType))
						magics[entry.Name.FileType] = 0;

					magics[entry.Name.FileType]++;

					if (MagicHelper.GetFiletype(entry.Name.FileType) != AssetType.FlatArchive) continue;

					var container = forge.GetContainer(entry.Uid);
					if (container is not ForgeAsset forgeAsset) throw new InvalidDataException("Container is not asset");

					var assetStream = forgeAsset.GetDataStream(forge);
					var fa = FlatArchive.Read(assetStream);

					foreach (var fae in fa.Entries)
					{
						if (!magics.ContainsKey(fae.Meta.Magic))
							magics[fae.Meta.Magic] = 0;

						magics[fae.Meta.Magic]++;

						if (fae.Meta.Magic == 0x74F7311D) DumpHelper.DumpBin($"R:\\Siege Dumps\\Unpacked\\{fae.Meta.Uid}.bin", fa.GetEntryStream(assetStream.BaseStream, fae.Meta.Uid).BaseStream);
					}
				}
			}

			Console.Clear();

			foreach (var (magic, count) in magics.OrderByDescending(arg => arg.Value))
				if (!Enum.IsDefined(typeof(Magic), magic))
					Console.WriteLine($"0x{magic:X8} - {count}");
				else
					Console.WriteLine($"0x{magic:X8} ({(Magic)magic}) - {count}");

			// using var sr = new StreamReader("E:\\colby\\Desktop\\temp\\output.txt");
			// using var sw = new StreamWriter("E:\\colby\\Desktop\\temp\\words.txt");
			//
			// var knownLines = new List<string>();
			//
			// while (!sr.EndOfStream)
			// {
			// 	var line = sr.ReadLine()!.ToLower();
			// 	if (Regex.IsMatch(line, "[^a-zA-Z]") || knownLines.Contains(line)) continue;
			// 	
			// 	knownLines.Add(line);
			// 	sw.WriteLine(line);
			// }

			// using var sr = new StreamReader("E:\\colby\\Desktop\\temp\\ac_names.txt");
			// var words = sr.ReadToEnd().Split("\r\n").Select(UpperFirst).ToList();
			// words.Add("");
			//
			// var results = new ConcurrentDictionary<uint, List<string>>();
			//
			// var numWords = words.Count;
			// var completedWords = 0;
			//
			// Parallel.ForEach(words, wordA =>
			// {
			// 	var bufferBytes = new byte[256];
			// 	foreach (var wordB in words)
			// 	{
			// 		foreach (var wordC in words)
			// 		{
			// 			var lineLength = wordA.Length + wordB.Length + wordC.Length;
			//
			// 			for (var i = 0; i < wordA.Length; i++)
			// 				bufferBytes[i] = (byte)wordA[i];
			//
			// 			for (var i = 0; i < wordB.Length; i++)
			// 				bufferBytes[i + wordA.Length] = (byte)wordB[i];
			//
			// 			for (var i = 0; i < wordC.Length; i++)
			// 				bufferBytes[i + wordA.Length + wordB.Length] = (byte)wordC[i];
			//
			// 			var crc = Crc32.CalculateHash(Crc32.DefaultSeed, bufferBytes, 0, lineLength);
			// 			if (!UnknownMagics.Contains(crc)) continue;
			//
			// 			if (!results.ContainsKey(crc))
			// 				results[crc] = new List<string>();
			//
			// 			results[crc].Add($"{wordA}{wordB}{wordC}");
			// 		}
			// 	}
			//
			// 	Interlocked.Increment(ref completedWords);
			// 	Console.WriteLine($"{completedWords / (float)numWords * 100:F2}% - {wordA}");
			// });
			//
			// using var sw = new StreamWriter("out.txt");
			// foreach (var (magic, possibleValues) in results)
			// {
			// 	sw.WriteLine($"0x{magic:X8}");
			//
			// 	foreach (var value in possibleValues)
			// 		sw.WriteLine($"\t{value}");
			//
			// 	sw.WriteLine();
			// }

			Console.WriteLine("Done.");
		}

		private static string UpperFirst(string arg)
		{
			return char.ToUpper(arg[0]) + arg[1..];
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