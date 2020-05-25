using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed class WaverF
		: Form
	{
		#region fields (static)
		static int _factor = 2;
		#endregion fields (static)


		#region fields
		EditorPhonF _f;
		short[] _samples;
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="wavefile">fullpath of PCM-wave file</param>
		internal WaverF(EditorPhonF f, string wavefile)
		{
			InitializeComponent();
			_f = f;

			Conatiner(wavefile);
		}
		#endregion cTor


		#region handlers (override)
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_f.Waver = null;
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
		#endregion handlers (override)


		#region handlers
		void paint_WavePanel(object sender, PaintEventArgs e)
		{
			Cursor = Cursors.WaitCursor;

			e.Graphics.DrawLine(Pens.Snow,
								pa_wave.Left,  pa_wave.Height / 2,
								pa_wave.Right, pa_wave.Height / 2);

			int offsetVert = pa_wave.Height / 2;

			decimal factorHori = (decimal)pa_wave.Width / _samples.Length;
			decimal factorVert = (decimal)pa_wave.Height * _factor / 65536;

			int pixelGroupCount = _samples.Length / pa_wave.Width + 1;

			int hi, hitest, j, x, length = _samples.Length;
			for (int i = 0; i < length; ++i)
			{
				hi = 0; // draw only the highest/lowest amplitude in each pixel-group ->
				for (j = 0; j != pixelGroupCount && i + j < length; ++j)
				{
					hitest = _samples[i + j];
					if (Math.Abs(hitest) > Math.Abs(hi))
						hi = hitest;
				}
				i += j - 1;

				x = (int)(i * factorHori);
				e.Graphics.DrawLine(Pens.Lime,
									x, offsetVert,
									x, offsetVert + (int)(hi * factorVert));
			}
			Cursor = Cursors.Default;
		}
		#endregion handlers


		#region methods
		void Conatiner(string wavefile)
		{
			using (var fs = new FileStream(wavefile, FileMode.Open))
			{
				var br = new BinaryReader(fs);

				br.BaseStream.Seek(40, SeekOrigin.Begin);
				uint size = br.ReadUInt32() / 2u;
				_samples = new short[size];

				int i = -1;
				while (br.BaseStream.Position < br.BaseStream.Length)
					_samples[++i] = br.ReadInt16();

				br.Close();
			}
		}
		#endregion methods



		#region designer
		BufferedPanel pa_wave;

		void InitializeComponent()
		{
			this.pa_wave = new lipsync_editor.BufferedPanel();
			this.SuspendLayout();
			// 
			// pa_wave
			// 
			this.pa_wave.BackColor = System.Drawing.Color.Black;
			this.pa_wave.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pa_wave.Location = new System.Drawing.Point(0, 0);
			this.pa_wave.Margin = new System.Windows.Forms.Padding(0);
			this.pa_wave.Name = "pa_wave";
			this.pa_wave.Size = new System.Drawing.Size(567, 139);
			this.pa_wave.TabIndex = 0;
			this.pa_wave.Paint += new System.Windows.Forms.PaintEventHandler(this.paint_WavePanel);
			// 
			// WaverF
			// 
			this.ClientSize = new System.Drawing.Size(567, 139);
			this.Controls.Add(this.pa_wave);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WaverF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Waver";
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
