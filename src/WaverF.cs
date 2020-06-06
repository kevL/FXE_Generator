using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

using kL_audio;


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
		readonly DataTable _dt;		// fxe-data to set ortheme/phoneme markers in the wave-panel

		short[] _shorts;			// the samples of the current wave for drawing the waveform in the wave-panel

		decimal _dur;				// total duration of the wavefile
		decimal _sapiDelay;			// sync-delay starttime offset
		decimal _sapiDelay0;		// for reseting the offset to its hardcoded estimation

		string _offsetPre;			// for reseting the offset on user-error

		WaveOutEvent _waveout;					// pushes a wavestream through WindowsAPI to a soundcard-device
		readonly WaveFileReader _wavereader;	// the frontend for the 'WaveOutEvent'

		Timer _t1 = new Timer();	// redraws the wave-panel every ~15 millisec during playback
		bool _close;				// true to prevent an 'AudioFileReader' exception when this form closes

		int _posStart;				// position of the start-caret in samples
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor. Instantiates a 'WaverF' object.
		/// </summary>
		/// <param name="f">parent</param>
		/// <param name="wavefile">fullpath of PCM-wave file</param>
		/// <param name="dt">pointer to the 'FxeGeneratorF._dt1' PHONEMES
		/// datatable</param>
		internal WaverF(EditorPhonF f, string wavefile, DataTable dt)
		{
//#if DEBUG
//			logfile.Log();
//			logfile.Log("WaverF.cTor");
//#endif
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
			_sapiDelay0 = _sapiDelay;

			if (ClientSize.Width > _shorts.Length) // ensure min 1 sample/pixel
			{
				ClientSize = new Size(_shorts.Length, ClientSize.Height);
				// TODO: Should set the form's MaxSize restriction here.
				// But what are the odds that user is going to try to deal with
				// a wave that's less than ~20 millisecs ...
				//
				// I'm just saying that I'm not considering rounding errors/
				// ambiguities that could and will occur when one sample is
				// represented by 2+ pixels; my plate is full dealing with the
				// standard case where one pixel represents 1+ samples: recall
				// that 1 sec of audio drawn across a 1000 pixel panel shall
				// have 44.1 samples in each pixel. And that's a lowball
				// estimate
			}

			Text = TITLE + " - " + FxeGeneratorF.Filelabel + " - " + _dur.ToString("F3") + " sec";

			tb_offset.BackColor = Color.MintCream;
			tb_offset.Text = _sapiDelay.ToString("F3");

			bu_reset.Text = tb_offset.Text;

			pa_wave.Select();

			_wavereader = new WaveFileReader(SapiLipsync.That.Wavefile);
			_waveout = new WaveOutEvent(_wavereader);

			_waveout.PlaybackStopped += OnPlaybackStopped;

			_t1.Tick += Track;
			_t1.Interval = 15;
		}
		#endregion cTor


		#region methods
		/// <summary>
		/// Parses and pushes 16-bit samples to a short-array that's used for
		/// screen-display.
		/// @note The wavefile shall be PCM 44.1kHz 16-bit Mono.
		/// </summary>
		/// <param name="wavefile"></param>
		void Conatiner(string wavefile)
		{
			using (var fs = new FileStream(wavefile, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var br = new BinaryReader(fs);

				fs.Seek(40, SeekOrigin.Begin);
				uint datasize = br.ReadUInt32();
				_shorts = new short[datasize / 2u];

				int i = -1;
				short val;
				while (fs.Position < fs.Length)
				{
					val = br.ReadInt16();
					_shorts[++i] = val;

					if (_sapiDelay == 0 && Math.Abs(val) > THRESHOLD) // TODO: arbitrary. Fix this in the Sapi filestream. if possible ...
						_sapiDelay = (decimal)i / 44100;
				}
				br.Close();
			}
			_dur = (decimal)_shorts.Length / 44100;
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
			_close = true;
			_waveout.Dispose();
			_t1.Dispose();
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
				_sapiDelay = result;
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
			tb_offset.Text = _sapiDelay.ToString("F3");
		}

		/// <summary>
		/// Resets the sync-delay to its hardcoded estimation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Syncreset(object sender, EventArgs e)
		{
			_sapiDelay = _sapiDelay0;
			tb_offset.Text = _sapiDelay.ToString("F3");
		}
		#endregion handlers sync-delay


		#region handlers kL_audio
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
					EnableButtons(false);
					_wavereader.Position = (long)_posStart * 2L;
					goto case PlaybackState.Paused;

				case PlaybackState.Paused:
					bu_play.Image = global::FXE_Generator.Properties.Resource.transport_pause;
					_waveout.Play();
					_t1.Start();
					break;

				case PlaybackState.Playing:
					bu_play.Image = global::FXE_Generator.Properties.Resource.transport_play;
					_t1.Stop();
					_waveout.Pause();
					pa_wave.Invalidate();
					break;
			}
		}

		/// <summary>
		/// Dis/enables a few buttons on play/stop.
		/// </summary>
		/// <param name="enable"></param>
		void EnableButtons(bool enable = true)
		{
			bu_rewind.Enabled =
			bu_back  .Enabled =
			bu_next  .Enabled = enable;
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
		/// @note The wave-device's output buffer needs to be cleared or else
		/// the buffer will *usually* just grow larger and larger; however
		/// 'WaverF' uses the buffer's current position to draw the track-caret
		/// in the wave-panel so the output-buffer needs to be reset (recreated)
		/// whenever the 'PlaybackStopped' event fires.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void OnPlaybackStopped(object sender, StoppedEventArgs args)
		{
			if (!_close)
			{
				_waveout.Dispose(); // ... gr

				_waveout = new WaveOutEvent(_wavereader);
				_waveout.PlaybackStopped += OnPlaybackStopped;

				EnableButtons();
				bu_play.Image = global::FXE_Generator.Properties.Resource.transport_play;
				_t1.Stop();

				pa_wave.Invalidate();
			}
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


		bool _dragCaret;
		/// <summary>
		/// Positions the start-caret when the wave-panel is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mousedown_WavePanel(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left
				&& _waveout.PlaybackState == PlaybackState.Stopped)
			{
				_posStart = e.X * _shorts.Length / pa_wave.Width + 1;
				pa_wave.Invalidate();

				_dragCaret = true;
			}
		}

		/// <summary>
		/// Releases '_dragCaret' on MouseUp.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mouseup_WavePanel(object sender, MouseEventArgs e)
		{
			_dragCaret = false;
		}

		/// <summary>
		/// Drags the start-caret on MouseMove.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void mousemove_WavePanel(object sender, MouseEventArgs e)
		{
			if (_dragCaret
				&& e.X > -1 && e.X < pa_wave.Width)
			{
				_posStart = e.X * _shorts.Length / pa_wave.Width + 1;
				pa_wave.Invalidate();
			}
		}

		/// <summary>
		/// Sets the start-caret to the start of the wave.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void click_Rewind(object sender, EventArgs args)
		{
			if (_waveout.PlaybackState == PlaybackState.Stopped)
			{
				_posStart = 0;
				pa_wave.Invalidate();
			}
		}


		/// <summary>
		/// Positions the start-caret at the start of the previous word.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void click_Back(object sender, EventArgs args)
		{
			FindWordStartPosition(false);
		}

		/// <summary>
		/// Positions the start-caret at the start of the next word.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		void click_Next(object sender, EventArgs args)
		{
			FindWordStartPosition(true);
		}

		/// <summary>
		/// Positions the start-caret at the start of the previous or next word.
		/// </summary>
		/// <param name="next">true for nextword; false for previousword</param>
		void FindWordStartPosition(bool next)
		{
			if (_waveout.PlaybackState == PlaybackState.Stopped)
			{
				decimal factorHori = (decimal)pa_wave.Width / _shorts.Length;
				int x = (int)((decimal)_posStart * factorHori);

				var b = new Bitmap(pa_wave.Width, pa_wave.Height);
				pa_wave.DrawToBitmap(b, new Rectangle(0,0, pa_wave.Width, pa_wave.Height));

				int j = pa_wave.Height / 2;

				if (next)
				{
					for (int i = x + 1; i != pa_wave.Width; ++i)
					{
						if (PositionStartCaret(b,i,j))
							break;
					}
				}
				else // back
				{
					for (int i = x - 1; i != -1; --i)
					{
						if (PositionStartCaret(b,i,j))
							break;
					}
				}
			}
		}

		/// <summary>
		/// helper for FindWordStartPosition(). Checks for a red pixel in a
		/// specified bitmap-object.
		/// @note Red-marker shall be used for ortheme-starts and Blue-marker shall
		/// be used for phoneme-starts. NO OTHER MARKERS SHALL USE FULL RED-
		/// COMPONENT OR FULL BLUE-COMPONENT.
		/// </summary>
		/// <param name="b">the bitmap</param>
		/// <param name="i">x-position to check</param>
		/// <param name="j">y-position to check</param>
		/// <returns></returns>
		bool PositionStartCaret(Bitmap b, int i, int j)
		{
			Color color = b.GetPixel(i,j);
			if (color.R == Byte.MaxValue)// || color.B == Byte.MaxValue)
			{
				_posStart = i * _shorts.Length / pa_wave.Width + 1;
				pa_wave.Invalidate();
				return true;
			}
			return false;
		}
		#endregion handlers kL_audio


