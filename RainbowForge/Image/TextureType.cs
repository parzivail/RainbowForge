namespace RainbowForge.Image
{
	public enum TextureType : uint
	{
		Diffuse = 0x0, // older GUI tetxures
		Normal = 0x1, // not just yellow (RG = XY) ones, head detail (RGA = XYZ) as well
		Specular = 0x2, // usually holds gloss, metalness and cavity
		Misc = 0x3, // Misc Map; can be Diffuse, GUI, Normal, Emission, Mask, Distortion, Cubemap or pretty much anything
		Displacement = 0x5,
		Translucence = 0x6,
		ColorMask = 0x7
	}
}