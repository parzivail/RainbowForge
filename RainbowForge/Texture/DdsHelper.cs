using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RainbowForge.Texture
{
	using ImageFormat = Pfim.ImageFormat;

	public class DdsHelper
	{
		public static readonly Dictionary<uint, DirectXTexUtil.DXGIFormat> TextureFormats = new()
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
			var dxgiFormat = TextureFormats[texture.TexFormat];

			var meta = DirectXTexUtil.GenerateMataData(texture.Width, texture.Height, (int) texture.Mips, dxgiFormat, false);
			DirectXTexUtil.GenerateDDSHeader(meta, DirectXTexUtil.DDSFlags.NONE, out var header, out var dx10);

			var ms = new MemoryStream();
			DirectXTexUtil.EncodeDDSHeader(ms, header, dx10);
			ms.Write(surface, 0, surface.Length);

			ms.Seek(0, SeekOrigin.Begin);

			return ms;
		}

		public static Bitmap GetBitmap(MemoryStream ms)
		{
			using (Pfim.IImage image = Pfim.Pfim.FromStream(ms))
			{
				PixelFormat format;

				switch (image.Format)
				{
                    case ImageFormat.Rgb24:
                        format = PixelFormat.Format24bppRgb;
                        break;

                    case ImageFormat.Rgba32:
						format = PixelFormat.Format32bppArgb;
						break;

                    case ImageFormat.R5g5b5:
                        format = PixelFormat.Format16bppRgb555;
                        break;

                    case ImageFormat.R5g6b5:
                        format = PixelFormat.Format16bppRgb565;
                        break;

                    case ImageFormat.R5g5b5a1:
                        format = PixelFormat.Format16bppArgb1555;
                        break;

                    case ImageFormat.Rgb8:
                        format = PixelFormat.Format8bppIndexed;
                        break;

                    default:
						var msg = $"{image.Format} is not recognized";
						throw new System.NotImplementedException(msg);
				}

				var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
				try
				{
					var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
					return new Bitmap(image.Width, image.Height, image.Stride, format, data);
				}
				finally
				{
					handle.Free();
				}
			}
		}
	}
}
