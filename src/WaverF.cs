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

		const short THRESHOLD = (short)60;	// sample-values below the threshold are considered noise at the start of the wave
		static int _scale = 2;				// scale factor when drawing the waveform

		static int _x = -1; // telemetry ->
		static int _y = -1;
		static int _w = -1;
		static int _h = -1;
		#endregion fields (static)


		#region fields
		readonly EditorPhonF _f;	// parent
		readonly DataTable _dt;		// fxe-data to set phoneme markers in the wave-panel

		short[] _samples;			// the samples of the current wave for drawing the waveform in the wave-panel

		decimal _dur;				// total duration of the wavefile
		decimal _durSapistart;		// sync-delay starttime offset
		decimal _durSapistart0;		// for reseting the offset to its hardcoded estimation

		string _offsetPre;			// for reseting the offset on user-error

		WaveOutEvent _waveout = new WaveOutEvent();	// pushes a wavestream to a soundcard-device
		AudioFileReader _audioreader;				// the frontend for the 'WaveOutEvent'

		Timer _t1 = new Timer();	// redraws the wave-panel every ~15 millisec
		bool _closing;				// true to prevent an 'AudioFileReader' exception when this form closes
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor. Instantiates a 'WaverF' object.
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="wavefile">fullpath of PCM-wave file</param>
		/// <param name="dt">datatable</param>
		internal WaverF(EditorPhonF f, string wavefile, DataTable dt)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("WaverF.cTor");
#endif
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
#if DEBUG
			logfile.Log(". _audioreader.FileName= "     + _audioreader.FileName);
			logfile.Log(". _audioreader.Volume= "       + _audioreader.Volume);
			logfile.Log(". _audioreader.WaveFormat= "   + _audioreader.WaveFormat);
			logfile.Log(". _audioreader.Length= "       + _audioreader.Length);
			logfile.Log(". _audioreader.TotalTime= "    + _audioreader.TotalTime);
			logfile.Log(". _audioreader.Position= "     + _audioreader.Position);
			logfile.Log(". _audioreader.CurrentTime= "  + _audioreader.CurrentTime);
			logfile.Log(". _audioreader.BlockAlign= "   + _audioreader.BlockAlign);
//			logfile.Log(". _audioreader.ReadTimeout= "  + _audioreader.ReadTimeout);	// not supported.
//			logfile.Log(". _audioreader.WriteTimeout= " + _audioreader.WriteTimeout);	// not supported.
			logfile.Log(". _audioreader.CanTimeout= "   + _audioreader.CanTimeout);
			logfile.Log(". _audioreader.CanRead= "      + _audioreader.CanRead);
			logfile.Log(". _audioreader.CanWrite= "     + _audioreader.CanWrite);
			logfile.Log(". _audioreader.CanSeek= "      + _audioreader.CanSeek);
#endif
			_waveout.Init(_audioreader);
#if DEBUG
			logfile.Log(". _waveout.OutputWaveFormat= " + _waveout.OutputWaveFormat);
			logfile.Log(". _waveout.DesiredLatency= "   + _waveout.DesiredLatency);
			logfile.Log(". _waveout.DeviceNumber= "     + _waveout.DeviceNumber);
			logfile.Log(". _waveout.NumberOfBuffers= "  + _waveout.NumberOfBuffers);
			logfile.Log(". _waveout.Volume= "           + _waveout.Volume);
