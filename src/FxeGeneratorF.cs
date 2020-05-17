﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

//#if DEBUG
//using System.Speech.Recognition;
//#endif


namespace lipsync_editor
{
	sealed partial class FxeGeneratorF
		: Form
	{
		#region fields (static)
		internal static FxeGeneratorF That;

		const string TITLE = "0x22 - FXE LipSyncer - ";

		internal const string EXT_FXE = "fxe";
				 const string EXT_TXT = "txt";

		const string HEAD_PHONS_0 = "pos";
		const string HEAD_PHONS_1 = "phoneme"; // headers for the phonemes table ->
		const string HEAD_PHONS_2 = "stop (secs)";
		const string HEAD_PHONS_3 = "viseme";
		const string HEAD_PHONS_4 = "truth";
		const string HEAD_PHONS_5 = "level";

		const string HEAD_BLOCKS_0 = "viseme"; // headers for the datablocks table ->
		const string HEAD_BLOCKS_1 = "frame stop";
		const string HEAD_BLOCKS_2 = "morph weight";

		internal static bool isConsole;
		#endregion fields (static)


		#region fields
		/// <summary>
		/// SAPI setup and usage object.
		/// </summary>
		readonly SapiLipsync _sapi;

		/// <summary>
		/// The fullpath of an audio-file to be analyzed.
		/// </summary>
		string _wavefile = String.Empty;

		/// <summary>
		/// The headtype of the model/skeleton that a generated FXE will be used
		/// by.
		/// @note Used by Console only; the GUI will get the string directly
		/// from the currently selected item of the dropdown Combobox.
		/// </summary>
		string _headtype = String.Empty;

		/// <summary>
		/// The list of AlignmentResults calculated by default SpeechRecognition
		/// (w/out typed-text).
		/// </summary>
		List<OrthographicResult> _ars_def;

		/// <summary>
		/// The list of AlignmentResults calculated by enhanced SpeechRecognition
		/// (w/ typed-text).
		/// </summary>
		List<OrthographicResult> _ars_enh;

		/// <summary>
		/// The user-selected data-set: def or enh. The data-set can be toggled
		/// with the 2 radio buttons.
		/// </summary>
			Dictionary<string, List<FxeDataBlock>> _fxedata =
		new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The data-set generated by default SpeechRecognition (w/out typed-text).
		/// @note "def" or "default" actually means a basic/raw analysis - an
		/// analysis of the audio(file) without any typed text.
		/// </summary>
		readonly Dictionary<string, List<FxeDataBlock>> _fxedata_def =
			 new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The data-set generated by enhanced SpeechRecognition (w/ typed-text).
		/// </summary>
		readonly Dictionary<string, List<FxeDataBlock>> _fxedata_enh =
			 new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The data to be shown in the "PHONEMES" datagrid.
		/// </summary>
		readonly DataTable _dt1 = new DataTable();

		/// <summary>
		/// The data to be shown in the "Data Blocks" datagrid.
		/// </summary>
		readonly DataTable _dt2 = new DataTable();
		#endregion fields


		#region cTor
		/// <summary>
		/// cTor for GUI interface.
		/// </summary>
		internal FxeGeneratorF()
			: this(String.Empty, String.Empty)
		{}

		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="wavefile"></param>
		/// <param name="headtype"></param>
		internal FxeGeneratorF(string wavefile, string headtype)
		{
#if DEBUG
//			LogSpeechRecognitionEngines();
			logfile.Log("FxeGeneratorF() cTor wavefile= " + wavefile + " headtype= " + headtype);
#endif

			That = this;

			FxeData.LoadTrigrams();

			if (wavefile == String.Empty) // is GUI interface ->
			{
#if DEBUG
				logfile.Log(". is GUI");
#endif
				InitializeComponent();

				co_headtype.SelectedIndex = 0;

				la_def_word_pct.Text =
				la_def_phon_pct.Text =
				la_enh_word_pct.Text =
				la_enh_phon_pct.Text = String.Empty;

				tb_text    .BackColor =
				tb_expected.BackColor = Color.AntiqueWhite;

				tb_def_words.BackColor =
				tb_def_phons.BackColor =
				tb_enh_words.BackColor =
				tb_enh_phons.BackColor = Color.GhostWhite;


				DataColumn col;
				col = new DataColumn(HEAD_PHONS_0, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_1, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_2, typeof(decimal));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_3, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_4, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_5, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				dg_phons.DataSource = _dt1;
				dg_phons.Columns[0].Width = 50; // 50
				dg_phons.Columns[1].Width = 76; // 76
				dg_phons.Columns[2].Width = 86; // 86
				dg_phons.Columns[3].Width = 67; // 67
				dg_phons.Columns[4].Width = 57; // 57
				dg_phons.Columns[5].Width = 61; // 56

				col = new DataColumn(HEAD_BLOCKS_0, typeof(string));
				col.ReadOnly = true;
				_dt2.Columns.Add(col);

				col = new DataColumn(HEAD_BLOCKS_1, typeof(float));
				col.ReadOnly = true;
				_dt2.Columns.Add(col);

				col = new DataColumn(HEAD_BLOCKS_2, typeof(float));
				col.ReadOnly = true;
				_dt2.Columns.Add(col);

				dg_blocks.DataSource = _dt2;
				dg_blocks.Columns[0].Width =  80; //  68
				dg_blocks.Columns[1].Width =  97; //  87
				dg_blocks.Columns[2].Width = 110; // 100

				dg_phons .RowHeadersVisible =
				dg_blocks.RowHeadersVisible = false;

				printversion();


				// initialize SAPI
				_sapi = new SapiLipsync();
				_sapi.TtsStreamEnded += OnTtsStreamEnded;
				_sapi.SrStreamEnded  += OnSrStreamEnded;

				// this will set '_sapi._recognizer'
				// this will set '_sapi._phoneConverter.LanguageId'
				// and the Titletext
				if (!SpeechRecognizerLister.AddSpeechRecognizers(co_recognizers))
				{
					string info = "FXE LipSyncer requires a SAPI 5.4 compliant"            + Environment.NewLine
								+ "Microsoft Speech Recognizer"                            + Environment.NewLine
								+ "as displayed in Windoz ControlPanel|SpeechRecognition." + Environment.NewLine
								+ Environment.NewLine
								+ "none was found ...";
					var d = new InfoDialog("FATAL Error", info);
					d.ShowDialog(this);

					Environment.Exit(0); // fatality.
				}
			}
			else if (headtype != String.Empty && File.Exists(wavefile)) // is CL interface ->
			{
#if DEBUG
				logfile.Log(". is Console");
#endif
				// TODO: Ensure that 'head Model/Skeleton type' is a recognized type.
				// Eg. "P_HHM"

				isConsole = true;

				_wavefile = wavefile;
				_headtype = headtype;

				_sapi = new SapiLipsync(_wavefile);
				if (_sapi.Audiopath != String.Empty)
				{
					_sapi.SrStreamEnded += OnSrStreamEnded;
					_sapi.Start(LoadTypedTextFile());
				}
			}
		}


//#if DEBUG
//		static void LogSpeechRecognitionEngines()
//		{
//			logfile.Log("--- SpeechRecognitionEngines LOCALLY INSTALLED ---");
//			logfile.Log("System.Speech.Recognition");
//
//			int i = -1;
//			var recs = SpeechRecognitionEngine.InstalledRecognizers();
//			foreach (var rec in recs)
//			{
//				logfile.Log();
//
//				logfile.Log((++i) + "= " + rec.Id + " : " + rec.Name);
//				logfile.Log("culture= " + rec.Culture);
//				logfile.Log("desc= "    + rec.Description);
//
//				foreach (var info in rec.AdditionalInfo)
//					logfile.Log(". info= " + info.Key + " : " + info.Value);
//
//				if (rec.SupportedAudioFormats.Count != 0)
//				{
//					logfile.Log("Supported audio formats");
//					int j = -1;
//					foreach (var format in rec.SupportedAudioFormats)
//					{
//						logfile.Log(". " + (++j));
//						logfile.Log(". format= "   + format.EncodingFormat);
//						logfile.Log(". freq= "     + format.SamplesPerSecond);
//						logfile.Log(". bits= "     + format.BitsPerSample);
//						logfile.Log(". bps= "      + format.AverageBytesPerSecond);
//						logfile.Log(". chans= "    + format.ChannelCount);
//						logfile.Log(". blockaln= " + format.BlockAlign);
//					}
//				}
//				else logfile.Log("No supported audio formats");
//			}
//			logfile.Log();
//			logfile.Log("--- SpeechRecognitionEngines END ---");
//			logfile.Log();
//			logfile.Log();
//		}
//#endif

		/// <summary>
		/// Prints the current version of this LipSyncer app.
		/// </summary>
		void printversion()
		{
			var ver = Assembly.GetExecutingAssembly().GetName().Version;
			string version = "version " + ver.Major
						   + "."        + ver.Minor
						   + "."        + ver.Build
						   + "."        + ver.Revision;

//			if (an.Version.Build != 0 || an.Version.Revision != 0)
//			{
//				ver += "." + an.Version.Build;
//
//				if (an.Version.Revision != 0)
//					ver += "." + an.Version.Revision;
//			}
#if DEBUG
			version += "-d";
#else
			version += "-r";
#endif
			la_version.Text = version;
		}
		#endregion cTor


		#region control handlers
		/// <summary>
		/// Handles changing the Recognizer combobox.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnRecognizerChanged(object sender, EventArgs e)
		{
			//logfile.Log("OnRecognizerChanged()");

			var recognizer = co_recognizers.SelectedItem as Recognizer;
//			_sapi.SetRecognizer(recognizer);

			tssl_token  .Text = recognizer.Tok.Id;
			tssl_langids.Text = recognizer.Langids;

			Text = TITLE + recognizer.Id;
		}

		/// <summary>
		/// Gets the current recognizer from the Recognizers combobox.
		/// @note The '_sapi._recognizer' needs to be re-created when the
		/// Generate button is clicked; if not then (roughly speaking) the
		/// handle to the wavefile on disk remains open, even though the call
		/// that closes it appears to be correctly implemented. In short, it's
		/// really effin' borked - just re-instantiate the recognizer ...
		/// 
		/// If you want to see what the bad behavior is like just remark the
		/// call in SapiLipsync.Start() and unremark the call in
		/// OnRecognizerChanged() and use the app a bit. Things quickly go south
		/// on 2+ generations.
		/// </summary>
		/// <returns></returns>
		internal Recognizer GetRecognizer()
		{
			return co_recognizers.SelectedItem as Recognizer;
		}

		/// <summary>
		/// Opens an audio-file w/out processing it.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Open(object sender, EventArgs e)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("click_Open()");
#endif
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title  = "Select a WAV or MP3 Audio file";
				ofd.Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|"
						   + "Wave files (*.wav)|*.wav|"
						   + "Mp3 files (*.mp3)|*.mp3|"
						   + "All files (*.*)|*.*";

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					tb_wavefile.Text = _wavefile = ofd.FileName;
#if DEBUG
					logfile.Log(". _wavefile= " + _wavefile);
#endif
					tb_text.Text = LoadTypedTextFile();

					tb_expected    .Text =
					tb_def_words   .Text =
					la_def_word_pct.Text =
					tb_def_phons   .Text =
					la_def_phon_pct.Text =
					tb_enh_words   .Text =
					la_enh_word_pct.Text =
					tb_enh_phons   .Text =
					la_enh_phon_pct.Text = String.Empty;

					co_headtype .Enabled =
					bu_createfxe.Enabled = false;

					rb_def.Checked =
					rb_enh.Checked =
					rb_def.Visible =
					rb_enh.Visible = false;


					_dt1.Rows.Clear();
					_dt2.Rows.Clear();

					_fxedata    .Clear();
					_fxedata_def.Clear();
					_fxedata_enh.Clear();

					if (FxeReader.ReadFile(_wavefile, _fxedata))
						PopulateDataGrid(_fxedata);

					_sapi.Audiopath = AudioConverter.deterAudiopath(_wavefile);
#if DEBUG
					logfile.Log(". _sapi.Audiopath= " + _sapi.Audiopath);
#endif
					bu_generate .Enabled =
					bu_play     .Enabled = (_sapi.Audiopath != String.Empty);
				}
			}
		}

		/// <summary>
		/// Loads a user-prepared file w/ typed-text.
		/// </summary>
		/// <returns></returns>
		string LoadTypedTextFile()
		{
			string file = _wavefile.Substring(0, _wavefile.Length - 3) + EXT_TXT;
			if (File.Exists(file))
			{
				using (StreamReader sr = File.OpenText(file))
				{
					return TypedText.SanitizeTypedText(sr.ReadToEnd());
				}
			}
			return String.Empty;
		}

		/// <summary>
		/// Generates data for an FXE file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Generate(object sender, EventArgs e)
		{
#if DEBUG
			logfile.Log();
			logfile.Log();
			logfile.Log("click_Generate() ===============================================================");
#endif
			Cursor = Cursors.WaitCursor;

			tb_text.Text = TypedText.SanitizeTypedText(tb_text.Text);

			tb_expected    .Text =
			tb_def_words   .Text =
			la_def_word_pct.Text =
			tb_def_phons   .Text =
			la_def_phon_pct.Text =
			tb_enh_words   .Text =
			la_enh_word_pct.Text =
			tb_enh_phons   .Text =
			la_enh_phon_pct.Text = String.Empty;

			rb_def.Checked =
			rb_enh.Checked =
			rb_def.Visible =
			rb_enh.Visible = false;


			_dt1.Rows.Clear();
			_dt2.Rows.Clear();

			_fxedata    .Clear();
			_fxedata_def.Clear();
			_fxedata_enh.Clear();

			_sapi.Start(tb_text.Text);
		}

		/// <summary>
		/// Writes an FXE file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_CreateFxe(object sender, EventArgs e)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("click_CreateFxe()");
