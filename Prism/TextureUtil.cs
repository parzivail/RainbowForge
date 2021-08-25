using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Prism
{
    class TextureUtil
    {
        public static void FlipChannel(Bitmap bmp, int channel)
        {
            var bits = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var pointer = bits.Scan0;
            var size = Math.Abs(bits.Stride) * bmp.Height;
            var pixels = new byte[size];
            Marshal.Copy(pointer, pixels, 0, size);

            for (var i = channel; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]); // BGRA
            }

            Marshal.Copy(pixels, 0, pointer, size);
            bmp.UnlockBits(bits);
        }

        public static void PatchNormalMap(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var pixCol = bmp.GetPixel(x, y);
                    bmp.SetPixel(x, y, Color.FromArgb(pixCol.A, pixCol.R, pixCol.G, CalculateNormalMapZ(pixCol.R, pixCol.G)));
                }
            }
        }

        private static byte CalculateNormalMapZ(byte r, byte g)
        {
            float x = r / 255.0f * 2 - 1;
            float y = g / 255.0f * 2 - 1;

            var dot1 = 1 - x * x - y * y;
            float z = dot1 > 0 ? MathF.Sqrt(dot1) : 0.0f;

            return (byte)(Math.Clamp((z + 1) / 2.0f, 0.0f, 1.0f) * 255);
        }
    }
}
