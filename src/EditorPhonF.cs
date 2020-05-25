using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorPhonF
		: Form
	{
		#region cTor
		internal EditorPhonF(FxeGeneratorF f, DataTable dt)
		{
			InitializeComponent();

			Location = new Point(f.Left + 20, f.Top + 20);

			int i = 0;
			for (; i != dt.Rows.Count; ++i)
			{
				dg_phon.Rows.Add();
				dg_phon.Rows[i].HeaderCell.Value = dt.Rows[i][0].ToString();
				dg_phon.Rows[i].Cells[0]  .Value = dt.Rows[i][1].ToString();
				dg_phon.Rows[i].Cells[1]  .Value = dt.Rows[i][2].ToString();
			}

			int w = dg_phon.RowHeadersWidth
				  + dg_phon.Columns[0].Width
				  + dg_phon.Columns[1].Width;
			int h = dg_phon.ColumnHeadersHeight
				  + (dg_phon.RowTemplate.Height + 1) * (i + 2)
				  + pa_bot.Height;

			if (h > 600) h = 600;

			ClientSize  = new Size(w, h);
			MaximumSize = new Size(w, 1000);
			MinimumSize = new Size(w, 0);
		}
		#endregion cTor


		#region designer
		DataGridView dg_phon;
		Panel pa_bot;
		Button bu_ok;
		Button bu_cancel;

		DataGridViewTextBoxColumn phon;
		DataGridViewTextBoxColumn stop;


		void InitializeComponent()
		{
			this.dg_phon = new System.Windows.Forms.DataGridView();
			this.phon = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.stop = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_ok = new System.Windows.Forms.Button();
			this.bu_cancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dg_phon)).BeginInit();
			this.pa_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// dg_phon
			// 
			this.dg_phon.AllowUserToResizeRows = false;
			this.dg_phon.ColumnHeadersHeight = 25;
			this.dg_phon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dg_phon.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.phon,
			this.stop});
			this.dg_phon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_phon.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.dg_phon.Location = new System.Drawing.Point(0, 0);
			this.dg_phon.Margin = new System.Windows.Forms.Padding(0);
			this.dg_phon.MultiSelect = false;
			this.dg_phon.Name = "dg_phon";
			this.dg_phon.RowHeadersWidth = 65;
			this.dg_phon.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dg_phon.RowTemplate.Height = 20;
			this.dg_phon.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.dg_phon.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dg_phon.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dg_phon.Size = new System.Drawing.Size(227, 542);
			this.dg_phon.StandardTab = true;
			this.dg_phon.TabIndex = 0;
			// 
			// phon
			// 
			this.phon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.phon.Frozen = true;
			this.phon.HeaderText = "phon";
			this.phon.MinimumWidth = 55;
			this.phon.Name = "phon";
			this.phon.ReadOnly = true;
			this.phon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.phon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.phon.Width = 55;
			// 
			// stop
			// 
			this.stop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			this.stop.Frozen = true;
			this.stop.HeaderText = "stop";
			this.stop.MinimumWidth = 105;
			this.stop.Name = "stop";
			this.stop.ReadOnly = true;
			this.stop.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.stop.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.stop.Width = 105;
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.bu_ok);
			this.pa_bot.Controls.Add(this.bu_cancel);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 542);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(227, 32);
			this.pa_bot.TabIndex = 1;
			// 
			// bu_ok
			// 
			this.bu_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bu_ok.Location = new System.Drawing.Point(131, 3);
			this.bu_ok.Margin = new System.Windows.Forms.Padding(0);
			this.bu_ok.Name = "bu_ok";
			this.bu_ok.Size = new System.Drawing.Size(80, 26);
			this.bu_ok.TabIndex = 1;
			this.bu_ok.Text = "ok";
			this.bu_ok.UseVisualStyleBackColor = true;
			// 
			// bu_cancel
			// 
			this.bu_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bu_cancel.Location = new System.Drawing.Point(16, 3);
			this.bu_cancel.Margin = new System.Windows.Forms.Padding(0);
			this.bu_cancel.Name = "bu_cancel";
			this.bu_cancel.Size = new System.Drawing.Size(80, 26);
			this.bu_cancel.TabIndex = 0;
			this.bu_cancel.Text = "cancel";
			this.bu_cancel.UseVisualStyleBackColor = true;
			// 
			// EditorPhonF
			// 
			this.AcceptButton = this.bu_ok;
			this.CancelButton = this.bu_cancel;
			this.ClientSize = new System.Drawing.Size(227, 574);
			this.Controls.Add(this.dg_phon);
			this.Controls.Add(this.pa_bot);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.Name = "EditorPhonF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Edit phons";
			((System.ComponentModel.ISupportInitialize)(this.dg_phon)).EndInit();
			this.pa_bot.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion designer
	}
}
