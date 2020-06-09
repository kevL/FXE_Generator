using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorGrid
		: DataGridView
	{
		#region
		internal DataTable Table
		{ private get; set; }

		internal WaverF Waver
		{ private get; set; }
		#endregion


		#region cTor
		internal EditorGrid()
		{
			ContextMenu = new ContextMenu();
			ContextMenu.MenuItems.Add("insert", OnRowInsert);
			ContextMenu.MenuItems.Add("delete", OnRowDelete);
		}
		#endregion cTor


		#region handlers context
		void OnRowInsert(object sender, EventArgs e)
		{
			var row = (DataGridViewRow)RowTemplate.Clone();
			row.CreateCells(this, String.Empty, String.Empty, String.Empty);
			Rows.Insert(_r, row);

			DataRow dr = Table.NewRow();
			int cols = Table.Columns.Count;
			for (int i = 0; i != cols; ++i)
				dr[i] = String.Empty;

			Table.Rows.InsertAt(dr, _r);
			LogTable();

			if (Waver != null) // NOTE: technically that's not necessary since cells are blank.
				Waver.Invalidate();
		}

		void OnRowDelete(object sender, EventArgs e)
		{
			if (_r != Rows.Count - 1)
				Rows.RemoveAt(_r);

			Table.Rows.RemoveAt(_r);
			LogTable();

			if (Waver != null) // NOTE: is necessary only if there's a start or stop value.
				Waver.Refresh(); // req'd
		}
		#endregion handlers context


		#region handlers override
		/// <summary>
		/// NOTE: KeyDown won't fire when a cell is in edit.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = e.SuppressKeyPress = true;
				BeginEdit(false);
			}
			else
				base.OnKeyDown(e);
		}

		/// <summary>
		/// NOTE: This fires even when not in edit.
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (IsCurrentCellInEditMode)
			{
				switch (keyData)
				{
					case Keys.Escape:
						CancelEdit();
						// -> Sorry but that does not cancel edit.
						// It reverts a cell's text to what it was before and selects the text.
						// So call EndEdit() - text has been reverted and gets committed ->
						goto case Keys.Enter;

					case Keys.Enter:
						EndEdit();
						return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}


		int _r;

		/// <summary>
		/// RMB opens the context on the row-heads.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowHeaderMouseClick(DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_r = e.RowIndex;

				int y_offset = e.RowIndex * Rows[0].Height + ColumnHeadersHeight;
				ContextMenu.Show(this, new Point(e.X, e.Y + y_offset));
			}
		}


		/// <summary>
		/// Begins edit if the currently active cell is clicked.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left
				&& e.RowIndex != -1 && e.ColumnIndex != -1)
			{
				if (this[e.ColumnIndex, e.RowIndex] == CurrentCell)
				{
					BeginEdit(false);
				}
				else
					base.OnCellMouseDown(e);
			}
		}


		string _val;

		protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
		{
			_val = CurrentCell.Value.ToString();
			base.OnCellBeginEdit(e);
		}

		protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
		{
			base.OnCellEndEdit(e);

			if (CurrentCell.Value.ToString() != _val)
			{
				SetValue(CurrentCell.Value);

				if (Waver != null) // NOTE: is necessary only if start/stop was edited.
					Waver.Refresh(); // req'd
			}
		}

		void SetValue(object val)
		{
			int r = CurrentCell.RowIndex;
			int c = CurrentCell.ColumnIndex;

			switch (c)
			{
				case 0: // phon
					// TODO: check that user-entered phon is valid for the current SpeechRecognizer's language
					break;

				case 1: // start
					// TODO: check that start is greater than the previous start and less than the next start
					// 3 decimal places
					break;

				case 2: // stop
					// TODO: a bunch of stuff ...
					// 3 decimal places
					break;
			}

			Table.Rows[r][c + 1] = CurrentCell.Value; //.ToString()

			LogTable(); // debug
		}
		void LogTable() // debug
		{
			for (int r = 0; r != Table.Rows   .Count; ++r)
			for (int c = 0; c != Table.Columns.Count; ++c)
			{
				logfile.Log("[" + r + "]" + "[" + c + "]= " + Table.Rows[r][c]);
			}
			logfile.Log();
		}


		/// <summary>
		/// Colors rows at word-starts.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
		{
			object val = Rows[e.RowIndex].HeaderCell.Value;
			if (val != null && val.ToString().EndsWith(".0", StringComparison.OrdinalIgnoreCase))
			{
				Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Thistle;
			}
			else
				Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
		}
		#endregion handlers override
	}
}
