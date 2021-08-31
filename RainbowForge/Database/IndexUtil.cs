using System.Collections.Generic;
using System.IO;
using RainbowForge.Core;

namespace RainbowForge.Database
{
	public class IndexUtil
	{
		public static Dictionary<ulong, AssetPath> CreateIndex(string forgeDirectory)
		{
			var idTable = new Dictionary<ulong, AssetPath>();

			foreach (var file in Directory.GetFiles(forgeDirectory, "*.forge"))
			{
				var forgeFilename = Path.GetFileName(file);

				var forge = Forge.GetForge(file);

				foreach (var entry in forge.Entries)
				{
					idTable[entry.Uid] = new AssetPath(forgeFilename, entry.MetaData.FileName, null);

					// var container = forge.GetContainer(entry.Uid);
					// if (container is not ForgeAsset forgeAsset) continue;
					//
					// try
					// {
					// 	var assetStream = forgeAsset.GetDataStream(forge);
					// 	var arc = FlatArchive.Read(assetStream);
					//
					// 	foreach (var arcEntry in arc.Entries)
					// 	{
					// 		idTable[arcEntry.MetaData.Uid] = new AssetPath(file, entry.Uid);
					// 	}
					// }
					// catch (Exception e)
					// {
					// 	// TODO: some flat archives throw invalid frame descriptor
					// }
				}
			}

			return idTable;
		}
	}
}