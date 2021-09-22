using System.IO;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarFastLoadTableOfContents()
	{
		public static ScimitarFastLoadTableOfContents Read(Stream bundleStream)
		{
			return new ScimitarFastLoadTableOfContents();
		}
	}
}