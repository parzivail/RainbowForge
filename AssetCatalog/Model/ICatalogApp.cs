using System.Collections.Generic;
using System.IO;
using RainbowForge.Forge;

namespace AssetCatalog.Model
{
	public interface ICatalogApp
	{
		public string Status { get; set; }

		public ICatalogDb CatalogDb { get; }
		public Forge OpenedForge { get; }

		public IEnumerable<Entry> FilteredEntries { get; }

		public void OpenForge(Stream stream);
	}
}