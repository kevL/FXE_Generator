using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace lipsync_editor
{
	/// <summary>
	/// A dialog for showing generic information to the user.
	/// </summary>
	sealed class InfoDialog
		: Form
	{
		#region fields
		Timer _t1 = new Timer();
		string _info;
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal InfoDialog(string title, string info)
		{
			InitializeComponent();

			Text = title;
			tb_info.Text = info;

			int linecount = Regex.Matches(info, Environment.NewLine).Count + 2;

			Size size = TextRenderer.MeasureText(tb_info.Text, tb_info.Font);
			ClientSize = new Size(size.Width + 20, linecount * 18);

			tb_info.SelectionStart = tb_info.Text.Length;
			tb_info.SelectionLength = 0;

			FxeGeneratorF.That.Cursor = Cursors.Default;

			_info = tb_info.Text;
			_t1.Tick += OnTick;
			_t1.Interval = 1450;
		}
		#endregion cTor


		#region handlers (override)
		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Escape:
					Close();
					break;

				case Keys.Enter:
					try
					{
						_t1.Start();

						Clipboard.SetText(_info + Environment.NewLine);
						tb_info.Text = "copied";

						tb_info.SelectionStart = tb_info.Text.Length;
						tb_info.SelectionLength = 0;
					}
					catch // yes I know.
					{}
					break;
			}
		}
		#endregion handlers (override)


		#region handlers
		void OnTick(object sender, EventArgs e)
		{
			_t1.Stop();
			tb_info.Text = _info;

			tb_info.SelectionStart = tb_info.Text.Length;
			tb_info.SelectionLength = 0;
		}
		#endregion handlers


		#region designer
		TextBox tb_info;

		void InitializeComponent()
		{
			this.tb_info = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tb_info
			// 
			this.tb_info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tb_info.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_info.Location = new System.Drawing.Point(0, 0);
			this.tb_info.Margin = new System.Windows.Forms.Padding(0);
			this.tb_info.Multiline = true;
			this.tb_info.Name = "tb_info";
			this.tb_info.ReadOnly = true;
			this.tb_info.Size = new System.Drawing.Size(392, 49);
			this.tb_info.TabIndex = 0;
			// 
			// InfoDialog
			// 
			this.ClientSize = new System.Drawing.Size(392, 49);
			this.Controls.Add(this.tb_info);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "InfoDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
