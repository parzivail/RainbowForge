using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Prism
{
	public class PrismSettings
	{
		[
			Category("Export"),
			DisplayName("Quick Export Directory"),
			Description("The directory to which \"Quick Export\" actions will output files."),
			DefaultValue("Quick Exports")
		]
		public string QuickExportLocation { get; set; } = "Quick Exports";

		[
			Category("Model Export"),
			DisplayName("Export All Model LODs"),
			Description("If the exported models should include all levels of detail present in the game files instead of just the highest."),
			DefaultValue(false)
		]
		public bool ExportAllModelLods { get; set; } = false;

		[
			Category("PNG Export"),
			DisplayName("Flip blue (Z) channel"),
			Description("Make exported PNGs have the blue (Z) channel flipped, which transforms normal maps from DirectX normal space to OpenGL normal space."),
			DefaultValue(false)
		]
		public bool FlipPngBlueChannel { get; set; } = false;

		public void Save(string filename)
		{
			File.WriteAllText(filename, JsonConvert.SerializeObject(this));
		}

		public static PrismSettings Load(string filename)
		{
			return !File.Exists(filename) ? new PrismSettings() : JsonConvert.DeserializeObject<PrismSettings>(File.ReadAllText(filename));
		}
	}
}