#endif
			FxeWriter.WriteFile(_wavefile, co_headtype.Text, _fxedata);
		}

		/// <summary>
		/// Plays the audio-file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Play(object sender, EventArgs e)
		{
			tb_text.Text = TypedText.SanitizeTypedText(tb_text.Text);

			using (var wavefile = new FileStream(_sapi.Audiopath, FileMode.Open))
			using (var player   = new SoundPlayer(wavefile))
			{
				player.SoundLocation = _sapi.Audiopath;
				player.Play();
			}
		}

		/// <summary>
		/// Opens the synth-player.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Synth(object sender, EventArgs e)
		{
			tb_text.Text = TypedText.SanitizeTypedText(tb_text.Text);

			bu_synth.Enabled = false;

			var synth = new VoiceSynthF(this, tb_text.Text);
			synth.Show(this);
		}

		/// <summary>
		/// Re-enables the synth-button when the synth closes (as long as there
		/// is still typed-text).
		/// </summary>
		internal void EnableSynth()
		{
			bu_synth.Enabled = (tb_text.Text != String.Empty);
		}

		/// <summary>
		/// Sets the typed-text when Ok is clicked in the VoiceSynthesizer
		/// dialog.
		/// </summary>
		/// <param name="text"></param>
		internal void SetText(string text)
		{
			tb_text.Text = TypedText.SanitizeTypedText(text);
		}

		/// <summary>
		/// Determines if the synth-button should be enabled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textchanged_Text(object sender, EventArgs e)
		{
			bu_synth.Enabled = !String.IsNullOrEmpty(tb_text.Text);
		}


		/// <summary>
		/// Toggles the data in the datagrids between default and enhanced
		/// generations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void checkedchanged_Radio(object sender, EventArgs e)
		{
			var rb = sender as RadioButton;
			if (rb.Checked)
			{
				if (rb == rb_def)
				{
					_fxedata = _fxedata_def;

					PopulatePhonGrid(_ars_def);
					PopulateDataGrid(_fxedata_def);
				}
				else //if (rb == rb_enh)
				{
					_fxedata = _fxedata_enh;

					PopulatePhonGrid(_ars_enh);
					PopulateDataGrid(_fxedata_enh);
				}
			}
		}

		/// <summary>
		/// Toggles the data in the datagrids between default and enhanced
		/// generations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_pct(object sender, EventArgs e)
		{
			var la = sender as Label;
			if (la == la_def_phon_pct)
			{
				rb_def.Checked = true;
			}
			else //if (la == la_enh_phon_pct)
			{
				rb_enh.Checked = true;
			}
		}


		/// <summary>
		/// Draws rows in the PHONEMES datagrid with suitable background colors.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void dgphons_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			if (dg_phons.Rows[e.RowIndex].Cells[1].Value.ToString() == "x")
			{
				dg_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightSkyBlue;
			}
			else if (dg_phons.Rows[e.RowIndex].Cells[5].Value.ToString() == "Low")
			{
				dg_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightPink;
			}
			else if (dg_phons.Rows[e.RowIndex].Cells[5].Value.ToString() == "Normal")
			{
				dg_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
			}
			else if (dg_phons.Rows[e.RowIndex].Cells[5].Value.ToString() == "High")
			{
				dg_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Aquamarine;
			}

