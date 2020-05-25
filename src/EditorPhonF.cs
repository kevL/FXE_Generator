﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorPhonF
		: Form
	{
		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="dt">datatable</param>
		internal EditorPhonF(FxeGeneratorF f, DataTable dt)
		{
			InitializeComponent();

			Location = new Point(f.Left + 20, f.Top + 20);

			int i = 0;
			for (; i != dt.Rows.Count; ++i)
			{
				eg_phons.Rows.Add();
				eg_phons.Rows[i].HeaderCell.Value = dt.Rows[i][0].ToString();
				eg_phons.Rows[i].Cells[0]  .Value = dt.Rows[i][1].ToString();
				eg_phons.Rows[i].Cells[1]  .Value = dt.Rows[i][2].ToString();
			}

			int w = eg_phons.RowHeadersWidth
				  + eg_phons.Columns[0].Width
				  + eg_phons.Columns[1].Width;
			int h = eg_phons.ColumnHeadersHeight
				  + (eg_phons.RowTemplate.Height + 1) * (i + 2)
				  + pa_bot.Height;

			if (h > 750) h = 750;

			ClientSize  = new Size(w, h);
			MaximumSize = new Size(w, 1000);
			MinimumSize = new Size(w, 0);
		}
		#endregion cTor



		#region designer
		EditorGrid eg_phons;
		Panel pa_bot;
		Button bu_ok;
		Button bu_cancel;

		DataGridViewTextBoxColumn phon;
		DataGridViewTextBoxColumn stop;


		void InitializeComponent()
		{
			this.eg_phons = new lipsync_editor.EditorGrid();
			this.phon = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.stop = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_ok = new System.Windows.Forms.Button();
			this.bu_cancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.eg_phons)).BeginInit();
			this.pa_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// eg_phons
			// 
			this.eg_phons.AllowUserToResizeColumns = false;
			this.eg_phons.AllowUserToResizeRows = false;
			this.eg_phons.ColumnHeadersHeight = 25;
			this.eg_phons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.eg_phons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.phon,
			this.stop});
			this.eg_phons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eg_phons.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.eg_phons.Location = new System.Drawing.Point(0, 0);
			this.eg_phons.Margin = new System.Windows.Forms.Padding(0);
			this.eg_phons.MultiSelect = false;
			this.eg_phons.Name = "eg_phons";
			this.eg_phons.RowHeadersWidth = 65;
			this.eg_phons.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.eg_phons.RowTemplate.Height = 20;
			this.eg_phons.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.eg_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eg_phons.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.eg_phons.Size = new System.Drawing.Size(227, 542);
			this.eg_phons.StandardTab = true;
			this.eg_phons.TabIndex = 0;
			// 
			// phon
			// 
			this.phon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.phon.Frozen = true;
			this.phon.HeaderText = "phon";
			this.phon.MinimumWidth = 55;
			this.phon.Name = "phon";
			this.phon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.phon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.phon.Width = 55;
			// 
			// stop
			// 
			this.stop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.stop.Frozen = true;
			this.stop.HeaderText = "stop";
			this.stop.MinimumWidth = 105;
			this.stop.Name = "stop";
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
			this.bu_ok.Size = new System.Drawing.Size(80, 27);
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
			this.bu_cancel.Size = new System.Drawing.Size(80, 27);
			this.bu_cancel.TabIndex = 0;
			this.bu_cancel.Text = "cancel";
			this.bu_cancel.UseVisualStyleBackColor = true;
			// 
			// EditorPhonF
			// 
			this.AcceptButton = this.bu_ok;
			this.CancelButton = this.bu_cancel;
			this.ClientSize = new System.Drawing.Size(227, 574);
			this.Controls.Add(this.eg_phons);
			this.Controls.Add(this.pa_bot);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.Name = "EditorPhonF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Edit phons";
			((System.ComponentModel.ISupportInitialize)(this.eg_phons)).EndInit();
			this.pa_bot.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion designer
	}
}
