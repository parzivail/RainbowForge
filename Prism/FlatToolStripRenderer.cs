using System.Windows.Forms;

namespace Prism
{
	public class FlatToolStripRenderer : ToolStripProfessionalRenderer
	{
		/// <inheritdoc />
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip.GetType() != typeof(MenuStrip))
				base.OnRenderToolStripBackground(e);
		}
	}
}