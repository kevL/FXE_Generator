using System;
using System.Drawing;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorGrid
		: DataGridView
	{
		#region handlers (override)
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
					case Keys.Enter:
						EndEdit();
						return true;
	
					case Keys.Escape:
						CancelEdit();	// Sorry but this does not cancel edit.
										// It reverts a cell's text to what it was before and selects the text.
						EndEdit();		// <- so that shall workaround (text has been reverted and gets committed).
						return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}


		/// <summary>
		/// Begins edit if the currently active cell is clicked.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
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
		#endregion handlers (override)
	}
}
