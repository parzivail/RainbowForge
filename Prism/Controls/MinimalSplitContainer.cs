using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Prism.Extensions;

namespace Prism.Controls
{
	public class MinimalSplitContainer : ContainerControl
	{
		private bool _dragging;

		private int _splitterDistance;

		public int SplitterDistance
		{
			get => _splitterDistance;
			set
			{
				if (value <= Panel1.Margin.Size.Width + 1 || value >= Width - Panel2.Margin.Size.Width - 1)
					return;

				if (value < MinWidth1)
					value = MinWidth1;

				if (value > Width - MinWidth2)
					value = Width - MinWidth2;

				if (_splitterDistance == value)
					return;

				_splitterDistance = value;

				UpdateContainerSizes();
			}
		}

		public int MinWidth1 { get; set; } = 30;
		public int MinWidth2 { get; set; } = 30;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Panel Panel1 { get; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Panel Panel2 { get; }

		[DefaultValue(10)] public int SplitterLinePadding { get; set; } = 10;

		[DefaultValue(3)] public int SplitterWidth { get; set; } = 3;

		public MinimalSplitContainer()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

			SuspendLayout();

			Panel1 = new Panel
			{
				Cursor = Cursors.Default
			};

			Panel2 = new Panel
			{
				Cursor = Cursors.Default
			};

			Controls.Add(Panel2);
			Controls.Add(Panel1);

			Cursor = Cursors.SizeWE;
			Name = "MinimalSplitContainer";
			Size = new Size(500, 300);

			UpdateContainerSizes();
			ResumeLayout(false);
		}

		/// <inheritdoc />
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button != MouseButtons.Left)
				return;
			_dragging = true;
		}

		/// <inheritdoc />
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button != MouseButtons.Left)
				return;
			_dragging = false;
		}

		/// <inheritdoc />
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (_dragging)
				SplitterDistance = e.X;
		}

		/// <inheritdoc />
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			UpdateContainerSizes();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			using (var fgPen = new Pen(ForeColor))
			{
				e.Graphics.DrawLine(fgPen, SplitterDistance, ClientRectangle.Top + SplitterLinePadding, SplitterDistance, ClientRectangle.Bottom - SplitterLinePadding);
			}
		}

		private void UpdateContainerSizes()
		{
			using (this.SuspendPainting())
			{
				Panel1.Location = new Point(0, 0);
				Panel1.Size = new Size(SplitterDistance - (int)Math.Ceiling(SplitterWidth / 2f), Height);

				Panel2.Location = new Point(SplitterDistance + Panel2.Margin.Left, 0);
				Panel2.Size = new Size(Width - SplitterDistance - SplitterWidth / 2, Height);
			}
		}
	}
}