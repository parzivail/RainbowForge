using System;
using System.Collections.Generic;
using System.Linq;
using RainbowForge;
using RainbowForge.Archive;
using RainbowForge.Core;
using RainbowForge.Core.Container;
using RainbowForge.Model;
using RainbowForge.RenderPipeline;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var hashes = new HashSet<BoneId>();
			var i = 0;

			var forge = "R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_ondemand.forge";

			var f = Forge.GetForge(forge);

			foreach (var entry in f.Entries)
			{
				var container = f.GetContainer(entry.Uid);

				if (MagicHelper.GetFiletype(entry.MetaData.FileType) != AssetType.FlatArchive || container is not ForgeAsset fa)
					continue;

				var faDataStream = fa.GetDataStream(f);
				var arc = FlatArchive.Read(faDataStream);

				foreach (var faEntry in arc.Entries)
				{
					if (!MagicHelper.Equals(Magic.Mesh, faEntry.MetaData.FileType))
						continue;

					var mesh = Mesh.Read(arc.GetEntryStream(faDataStream.BaseStream, faEntry.MetaData.Uid));

					foreach (var bone in mesh.Bones)
					{
						hashes.Add(bone.Id);
						i++;
					}
				}
			}

			foreach (var id in hashes.ToArray())
			{
				if (Enum.IsDefined(typeof(BoneId), id))
				{
					Console.WriteLine($"{id:X8} == {(BoneId)id}");
					hashes.Remove(id);
				}
			}

			Console.WriteLine("Done.");
		}
	}
}