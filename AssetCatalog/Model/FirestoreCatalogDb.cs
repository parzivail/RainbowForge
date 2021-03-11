using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace AssetCatalog.Model
{
	public class FirestoreCatalogDb : ICatalogDb
	{
		private readonly string _project;
		private QuerySnapshot _catalog;
		private FirestoreDb _db;

		public FirestoreCatalogDb(string project)
		{
			_project = project;
		}

		/// <inheritdoc />
		public async Task Connect()
		{
			_db = await FirestoreDb.CreateAsync(_project);

			var collection = _db.Collection("catalog");
			collection.Listen(snapshot =>
			{
				_catalog = snapshot;
				ForgeCatalog.Instance.OnCatalogChanged();
			});
		}

		/// <inheritdoc />
		public CatalogEntry Get(ulong uid)
		{
			if (_catalog == null)
				return new CatalogEntry
				{
					Status = CatalogEntryStatus.Incomplete,
					Category = CatalogAssetCategory.Uncategorized
				};

			var uidStr = uid.ToString();
			var document = _catalog.Documents.FirstOrDefault(snapshot => snapshot.Id == uidStr);
			if (document == null)
				return new CatalogEntry
				{
					Status = CatalogEntryStatus.Incomplete,
					Category = CatalogAssetCategory.Uncategorized
				};

			var fields = document.ToDictionary();

			var name = (string) fields["name"];
			var category = (CatalogAssetCategory) Enum.Parse(typeof(CatalogAssetCategory), (string) fields["category"]);
			var status = (CatalogEntryStatus) Enum.Parse(typeof(CatalogEntryStatus), (string) fields["status"]);

			var notes = fields.ContainsKey("notes") ? (string) fields["notes"] : "";

			return new CatalogEntry
			{
				Name = name,
				Category = category,
				Status = status,
				Notes = notes
			};
		}

		/// <inheritdoc />
		public void Put(ulong uid, CatalogEntry entry)
		{
			var doc = new Dictionary<string, object>
			{
				{"name", entry.Name},
				{"category", entry.Category.ToString()},
				{"status", entry.Status.ToString()}
			};

			if (!string.IsNullOrWhiteSpace(entry.Notes))
				doc["notes"] = entry.Notes;

			_db.Collection("catalog").Document(uid.ToString()).SetAsync(doc);
		}
	}
}