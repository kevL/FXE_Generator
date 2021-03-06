﻿using System;
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
		Button bu_rewind;
		Button bu_next;
		Button bu_back;
		Label la_start;
		TextBox tb_start;


		void InitializeComponent()
		{
			this.pa_wave = new lipsync_editor.BufferedPanel();
			this.pa_bot = new System.Windows.Forms.Panel();
			this.tb_start = new System.Windows.Forms.TextBox();
			this.la_start = new System.Windows.Forms.Label();
			this.bu_next = new System.Windows.Forms.Button();
			this.bu_back = new System.Windows.Forms.Button();
			this.bu_rewind = new System.Windows.Forms.Button();
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
			this.pa_wave.Size = new System.Drawing.Size(792, 125);
			this.pa_wave.TabIndex = 0;
			this.pa_wave.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_WavePanel);
			this.pa_wave.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mousedown_WavePanel);
			this.pa_wave.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mousemove_WavePanel);
			this.pa_wave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mouseup_WavePanel);
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.tb_start);
			this.pa_bot.Controls.Add(this.la_start);
			this.pa_bot.Controls.Add(this.bu_next);
			this.pa_bot.Controls.Add(this.bu_back);
			this.pa_bot.Controls.Add(this.bu_rewind);
			this.pa_bot.Controls.Add(this.bu_stop);
			this.pa_bot.Controls.Add(this.bu_play);
			this.pa_bot.Controls.Add(this.bu_reset);
			this.pa_bot.Controls.Add(this.tb_offset);
			this.pa_bot.Controls.Add(this.la_offset);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 125);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(792, 24);
			this.pa_bot.TabIndex = 1;
			this.pa_bot.SizeChanged += new System.EventHandler(this.sizechanged_BotPanel);
			this.pa_bot.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_BotPanel);
			// 
			// tb_start
			// 
			this.tb_start.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_start.Location = new System.Drawing.Point(503, 2);
			this.tb_start.Margin = new System.Windows.Forms.Padding(0);
			this.tb_start.Name = "tb_start";
			this.tb_start.ReadOnly = true;
			this.tb_start.Size = new System.Drawing.Size(51, 20);
			this.tb_start.TabIndex = 9;
			this.tb_start.TabStop = false;
			this.tb_start.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tb_start.WordWrap = false;
			this.tb_start.Enter += new System.EventHandler(this.enter_Startpos);
			// 
			// la_start
			// 
			this.la_start.Location = new System.Drawing.Point(466, 4);
			this.la_start.Margin = new System.Windows.Forms.Padding(0);
			this.la_start.Name = "la_start";
			this.la_start.Size = new System.Drawing.Size(35, 15);
			this.la_start.TabIndex = 8;
			this.la_start.Text = "Start";
			this.la_start.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// bu_next
			// 
			this.bu_next.Image = global::FXE_Generator.Properties.Resource.transport_next;
			this.bu_next.Location = new System.Drawing.Point(402, 1);
			this.bu_next.Margin = new System.Windows.Forms.Padding(0);
			this.bu_next.Name = "bu_next";
			this.bu_next.Size = new System.Drawing.Size(60, 22);
			this.bu_next.TabIndex = 7;
			this.bu_next.UseVisualStyleBackColor = true;
			this.bu_next.Click += new System.EventHandler(this.click_StartPosition);
			// 
			// bu_back
			// 
			this.bu_back.Image = global::FXE_Generator.Properties.Resource.transport_back;
			this.bu_back.Location = new System.Drawing.Point(343, 1);
			this.bu_back.Margin = new System.Windows.Forms.Padding(0);
			this.bu_back.Name = "bu_back";
			this.bu_back.Size = new System.Drawing.Size(60, 22);
			this.bu_back.TabIndex = 6;
			this.bu_back.UseVisualStyleBackColor = true;
			this.bu_back.Click += new System.EventHandler(this.click_StartPosition);
			// 
			// bu_rewind
			// 
			this.bu_rewind.Image = global::FXE_Generator.Properties.Resource.transport_rewind;
			this.bu_rewind.Location = new System.Drawing.Point(163, 1);
			this.bu_rewind.Margin = new System.Windows.Forms.Padding(0);
			this.bu_rewind.Name = "bu_rewind";
			this.bu_rewind.Size = new System.Drawing.Size(60, 22);
			this.bu_rewind.TabIndex = 3;
			this.bu_rewind.UseVisualStyleBackColor = true;
			this.bu_rewind.Click += new System.EventHandler(this.click_Rewind);
			// 
			// bu_stop
			// 
			this.bu_stop.Image = global::FXE_Generator.Properties.Resource.transport_stop;
			this.bu_stop.Location = new System.Drawing.Point(281, 1);
			this.bu_stop.Margin = new System.Windows.Forms.Padding(0);
			this.bu_stop.Name = "bu_stop";
			this.bu_stop.Size = new System.Drawing.Size(60, 22);
			this.bu_stop.TabIndex = 5;
			this.bu_stop.UseVisualStyleBackColor = true;
			this.bu_stop.Click += new System.EventHandler(this.click_Stop);
			// 
			// bu_play
			// 
			this.bu_play.Image = global::FXE_Generator.Properties.Resource.transport_play;
			this.bu_play.Location = new System.Drawing.Point(222, 1);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(60, 22);
			this.bu_play.TabIndex = 4;
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.click_Play);
			// 
			// bu_reset
			// 
			this.bu_reset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.bu_reset.Location = new System.Drawing.Point(94, 1);
			this.bu_reset.Margin = new System.Windows.Forms.Padding(0);
			this.bu_reset.Name = "bu_reset";
			this.bu_reset.Size = new System.Drawing.Size(60, 22);
			this.bu_reset.TabIndex = 2;
			this.bu_reset.UseVisualStyleBackColor = true;
			this.bu_reset.Click += new System.EventHandler(this.click_Offsetreset);
			// 
			// tb_offset
			// 
			this.tb_offset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_offset.Location = new System.Drawing.Point(42, 2);
			this.tb_offset.Margin = new System.Windows.Forms.Padding(0);
			this.tb_offset.Name = "tb_offset";
			this.tb_offset.Size = new System.Drawing.Size(51, 20);
			this.tb_offset.TabIndex = 1;
			this.tb_offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.tb_offset.WordWrap = false;
			this.tb_offset.TextChanged += new System.EventHandler(this.textchanged_Offset);
			this.tb_offset.Leave += new System.EventHandler(this.leave_Offset);
			// 
			// la_offset
			// 
			this.la_offset.Location = new System.Drawing.Point(5, 4);
			this.la_offset.Margin = new System.Windows.Forms.Padding(0);
			this.la_offset.Name = "la_offset";
			this.la_offset.Size = new System.Drawing.Size(36, 15);
			this.la_offset.TabIndex = 0;
			this.la_offset.Text = "Delay";
			this.la_offset.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WaverF
			// 
			this.ClientSize = new System.Drawing.Size(792, 149);
			this.Controls.Add(this.pa_wave);
			this.Controls.Add(this.pa_bot);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(377, 150);
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
