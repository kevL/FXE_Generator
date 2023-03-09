using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorPhonF
		: Form
	{
		#region fields (static)
		static int _x = -1;
		static int _y = -1;

		const int START_H = 575;
		#endregion fields (static)


		#region fields
		FxeGeneratorF _f;
		DataTable _dt;
		#endregion fields


		#region properties
		internal WaverF Waver
		{ private get; set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="dt">datatable</param>
		internal EditorPhonF(FxeGeneratorF f, DataTable dt)
		{
			InitializeComponent();
			_f  = f;
			_dt = dt;


			foreach (DataColumn dc in _dt.Columns) // datatable editable
				dc.ReadOnly = false;

			grid.Table = _dt; // TODO: grid initialization to 'grid.Table.set' ->

			int i = 0;
			for (; i != _dt.Rows.Count; ++i)
			{
				grid.Rows.Add();
				grid.Rows[i].HeaderCell.Value = _dt.Rows[i][0].ToString(); // pos
				grid.Rows[i].Cells[0]  .Value = _dt.Rows[i][1].ToString(); // phon
				grid.Rows[i].Cells[1]  .Value = _dt.Rows[i][2].ToString(); // start
				grid.Rows[i].Cells[2]  .Value = _dt.Rows[i][3].ToString(); // stop
			}

			int w = grid.RowHeadersWidth
				  + grid.Columns[0].Width
				  + grid.Columns[1].Width
				  + grid.Columns[2].Width;
			int h = grid.ColumnHeadersHeight
				  + (grid.RowTemplate.Height + 1) * (i + 2)
				  + pa_bot.Height;

			if (h > START_H) h = START_H;

			ClientSize  = new Size(w, h);
			MaximumSize = new Size(w, 1000);
			MinimumSize = new Size(w, 0);

			Screen screen = Screen.FromControl(this);
			int height = screen.WorkingArea.Height;

			int bordersVert = Height - h;

			int x,y;
			if (_x != -1)
			{
				x = _x;

				if (_y + h + bordersVert > height)
					y = height - h - bordersVert;
				else
					y = _y;
			}
			else
			{
				x = _f.Left + 20;

				if (_f.Top + 20 + h + bordersVert > height)
					y = height - h - bordersVert;
				else
					y = _f.Top + 20;
			}
			Location = new Point(x,y);
		}
		#endregion cTor


		#region handlers (override)
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			foreach (DataColumn dc in _dt.Columns) // datatable NOT editable
				dc.ReadOnly = true;

			if (Waver != null)
			{
				// NOTE: Dispose() bypasses the OnFormClosing event that's
				// needed to cache the Waver's telemetry.
				Waver.Close();
				Waver = null;
			}

			switch (e.CloseReason)
			{
				case CloseReason.UserClosing:
				case CloseReason.None:
					_x = Math.Max(0, Left);
					_y = Math.Max(0, Top);
					break;
			}
			base.OnFormClosing(e);
		}
		#endregion handlers (override)


		#region handlers
		/// <summary>
		/// Opens <c><see cref="WaverF"/></c>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Waver(object sender, EventArgs e)
		{
			if (Waver == null)
			{
				Waver = new WaverF(this, _f._sapi.Wavefile, _dt);
				grid.Waver = Waver;
				Waver.Show(_f);
			}
			else
				Waver.Activate();
		}


		/// <summary>
		/// Builds a list of OrthographicResults from the edited DataTable.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Accept(object sender, EventArgs e)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("click_Accept()");
#endif
			// TODO: only if changed

			_f._ars_alt = new List<OrthographicResult>();

			OrthographicResult result;
			bool decr;

			for (int r = 0; r != _dt.Rows.Count; ++r)
			{
				string pos = _dt.Rows[r][0] as String;										// pos
#if DEBUG
				logfile.Log(". _dt.Rows[" + r + "][0]= " + _dt.Rows[r][0]);
#endif
				if (Utility.isWordstart(pos))
				{
					decr = false;

					result = new OrthographicResult();
					result.Orthography = String.Empty;
					result.Confi       = 0f;
					result.Level       = String.Empty;

					result.Phons = new List<string>();
#if DEBUG
					logfile.Log(". . _dt.Rows[" + r + "][1]= " + _dt.Rows[r][1]);
					logfile.Log(". . _dt.Rows[" + r + "][2]= " + _dt.Rows[r][2]);
					logfile.Log(". . _dt.Rows[" + r + "][3]= " + _dt.Rows[r][3]);
#endif
					result.Phons.Add(_dt.Rows[r][1] as String);								// phon

					result.Start = Decimal.Parse(_dt.Rows[r][2].ToString());				// start

					result.phStops.Add(Decimal.Parse(_dt.Rows[r][3].ToString()));			// stop - 1st phon


					if (r != _dt.Rows.Count - 1)
					{
						decr = true;

						pos = _dt.Rows[++r][0] as String;
						while (!Utility.isWordstart(pos))
						{
#if DEBUG
							logfile.Log(". . . _dt.Rows[" + r + "][1]= " + _dt.Rows[r][1]);
							logfile.Log(". . . _dt.Rows[" + r + "][3]= " + _dt.Rows[r][3]);
#endif
							result.Phons.Add(_dt.Rows[r][1] as String);						// phon - 2+
							result.phStops.Add(Decimal.Parse(_dt.Rows[r][3].ToString()));	// stop - 2+

							if (r == _dt.Rows.Count - 1)
								break;

							pos = _dt.Rows[++r][0] as String;
						}
					}
#if DEBUG
					logfile.Log(". . _dt.Rows[" + r + "][3]= " + _dt.Rows[r][3]);
#endif
					result.Stop = Decimal.Parse(_dt.Rows[r][3].ToString());					// stop - word

					_f._ars_alt.Add(result);

					if (decr) --r;
				}
			}


			if (_f._ars_alt.Count != 0)
				_f.AlternateData();
		}

		/// <summary>
		/// Reverts the displayed data in 'FxeGeneratorF' per its selected
		/// radio-button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Cancel(object sender, EventArgs e)
		{
			_f.AlternateData(true);
		}
		#endregion handlers



		#region designer
		internal EditorGrid grid;
		Panel pa_bot;
		Button bu_ok;
		Button bu_cancel;

		DataGridViewTextBoxColumn phon;
		DataGridViewTextBoxColumn start;
		DataGridViewTextBoxColumn stop;

		Button bu_waver;


		void InitializeComponent()
		{
			this.grid = new lipsync_editor.EditorGrid();
			this.phon = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.start = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.stop = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_waver = new System.Windows.Forms.Button();
			this.bu_ok = new System.Windows.Forms.Button();
			this.bu_cancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.pa_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowUserToResizeColumns = false;
			this.grid.AllowUserToResizeRows = false;
			this.grid.ColumnHeadersHeight = 25;
			this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.phon,
			this.start,
			this.stop});
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.grid.Location = new System.Drawing.Point(0, 0);
			this.grid.Margin = new System.Windows.Forms.Padding(0);
			this.grid.MultiSelect = false;
			this.grid.Name = "grid";
			this.grid.RowHeadersWidth = 65;
			this.grid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.grid.RowTemplate.Height = 20;
			this.grid.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.grid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.grid.Size = new System.Drawing.Size(332, 542);
			this.grid.StandardTab = true;
			this.grid.TabIndex = 0;
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
			// start
			// 
			this.start.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.start.Frozen = true;
			this.start.HeaderText = "start";
			this.start.MinimumWidth = 105;
			this.start.Name = "start";
			this.start.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.start.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.start.Width = 105;
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
			this.pa_bot.Controls.Add(this.bu_waver);
			this.pa_bot.Controls.Add(this.bu_ok);
			this.pa_bot.Controls.Add(this.bu_cancel);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 542);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(332, 32);
			this.pa_bot.TabIndex = 1;
			// 
			// bu_waver
			// 
			this.bu_waver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bu_waver.Location = new System.Drawing.Point(90, 3);
			this.bu_waver.Margin = new System.Windows.Forms.Padding(0);
			this.bu_waver.Name = "bu_waver";
			this.bu_waver.Size = new System.Drawing.Size(152, 27);
			this.bu_waver.TabIndex = 1;
			this.bu_waver.Text = "~";
			this.bu_waver.UseVisualStyleBackColor = true;
			this.bu_waver.Click += new System.EventHandler(this.click_Waver);
			// 
			// bu_ok
			// 
			this.bu_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bu_ok.Location = new System.Drawing.Point(251, 3);
			this.bu_ok.Margin = new System.Windows.Forms.Padding(0);
			this.bu_ok.Name = "bu_ok";
			this.bu_ok.Size = new System.Drawing.Size(70, 27);
			this.bu_ok.TabIndex = 2;
			this.bu_ok.Text = "ok";
			this.bu_ok.UseVisualStyleBackColor = true;
			this.bu_ok.Click += new System.EventHandler(this.click_Accept);
			// 
			// bu_cancel
			// 
			this.bu_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bu_cancel.Location = new System.Drawing.Point(11, 3);
			this.bu_cancel.Margin = new System.Windows.Forms.Padding(0);
			this.bu_cancel.Name = "bu_cancel";
			this.bu_cancel.Size = new System.Drawing.Size(70, 27);
			this.bu_cancel.TabIndex = 0;
			this.bu_cancel.Text = "no";
			this.bu_cancel.UseVisualStyleBackColor = true;
			this.bu_cancel.Click += new System.EventHandler(this.click_Cancel);
			// 
			// EditorPhonF
			// 
			this.AcceptButton = this.bu_ok;
			this.CancelButton = this.bu_cancel;
			this.ClientSize = new System.Drawing.Size(332, 574);
			this.Controls.Add(this.grid);
			this.Controls.Add(this.pa_bot);
			this.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.Name = "EditorPhonF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Edit";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.pa_bot.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion designer
	}
}