//			if (dg_phons.Rows[e.RowIndex].Cells[0].Value.ToString().EndsWith(".0", StringComparison.Ordinal))
//				dg_phons.Rows[e.RowIndex].Cells[0].Style.BackColor = Color.Azure;
		}
		#endregion control handlers


		#region lipsync handlers
		/// <summary>
		/// If there is typed-text its phonemes are displayed after the TTS
		/// (text-to-speech) stream finishes.
		/// </summary>
		void OnTtsStreamEnded()
		{
#if DEBUG
			logfile.Log("OnTtsStreamEnded()");
#endif
			string expected = String.Empty;
			foreach (var expect in _sapi.Expected)
			{
				if (expected != String.Empty) expected += " ";
				expected += expect;
			}
#if DEBUG
			logfile.Log(". expected= " + expected);
			logfile.Log();
#endif
			tb_expected.Text = expected;

			Cursor = Cursors.Default;
		}

		/// <summary>
		/// This is the biggie. Generates and prints data after the
		/// SpeechRecognition stream of an audio-file finishes.
		/// </summary>
		/// <param name="ars_def"></param>
		/// <param name="ars_enh"></param>
		void OnSrStreamEnded(List<OrthographicResult> ars_def, List<OrthographicResult> ars_enh)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("OnSrStreamEnded() ars_def.Count= " + ars_def.Count + " ars_enh.Count= " + ars_enh.Count);
