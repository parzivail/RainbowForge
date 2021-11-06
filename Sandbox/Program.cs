using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssetDiff;
using RainbowForge;
using RainbowForge.Components;
using RainbowScimitar.DataTypes;
using RainbowScimitar.Scimitar;
using R6AIWorldComponent = RainbowScimitar.DataTypes.R6AIWorldComponent;

namespace Sandbox
{
	internal class Program
	{
		public class FileSysCall
		{
			[JsonPropertyName("file")] public string File { get; set; }

			[JsonPropertyName("offset")] public long Offset { get; set; }

			[JsonPropertyName("length")] public int Length { get; set; }
		}

		private static void Main(string[] args)
		{
			// var ttsPath = @"R:\Siege Dumps\Y6S3 v33861727";
			var ttsPath = @"R:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege - Test Server";
			var dumpPath = @"R:\Siege Dumps\Annotated\Character";

			// // foreach (var file in Directory.GetFiles(@"R:\Siege Dumps\Annotated\depgraphbin", "*.bin"))
			// {
			// 	var file = @"R:\Siege Dumps\Annotated\Character\0000000000000800_Game Bootstrap - 0000003F3DFB28F0_ASH 43377of173118.Character.bin";
			// 	using var dfs = File.OpenRead(file);
			// 	var r = new BinaryReader(dfs);
			// 	var d = Character.Read(r);
			// }
			//
			// return;

			// // foreach (var filename in Directory.GetFiles(dumpPath, "*.bin"))
			// // foreach (var filename in Directory.GetFiles(ttsPath, "*.forge"))
			// {
			// 	var filename = @"R:\Siege Dumps\Y6S3 v33861727\datapc64_mtx_bnk_soundbank.forge";
			// 	// try
			// 	{
			// 		using var fs = File.OpenRead(filename);
			// 		var sc = Scimitar.Read(fs);
			// 		
			// 		using var fso = File.OpenWrite(filename + ".repacked.forge");
			// 		sc.Write(fso);
			// 	}
			// 	// catch (IOException e)
			// 	// {
			// 	// 	Console.WriteLine($"Error ({Path.GetFileName(filename)}): {e.Message}");
			// 	// }
			// }
			//
			// return;

			var parsers = new Dictionary<Magic, Func<BinaryReader, object>>
			{
				[Magic.BoundingVolume] = BoundingVolume.Read,
				[Magic.CompiledSoundMedia] = CompiledSoundMedia.Read,
				[Magic.GIBoundingVolume] = GIBoundingVolume.Read,
				[Magic.GISettings] = GISettings.Read,
				[Magic.TagClient] = TagClient.Read,
				[Magic.IColor] = IColor.Read,
				[Magic.LoadUnit] = LoadUnit.Read,
				[Magic.R6AIWorldComponent] = R6AIWorldComponent.Read,
				[Magic.SoundID] = SoundId.Read,
				[Magic.SoundState] = SoundState.Read,
				[Magic.SpaceComponentNode] = SpaceComponentNode.Read,
				[Magic.SpaceManager] = SpaceManager.Read,
				[Magic.WindDefinition] = WindDefinition.Read,
				[Magic.World] = World.Read,
				[Magic.WorldDivisionToTagLoadUnitLookup] = WorldDivisionToTagLoadUnitLookup.Read,
				[Magic.WorldGraphicData] = WorldGraphicData.Read,
				[Magic.WorldLoader] = WorldLoader.Read,
				[Magic.WorldLoadUnit_LoadByTag] = WorldLoadUnitLoadByTag.Read,
				[Magic.WorldLoadUnit_WorldDivision] = WorldLoadUnitWorldDivision.Read,
			};

			// Console.WriteLine("Loading index...");
			// var index = AssetIndex.LoadCompleteIndex(Path.Combine(ttsPath, "index.sbidx"));
			//
			// Console.WriteLine("Generating SDK class name table...");
			// var lines = File.ReadLines(@"R:\Siege Dumps\all_names.txt");
			// var nameTable = new Dictionary<uint, string>();
			//
			// foreach (var line in lines) 
			// 	nameTable[Crc32.Compute(Encoding.ASCII.GetBytes(line))] = line;
			//
			// foreach (var file in Directory.GetFiles(dumpPath, "*.bin"))
			// 	MarkKnownValues(file, index, nameTable, parsers);
			//
			// Console.WriteLine("Done.");
			// return;

			// foreach (var forgeFilename in Directory.GetFiles(ttsPath, "*.forge"))
			{
				var forgeFilename = @"R:\Siege Dumps\R6E_TT_v906451\datapc64.forge";
				// var forgeFilename = @"R:\Siege Dumps\Y6S3 v33861727\datapc64_merged_bnk_soundmedia_en-us.forge";
				// if (forgeFilename.Contains("bnk") || Path.GetFileName(forgeFilename) != "datapc64_pvp16_ibiza.forge")
				// 	continue;
				// if (forgeFilename.Contains("bnk"))
				// 	continue;

				// Console.Error.WriteLine(forgeFilename);
				Console.Write('.');

				const int KiB = 1024;
				const int MiB = KiB * 1024;
				using var fs = new FileStream(forgeFilename, FileMode.Open, FileAccess.Read, FileShare.Read, 4 * MiB);
				var bundle = Scimitar.Read(fs);

				foreach (var (uid, entry) in bundle.EntryMap)
				{
					if (!Scimitar.IsFile(uid))
						continue;

					var fte = bundle.GetFileEntry(entry);
					var mte = bundle.GetMetaEntry(entry);
					var name = mte.DecodeName(fte);

					Console.WriteLine(name);

					var file = Scimitar.ReadFile(fs, fte);
					if (file.SubFileData.Length == 0)
						continue;

					var stream = file.FileData.GetStream(fs);

					for (var i = 0; i < file.SubFileData.Length; i++)
					{
						var (subMeta, subStream) = file.GetSubFile(stream, i);

						if (!MagicHelper.Equals(Magic.LocalizationPackage, subMeta.FileType))
							continue;

						var br2 = new BinaryReader(subStream);
						br2.ReadBytes(12);
						var cld = CompressedLocalizationData.Read(br2);

						File.AppendAllLines("strings.txt", cld.Strings);

						// var fname = $"{fte.Uid:X16}_{Trim(name)} - {subMeta.Uid:X16}_{Trim(subMeta.Filename)} {i}of{file.SubFileData.Length}.{((Magic)subMeta.FileType)}.bin";
						// fname = Path.GetInvalidFileNameChars().Aggregate(fname, (current, c) => current.Replace(c, '_'));
						// using var outfs = new FileStream(Path.Combine(dumpPath, fname), FileMode.Create, FileAccess.Write, FileShare.None);
						// subStream.Seek(0, SeekOrigin.Begin);
						// subStream.CopyTo(outfs);

						continue;

						var br = new BinaryReader(subStream);
						while (subStream.Position < subStream.Length - sizeof(long))
						{
							var needle = br.ReadUInt64();

							// datapc64.forge: 0000000000000800/Game Bootstrap > 0000000000000800/Game Bootstrap (idx 0) @ 104
							//  > datapc64.forge: 0000000000000800/Game Bootstrap > 0000000000000962/R6Configs (idx 11) @ 300
							//    > datapc64.forge: 0000000000000800/Game Bootstrap > 0000000041972753/R6Configs - Online (idx 384) @ 12
							//      > 000000009D965AC8/R6Siege_PlayListConfig

							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 540
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 1254
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 1450
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 1755
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 2241
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000009D965AC8/R6Siege_PlayListConfig (idx 12517) @ 2554
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DE04/Newcomer_Playlist (idx 55281) @ 89
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DCC7/QuickMatch_Playlist (idx 55282) @ 281
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DCC7/QuickMatch_Playlist (idx 55282) @ 715
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DDC4/Unranked_Playlist (idx 55283) @ 257
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DDC4/Unranked_Playlist (idx 55283) @ 571
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DDDA/Ranked_Playlist (idx 55284) @ 257
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DDDA/Ranked_Playlist (idx 55284) @ 571
							// datapc64.forge: 0000000000000800/Game Bootstrap > 000000391891DE0A/TerroristHunt_Playlist (idx 55285) @ 401
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFCCC/PVP16-Ibiza-Extraction_Day_i_Ranked (idx 90654) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFC57/PVP16-Ibiza-Extraction_Day (idx 90675) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFCD6/PVP16-Ibiza-PlantBomb_Day_i_Ranked (idx 90697) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000030942295A3/PVP16-Ibiza-PlantBomb_Day_i_Unranked (idx 90718) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFC5B/PVP16-Ibiza-PlantBomb_Day (idx 90739) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFCE0/PVP16-Ibiza-SecureArea_Day_i_Ranked (idx 90760) @ 183
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFC59/PVP16-Ibiza-SecureArea_Day (idx 90781) @ 183
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB56/PVE16-Ibiza-Extraction_ATK_COOP_Easy_Day (idx 90803) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBAB/PVE16-Ibiza-Extraction_ATK_COOP_Medium_Day (idx 90824) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBF7/PVE16-Ibiza-Extraction_ATK_COOP_Hard_Day (idx 90845) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB67/PVE16-Ibiza-Extraction_DEF_COOP_Easy_Day (idx 90867) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBAD/PVE16-Ibiza-Extraction_DEF_COOP_Medium_Day (idx 90888) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBF9/PVE16-Ibiza-Extraction_DEF_COOP_Hard_Day (idx 90909) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB73/PVE16-Ibiza-PlantBomb_ATK_COOP_Easy_Day (idx 90931) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBAF/PVE16-Ibiza-PlantBomb_ATK_COOP_Medium_Day (idx 90952) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBFB/PVE16-Ibiza-PlantBomb_ATK_COOP_Hard_Day (idx 90973) @ 145
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB77/PVE16-Ibiza-TerroHuntClassic_Easy_Day (idx 90995) @ 151
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBB1/PVE16-Ibiza-TerroHuntClassic_Medium_Day (idx 91016) @ 151
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBFD/PVE16-Ibiza-TerroHuntClassic_Hard_Day (idx 91037) @ 151
							// datapc64.forge: 0000000000000800/Game Bootstrap > 0000000ABFAC1C27/PVP16-Ibiza-Canister_Day (idx 91165) @ 169
							// datapc64.forge: 0000000000000800/Game Bootstrap > 0000000ABFAC1C28/PVP16-Ibiza-Canister_Night (idx 91166) @ 173
							// datapc64.forge: 0000000000000800/Game Bootstrap > 0000000AA097C9C3/PVP16-Ibiza-Canister_Day_Ranked (idx 91196) @ 185
							// datapc64.forge: 0000000000000800/Game Bootstrap > 0000000AA097C9C8/PVP16-Ibiza-Canister_Night_Ranked (idx 91197) @ 189
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000032112459E9/PVP16-Ibiza-Extraction_i_Night (idx 122543) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000032112459EB/PVP16-Ibiza-SecureArea_i_Night (idx 122557) @ 183
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000032112459EA/PVP16-Ibiza-PlantBomb_i_Night (idx 122570) @ 181
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB57/PVE16-Ibiza-Extraction_ATK_COOP_Easy_Night (idx 131183) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB6C/PVE16-Ibiza-Extraction_DEF_COOP_Easy_Night (idx 131184) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB74/PVE16-Ibiza-PlantBomb_ATK_COOP_Easy_Night (idx 131185) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFB78/PVE16-Ibiza-TerroHuntClassic_Easy_Night (idx 131186) @ 155
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBAC/PVE16-Ibiza-Extraction_ATK_COOP_Medium_Night (idx 131246) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBAE/PVE16-Ibiza-Extraction_DEF_COOP_Medium_Night (idx 131247) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBB0/PVE16-Ibiza-PlantBomb_ATK_COOP_Medium_Night (idx 131248) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBB2/PVE16-Ibiza-TerroHuntClassic_Medium_Night (idx 131249) @ 155
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBF8/PVE16-Ibiza-Extraction_ATK_COOP_Hard_Night (idx 131308) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBFA/PVE16-Ibiza-Extraction_DEF_COOP_Hard_Night (idx 131309) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBFC/PVE16-Ibiza-PlantBomb_ATK_COOP_Hard_Night (idx 131310) @ 149
							// datapc64.forge: 0000000000000800/Game Bootstrap > 00000009D57FFBFE/PVE16-Ibiza-TerroHuntClassic_Hard_Night (idx 131311) @ 155

							// > datapc64_pvp16_ibiza.forge: 00000009CCC3D997/PVP16_Ibiza > 00000009CCC3D997/PVP16_Ibiza (idx 0) @ 11747
							//   > datapc64_pvp16_ibiza.forge: 0000000A10795033/WorldSection01_DivLU > 0000000A10795033/WorldSection01_DivLU (idx 0) @ 20280
							//     > datapc64_pvp16_ibiza.forge: 0000000A10795033/WorldSection01_DivLU > 0000000B12AD171A/PVP16_EXT_VEH_SportCar_A_003 (idx 2534) @ 202 (multiple)
							//       > datapc64_pvp16_ibiza.forge: 0000000A10795033/WorldSection01_DivLU > 0000000B12AC9F54/PVP16_EXT_VEH_SportCar_A_LOD0 (idx 11174) @ 68
							//         > compiled mesh uid: 0x0000000B12AC9F5D

							if (needle == 0x0000000000000962)
							{
								Console.WriteLine($"{forgeFilename}: {fte.Uid:X16}/{name} > {subMeta.Uid:X16}/{subMeta.Filename} (idx {i}) @ {subStream.Position}");
							}

							subStream.Seek(-sizeof(long) + 1, SeekOrigin.Current);
						}
					}
				}
			}

			Console.Error.WriteLine("Done.");
		}

