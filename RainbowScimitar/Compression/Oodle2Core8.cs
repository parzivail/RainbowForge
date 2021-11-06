using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RainbowScimitar.Compression
{
	public static class Oodle2Core8
	{
		public enum FuzzSafe : byte
		{
			No = 0,
			Yes = 1
		}

		public enum CheckCRC : int
		{
			No = 0,
			Yes = 1
		}

		public enum Verbosity : int
		{
			None = 0,
			Minimal = 1,
			Some = 2,
			Lots = 3
		}

		public enum ThreadPhase : byte
		{
			ThreadPhase1 = 1,
			ThreadPhase2 = 2,
			ThreadPhaseAll = 3,
			Unthreaded = ThreadPhaseAll
		}

		public enum Profile : int
		{
			Main = 0,
			Reduced = 1
		}

		public enum Jobify : int
		{
			Default = 0,
			Disable = 1,
			Normal = 2,
			Aggressive = 3,
			Count = 4,
		}

		public enum CompressionLevel : int
		{
			None = 0,
			SuperFast = 1,
			VeryFast = 2,
			Fast = 3,
			Normal = 4,

			Optimal1 = 5,
			Optimal2 = 6,
			Optimal3 = 7,
			Optimal4 = 8,
			Optimal5 = 9,

			HyperFast1 = -1,
			HyperFast2 = -2,
			HyperFast3 = -3,
			HyperFast4 = -4,

			HyperFast = HyperFast1,
			Optimal = Optimal2,
			Max = Optimal5,
			Min = HyperFast4,
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct CompressOptions
		{
			public uint UnusedWasVerbosity;
			public int MinMatchLength;
			public bool SeekChunkReset;
			public int SeekChunkLength;
			public Profile Profile;
			public int DictionarySize;
			public int SpaceSpeedTradeoffBytes;
			public int UnusedWasMaxHuffmansPerChunk;
			public bool SendQuantumCRCs;
			public int MaxLocalDictionarySize;
			public bool MakeLongRangeMatcher;
			public int MatchTableSizeLog2;
			public Jobify Jobify;
			public IntPtr JobifyUserPtr;
			public int FarMatchMinLength;
			public int FarMatchOffsetLog2;
			public uint Reserved1;
			public uint Reserved2;
			public uint Reserved3;
			public uint Reserved4;
		}

		[DllImport("oo2core_8_win64.dll")]
		private static extern int OodleLZ_Decompress(byte[] buffer, int bufferLength, byte[] outputBuffer, int outputBufferLength, FuzzSafe fuzzSafe, CheckCRC checkCrc, Verbosity verbosity,
			IntPtr dictionaryBuffer, int dictionaryBufferLength, IntPtr progressCallback, IntPtr callbackUserData, IntPtr decoderMemory, int decoderMemoryLength,
			ThreadPhase threadPhase);

		[DllImport("oo2core_8_win64.dll")]
		private static extern IntPtr OodleLZ_CompressOptions_GetDefault(CompressionLevel level);

		[DllImport("oo2core_8_win64.dll")]
		private static extern int OodleLZ_Compress(byte[] buffer, int bufferLength, byte[] outputBuffer, int outputBufferLength, ref CompressOptions pOptions, IntPtr dictionaryBuffer, IntPtr lrm,
			IntPtr scratchMemory, int scratchMemoryLength);

		public static byte[] Decompress(byte[] compressedData, int decompressedLength, FuzzSafe fuzzSafe = FuzzSafe.No, CheckCRC checkCrc = CheckCRC.No, Verbosity verbosity = Verbosity.None,
			ThreadPhase threadPhase = ThreadPhase.Unthreaded)
		{
			var decompressionBuffer = new byte[decompressedLength];

			var decodedBytes = OodleLZ_Decompress(compressedData, compressedData.Length, decompressionBuffer, decompressedLength, fuzzSafe, checkCrc, verbosity,
				IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, threadPhase);

			if (decodedBytes != decompressedLength)
			{
				File.WriteAllBytes("failed.bin", compressedData);
				throw new IOException("Failed to decompress");
			}

			return decompressionBuffer;
		}
	}
}