using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class WaverF
		: Form
	{
		#region fields (static)
		const string TITLE = "Waver";

		const short THRESHOLD = (short)60;
		static int _factor = 2;

		static int _x = -1;
		static int _y = -1;
		static int _w = -1;
		static int _h = -1;
		#endregion fields (static)


		#region fields
		EditorPhonF _f;
		DataTable _dt;

		short[] _samples;

		decimal _dur;
		decimal _durSapistart;
		decimal _durSapistart0;

		string _offsetPre;
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="wavefile">fullpath of PCM-wave file</param>
		/// <param name="dt">datatable</param>
		internal WaverF(EditorPhonF f, string wavefile, DataTable dt)
		{
			InitializeComponent();
			_f  = f;
			_dt = dt;

			if (_x != -1)
			{
				Location = new Point(_x, _y);
				ClientSize = new Size(_w, _h);
			}
			else
				Location = new Point(_f.Left + 20, _f.Top + 20);


			Conatiner(wavefile);
			_durSapistart0 = _durSapistart;

			Text = TITLE + " - " + FxeGeneratorF.Filelabel + " - " + _dur.ToString("F3") + " sec";

			tb_offset.BackColor = Color.MintCream;
			tb_offset.Text = _durSapistart.ToString("F3");
			tb_offset.TextChanged += textchanged_Visdelay;
			tb_offset.Leave       += leave_Visdelay;

			bu_reset.Text = tb_offset.Text;
			bu_reset.Click += click_Reset;

			pa_wave.Select();
		}
		#endregion cTor


		#region handlers (override)
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_f.Waver = null;

			switch (e.CloseReason)
			{
				case CloseReason.UserClosing:
				case CloseReason.None:
					if (WindowState == FormWindowState.Normal)
					{
						_x = Math.Max(0, Left);
						_y = Math.Max(0, Top);
						_w = ClientSize.Width;
						_h = ClientSize.Height;
					}
					break;
			}
			base.OnFormClosing(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			int factor = 0;
			switch (e.KeyData)
			{
				case Keys.F5: factor = 1; break;
				case Keys.F6: factor = 2; break;
				case Keys.F7: factor = 3; break;
				case Keys.F8: factor = 4; break;
			}

			if (factor != 0 && factor != _factor)
			{
				e.Handled = e.SuppressKeyPress = true;
				_factor = factor;
				pa_wave.Refresh();
			}
			else
				base.OnKeyDown(e);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Enter)
			{
				pa_wave.Select();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}
		#endregion handlers (override)


		#region handlers
		void textchanged_Visdelay(object sender, EventArgs e)
		{
			decimal result;
			if (Decimal.TryParse(tb_offset.Text, out result)
				&& result >= 0 && result < 1000)
			{
				_offsetPre = tb_offset.Text;
				_durSapistart = result;
				pa_wave.Invalidate();
			}
			else
				tb_offset.Text = _offsetPre;
		}

		void leave_Visdelay(object sender, EventArgs e)
		{
			tb_offset.Text = _durSapistart.ToString("F3");
		}

		void click_Reset(object sender, EventArgs e)
		{
			_durSapistart = _durSapistart0;
			tb_offset.Text = _durSapistart.ToString("F3");
		}


		void paint_WavePanel(object sender, PaintEventArgs e)
		{
			int offsetVert = pa_wave.Height / 2;

			e.Graphics.DrawLine(Pens.OliveDrab,
								pa_wave.Left,  offsetVert,
								pa_wave.Right, offsetVert);

			decimal factorHori = (decimal)pa_wave.Width / _samples.Length;
			decimal factorVert = (decimal)pa_wave.Height * _factor / 65536;

			int pixelGroupCount = _samples.Length / pa_wave.Width + 1;

			Pen pen;

			short hi, hitest;
			int j, x,y, length = _samples.Length;
			for (int i = 0; i < length; ++i)
			{
				hi = (short)0; // draw only the highest/lowest amplitude in each pixel-group ->
				for (j = 0; j != pixelGroupCount && i + j < length; ++j)
				{
					hitest = _samples[i + j];
					if (Math.Abs(hitest) > Math.Abs(hi))
						hi = hitest;
				}
				i += j - 1;

				if (hi != (short)0)
				{
					if (Math.Abs(hi) > THRESHOLD) pen = Pens.Lime;
					else                          pen = Pens.Firebrick;

					x = (int)((decimal)i  * factorHori);
					y = (int)((decimal)hi * factorVert);
					if (y == 0) // always pip a non-zero amplitude ->
						y = (int)hi / Math.Abs(hi); // pos/neg

					e.Graphics.DrawLine(pen,
										x, offsetVert,
										x, offsetVert + y);
				}
			}


			// draw the word starts and stops
			factorHori = (decimal)pa_wave.Width / _dur;

			for (int i = 0; i != _dt.Rows.Count; ++i)
			{
				string pos = _dt.Rows[i][0].ToString();
				if (pos.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
				{
					x = (int)(((Decimal.Parse(_dt.Rows[i][2].ToString(), CultureInfo.InvariantCulture)) + _durSapistart) * factorHori); // start-line
					e.Graphics.DrawLine(Pens.Red,
										x, 0,
										x, pa_wave.Height);

					pos = pos.Substring(0, pos.Length - 2);
					e.Graphics.DrawString(pos, pa_wave.Font, Brushes.AliceBlue, (float)x + 1f, 1f);
				}

				x = (int)(((Decimal.Parse(_dt.Rows[i][3].ToString(), CultureInfo.InvariantCulture)) + _durSapistart) * factorHori); // stop-line
				e.Graphics.DrawLine(Pens.Blue,
									x, 16,
									x, pa_wave.Height - 16);
			}
		}
		#endregion handlers


		#region methods
		/// <summary>
		/// Parses and pushes 16-bit samples to an array.
		/// @note The wavefile shall be PCM 44.1kHz 16-bit Mono.
		/// </summary>
		/// <param name="wavefile"></param>
		void Conatiner(string wavefile)
		{
			using (var fs = new FileStream(wavefile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var br = new BinaryReader(fs);

				br.BaseStream.Seek(40, SeekOrigin.Begin);
				uint size = br.ReadUInt32() / 2u;
				_samples = new short[size];

				int i = -1;
				short val;
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					val = br.ReadInt16();
					_samples[++i] = val;

					if (_durSapistart == 0 && Math.Abs(val) > THRESHOLD) // TODO: arbitrary. Fix this in the Sapi filestream. if possible ...
						_durSapistart = (decimal)i / 44100;
				}
				br.Close();
			}
			_dur = (decimal)_samples.Length / 44100;
		}
		#endregion methods



		#region designer
		BufferedPanel pa_wave;
		private System.Windows.Forms.Panel pa_bot;
		private System.Windows.Forms.TextBox tb_offset;
		private System.Windows.Forms.Label la_offset;
		private System.Windows.Forms.Button bu_reset;

		void InitializeComponent()
		{
			this.pa_wave = new lipsync_editor.BufferedPanel();
			this.pa_bot = new System.Windows.Forms.Panel();
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
			this.pa_wave.Size = new System.Drawing.Size(567, 114);
			this.pa_wave.TabIndex = 0;
			this.pa_wave.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_WavePanel);
			// 
			// pa_bot
			// 
			this.pa_bot.Controls.Add(this.bu_reset);
			this.pa_bot.Controls.Add(this.tb_offset);
			this.pa_bot.Controls.Add(this.la_offset);
			this.pa_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pa_bot.Location = new System.Drawing.Point(0, 114);
			this.pa_bot.Margin = new System.Windows.Forms.Padding(0);
			this.pa_bot.Name = "pa_bot";
			this.pa_bot.Size = new System.Drawing.Size(567, 25);
			this.pa_bot.TabIndex = 1;
			// 
			// bu_reset
			// 
			this.bu_reset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.bu_reset.Location = new System.Drawing.Point(121, 2);
			this.bu_reset.Margin = new System.Windows.Forms.Padding(0);
			this.bu_reset.Name = "bu_reset";
			this.bu_reset.Size = new System.Drawing.Size(60, 22);
			this.bu_reset.TabIndex = 3;
			this.bu_reset.UseVisualStyleBackColor = true;
			// 
			// tb_offset
			// 
			this.tb_offset.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tb_offset.Location = new System.Drawing.Point(68, 3);
			this.tb_offset.Margin = new System.Windows.Forms.Padding(0);
			this.tb_offset.Name = "tb_offset";
			this.tb_offset.Size = new System.Drawing.Size(51, 20);
			this.tb_offset.TabIndex = 0;
			this.tb_offset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// la_offset
			// 
			this.la_offset.Location = new System.Drawing.Point(5, 5);
			this.la_offset.Margin = new System.Windows.Forms.Padding(0);
			this.la_offset.Name = "la_offset";
			this.la_offset.Size = new System.Drawing.Size(65, 15);
			this.la_offset.TabIndex = 1;
			this.la_offset.Text = "sync Delay";
			this.la_offset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// WaverF
			// 
			this.ClientSize = new System.Drawing.Size(567, 139);
			this.Controls.Add(this.pa_wave);
			this.Controls.Add(this.pa_bot);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.KeyPreview = true;
			this.Name = "WaverF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
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
