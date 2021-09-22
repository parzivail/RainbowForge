using System.IO;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarGlobalMeta()
	{
		public static ScimitarGlobalMeta Read(Stream r)
		{
			return new ScimitarGlobalMeta();
		}
	}
}