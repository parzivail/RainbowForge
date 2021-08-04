using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AssetCatalog.Model;
using RainbowForge.Forge;

namespace AssetCatalog
{
	public class ForgeCatalog : ICatalogApp, INotifyPropertyChanged
	{
		public static readonly ForgeCatalog Instance = new();
		private CatalogAssetCategory[] _filteredCategories = Array.Empty<CatalogAssetCategory>();
		private CatalogEntryStatus[] _filteredStatuses = Array.Empty<CatalogEntryStatus>();
		private string _nameFilter = "";

		private Forge _openedForge;
		private CatalogEntry _selectedCatalogEntry;
		private Entry _selectedEntry;
		private string _status;
		private ulong _uidFilter;

		public event PropertyChangedEventHandler PropertyChanged;

		public string Status
		{
			get => _status;
			set
			{
				_status = value;
				OnPropertyChanged();
			}
		}

		public ICatalogDb CatalogDb { get; } = new LocalCatalogDb();

		public Forge OpenedForge
		{
			get => _openedForge;
			private set
			{
				_openedForge = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(FilteredEntries));
			}
		}

		public Entry SelectedEntry
		{
			get => _selectedEntry;
			set
			{
				_selectedEntry = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IsEntrySelected));

				SelectedCatalogEntry = value == null ? null : CatalogDb.Get(value.Uid);
			}
		}

		public bool IsEntrySelected => SelectedEntry != null;

		public CatalogEntry SelectedCatalogEntry
		{
			get => _selectedCatalogEntry;
			private set
			{
				_selectedCatalogEntry = value;
				OnPropertyChanged();
			}
		}

		/// <inheritdoc />
		public IEnumerable<Entry> FilteredEntries => OpenedForge == null ? Array.Empty<Entry>() : OpenedForge.Entries.Where(EntryMatchesFilter);

		private ForgeCatalog()
		{
		}

		public void OpenForge(Stream stream)
		{
			var forgeStream = new BinaryReader(stream);
			OpenedForge = Forge.Read(forgeStream);
		}

		public void SetFilter(CatalogEntryStatus[] filteredStatuses, CatalogAssetCategory[] filteredCategories, ulong uidFilter, string nameFilter)
		{
			_uidFilter = uidFilter;
			_nameFilter = nameFilter;
			_filteredCategories = filteredCategories;
			_filteredStatuses = filteredStatuses;
			OnPropertyChanged(nameof(FilteredEntries));
		}

		public void OnCatalogChanged()
		{
			OnPropertyChanged(nameof(OpenedForge));
			OnPropertyChanged(nameof(FilteredEntries));
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool EntryMatchesFilter(Entry arg)
		{
			if (_uidFilter != 0 && arg.Uid != _uidFilter)
				return false;

			var catalogEntry = CatalogDb.Get(arg.Uid);

			if (!string.IsNullOrWhiteSpace(_nameFilter) && !catalogEntry.Name.Contains(_nameFilter))
				return false;

			if (_filteredStatuses.Length != 0 && !_filteredStatuses.Contains(catalogEntry.Status))
				return false;

			if (_filteredCategories.Length != 0 && !_filteredCategories.Contains(catalogEntry.Category))
				return false;

			return true;
		}
	}
}