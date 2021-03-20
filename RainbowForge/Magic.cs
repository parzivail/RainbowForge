namespace RainbowForge
{
	public enum Magic : ulong
	{
		Unknown = 0,
		FileContainer = 0x1014_FA99,
		Mesh = 0xABEB2DFB,
		InnerModelStruct = 0xFC9E1595,
		DdsPayload = 0x13237FE9,
		TextureA = 0xD7B5C478,
		TextureB = 0xF9C80707,
		TextureC = 0x59CE4D13,
		TextureD = 0x9F492D22,
		TextureE = 0x3876ccdf,
		TextureGui1 = 0x9468B9E2,
		TextureGui2 = 0x05A61FAD,
		WemSound = 0x427411A3,
		FlatArchive = 0x22ECBE63,
		FlatArchiveUidLinkContainer = 0x22ECBE63,
		FlatArchiveShader = 0x1C9A0555,
		FlatArchiveMaterialContainer = 0x85C817C3,
		FlatArchiveMipContainer = 0x989DC6B2
	}

	public enum ContainerMagic : uint
	{
		Descriptor = 1,
		Hash = 6,
		File = 0x57FBAA34
	}

	public enum AssetType
	{
		Unknown,
		Mesh,
		Texture,
		Sound,
		FlatArchive
	}
}