#endif
			_ars_def = ars_def;
			_ars_enh = ars_enh;


			if (!isConsole)
			{
				PrintResults(_ars_def, tb_def_words, tb_def_phons);

				if (tb_text.Text != String.Empty)
				{
					PrintResults(_ars_enh, tb_enh_words, tb_enh_phons);

					la_def_word_pct.Text = _sapi.RatioWords_def.ToString("P1");
					la_enh_word_pct.Text = _sapi.RatioWords_enh.ToString("P1");
				}
				else
					la_def_word_pct.Text = _sapi.Confidence_def.ToString("P1");

				if (_sapi.Expected.Count != 0)
				{
					la_def_phon_pct.Text = _sapi.RatioPhons_def.ToString("P1");
					la_enh_phon_pct.Text = _sapi.RatioPhons_enh.ToString("P1");

					rb_def.Visible =
					rb_enh.Visible = true;
				}

				ColorPercents(tb_text.Text == String.Empty);

				bu_createfxe.Enabled =
				co_headtype .Enabled = true;
			}


			FxeData.GenerateData(_ars_def, _fxedata_def);
			FxeData.GenerateData(_ars_enh, _fxedata_enh);

#if DEBUG
			logfile.Log(". _sapi.RatioPhons_def= " + _sapi.RatioPhons_def);
			logfile.Log(". _sapi.RatioPhons_enh= " + _sapi.RatioPhons_enh);
