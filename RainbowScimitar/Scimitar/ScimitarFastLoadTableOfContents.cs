using System.IO;

namespace RainbowScimitar.Scimitar
{
	public record ScimitarFastLoadTableOfContents()
	{
		public static ScimitarFastLoadTableOfContents Read(BinaryReader r)
		{
			return new ScimitarFastLoadTableOfContents();
		}
	}
}