#endif
			_waveout.PlaybackStopped += OnPlaybackStopped;

			_t1.Tick += Track;
			_t1.Interval = 15;
		}
		#endregion cTor


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


		#region handlers override
		/// <summary>
		/// Handles the 'FormClosing' event. Cleans up the 'WaveOutEvent' and
		/// the 'AudioFileReader' and caches telemetry, etc.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_closing = true;
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

		/// <summary>
		/// Handles the 'KeyDown' event. Adjusts the waveform's scale factor on
		/// [F5]..[F8].
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			int scale = 0;
			switch (e.KeyData)
			{
				case Keys.F5: scale = 1; break;
				case Keys.F6: scale = 2; break;
				case Keys.F7: scale = 3; break;
				case Keys.F8: scale = 4; break;
			}

			if (scale != 0 && scale != _scale)
			{
				e.Handled = e.SuppressKeyPress = true;
				_scale = scale;
				pa_wave.Invalidate();
			}
			else
				base.OnKeyDown(e);
		}

		/// <summary>
		/// Handles key-events when the sync-delay textbox has focus. Switches
		/// focus to the Reset-button on [Enter].
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (tb_offset.Focused && keyData == Keys.Enter)
			{
				bu_reset.Select();
				return true;
			}
			return base.ProcessDialogKey(keyData);
		}
		#endregion handlers override


		#region handlers sync-delay
		/// <summary>
		/// Ensures a valid delay when the sync-delay text changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textchanged_Syncdelay(object sender, EventArgs e)
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

		/// <summary>
		/// Ensures that the sync-delay text prints to 3 decimal places.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void leave_Syncdelay(object sender, EventArgs e)
		{
			tb_offset.Text = _durSapistart.ToString("F3");
		}

		/// <summary>
		/// Resets the sync-delay to its hardcoded estimation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Syncreset(object sender, EventArgs e)
		{
			_durSapistart = _durSapistart0;
			tb_offset.Text = _durSapistart.ToString("F3");
		}
		#endregion handlers sync-delay


		#region handlers NAudio
		/// <summary>
		/// Plays or pauses the wave playback.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Play(object sender, EventArgs e)
		{
			switch (_waveout.PlaybackState)
			{
				case PlaybackState.Stopped:
					bu_play.Text = "pause";
					_waveout.Play();
					_t1.Start();
					break;

				case PlaybackState.Playing:
					bu_play.Text = "play";
					_t1.Stop();
					_waveout.Pause();
					pa_wave.Invalidate();
					break;

				case PlaybackState.Paused:
					bu_play.Text = "pause";
					_waveout.Play();
					_t1.Start();
					break;
			}
		}

		/// <summary>
		/// Stops playing the wave.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void click_Stop(object sender, EventArgs args)
		{
			_waveout.Stop();
		}

		/// <summary>
		/// Handles the 'WaveOutEvent.PlaybackStopped'.
		/// @note The wave-device's output buffer needs to be cleared
		/// (re-initialized) or else the buffer will *usually* just grow larger
		/// and larger; however 'WaverF' uses the buffer's current position to
		/// draw the track-caret in the wave-panel so the output-buffer needs to
		/// be reset whenever the 'PlaybackStopped' event fires.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void OnPlaybackStopped(object sender, StoppedEventArgs args)
		{
			_waveout.Dispose(); // ... gr

			_waveout = new WaveOutEvent();
			_waveout.Init(_audioreader);
			_waveout.PlaybackStopped += OnPlaybackStopped;

			if (!_closing)
				_audioreader.Position = 0L;

			bu_play.Text = "play";

			_t1.Stop();
			pa_wave.Invalidate();
		}

		/// <summary>
		/// Forces the wave-panel to redraw every ~15 millisecs.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Track(object sender, EventArgs e)
		{
			pa_wave.Invalidate();
		}
		#endregion handlers NAudio


		#region handlers paint
		/// <summary>
		/// Paints the wave-panel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void paint_WavePanel(object sender, PaintEventArgs e)
		{
			int offsetVert = pa_wave.Height / 2;

			e.Graphics.DrawLine(Pens.OliveDrab,
								pa_wave.Left,  offsetVert,
								pa_wave.Right, offsetVert);

			decimal factorHori = (decimal)pa_wave.Width / _samples.Length;
			decimal factorVert = (decimal)pa_wave.Height * _scale / 65536;

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


			// draw the caret-line
			//logfile.Log(_waveout.GetPosition().ToString());
			// NOTE: Get position from '_waveout' NOT '_audioreader' because the
			// latter is very sluggish here - be aware that the streams are NOT
			// the same however.
			x = (int)((decimal)_waveout.GetPosition() / 4 * factorHori);	// TODO: why 4, should be 2 - actually it's 4+ ARBITRARY.
																			// It's because NAudio is converting 16-bit to 32-bit float
																			// and it could be forcing stereo-channels and whatever else
																			// it feels like.
			e.Graphics.DrawLine(Pens.White,
								x, 0,
								x, pa_wave.Height);


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


		/// <summary>
		/// Paints borders on the bot-panel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void paint_BotPanel(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawLine(Pens.Black, // bot-line
								1,            pa_bot.Height - 1,
								pa_bot.Width, pa_bot.Height - 1);
			e.Graphics.DrawLine(Pens.Black, // left-line
								0, 0,
								0, pa_bot.Height - 1);
			e.Graphics.DrawLine(Pens.Black, // right-line
								pa_bot.Width - 1, 0,
								pa_bot.Width - 1, pa_bot.Height - 1);
		}
		#endregion handlers paint
	}
}

/*		void Track()
		{
			var thread = new Thread(new ThreadStart(track)); // track the Audio-Data on 'track' method
			thread.Start();
		}
		void track()
		{
			while (_track) // create a pseudo-infinite loop
			{
				if (_waveout.PlaybackState == PlaybackState.Playing)
				{
					logfile.Log("bytes= " + _waveout.GetPosition());

					double millisec = _waveout.GetPosition() * 1000.0
						/ _waveout.OutputWaveFormat.BitsPerSample
						/ _waveout.OutputWaveFormat.Channels * 8
						/ _waveout.OutputWaveFormat.SampleRate;

					logfile.Log(millisec.ToString("F3"));
				}

				Thread.Sleep(1000); // sleep for 1 sec
			}
		} */
