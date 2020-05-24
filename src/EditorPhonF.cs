using System;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class EditorPhonF
		: Form
	{
		#region cTor
		internal EditorPhonF()
		{
			InitializeComponent();
		}
		#endregion cTor


		#region designer
		DataGridView dg_phon;

		void InitializeComponent()
		{
			this.dg_phon = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.dg_phon)).BeginInit();
			this.SuspendLayout();
			// 
			// dg_phon
			// 
			this.dg_phon.AllowUserToResizeRows = false;
			this.dg_phon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dg_phon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_phon.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.dg_phon.Location = new System.Drawing.Point(0, 0);
			this.dg_phon.Margin = new System.Windows.Forms.Padding(0);
			this.dg_phon.MultiSelect = false;
			this.dg_phon.Name = "dg_phon";
			this.dg_phon.RowHeadersWidth = 40;
			this.dg_phon.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dg_phon.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dg_phon.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			this.dg_phon.Size = new System.Drawing.Size(292, 574);
			this.dg_phon.StandardTab = true;
			this.dg_phon.TabIndex = 0;
			// 
			// EditorPhonF
			// 
			this.ClientSize = new System.Drawing.Size(292, 574);
			this.Controls.Add(this.dg_phon);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "EditorPhonF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Edit phon";
			((System.ComponentModel.ISupportInitialize)(this.dg_phon)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion designer
	}
}
