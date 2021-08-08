using System;
using System.IO;

namespace RainbowForge
{
    public class FileSystemUtil
    {
		public static void AssertFileExists(string filename)
		{
			if (File.Exists(filename)) return;

			Console.Error.WriteLine($"File not found: {filename}");
			Environment.Exit(-1);
		}

		public static void AssertDirectoryExists(string dir)
		{
			if (Directory.Exists(dir)) return;

			Console.Error.WriteLine($"Directory not found: {dir}");
			Environment.Exit(-1);
		}
	}
}
