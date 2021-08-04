using System;
using System.Collections.Generic;
using CommandLine;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Dump;
using RainbowForge.Forge;
using RainbowForge.Forge.Container;

namespace DumpTool
{
	[Verb("findallmeshprops", HelpText = "Find all MeshProperties containers which reference the given UID in all flat archives in the given forge file")]
	public class FindAllMeshPropsCommand
	{
		[Value(0, HelpText = "The forge file to reference")]
		public string ForgeFilename { get; set; }

		[Value(1, HelpText = "The UID to search for")]
		public ulong Uid { get; set; }

		public static void Run(FindAllMeshPropsCommand args)
		{
			var forge = Program.GetForge(args.ForgeFilename);
			foreach (var entry in forge.Entries)
				try
				{
					if (SearchFlatArchive(forge, entry, args.Uid))
						Console.WriteLine(entry.Uid);
				}
				catch (Exception e)
				{
					// Console.Error.WriteLine($"Error while dumping: {e}");
				}
		}

		public static bool SearchFlatArchive(Forge forge, Entry entry, ulong uid)
		{
			var container = forge.GetContainer(entry.Uid);
			if (container is not ForgeAsset forgeAsset || MagicHelper.GetFiletype(entry.Name.FileType) != AssetType.FlatArchive)
				return false;

			var assetStream = forgeAsset.GetDataStream(forge);
			var arc = FlatArchive.Read(assetStream);

			foreach (var meshProp in arc.Entries)
			{
				var unresolvedExterns = new List<ulong>();

				try
				{
					DumpHelper.SearchNonContainerChildren(assetStream, arc, meshProp, unresolvedExterns);
				}
				catch
				{
					// ignored
				}

				if (unresolvedExterns.Contains(uid))
					return true;
			}

			return false;
		}
	}
}