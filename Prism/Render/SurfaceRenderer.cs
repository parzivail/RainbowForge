using System.Collections.Generic;
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
		private static readonly SKPaint _textPaint;

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

			_textPaint = new SKPaint(new SKFont(SKTypeface.Default, 16))
			{
				Color = SKColors.White,
				Style = SKPaintStyle.Fill
			};
		}

		private SKBitmap _texture;
		private Point _lastMousePos;
		private KeyValuePair<string, string>[] _hudData;

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

		public void SetTexture(SKBitmap bitmap, KeyValuePair<string, string>[] hudData)
		{
			_hudData = hudData;

			if (_texture == null)
				_texture = bitmap;
			else
				lock (_texture)
				{
					_texture?.Dispose();
					_texture = bitmap;
				}

			ContentTransformation = SKMatrix.Identity
				.PreConcat(SKMatrix.CreateTranslation((_imageControl.Width - bitmap.Width) / 2f, (_imageControl.Height + bitmap.Height) / 2f))
				.PreConcat(SKMatrix.CreateScale(1, -1));

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
			if (_texture != null)
				lock (_texture)
				{
					canvas.SetMatrix(ContentTransformation);
					canvas.DrawBitmap(_texture, 0, 0);
				}

			canvas.Restore();

			if (_hudData != null)
			{
				var lineOffset = _textPaint.FontSpacing + _textPaint.FontMetrics.Leading;
				var y = 10 + lineOffset;
				foreach (var (key, value) in _hudData)
				{
					canvas.DrawText($"{key}: {value}", 10, y, _textPaint);
					y += lineOffset;
				}
			}
		}
	}
}