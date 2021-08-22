using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Prism.Extensions
{
	public static class BitmapExt
	{
		public static void LoadGlTexture(this Bitmap bitmap, int texId, TextureTarget texTarget)
		{
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

			GL.BindTexture(texTarget, texId);

			var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			GL.TexImage2D(texTarget, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			bitmap.UnlockBits(data);

			GL.TexParameter(texTarget, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(texTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(texTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(texTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
		}
	}
}