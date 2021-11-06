using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RainbowForge.Compression
{
	public static class OodleHelper
	{
		private static bool _oodleLoaded = false;

		public static void EnsureOodleLoaded()
		{
			if (_oodleLoaded)
				return;

			var libraryPath = Environment.GetEnvironmentVariable("OODLE2_8_PATH");

			if (libraryPath == null || !File.Exists(libraryPath))
				throw new Exception("Failed to locate Oodle library file! Make sure the OODLE2_8_PATH path is set to oo2core_8_win64.dll's path");

			NativeLibrary.Load(libraryPath);

			_oodleLoaded = true;
		}
	}
}