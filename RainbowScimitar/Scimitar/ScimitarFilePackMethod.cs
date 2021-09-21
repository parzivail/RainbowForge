namespace RainbowScimitar.Scimitar
{
	public enum ScimitarFilePackMethod : short
	{
		/// <summary>
		/// Indicates the Block strategy for packing file data chunks. The asset
		/// serializes the list of block sizes, and each block is accompanied by
		/// a checksum at the start of each block.
		/// </summary>
		Block = 3,

		/// <summary>
		/// Indicates the Streaming strategy for packing file data chunks. The
		/// asset serializes the list of block sizes as well as the checksums,
		/// and blocks are tightly packed with no interruptions.
		/// </summary>
		Streaming = 7
	}
}