		private static string Trim(string s)
		{
			const int length = 64;
			return s.Length < length ? s : s[..length];
		}

		private static void MarkKnownValues(string binFilename, AssetIndex index, Dictionary<uint, string> nameTable, Dictionary<Magic, Func<BinaryReader, object>> parsers)
		{
			Console.WriteLine($"Matching {binFilename}");
			using var br = new BinaryReader(File.OpenRead(binFilename));

			var hp = new HighlightProvider();

			string[] colors =
			{
				"Yellow",
				"Fuchsia",
				"0, 128, 255",
				"0, 255, 64",
				"0, 64, 128",
				"0, 64, 192",
				"0, 128, 64",
				"128, 255, 255",
				"255, 128, 0",
			};
			var colorIdx = 0;

			string GetNextColor(uint color = 0)
			{
				if (color != 0)
				{
					var r = color & 0xFF;
					var g = (color >> 8) & 0xFF;
					var b = (color >> 16) & 0xFF;
					return $"{r}, {g}, {b}";
				}

				var s = colors[colorIdx];
				colorIdx++;
				colorIdx %= colors.Length;
				return s;
			}

			for (var i = 0; i <= br.BaseStream.Length - sizeof(long); i++)
			{
				br.BaseStream.Seek(i, SeekOrigin.Begin);

				var qWord = br.ReadUInt64();
				var id = (ScimitarId)qWord;

				var loDWord = (uint)(qWord & 0xFFFFFFFF);

				if (index.Data.ContainsKey(id))
				{
					var data = index.Data[id];
					hp.Highlight(i, i + sizeof(ulong) - 1, new FakeBrush(GetNextColor()), $"UID -> {data.Name} (child of {data.ParentId.Id:X16})");
				}
				else if (id.IsRelative)
				{
					hp.Highlight(i, i + sizeof(ulong) - 1, new FakeBrush(GetNextColor()), $"UID <> {id.RelativeIndex}");
				}
				// else if (parsers.ContainsKey((Magic)loDWord))
				// {
				// 	var pos = br.BaseStream.Seek(-8, SeekOrigin.Current);
				// 	parsers[(Magic)loDWord].Invoke(br);
				// 	hp.Highlight(pos, br.BaseStream.Position - 1, new FakeBrush(GetNextColor()), $"{(Magic)loDWord}", -1);
				// }
				else if (loDWord > 0 && Enum.IsDefined(typeof(Magic), (ulong)loDWord))
				{
					hp.Highlight(i, i + sizeof(uint) - 1, new FakeBrush(GetNextColor(loDWord)), $"Magic == {(Magic)loDWord}");
				}
				else if (nameTable.ContainsKey(loDWord))
				{
					hp.Highlight(i, i + sizeof(uint) - 1, new FakeBrush(GetNextColor(loDWord)), $"SDK == {nameTable[loDWord]}");
				}
			}

			File.WriteAllText(binFilename + ".metadata", JsonSerializer.Serialize(new MetadataContainer(hp)));
		}
	}

