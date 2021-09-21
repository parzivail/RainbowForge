using Microsoft.IO;

namespace RainbowScimitar.Helper
{
	public class StreamHelper
	{
		public static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
	}
}