//		decimal pixelsPerSample()
//		{
//			return (decimal)pa_wave.Width / _samples.Length;
//		}
//		int samplesPerPixel()
//		{
//			return _samples.Length / pa_wave.Width + 1;
//		}


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

			decimal factorHori = (decimal)pa_wave.Width / _shorts.Length;
			decimal factorVert = (decimal)pa_wave.Height * _scale / 65536;

			int pixelGroupCount = _shorts.Length / pa_wave.Width + 1;

			Pen pen;

// draw the wave
			short hi, hitest;
			int j, x,y, length = _shorts.Length;
			for (int i = 0; i < length; ++i)
			{
				hi = (short)0; // draw only the highest/lowest amplitude in each pixel-group ->
				for (j = 0; j != pixelGroupCount && i + j < length; ++j)
				{
					hitest = _shorts[i + j];
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
					if (y == 0)						// always pip a non-zero amplitude ->
						y = (int)hi / Math.Abs(hi);	// pos/neg

					e.Graphics.DrawLine(pen,
										x, offsetVert,
										x, offsetVert + y);
				}
			}


// draw the ortheme starts and phoneme stops
			decimal factorHori_dur = (decimal)pa_wave.Width / _dur;

			for (int i = 0; i != _dt.Rows.Count; ++i)
			{
				string pos = _dt.Rows[i][0].ToString();
				if (pos.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
				{
					x = (int)((Decimal.Parse(_dt.Rows[i][2].ToString(), CultureInfo.InvariantCulture) + _sapiDelay) * factorHori_dur); // ortheme start-marker
					e.Graphics.DrawLine(Pens.Red,
										x, 0,
										x, pa_wave.Height);

					pos = pos.Substring(0, pos.Length - 2);
					e.Graphics.DrawString(pos, pa_wave.Font, Brushes.AliceBlue, (float)x + 1f, 1f);
				}

				x = (int)((Decimal.Parse(_dt.Rows[i][3].ToString(), CultureInfo.InvariantCulture) + _sapiDelay) * factorHori_dur); // phoneme stop-marker
				e.Graphics.DrawLine(Pens.Blue,
									x, 16,
									x, pa_wave.Height - 16);
			}


// draw the track-caret
			int h_4 = pa_wave.Height / 4;
			int top = h_4 - 1;
			int bot = h_4 * 3 + 1;

			//logfile.Log(_waveout.GetPosition().ToString());
			// NOTE: Get position from '_waveout' NOT '_wavereader' because the
			// latter is very sluggish here - be aware that the streams are NOT
			// the same however.
			x = (int)(((decimal)_waveout.GetPosition() / 2 + _posStart) * factorHori);
			e.Graphics.DrawLine(Pens.White,
								x, top,
								x, bot);

// draw the start-caret as an I-bar
			x = (int)((decimal)_posStart * factorHori);
			e.Graphics.DrawLine(Pens.Wheat,
								x, top,
								x, bot);
			e.Graphics.DrawLine(Pens.Wheat,
								x - 3, top,
								x + 3, top);
			e.Graphics.DrawLine(Pens.Wheat,
								x - 3, bot,
								x + 3, bot);
		}


		/// <summary>
		/// Paints borders on the bot-panel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void paint_BotPanel(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawLine(Pens.DarkGray,						// bot-line
								1,            pa_bot.Height - 1,
								pa_bot.Width, pa_bot.Height - 1);
			e.Graphics.DrawLine(Pens.DarkGray,						// left-line
								0, 0,
								0, pa_bot.Height - 1);
			e.Graphics.DrawLine(Pens.DarkGray,						// right-line
								pa_bot.Width - 1, 0,
								pa_bot.Width - 1, pa_bot.Height - 1);
		}

		/// <summary>
		/// Redraws the bot-panel when its size changes because the first time
		/// the user resizes the form the right edge border-line doesn't get
		/// cleared. It does clear correctly without this after the first resize
		/// though.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sizechanged_BotPanel(object sender, EventArgs e)
		{
			pa_bot.Invalidate();
		}
		#endregion handlers paint
	}
}
