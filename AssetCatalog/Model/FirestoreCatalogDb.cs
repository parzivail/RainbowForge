using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Core;

namespace AssetCatalog.Model
{
	public class FirestoreCatalogDb : ICatalogDb
	{
		private QuerySnapshot _catalog;
		private FirestoreDb _db;

		private static async Task<FirestoreDb> CreateFirestoreDbWithEmailAuthentication(string emailAddress, string password)
		{
			// Create a custom authentication mechanism for Email/Password authentication
			// If the authentication is successful, we will get back the current authentication token and the refresh token
			// The authentication expires every hour, so we need to use the obtained refresh token to obtain a new authentication token as the previous one expires

			// This requires the following ruleset or similar to be applied to the Firestore:
			/*
				rules_version = '2';
				service cloud.firestore {
				  match /databases/{database}/documents {
				    match /{document=**} {
				      allow read, write: if request.auth != null;
				    }
				  }
				}
			 */

			var authProvider = new FirebaseAuthClient(new FirebaseAuthConfig
			{
				ApiKey = "AIzaSyCcV55HIDDdeIp8SP5aki-eX4FfJtpyWB4",
				AuthDomain = "parzi-rainbowforge.firebaseapp.com",
				Providers = new FirebaseAuthProvider[] {new EmailProvider()}
			});
			var auth = await authProvider.SignInWithEmailAndPasswordAsync(emailAddress, password);

			var callCredentials = CallCredentials.FromInterceptor(async (context, metadata) =>
			{
				var token = await auth.User.GetIdTokenAsync();

				metadata.Clear();
				metadata.Add("Authorization", $"Bearer {token}");
			});
			var credentials = ChannelCredentials.Create(new SslCredentials(), callCredentials);

			var client = await new FirestoreClientBuilder
			{
				ChannelCredentials = credentials
			}.BuildAsync();

			return await FirestoreDb.CreateAsync("parzi-rainbowforge", client);
		}

		/// <inheritdoc />
		public async Task Connect(string email, string password)
		{
			_db = await CreateFirestoreDbWithEmailAuthentication(email, password);

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