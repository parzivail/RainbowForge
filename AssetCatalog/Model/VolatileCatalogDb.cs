using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetCatalog.Model
{
	public class VolatileCatalogDb : ICatalogDb
	{
		private static readonly CatalogEntry UncatalogedEntry = new()
		{
			Status = CatalogEntryStatus.Incomplete,
			Category = CatalogAssetCategory.Uncategorized
		};

		private readonly Dictionary<ulong, CatalogEntry> _entries = new();

		/// <inheritdoc />
		public Task Connect()
		{
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public CatalogEntry Get(ulong uid)
		{
			return _entries.ContainsKey(uid) ? _entries[uid] : UncatalogedEntry;
		}

		/// <inheritdoc />
		public void Put(ulong uid, CatalogEntry entry)
		{
			_entries[uid] = entry;
		}
	}
}