using System.Collections.Generic;
using System.IO;

namespace RainbowForge.Texture
{
	public class DdsHelper
	{
		private static readonly Dictionary<uint, DirectXTexUtil.DXGIFormat> TextureTypes = new()
		{
			{0x0, DirectXTexUtil.DXGIFormat.B8G8R8A8UNORM},
			{0x2, DirectXTexUtil.DXGIFormat.BC1UNORM},
			{0x3, DirectXTexUtil.DXGIFormat.BC1UNORM},
			{0x4, DirectXTexUtil.DXGIFormat.BC2UNORM},
			{0x5, DirectXTexUtil.DXGIFormat.BC3UNORM},
			{0x6, DirectXTexUtil.DXGIFormat.BC5UNORM},
			{0x7, DirectXTexUtil.DXGIFormat.R8UNORM},
			{0x8, DirectXTexUtil.DXGIFormat.R8UNORM},
			{0x9, DirectXTexUtil.DXGIFormat.R16UNORM},
			{0xB, DirectXTexUtil.DXGIFormat.R32UINT},
			{0xC, DirectXTexUtil.DXGIFormat.R32G32B32A32UINT}, // r32g32b32a32_uint???
			{0xE, DirectXTexUtil.DXGIFormat.BC4UNORM}, // bc4???
			{0xF, DirectXTexUtil.DXGIFormat.BC6HUF16},
			{0x10, DirectXTexUtil.DXGIFormat.BC7UNORM},
			{0x11, DirectXTexUtil.DXGIFormat.B8G8R8A8UNORM}
		};

		public static MemoryStream GetDdsStream(Texture texture, byte[] surface)
		{
			var dxgiFormat = TextureTypes[texture.TexFormat];

			var meta = DirectXTexUtil.GenerateMataData(texture.Width, texture.Height, (int) texture.Mips, dxgiFormat, false);
			DirectXTexUtil.GenerateDDSHeader(meta, DirectXTexUtil.DDSFlags.NONE, out var header, out var dx10);

			var ms = new MemoryStream();
			DirectXTexUtil.EncodeDDSHeader(ms, header, dx10);
			ms.Write(surface, 0, surface.Length);

			ms.Seek(0, SeekOrigin.Begin);

			return ms;
		}
	}
}