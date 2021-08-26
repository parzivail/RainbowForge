using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using ImageFormat = Pfim.ImageFormat;

namespace RainbowForge.Image
{
	public class DdsHelper
	{
		/*
			PixelFormat_RGBA8888 = 0x0,
			PixelFormat_RGBA8888Signed = 0x1,
			PixelFormat_BC1 = 0x2,
			PixelFormat_BC1A = 0x3,
			PixelFormat_BC2 = 0x4,
			PixelFormat_BC3 = 0x5,
			PixelFormat_BC5 = 0x6,
			PixelFormat_A8 = 0x7,
			PixelFormat_I8 = 0x8,
			PixelFormat_I16 = 0x9,
			PixelFormat_A8I8 = 0xa,
			PixelFormat_R32F = 0xb,
			PixelFormat_RGBA32F = 0xc,
			PixelFormat_RGBA16F = 0xd,
			PixelFormat_BC4 = 0xe,
			PixelFormat_BC6 = 0xf,
			PixelFormat_BC7 = 0x10,
			PixelFormat_RGBA8888Srgb = 0x11,
			PixelFormat_RGBA8888RAW = 0x12,
			AutoSelect = 0x13,
			NbPixelFormats = 0x14
		 */
		public static readonly Dictionary<uint, DirectXTexUtil.DXGIFormat> TextureFormats = new()
		{
			{ 0x0, DirectXTexUtil.DXGIFormat.B8G8R8A8UNORM },
			{ 0x1, DirectXTexUtil.DXGIFormat.R8G8B8A8SNORM },
			{ 0x2, DirectXTexUtil.DXGIFormat.BC1UNORM },
			{ 0x3, DirectXTexUtil.DXGIFormat.BC1UNORM },
			{ 0x4, DirectXTexUtil.DXGIFormat.BC2UNORM },
			{ 0x5, DirectXTexUtil.DXGIFormat.BC3UNORM },
			{ 0x6, DirectXTexUtil.DXGIFormat.BC5UNORM },
			{ 0x7, DirectXTexUtil.DXGIFormat.R8UNORM },
			{ 0x8, DirectXTexUtil.DXGIFormat.R8UNORM },
			{ 0x9, DirectXTexUtil.DXGIFormat.R16UNORM },
			{ 0xA, DirectXTexUtil.DXGIFormat.A8P8 }, // actually A8I8 -- does this work?
			{ 0xB, DirectXTexUtil.DXGIFormat.R32UINT },
			{ 0xC, DirectXTexUtil.DXGIFormat.R32G32B32FLOAT }, // r32g32b32a32_uint???
			{ 0xD, DirectXTexUtil.DXGIFormat.R16G16B16A16FLOAT },
			{ 0xE, DirectXTexUtil.DXGIFormat.BC4UNORM }, // bc4???
			{ 0xF, DirectXTexUtil.DXGIFormat.BC6HUF16 },
			{ 0x10, DirectXTexUtil.DXGIFormat.BC7UNORM },
			{ 0x11, DirectXTexUtil.DXGIFormat.B8G8R8A8UNORM }
		};

		public static MemoryStream GetDdsStream(Texture texture, byte[] surface)
		{
			var dxgiFormat = TextureFormats[texture.TexFormat];

			var meta = DirectXTexUtil.GenerateMataData(texture.Width, texture.Height, (int)texture.Mips, dxgiFormat, false);
			DirectXTexUtil.GenerateDDSHeader(meta, DirectXTexUtil.DDSFlags.NONE, out var header, out var dx10);

			var ms = new MemoryStream();
			DirectXTexUtil.EncodeDDSHeader(ms, header, dx10);
			ms.Write(surface, 0, surface.Length);

			ms.Seek(0, SeekOrigin.Begin);

			return ms;
		}

		public static Bitmap GetBitmap(MemoryStream ms)
		{
			using var image = Pfim.Pfim.FromStream(ms);

			var format = image.Format switch
			{
				ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
				ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
				ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
				ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
				ImageFormat.R5g5b5a1 => PixelFormat.Format16bppArgb1555,
				ImageFormat.Rgb8 => PixelFormat.Format8bppIndexed,
				_ => throw new NotImplementedException($"{image.Format} is not recognized")
			};

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