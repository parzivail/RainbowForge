using System.IO;
using RainbowScimitar;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using var br = new BinaryReader(File.Open("R:\\Siege Dumps\\Y6S1 v15500403\\datapc64_ondemand.forge", FileMode.Open));

			var sc = Scimitar.Read(br);
		}
	}
}