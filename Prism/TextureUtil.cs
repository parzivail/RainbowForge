using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Prism
{
    class TextureUtil
    {
        public static void PatchNormalMap(Bitmap bmp, int channelFlipIdx, bool bRecalculateZ)
        {
            var bits = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var pointer = bits.Scan0;
            var size = Math.Abs(bits.Stride) * bmp.Height;
            var pixels = new byte[size];
            Marshal.Copy(pointer, pixels, 0, size);

            for (var i = 0; i < pixels.Length; i += 4)
            {
                if (channelFlipIdx >= 0)
                    pixels[i + channelFlipIdx] = (byte)(255 - pixels[i + channelFlipIdx]); // BGRA
                if (bRecalculateZ)
                    pixels[i] = CalculateNormalMapZ(pixels[i + 2], pixels[i + 1]);
            }

            Marshal.Copy(pixels, 0, pointer, size);
            bmp.UnlockBits(bits);
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