	public record MetadataContainer(HighlightProvider HighlightProvider);

	/// <summary>
	///     Provides a way to highlight specific byte ranges
	/// </summary>
	public class HighlightProvider
	{
		/// <summary>
		///     The brushes and ranges to highlight
		/// </summary>
		public List<KeyValuePair<ByteRange, Highlight>> Highlights { get; private set; }

		/// <summary>
		///     Initializes a new HighlightProvider
		/// </summary>
		public HighlightProvider()
		{
			Highlights = new List<KeyValuePair<ByteRange, Highlight>>();
		}

		/// <summary>
		///     Checks to see if a specific byte is highlighted
		/// </summary>
		/// <param name="l">The byte to check</param>
		/// <returns>True if the byte is highlighted</returns>
		public bool IsHighlighted(long l)
		{
			return Highlights.Any(highlightBrush => highlightBrush.Key.Contains(l) && highlightBrush.Value.Visible);
		}

		/// <summary>
		///     Gets the highlight color for a byte
		/// </summary>
		/// <param name="l">The byte to check</param>
		/// <returns>The brush for that byte</returns>
		public FakeBrush GetHighlightBrush(long l)
		{
			return
				(from highlightBrush in Highlights
					where highlightBrush.Key.Contains(l)
					select highlightBrush.Value.Brush).FirstOrDefault();
		}

