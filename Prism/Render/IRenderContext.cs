namespace Prism.Render
{
	public interface IRenderContext
	{
		int Width { get; }
		int Height { get; }
		int DefaultFramebuffer { get; }
		void MarkDirty();
	}
}