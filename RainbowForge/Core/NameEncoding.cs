using System;

namespace RainbowForge.Core
{
	public class NameEncoding
	{
		public const ulong FILENAME_ENCODING_BASE_KEY = 0xA860F0ECDE3339FB;
		public const ulong FILENAME_ENCODING_ENTRY_KEY_STEP = 0x357267C76FFB9EB2;
		public const ulong FILENAME_ENCODING_FILE_KEY_STEP = 0xE684BFF857699452;

		public static byte[] DecodeName(byte[] name, uint fileType, ulong uid, ulong dataOffset, ulong keyStep)
		{
			var key = FILENAME_ENCODING_BASE_KEY + uid + dataOffset + fileType + ((ulong)fileType << 32);

			var blocks = (name.Length + 8) / 8;

			var output = new ulong[blocks];
			Buffer.BlockCopy(name, 0, output, 0, name.Length);

			for (var i = 0; i < blocks; i++)
			{
				output[i] ^= key;
				key += keyStep;
			}

			var bytes = new byte[name.Length];
			Buffer.BlockCopy(output, 0, bytes, 0, bytes.Length);
			return bytes;
		}
	}
}