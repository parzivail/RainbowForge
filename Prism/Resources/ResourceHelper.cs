using System.IO;
using System.Reflection;

namespace Prism.Resources
{
	public static class ResourceHelper
	{
		public static Stream GetResource(string filename)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream("Prism.Resources." + filename);
		}

		public static string ReadResource(string filename)
		{
			var assembly = Assembly.GetExecutingAssembly();
			using var stream = new StreamReader(assembly.GetManifestResourceStream("Prism.Resources." + filename));
			return stream.ReadToEnd();
		}
	}
}