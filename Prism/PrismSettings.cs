using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Prism
{
	public class PrismSettings
	{
		[
			Category("3D Viewport"),
			DisplayName("Use reflections"),
			Description("Whether or not skybox reflections are enabled in the 3D viewport."),
			DefaultValue(false)
		]
		public bool Use3DReflections { get; set; } = false;

		[
			Category("3D Viewport"),
			DisplayName("Use checkerboard texture"),
			Description("Whether or not models in the 3D viewport render using the checkerboard debug texture."),
			DefaultValue(false)
		]
		public bool Use3DCheckerboard { get; set; } = false;

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
			DisplayName("Horizontally flip entire image"),
			Description("Make exported PNGs vertically flipped, which transforms texture maps from DirectX texture coordinate space to the texture coordinate space of exported models."),
			DefaultValue(true)
		]
		public bool FlipPngSpace { get; set; } = true;

		[
			Category("PNG Export"),
			DisplayName("Flip normal map green (Y) channel"),
			Description("Make exported PNGs have the green (Y) channel flipped, which transforms normal maps from DirectX normal space to OpenGL normal space."),
			DefaultValue(true)
		]
		public bool FlipPngGreenChannel { get; set; } = true;

		[
			Category("PNG Export"),
			DisplayName("Recalculate normal maps blue (Z) channel"),
			Description("Make exported PNGs have their blue (Z) channel recalculated."),
			DefaultValue(true)
		]
		public bool RecalculatePngBlueChannel { get; set; } = true;

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