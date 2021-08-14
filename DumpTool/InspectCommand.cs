using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using RainbowForge;
using RainbowForge.Core;
using RainbowForge.Core.Container;

namespace DumpTool
{
	[Verb("inspect", HelpText = "Get details on the given asset")]
	public class InspectCommand
	{
		[Value(0, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		[Value(1, HelpText = "The UID to inspect")]
		public ulong Uid { get; set; }

		public static void Run(InspectCommand args)
		{
			var forge = Forge.GetForge(args.ForgeFilename);

			try
			{
				var entry = forge.GetContainer(args.Uid);
				var metaEntry = forge.Entries.First(entry1 => entry1.Uid == args.Uid);
				var magic = MagicHelper.GetFiletype(metaEntry.Name.FileType);

				Console.WriteLine($"UID: {metaEntry.Uid}");
				Console.WriteLine($"Offset: 0x{metaEntry.Offset:X}");
				Console.WriteLine($"End: 0x{metaEntry.End:X}");
				Console.WriteLine($"Size: 0x{metaEntry.Size:X}");
				Console.WriteLine("Name Table:");

				Console.WriteLine($"\tFile Magic: {magic}");
				DateTime date = DateTimeOffset.FromUnixTimeSeconds(metaEntry.Name.Timestamp).DateTime;
				Console.WriteLine($"\tTimestamp: {date} (epoch: {metaEntry.Name.Timestamp})");

				switch (entry)
				{
					case Descriptor d:
					{
						Console.WriteLine("Container: Descriptor");

						Console.WriteLine("String data:");
						foreach (var (k, v) in d.StringData)
							Console.WriteLine($"\"{k}\" = \"{v}\"");

						Console.WriteLine("Uint data:");
						foreach (var (k, v) in d.UIntData)
							Console.WriteLine($"\"{k}\" = {v}");

						Console.WriteLine("Ulong data:");
						foreach (var (k, v) in d.ULongData)
							Console.WriteLine($"\"{k}\" = {v}");

						break;
					}
					case Hash h:
					{
						Console.WriteLine("Container: Hash");
						Console.WriteLine($"Name: \"{h.Name}\"");
						Console.WriteLine($"Hash 1: \"{h.Hash1}\"");
						Console.WriteLine($"Hash 2: \"{h.Hash2}\"");
						break;
					}
					case ForgeAsset fa:
					{
						Console.WriteLine("Container: Forge Asset");
						Console.WriteLine($"Has Metadata Block: {fa.HasMeta}");
						break;
					}
					default:
					{
						Console.WriteLine("Unknown Container");
						break;
					}
				}
			}
			catch (KeyNotFoundException)
			{
				Console.Error.WriteLine($"No asset with UID {args.Uid} present in \"{args.ForgeFilename}\"");
			}
		}
	}
}