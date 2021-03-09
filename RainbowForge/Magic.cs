using System;
using System.IO;

namespace RainbowForge
{
	public enum Magic : ulong
	{
		Unknown = 0,
		FileContainer = 0x1014FA99,
		FileContainerKnownType = 0x1014FA9957FBAA34,
		Mesh = 0xABEB2DFB,
		InnerModelStruct = 0xFC9E1595,
		DdsPayload = 0x13237FE9,
		TextureA = 0xD7B5C478,
		TextureB = 0xF9C80707,
		TextureC = 0x59CE4D13,
		TextureD = 0x9F492D22,
		TextureE = 0x3876ccdf,
		TextureGui1 = 0x9468B9E2,
		TextureGui2 = 0x5A61FAD
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
		Sound
	}

	public class MagicHelper
	{
		public static void AssertEquals(Magic magic, ulong value)
		{
			if (!Equals(magic, value))
				throw new InvalidDataException();
		}

		public static bool Equals(Magic magic, ulong value)
		{
			return (Magic) value == magic;
		}

		public static AssetType GetFiletype(ulong magic)
		{
			if (!Enum.IsDefined(typeof(Magic), magic))
				return AssetType.Unknown;

			return (Magic) magic switch
			{
				Magic.Mesh => AssetType.Mesh,
				Magic.TextureA => AssetType.Texture,
				Magic.TextureB => AssetType.Texture,
				Magic.TextureC => AssetType.Texture,
				Magic.TextureD => AssetType.Texture,
				Magic.TextureE => AssetType.Texture,
				Magic.TextureGui1 => AssetType.Texture,
				Magic.TextureGui2 => AssetType.Texture,
				_ => AssetType.Unknown
			};
		}
	}
}