#endif
			if (tb_text.Text == String.Empty
				|| _sapi.RatioPhons_def > _sapi.RatioPhons_enh)
			{
#if DEBUG
				logfile.Log(". select DEFault");
#endif
				_fxedata = _fxedata_def;

				if (!isConsole)
				{
#if DEBUG
					logfile.Log(". . rb_def");
#endif
					rb_def.Checked = true; // fire rb_CheckChanged
				}
			}
			else
			{
#if DEBUG
				logfile.Log(". select ENHanced");
#endif
				_fxedata = _fxedata_enh;

				if (!isConsole)
				{
#if DEBUG
					logfile.Log(". . rb_enh");
#endif
					rb_enh.Checked = true; // fire rb_CheckChanged
				}
			}


			if (isConsole)
			{
				FxeWriter.WriteFile(_wavefile, _headtype, _fxedata);
				Application.Exit();
			}
		}

		/// <summary>
		/// Prints the results.
		/// </summary>
		/// <param name="ars"></param>
		/// <param name="tb_words"></param>
		/// <param name="tb_phons"></param>
		void PrintResults(IList<OrthographicResult> ars, Control tb_words, Control tb_phons)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("PrintResults() ars.Count= " + ars.Count);
#endif
			string words = String.Empty;
			string phons = String.Empty;

			OrthographicResult ar;
			for (int i = 0; i != ars.Count; ++i)
			{
				if (!String.IsNullOrEmpty((ar = ars[i]).Orthography))
				{
					if (words != String.Empty) words += " ";
					words += ar.Orthography;

					for (int j = 0; j != ar.Phons.Count; ++j)
					{
						if (phons != String.Empty) phons += " ";
						phons += ar.Phons[j];
					}
				}
			}
#if DEBUG
			logfile.Log(". words= " + ((words.Length != 0) ? words : "NO WORDS"));
			logfile.Log(". phons= " + ((phons.Length != 0) ? phons : "NO PHONS"));
