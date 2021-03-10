using System.ComponentModel;
using System.Threading.Tasks;
using AssetCatalog.Converters;

namespace AssetCatalog.Model
{
	public interface ICatalogDb
	{
		public Task Connect();

		public CatalogEntry Get(ulong uid);
		public void Put(ulong uid, CatalogEntry entry);
	}

	public class CatalogEntry
	{
		public CatalogEntryStatus Status { get; set; } = CatalogEntryStatus.PartiallyComplete;
		public CatalogAssetCategory Category { get; set; } = CatalogAssetCategory.Uncategorized;

		public string Name { get; set; }
		public string Notes { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}
	}

	[TypeConverter(typeof(EnumDescriptionTypeConverter))]
	public enum CatalogAssetCategory
	{
		[Description("Uncategorized")] Uncategorized,
		[Description("GUI Texture")] GuiTexture,
		[Description("Model Texture")] ModelTexture,
		[Description("Operator Headgear")] OperatorHeadgear,
		[Description("Operator Body")] OperatorFullBody,
		[Description("Operator Hands")] OperatorHands,
		[Description("Operator Legs")] OperatorLegs,
		[Description("Gadget")] Gadget,
		[Description("Weapon")] Weapon,
		[Description("Weapon Accessory")] WeaponAccessory, // TODO: split
		[Description("Charm")] Charm,
		[Description("Map Prop")] MapProp,
		[Description("Junk")] Junk
	}

	[TypeConverter(typeof(EnumDescriptionTypeConverter))]
	public enum CatalogEntryStatus
	{
		[Description("Incomplete")] Incomplete,
		[Description("Partially Complete")] PartiallyComplete,
		[Description("Complete")] Complete
	}
}