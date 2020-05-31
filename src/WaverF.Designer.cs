using System;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed partial class WaverF
	{
		#region designer
		BufferedPanel pa_wave;
		Panel pa_bot;
		TextBox tb_offset;
		Label la_offset;
		Button bu_reset;
		Button bu_play;
		Button bu_stop;


		void InitializeComponent()
		{
			this.pa_wave = new lipsync_editor.BufferedPanel();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.bu_stop = new System.Windows.Forms.Button();
			this.bu_play = new System.Windows.Forms.Button();
			this.bu_reset = new System.Windows.Forms.Button();
			this.tb_offset = new System.Windows.Forms.TextBox();
			this.la_offset = new System.Windows.Forms.Label();
			this.pa_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// pa_wave
			// 
			this.pa_wave.BackColor = System.Drawing.Color.Black;
			this.pa_wave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pa_wave.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.pa_wave.Location = new System.Drawing.Point(0, 0);
			this.pa_wave.Margin = new System.Windows.Forms.Padding(0);
			this.pa_wave.Name = "pa_wave";
			this.pa_wave.Size = new System.Drawing.Size(567, 115);
			this.pa_wave.TabIndex = 0;
			this.pa_wave.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_WavePanel);
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.bu_stop);
			this.pa_bot.Controls.Add(this.bu_play);
			this.pa_bot.Controls.Add(this.bu_reset);
			this.pa_bot.Controls.Add(this.tb_offset);
			this.pa_bot.Controls.Add(this.la_offset);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 115);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(567, 24);
			this.pa_bot.TabIndex = 1;
			this.pa_bot.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_BotPanel);
			// 
			// bu_stop
			// 
			this.bu_stop.Location = new System.Drawing.Point(310, 1);
			this.bu_stop.Margin = new System.Windows.Forms.Padding(0);
			this.bu_stop.Name = "bu_stop";
			this.bu_stop.Size = new System.Drawing.Size(60, 22);
			this.bu_stop.TabIndex = 5;
			this.bu_stop.Text = "stop";
			this.bu_stop.UseVisualStyleBackColor = true;
			this.bu_stop.Click += new System.EventHandler(this.click_Stop);
			// 
			// bu_play
			// 
			this.bu_play.Location = new System.Drawing.Point(251, 1);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(60, 22);
			this.bu_play.TabIndex = 4;
			this.bu_play.Text = "play";
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.click_Play);
			// 
			// bu_reset
			// 
			this.bu_reset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.bu_reset.Location = new System.Drawing.Point(120, 1);
			this.bu_reset.Margin = new System.Windows.Forms.Padding(0);
			this.bu_reset.Name = "bu_reset";
			this.bu_reset.Size = new System.Drawing.Size(60, 22);
			this.bu_reset.TabIndex = 3;
			this.bu_reset.UseVisualStyleBackColor = true;
			this.bu_reset.Click += new System.EventHandler(this.click_Syncreset);
			// 
			// tb_offset
			// 
			this.tb_offset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_offset.Location = new System.Drawing.Point(68, 2);
			this.tb_offset.Margin = new System.Windows.Forms.Padding(0);
			this.tb_offset.Name = "tb_offset";
			this.tb_offset.Size = new System.Drawing.Size(51, 20);
			this.tb_offset.TabIndex = 0;
			this.tb_offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tb_offset.TextChanged += new System.EventHandler(this.textchanged_Syncdelay);
			this.tb_offset.Leave += new System.EventHandler(this.leave_Syncdelay);
			// 
			// la_offset
			// 
			this.la_offset.Location = new System.Drawing.Point(5, 4);
			this.la_offset.Margin = new System.Windows.Forms.Padding(0);
			this.la_offset.Name = "la_offset";
			this.la_offset.Size = new System.Drawing.Size(62, 15);
			this.la_offset.TabIndex = 1;
			this.la_offset.Text = "sync Delay";
			this.la_offset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WaverF
			// 
			this.ClientSize = new System.Drawing.Size(567, 139);
			this.Controls.Add(this.pa_wave);
			this.Controls.Add(this.pa_bot);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "WaverF";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.pa_bot.ResumeLayout(false);
			this.pa_bot.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion designer
	}



	/// <summary>
	/// A derived Panel that flags DoubleBuffered and ResizeRedraw.
	/// </summary>
	sealed class BufferedPanel
		: Panel
	{
//		#region Properties (override)
//		/// <summary>
//		/// This works great. Absolutely kills flicker on redraws.
//		/// </summary>
//		protected override CreateParams CreateParams
//		{
//			get
//			{
//				CreateParams cp = base.CreateParams;
//				cp.ExStyle |= 0x02000000;
//				return cp;
//			}
//		}
//		#endregion Properties (override)


		#region cTor
		public BufferedPanel()
		{
			DoubleBuffered =
			ResizeRedraw = true;
		}
		#endregion cTor
	}
}