#endif
			tb_words.Text = words;
			tb_phons.Text = phons;
		}

		/// <summary>
		/// Colors the percents.
		/// </summary>
		/// <param name="confidence">true if default-text uses EngineConfidence</param>
		void ColorPercents(bool confidence)
		{
			if      (_sapi.RatioPhons_def < 0.65) la_def_phon_pct.ForeColor = Color.Crimson;
			else if (_sapi.RatioPhons_def > 0.80) la_def_phon_pct.ForeColor = Color.LimeGreen;
			else                                  la_def_phon_pct.ForeColor = SystemColors.ControlText;

			if      (_sapi.RatioPhons_enh < 0.65) la_enh_phon_pct.ForeColor = Color.Crimson;
			else if (_sapi.RatioPhons_enh > 0.80) la_enh_phon_pct.ForeColor = Color.LimeGreen;
			else                                  la_enh_phon_pct.ForeColor = SystemColors.ControlText;

			if (confidence)
			{
				if      (_sapi.Confidence_def < 0.65) la_def_word_pct.ForeColor = Color.Crimson;
				else if (_sapi.Confidence_def > 0.80) la_def_word_pct.ForeColor = Color.LimeGreen;
				else                                  la_def_word_pct.ForeColor = Color.SteelBlue;
			}
			else
				la_def_word_pct.ForeColor = SystemColors.ControlText;
		}

		/// <summary>
		/// Populates the PHONEMES datagrid.
		/// </summary>
		/// <param name="ars"></param>
		void PopulatePhonGrid(List<OrthographicResult> ars)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("PopulatePhonGrid() ars.Count= " + ars.Count);
#endif
			int col = -1, row = -1;
			if (dg_phons.SelectedCells.Count != 0)
			{
				col = dg_phons.SelectedCells[0].ColumnIndex;
				row = dg_phons.SelectedCells[0].RowIndex;
			}

			_dt1.Rows.Clear();

			int j = -1;
			string confidence, level;
			foreach (OrthographicResult ar in ars)
			{
				++j;
				confidence = ar.Confidence.ToString("F3");
				level = ar.Level;
#if DEBUG
				logfile.Log(". word= " + ar.Orthography);
#endif
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
//					decimal strt = (decimal)ar.GetStart(i) / 10000000;
//					decimal stop = (decimal)ar.Stops[i]    / 10000000;

					string phon = ar.Phons[i];

					string vis;
					if (StaticData.Vices.ContainsKey(phon)) // fudge ->
					{
						vis = StaticData.Vices[phon];
					}
					else
						vis = "INVALID";
#if DEBUG
					logfile.Log(". . " + phon + " -> "+ vis);
#endif

					_dt1.Rows.Add(new object[] { j + "." + i,
												 phon,
												 (decimal)ar.Stops[i] / 10000000,
												 vis,
												 confidence,
												 level });
					confidence = level = String.Empty;
				}
			}
//			dg_phons.Sort(dg_phons.Columns[1], ListSortDirection.Ascending);

			if (row != -1 && dg_phons.Rows.Count > row)
			{
				dg_phons.CurrentCell = dg_phons[col,row];
			}
			else
				dg_phons.ClearSelection();
		}

		/// <summary>
		/// Populates the Data Blocks datagrid.
		/// </summary>
		/// <param name="fxedata"></param>
		void PopulateDataGrid(Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("PopulateDataGrid() fxedata.Count= " + fxedata.Count);
#endif
			int col = -1, row = -1;
			if (dg_blocks.SelectedCells.Count != 0)
			{
				col = dg_blocks.SelectedCells[0].ColumnIndex;
				row = dg_blocks.SelectedCells[0].RowIndex;
			}

			var blocks = new List<FxeDataBlock>();
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in fxedata)
			{
				blocks.AddRange(pair.Value);
			}

			_dt2.Rows.Clear();

			int j = -1; // debug
			foreach (FxeDataBlock block in blocks)
			{
				_dt2.Rows.Add(new object[] { "[" + ++j + "] " + block.Viseme, block.Val1, block.Val2 });
			}
//			dg_blocks.Sort(dg_blocks.Columns[1], ListSortDirection.Ascending);

			if (row != -1 && dg_blocks.Rows.Count > row)
			{
				dg_blocks.CurrentCell = dg_blocks[col,row];
			}
			else
				dg_blocks.ClearSelection();
		}
		#endregion lipsync handlers
	}
}