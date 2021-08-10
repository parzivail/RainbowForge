namespace RainbowForge
{
	public enum Magic : ulong
	{
		Unknown = 0,
		FileContainer = 0x1014FA99,
		Mesh = 0xABEB2DFB,
		InnerModelStruct = 0xFC9E1595,
		DdsPayload = 0x13237FE9,
		TextureA = 0xD7B5C478,
		TextureB = 0xF9C80707,
		TextureC = 0x59CE4D13,
		TextureD = 0x9F492D22,
		TextureE = 0x3876CCDF,
		TextureGui1 = 0x9468B9E2,
		TextureGui2 = 0x05A61FAD,
		WemSound = 0x427411A3,
		FlatArchive1 = 0x22ECBE63,
		FlatArchive2 = 0x971A842E,
		FlatArchive3 = 0xADBAB640,
		FlatArchive4 = 0xE5A83560,
		FlatArchive5 = 0x6E3C9C6F,
		FlatArchive6 = 0xAE88DE65,
		FlatArchive7 = 0xFBB63E47,
		FlatArchive8 = 0x943945C4,
		FlatArchive9 = 0x3E237DA3,
		FlatArchive10 = 0xD16E3EBE,
		FlatArchive11 = 0x9231EE0F,
		FlatArchive12 = 0x82688E42,
		Shader = 0x1C9A0555,
		MaterialContainer = 0x85C817C3,
		MipContainer = 0x989DC6B2,
		MipSet = 0xA2B7E917,
		MeshProperties = 0x415D9568
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