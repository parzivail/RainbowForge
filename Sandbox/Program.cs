using System;
using System.IO;
using RainbowScimitar.Scimitar;

namespace Sandbox
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var path = @"R:\Steam\steamapps\common\Tom Clancy's Rainbow Six Siege - Test Server";
			foreach (var forgeFilename in Directory.GetFiles(path, "*.forge"))
			{
				Console.Error.WriteLine(forgeFilename);

				using var fs = File.Open(forgeFilename, FileMode.Open);
				var bundle = Scimitar.Read(fs);

				foreach (var (uid, entry) in bundle.EntryMap)
				{
					if (!Scimitar.IsFile(uid))
						continue;

					var fte = bundle.GetFileEntry(entry);
					var mte = bundle.GetMetaEntry(entry);
					var name = mte.DecodeName(fte);

					var file = Scimitar.ReadFile(fs, fte);
					if (file.SubFileData.Length > 1)
					{
						using var stream = file.FileData.GetStream(fs);

						for (var i = 0; i < file.SubFileData.Length; i++)
						{
							var (subMeta, subStream) = file.GetSubFile(stream, i);
						}
					}
				}
			}

			Console.Error.WriteLine("Done.");
		}
	}
}