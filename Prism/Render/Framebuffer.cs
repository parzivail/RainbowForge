using System;
using OpenTK.Graphics.OpenGL;

namespace Prism.Render
{
	public class Framebuffer : IDisposable
	{
		private readonly int _unboundFbo;

		private int _samples;
		public int FboId { get; set; }
		public int Texture { get; set; }
		public int DepthId { get; set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public int Samples
		{
			get => _samples;
			set
			{
				_samples = value;
				Init(Width, Height);
			}
		}

		public Framebuffer(int samples, int unboundFbo = 0)
		{
			_unboundFbo = unboundFbo;
			_samples = samples;
			FboId = GL.GenFramebuffer();
			Texture = GL.GenTexture();
			DepthId = GL.GenRenderbuffer();
		}

		public void Init(int width, int height)
		{
			if (width == 0 || height == 0)
				return;

			Width = width;
			Height = height;

			Use();

			if (_samples == 1)
			{
				GL.BindTexture(TextureTarget.Texture2D, Texture);

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
					TextureTarget.Texture2D, Texture, 0);
			}
			else
			{
				GL.BindTexture(TextureTarget.Texture2DMultisample, Texture);

				GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, _samples, PixelInternalFormat.Rgba,
					width, height, true);
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
					TextureTarget.Texture2DMultisample, Texture, 0);
			}

			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthId);
			GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, _samples,
				RenderbufferStorage.Depth24Stencil8, width, height);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
				RenderbufferTarget.Renderbuffer, DepthId);

			var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (status != FramebufferErrorCode.FramebufferComplete)
				throw new ApplicationException(
					$"Framebuffer status expected to be FramebufferComplete, instead was {status}");

			GL.BindTexture(TextureTarget.Texture2DMultisample, 0);
			Release();
		}

		public void Use()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboId);
		}

		public void Release()
		{
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _unboundFbo);
		}

		private void ReleaseUnmanagedResources()
		{
			GL.DeleteFramebuffer(FboId);
			GL.DeleteTexture(Texture);
			GL.DeleteRenderbuffer(DepthId);
		}

		private void Dispose(bool disposing)
		{
			ReleaseUnmanagedResources();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}