		/// <summary>
		///     Highlights a section
		/// </summary>
		/// <param name="start">The start of the byte range, inclusive</param>
		/// <param name="end">The end of the byte range, inclusive</param>
		/// <param name="b">The brush to highlight with</param>
		/// <param name="name">The name of the highlight</param>
		/// <param name="priority">The priority of the highlight. Lower numbers are lower priority.</param>
		/// <param name="visible">True if the highlight is currently visible</param>
		public void Highlight(long start, long end, FakeBrush b, string name = "Highlight", int priority = 0,
			bool visible = true)
		{
			Highlights.Add(new KeyValuePair<ByteRange, Highlight>(new ByteRange(start, end),
				new Highlight { Brush = b, Name = name, Priority = priority, Visible = visible }));
			Highlights = (from entry in Highlights orderby entry.Value.Priority descending select entry).ToList();
		}
	}

	public class Highlight
	{
		public FakeBrush Brush { get; set; }
		public string Name { get; set; }
		public int Priority { get; set; }
		public bool Visible { get; set; }
	}

	public record FakeBrush(string Color);

	public class ByteRange
	{
		public ByteRange(long start, long end)
		{
			Start = start;
			End = end;
		}

		public long Start { get; set; }
		public long End { get; set; }

		public bool Contains(long l)
		{
			return l >= Start && l <= End;
		}
	}
}