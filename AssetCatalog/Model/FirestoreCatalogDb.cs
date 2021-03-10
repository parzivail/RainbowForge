using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace AssetCatalog.Model
{
	public class FirestoreCatalogDb : ICatalogDb
	{
		private static readonly CatalogEntry UncatalogedEntry = new()
		{
			Status = CatalogEntryStatus.Incomplete,
			Category = CatalogAssetCategory.Uncategorized
		};

		private QuerySnapshot _catalog;

		private readonly FirestoreDb _db;

		public FirestoreCatalogDb(string project)
		{
			_db = FirestoreDb.Create(project);
		}

		/// <inheritdoc />
		public Task Connect()
		{
			var collection = _db.Collection("catalog");
			collection.Listen(snapshot => { _catalog = snapshot; });

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public CatalogEntry Get(ulong uid)
		{
			var uidStr = uid.ToString();
			var document = _catalog.Documents.FirstOrDefault(snapshot => snapshot.Id == uidStr);
			if (document == null)
				return UncatalogedEntry;

			var fields = document.ToDictionary();

			var name = (string) fields["name"];
			var category = (CatalogAssetCategory) Enum.Parse(typeof(CatalogAssetCategory), (string) fields["category"]);
			var status = (CatalogEntryStatus) Enum.Parse(typeof(CatalogEntryStatus), (string) fields["status"]);

			return new CatalogEntry
			{
				Name = name,
				Category = category,
				Status = status
			};
		}

		/// <inheritdoc />
		public void Put(ulong uid, CatalogEntry entry)
		{
			throw new NotImplementedException();
		}
	}
}