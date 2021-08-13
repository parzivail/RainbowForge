using System.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Prism.Render
{
	public class SurfaceRenderer
	{
		private readonly SKControl _imageControl;

		private static readonly float _transparentCheckerboardScale = 8f;
		private static readonly SKPaint _transparentCheckerboardPaint;

		static SurfaceRenderer()
		{
			var path = new SKPath();
			path.AddRect(new SKRect(0, 0, _transparentCheckerboardScale, _transparentCheckerboardScale));
			var matrix = SKMatrix.CreateScale(2 * _transparentCheckerboardScale, _transparentCheckerboardScale)
				.PreConcat(SKMatrix.CreateSkew(0.5f, 0));
			_transparentCheckerboardPaint = new SKPaint
			{
				PathEffect = SKPathEffect.Create2DPath(matrix, path)
			};
		}

		private SKBitmap _texture;
		private Point _lastMousePos;

		public SurfaceRenderer(SKControl imageControl)
		{
			_imageControl = imageControl;
		}

		public SKMatrix ContentTransformation { get; set; } = SKMatrix.Identity;

		public void OnMouseMove(Point pos, bool leftMouse)
		{
			if (leftMouse && _lastMousePos != Point.Empty)
			{
				ContentTransformation = ContentTransformation.PostConcat(SKMatrix.CreateTranslation(pos.X - _lastMousePos.X, pos.Y - _lastMousePos.Y));
				_imageControl.Invalidate();
			}

			_lastMousePos = pos;
		}

		public void OnMouseWheel(Point pos, int delta)
		{
			var localPos = ContentTransformation.Invert().MapPoint(new SKPoint(pos.X, pos.Y));
			if (delta > 0)
				ContentTransformation =
					ContentTransformation.PreConcat(SKMatrix.CreateScale(2, 2, localPos.X, localPos.Y));
			else
				ContentTransformation =
					ContentTransformation.PreConcat(SKMatrix.CreateScale(0.5f, 0.5f, localPos.X, localPos.Y));

			_imageControl.Invalidate();
		}

		public void SetTexture(SKBitmap bitmap)
		{
			if (_texture == null)
				_texture = bitmap;
			else
				lock (_texture)
				{
					_texture?.Dispose();
					_texture = bitmap;
				}

			ContentTransformation = SKMatrix.Identity
				.PreConcat(SKMatrix.CreateTranslation((_imageControl.Width - bitmap.Width) / 2f, (_imageControl.Height - bitmap.Height) / 2f));

			_imageControl.Invalidate();
		}

		public void Render(SKPaintSurfaceEventArgs args)
		{
			var checkerboardColor1 = new SKColor(0xFF_808080);
			var checkerboardColor2 = new SKColor(0xFF_606060);

			var canvas = args.Surface.Canvas;
			canvas.Clear(checkerboardColor1);

			_transparentCheckerboardPaint.Color = checkerboardColor2;

			var rect = new SKRect(0, 0, args.Info.Width, args.Info.Height);
			rect.Inflate(_transparentCheckerboardScale, _transparentCheckerboardScale);
			canvas.DrawRect(rect, _transparentCheckerboardPaint);

			canvas.Save();

			canvas.SetMatrix(ContentTransformation);

			if (_texture != null)
				lock (_texture)
				{
					canvas.DrawBitmap(_texture, 0, 0);
				}

			canvas.Restore();
		}
	}
}