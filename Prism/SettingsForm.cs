using System.Drawing;
using System.Windows.Forms;

namespace Prism
{
	public class SettingsForm : Form
	{
		public string Filename { get; }

		public SettingsForm(string filename)
		{
			Filename = filename;
			SuspendLayout();

			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(400, 500);
			Text = "Prism";

			PropertyGrid pgSettings;

			Controls.Add((pgSettings = new PropertyGrid
			{
				Dock = DockStyle.Fill,
				SelectedObject = PrismSettings.Load(filename)
			}));

			Button bSave;
			Button bCancel;

			Controls.Add(new FlowLayoutPanel
			{
				Dock = DockStyle.Bottom,
				FlowDirection = FlowDirection.RightToLeft,
				AutoSize = true,
				Controls =
				{
					(bSave = new Button
					{
						Text = "Save",
						Enabled = false,
						DialogResult = DialogResult.OK
					}),
					(bCancel = new Button
					{
						Text = "Cancel",
						DialogResult = DialogResult.Cancel
					})
				}
			});

			pgSettings.PropertyValueChanged += (o, args) => { bSave.Enabled = true; };

			bSave.Click += (sender, args) =>
			{
				if (pgSettings.SelectedObject is not PrismSettings po)
					return;

				po.Save(filename);

				Close();
			};

			bCancel.Click += (sender, args) => Close();

			ResumeLayout(true);
		}
	}
}