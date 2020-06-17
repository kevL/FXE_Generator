using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorGrid
		: DataGridView
	{
		#region fields (static)
		const int MI_INSERT_PHO = 0; // insert phoneme
		const int MI_INSERT_ORT = 1; // insert ortheme aka word
		const int MI_INSERT_SIL = 2; // insert silence
									 // SEPARATOR=3
		const int MI_DELETE_PHO = 4; // delete phoneme row
		const int MI_DELETE_ORT = 5; // delete ortheme row
		#endregion fields (static)


		#region fields
		/// <summary>
		/// Row-id for insert or delete.
		/// </summary>
		int _r;

		/// <summary>
		/// Caches the currently edited cell's field to check for changed.
		/// </summary>
		string _val;
		#endregion fields


		#region properties
		internal DataTable Table
		{ private get; set; }

		internal WaverF Waver
		{ private get; set; }
		#endregion properties


		#region cTor
		public EditorGrid()
		{
			ContextMenu = new ContextMenu();

			ContextMenu.MenuItems.Add("insert phoneme", OnRowInsert_pho);	// MI_INSERT_PHO=0
			ContextMenu.MenuItems.Add("insert ortheme", OnRowInsert_ort);	// MI_INSERT_ORT=1
			ContextMenu.MenuItems.Add("insert silence", OnRowInsert_sil);	// MI_INSERT_SIL=2
			ContextMenu.MenuItems.Add("-");										// SEPARATOR=3
			ContextMenu.MenuItems.Add("delete phoneme", OnRowDelete_pho);	// MI_DELETE_PHO=4
			ContextMenu.MenuItems.Add("delete ortheme", OnRowDelete_ort);	// MI_DELETE_ORT=5
		}
		#endregion cTor


		#region handlers context
		/// <summary>
		/// Inserts a phoneme-row.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRowInsert_pho(object sender, EventArgs e)
		{
			string pos = Table.Rows[_r - 1][0].ToString();			// deter the position-field ->

			int posSeparator = pos.IndexOf('.') + 1;				// ie. only the digits after the separator
			int p2 = Int32.Parse(pos.Substring(posSeparator)) + 1;	// current suffix

			pos = pos.Substring(0, posSeparator);					// current prefix w/ separator


			var row = RowTemplate.Clone() as DataGridViewRow; // insert row in DataGridView ->
			row.CreateCells(this, String.Empty, String.Empty, String.Empty);
			Rows.Insert(_r, row);
			Rows[_r].HeaderCell.Value = pos + p2;


			DataRow dr = Table.NewRow(); // insert row in the DataTable ->
			dr[0] = pos + p2;
			for (int i = 1; i != Table.Columns.Count; ++i)
				dr[i] = String.Empty;
			Table.Rows.InsertAt(dr, _r);


			for (++_r; _r != Table.Rows.Count; ++_r) // advance the post-insert rows ->
			{
				if (Utility.isWordstart(Table.Rows[_r][0].ToString()))
					break;

				++p2;
				Rows[_r].HeaderCell.Value = pos + p2; // in DataGridView
				Table.Rows[_r][0]         = pos + p2; // in the DataTable
			}

//			if (Waver != null) // NOTE: technically that's not necessary since cells are blank.
//				Waver.Invalidate();
		}

		/// <summary>
		/// Inserts an ortheme-row.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRowInsert_ort(object sender, EventArgs e)
		{
			string pos; int p1;

			if (Rows.Count != 1)
			{
				if (_r == Rows.Count - 1) // is last (ie. default) row
				{
					pos = Table.Rows[_r - 1][0].ToString();
					p1 = 1;
				}
				else
				{
					pos = Table.Rows[_r][0].ToString();
					p1 = 0;
				}

				int posSeparator = pos.IndexOf('.');
				pos = pos.Substring(0, posSeparator);

				p1 += Int32.Parse(pos);
			}
			else // has only a default row
				p1 = 0;


			var row = RowTemplate.Clone() as DataGridViewRow; // insert row in DataGridView ->
			row.CreateCells(this, String.Empty, String.Empty, String.Empty);
			Rows.Insert(_r, row);
			Rows[_r].HeaderCell.Value = p1 + ".0";


			DataRow dr = Table.NewRow(); // insert row in the DataTable ->
			dr[0] = p1 + ".0";
			for (int i = 1; i != Table.Columns.Count; ++i)
				dr[i] = String.Empty;
			Table.Rows.InsertAt(dr, _r);


			if (_sil)
			{
				_sil = false;

				Rows[_r].Cells[0].Value =
				Table.Rows[_r][1] = StaticData.SIL;
			}


			for (++_r; _r != Table.Rows.Count; ++_r) // advance the post-insert rows ->
			{
				if (Utility.isWordstart(pos = Table.Rows[_r][0].ToString()))
					++p1;

				int posSeparator = pos.IndexOf('.');
				pos = pos.Substring(posSeparator);

				Rows[_r].HeaderCell.Value =		// in DataGridView
				Table.Rows[_r][0] = p1 + pos;	// in the DataTable
			}
		}

		bool _sil;

		/// <summary>
		/// Inserts a silence-row.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRowInsert_sil(object sender, EventArgs e)
		{
			_sil = true;
			OnRowInsert_ort(null, EventArgs.Empty);
		}

		/// <summary>
		/// Deletes a phoneme row.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRowDelete_pho(object sender, EventArgs e)
		{
			string pos = Table.Rows[_r][0].ToString();			// full position

			int posSeparator = pos.IndexOf('.') + 1;			// position after separator
			int p2 = Int32.Parse(pos.Substring(posSeparator));	// current suffix

			pos = pos.Substring(0, posSeparator);				// current prefix w/ separator


			Rows      .RemoveAt(_r); // delete row in DataGridView
			Table.Rows.RemoveAt(_r); // delete row in the DataTable


			for (; _r != Table.Rows.Count; ++_r) // decrement trailing phoneme positions if any ->
			{
				if (Utility.isWordstart(Table.Rows[_r][0].ToString()))
					break;

				Rows[_r].HeaderCell.Value = pos + p2; // in DataGridView
				Table.Rows[_r][0]         = pos + p2; // in the DataTable

				++p2;
			}


			if (Waver != null) // NOTE: is necessary only if there's a stop value.
				Waver.Refresh(); // req'd
		}

		/// <summary>
		/// Deletes an ortheme row.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRowDelete_ort(object sender, EventArgs e)
		{
			Rows      .RemoveAt(_r); // delete row in DataGridView
			Table.Rows.RemoveAt(_r); // delete row in the DataTable

			while (_r != Table.Rows.Count) // delete the ortheme's phonemes if any ->
			{
				if (Utility.isWordstart(Table.Rows[_r][0].ToString()))
					break;

				Rows      .RemoveAt(_r); // delete row in DataGridView
				Table.Rows.RemoveAt(_r); // delete row in the DataTable
			}


			string pos; int posSeparator, p1;

			for (; _r != Table.Rows.Count; ++_r) // decrement trailing ortheme positions if any ->
			{
				pos = Table.Rows[_r][0].ToString();						// full position
				posSeparator = pos.IndexOf('.');						// position of separator
				p1 = Int32.Parse(pos.Substring(0, posSeparator)) - 1;	// prefix as int
				pos = pos.Substring(posSeparator);						// suffix w/ separator

				Rows[_r].HeaderCell.Value = p1 + pos; // in DataGridView
				Table.Rows[_r][0]         = p1 + pos; // in the DataTable
			}


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


		/// <summary>
		/// RMB opens the context on the row-heads.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowHeaderMouseClick(DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				_r = e.RowIndex;

				bool isDefault = _r == Rows.Count - 1; // is the last default/placeholder row

				bool isWordstart;
				if (!isDefault)
					isWordstart = Utility.isWordstart(Rows[_r].HeaderCell.Value.ToString());
				else
					isWordstart = false;

				ContextMenu.MenuItems[MI_INSERT_PHO].Visible = _r != 0;
				ContextMenu.MenuItems[MI_INSERT_ORT].Visible =
				ContextMenu.MenuItems[MI_INSERT_SIL].Visible =  isWordstart || isDefault;

				ContextMenu.MenuItems[MI_DELETE_PHO].Visible = !isWordstart && !isDefault;
				ContextMenu.MenuItems[MI_DELETE_ORT].Visible =  isWordstart;


				int y_offset = _r * Rows[0].Height + ColumnHeadersHeight;
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
				&& e.RowIndex    != -1 && e.RowIndex != Rows.Count - 1
				&& e.ColumnIndex != -1)
			{
				if (this[e.ColumnIndex, e.RowIndex] == CurrentCell)
				{
					BeginEdit(false);
				}
				else
					base.OnCellMouseDown(e);
			}
		}


		/// <summary>
		/// Caches current val for changed-comparison after edit.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
		{
			_val = CurrentCell.Value.ToString();
			base.OnCellBeginEdit(e);
		}

		/// <summary>
		/// Handles CellEndEdit event.
		/// </summary>
		/// <param name="e"></param>
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

		/// <summary>
		/// Sets the DataTable cell-value to the DataGridView cell-value after
		/// edit.
		/// </summary>
		/// <param name="val">the DataGridView cell-value</param>
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

			//LogtheTable(); // debug
		}

//		void LogtheTable() // debug: Prints the table to the logfile.
//		{
//			for (int r = 0; r != Table.Rows   .Count; ++r)
//			for (int c = 0; c != Table.Columns.Count; ++c)
//			{
//				logfile.Log("[" + r + "]" + "[" + c + "]= " + Table.Rows[r][c]);
//			}
//			logfile.Log();
//		}


		/// <summary>
		/// Colors rows at word-starts.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
		{
			object val = Rows[e.RowIndex].HeaderCell.Value;
			if (val != null && Utility.isWordstart(val.ToString()))
			{
				Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Thistle;
			}
			else
				Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
		}
		#endregion handlers override
	}
}
