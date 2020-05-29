using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using NAudio.Wave;


namespace lipsync_editor
{
	sealed partial class WaverF
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

		decimal _dur;			// total duration of the wavefile
		decimal _durSapistart;	// sync-delay starttime offset
		decimal _durSapistart0;	// for reseting the offset to its hardcoded estimation

		string _offsetPre;		// for reseting the offset on user-error

		WaveOutEvent _waveout = new WaveOutEvent();
		AudioFileReader _audioreader;
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

			bu_reset.Text = tb_offset.Text;

			pa_wave.Select();

			_audioreader = new AudioFileReader(SapiLipsync.That.Wavefile);
			_waveout.Init(_audioreader);
			_waveout.PlaybackStopped += OnPlaybackStopped;
		}
		#endregion cTor


		#region handlers (override)
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_waveout.Dispose();
			_audioreader.Dispose();

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
			if (tb_offset.Focused && keyData == Keys.Enter)
			{
				bu_reset.Select();
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


#region NAudio
		void click_Play(object sender, EventArgs e)
		{
			switch (_waveout.PlaybackState)
			{
				case PlaybackState.Stopped:
					bu_play.Text = "pause";
					_waveout.Play();
					break;

				case PlaybackState.Playing:
					bu_play.Text = "play";
					_waveout.Pause();
					break;

				case PlaybackState.Paused:
					bu_play.Text = "pause";
					_waveout.Play();
					break;
			}
		}

		void click_Stop(object sender, EventArgs args)
		{
			if (_waveout != null)
				_waveout.Stop();
		}

		void OnPlaybackStopped(object sender, StoppedEventArgs args)
		{
			bu_play.Text = "play";
			_audioreader.Position = 0L;
		}
#endregion NAudio


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
	}
}
