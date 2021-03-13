using System.IO;

namespace RainbowForge.Forge
{
	/// <summary>
	///     No-op class to serve as a common parent for all ForgeAsset subcomponents
	/// </summary>
	public interface IAssetBlock
	{
		public MemoryStream GetDataStream(BinaryReader r);
	}
}