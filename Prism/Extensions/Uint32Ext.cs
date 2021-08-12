namespace Prism.Extensions
{
	public static class Uint32Ext
	{
		private static readonly string[] Units =
		{
			"B",
			"KB",
			"MB",
			"GB"
		};

		public static string ToFileSizeString(this uint numBytes)
		{
			var i = 0;
			var length = (float)numBytes;
			for (; length >= 1024; length /= 1024, i++) ;

			return $"{length:0.##} {Units[i]}";
		}
	}
}