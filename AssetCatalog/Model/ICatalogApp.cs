using System.IO;
using RainbowForge.Forge;

namespace AssetCatalog.Model
{
	public interface ICatalogApp
	{
		public string Status { get; set; }

		public Forge OpenedForge { get; }

		public void OpenForge(Stream stream);
	}
}