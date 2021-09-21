using System.IO;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarGlobalMeta()
	{
		public static ScimitarGlobalMeta Read(BinaryReader r)
		{
			return new ScimitarGlobalMeta();
		}
	}
}