using System.Threading.Tasks;
using LiteDB;

namespace AssetCatalog.Model
{
	public class LocalCatalogDb : ICatalogDb
	{
		private LiteDatabase _db;
		private ILiteCollection<CatalogEntry> _catalog;

		/// <inheritdoc />
		public bool NeedsAuth()
		{
			return false;
		}

		/// <inheritdoc />
		public Task Connect(string email, string password)
		{
			_db = new LiteDatabase("catalog.db");
			_catalog = _db.GetCollection<CatalogEntry>("catalog");
			_catalog.EnsureIndex(entry => entry.Uid);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public CatalogEntry Get(ulong uid)
		{
			var entry = _catalog.FindById(uid);

			if (entry != null)
				return entry;

			return new CatalogEntry
			{
				Uid = uid,
				Status = CatalogEntryStatus.Incomplete,
				Category = CatalogAssetCategory.Uncategorized
			};
		}

		/// <inheritdoc />
		public void Put(ulong uid, CatalogEntry entry)
		{
			_catalog.Upsert(uid, entry);
			ForgeCatalog.Instance.OnCatalogChanged();
		}
	}
}