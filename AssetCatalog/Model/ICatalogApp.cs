using System.Collections.Generic;
using System.IO;
using RainbowForge.Core;

namespace AssetCatalog.Model
{
	public interface ICatalogApp
	{
		public string Status { get; set; }

		public ICatalogDb CatalogDb { get; }
		public Forge OpenedForge { get; }

		public bool IsEntrySelected { get; }
		public Entry SelectedEntry { get; set; }
		public CatalogEntry SelectedCatalogEntry { get; }

		public IEnumerable<Entry> FilteredEntries { get; }

		public void OpenForge(Stream stream);

		void SetFilter(CatalogEntryStatus[] filteredStatuses, CatalogAssetCategory[] filteredCategories, ulong uidFilter, string nameFilter);
		void OnCatalogChanged();
	}
}