using System;
using System.IO;
using RainbowForge.Model;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using var br = new BinaryReader(File.Open(
				"C:\\Users\\Admin\\RiderProjects\\RainbowForge\\Prism\\bin\\Debug\\net5.0-windows\\Quick Exports\\Character\\Addon_Shared_Operator_BaseBody_FirstPerson_Reflex3_copy.bin",
				FileMode.Open));

			var sk = Skeleton.Read(br);

			Console.WriteLine("Done.");
		}
	}
}