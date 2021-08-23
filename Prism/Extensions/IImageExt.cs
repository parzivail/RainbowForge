using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Pfim;
using SkiaSharp;
using ImageFormat = Pfim.ImageFormat;

namespace Prism.Extensions
{
	public static class IImageExt
	{
		public static Bitmap CreateBitmap(this IImage image, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
		{
			var format = image.Format switch
			{
				ImageFormat.Rgb24 => PixelFormat.Format24bppRgb,
				ImageFormat.Rgba32 => PixelFormat.Format32bppArgb,
				ImageFormat.R5g5b5 => PixelFormat.Format16bppRgb555,
				ImageFormat.R5g6b5 => PixelFormat.Format16bppRgb565,
				ImageFormat.R5g5b5a1 => PixelFormat.Format16bppArgb1555,
				ImageFormat.Rgb8 => PixelFormat.Format8bppIndexed,
				_ => throw new NotImplementedException()
			};

			var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
			using var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, ptr);

			// Generate greyscale indexed palette data
			if (format == PixelFormat.Format8bppIndexed)
			{
				var palette = bitmap.Palette;
				for (var i = 0; i < 256; i++)
					palette.Entries[i] = Color.FromArgb((byte)i, (byte)i, (byte)i);
				bitmap.Palette = palette;
			}

			return bitmap.Clone(new Rectangle(Point.Empty, bitmap.Size), pixelFormat);
		}

		public static SKImage CreateSkImage(this IImage image)
		{
			var newData = image.Data;
			var newDataLen = image.DataLen;
			var stride = image.Stride;
			SKColorType colorType;
			switch (image.Format)
			{
				case ImageFormat.Rgb8:
					colorType = SKColorType.Gray8;
					break;
				case ImageFormat.R5g6b5:
					// color channels still need to be swapped
					colorType = SKColorType.Rgb565;
					break;
				case ImageFormat.Rgba16:
					// color channels still need to be swapped
					colorType = SKColorType.Argb4444;
					break;
				case ImageFormat.Rgb24:
				{
					// Skia has no 24bit pixels, so we upscale to 32bit
					var pixels = image.DataLen / 3;
					newDataLen = pixels * 4;
					newData = new byte[newDataLen];
					for (var i = 0; i < pixels; i++)
					{
						newData[i * 4] = image.Data[i * 3];
						newData[i * 4 + 1] = image.Data[i * 3 + 1];
						newData[i * 4 + 2] = image.Data[i * 3 + 2];
						newData[i * 4 + 3] = 255;
					}

					stride = image.Width * 4;
					colorType = SKColorType.Bgra8888;
					break;
				}
				case ImageFormat.Rgba32:
					colorType = SKColorType.Bgra8888;
					break;
				default:
					throw new ArgumentException($"Skia unable to interpret pfim format: {image.Format}");
			}

			var imageInfo = new SKImageInfo(image.Width, image.Height, colorType, SKAlphaType.Unpremul);
			var handle = GCHandle.Alloc(newData, GCHandleType.Pinned);
			var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(newData, 0);

			using var data = SKData.Create(ptr, newDataLen, (address, context) => handle.Free());
			return SKImage.FromPixels(imageInfo, data, stride);
		}
	}
}