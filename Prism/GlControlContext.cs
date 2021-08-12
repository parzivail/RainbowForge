using OpenTK;
using Prism.Render;

namespace Prism
{
	public class GlControlContext : IRenderContext
	{
		private readonly GLControl _glContext;

		/// <inheritdoc />
		public int Width => _glContext.Width;

		/// <inheritdoc />
		public int Height => _glContext.Height;

		/// <inheritdoc />
		public int DefaultFramebuffer => 0;

		/// <inheritdoc />
		public void MarkDirty()
		{
			_glContext.Invalidate();
		}

		public GlControlContext(GLControl glContext)
		{
			_glContext = glContext;
		}
	}
}