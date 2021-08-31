using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RainbowForge.Core;
using RainbowForge.Database;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using var br = new BinaryReader(File.Open("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_ondemand.depgraphbin", FileMode.Open));

			var dg = DepGraph.Read(br);
			var index = IndexUtil.CreateIndex("R:\\Siege Dumps\\Y6S1 v15500403");

			var uid = 0x000000156B735451u;

			FindChildren(index, dg.Structs, uid);

			Console.WriteLine("Done.");
		}

		private static void FindChildren(Dictionary<ulong, AssetPath> assetPaths, DepGraphEntry[] dgStructs, ulong uid, int indent = 0)
		{
			var assetPath = assetPaths[uid];
			Console.WriteLine($"{new string('\t', indent)}0x{uid:X16} - {assetPath.Filename} ({assetPath.Forge})");
			var children = dgStructs.Where(entry => entry.ParentUid == uid);
			foreach (var child in children)
			{
				FindChildren(assetPaths, dgStructs, child.ChildUid, indent + 1);
			}
		}
	}
}