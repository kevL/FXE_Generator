﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		const int GRID_COL_PHON  = 1;
		const int GRID_COL_LEVEL = 6;

		const string HEAD_PHONS_0 = "pos";
		const string HEAD_PHONS_1 = "phoneme"; // headers for the PHONEMES table ->
		const string HEAD_PHONS_2 = "start";
		const string HEAD_PHONS_3 = "stop";
		const string HEAD_PHONS_4 = "viseme";
		const string HEAD_PHONS_5 = "truth";
		const string HEAD_PHONS_6 = "level";

		const string HEAD_BLOCKS_0 = "viseme"; // headers for the DataBlocks table ->
		const string HEAD_BLOCKS_1 = "frame stop";
		const string HEAD_BLOCKS_2 = "morph weight";

		internal static bool isConsole;

		internal static string Filelabel;
		#endregion fields (static)


		#region fields
		/// <summary>
		/// SAPI setup and usage object.
		/// </summary>
		internal readonly SapiLipsync _sapi;

		/// <summary>
		/// The fullpath of an audio-file to be analyzed.
		/// @note pfe = path_file_extension
		/// </summary>
		string _pfe = String.Empty;

		/// <summary>
		/// The headtype of the model/skeleton that a generated FXE will be used
		/// by.
		/// @note Used by Console only; the GUI will get the string directly
		/// from the currently selected item of the dropdown Combobox.
		/// </summary>
		string _headtype = String.Empty;


		/// <summary>
		/// The list of OrthographicResults calculated by default
		/// SpeechRecognition (w/out typed-text).
		/// </summary>
		List<OrthographicResult> _ars_def;

		/// <summary>
		/// The list of OrthographicResults calculated by enhanced
		/// SpeechRecognition (w/ typed-text).
		/// </summary>
		List<OrthographicResult> _ars_enh;

		/// <summary>
		/// The list of OrthographicResults defined by the editor.
		/// </summary>
		internal List<OrthographicResult> _ars_alt;

		/// <summary>
		/// The user-selected dataset: def or enh or alt. The dataset can be
		/// user-chosen per the 3 radio buttons.
		/// </summary>
			Dictionary<string, List<FxeDataBlock>> _fxedata =
		new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The dataset generated by default SpeechRecognition (w/out typed-text).
		/// @note "def" or "default" actually means a basic/raw analysis - an
		/// analysis of the audio(file) without any typed text. The engine uses
		/// its default Dictation lexicon to find speech-patterns.
		/// </summary>
		readonly Dictionary<string, List<FxeDataBlock>> _fxedata_def =
			 new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The dataset generated by enhanced SpeechRecognition (w/ typed-text).
		/// @note The engine uses a Rule based on typed-text to find speech-
		/// patterns.
		/// </summary>
		readonly Dictionary<string, List<FxeDataBlock>> _fxedata_enh =
			 new Dictionary<string, List<FxeDataBlock>>();

		/// <summary>
		/// The dataset generated by the editor.
		/// </summary>
		readonly Dictionary<string, List<FxeDataBlock>> _fxedata_alt =
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
		/// cTor.
		/// </summary>
		/// <param name="pfe">blank string if '!isConsole'</param>
		/// <param name="headtype">blank string if '!isConsole'</param>
		internal FxeGeneratorF(string pfe = "", string headtype = "")
		{
#if DEBUG
//			LogSpeechRecognitionEngines();
			logfile.Log("FxeGeneratorF() cTor pfe= " + pfe + " headtype= " + headtype);
#endif

			That = this;

			FxeData.LoadTrigrams();

			bool fatality = false;

			if (pfe == String.Empty) // is GUI interface ->
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

				tb_text.Text = String.Empty;

				tb_text    .BackColor =
				tb_expected.BackColor = Color.AntiqueWhite;

				tb_def_words.BackColor =
				tb_def_phons.BackColor =
				tb_enh_words.BackColor =
				tb_enh_phons.BackColor = Color.GhostWhite;


// PHONEMES data/grid ->
				DataColumn dc;
				dc = new DataColumn(HEAD_PHONS_0, typeof(string)); // pos
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_1, typeof(string)); // phon
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_2, typeof(string)); // start
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_3, typeof(string)); // stop
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_4, typeof(string)); // vis
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_5, typeof(string)); // truth
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				dc = new DataColumn(HEAD_PHONS_6, typeof(string)); // level
				dc.ReadOnly = true;
				_dt1.Columns.Add(dc);

				grid_phons.DataSource = _dt1;
				grid_phons.Columns[0].Width = 50; // 50
				grid_phons.Columns[1].Width = 76; // 76
				grid_phons.Columns[2].Width = 86; // 86
				grid_phons.Columns[3].Width = 86; // 86
				grid_phons.Columns[4].Width = 67; // 67
				grid_phons.Columns[5].Width = 57; // 57
				grid_phons.Columns[6].Width = 61; // 56

				for (int i = 0; i != grid_phons.Columns.Count; ++i)
					grid_phons.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;


// Data Blocks data/grid ->
				dc = new DataColumn(HEAD_BLOCKS_0, typeof(string));
				dc.ReadOnly = true;
				_dt2.Columns.Add(dc);

				dc = new DataColumn(HEAD_BLOCKS_1, typeof(float));
				dc.ReadOnly = true;
				_dt2.Columns.Add(dc);

				dc = new DataColumn(HEAD_BLOCKS_2, typeof(float));
				dc.ReadOnly = true;
				_dt2.Columns.Add(dc);

				grid_blocs.DataSource = _dt2;
				grid_blocs.Columns[0].Width =  80; //  68
				grid_blocs.Columns[1].Width =  97; //  87
				grid_blocs.Columns[2].Width = 110; // 100

//				for (int i = 0; i != grid_blocs.Columns.Count; ++i)
//					grid_blocs.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
				grid_blocs.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
				grid_blocs.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
				grid_blocs.ColumnHeaderMouseClick += dgblocs_ColumnHeaderMouseClick;

				grid_phons.RowHeadersVisible =
				grid_blocs.RowHeadersVisible = false;

				printversion();


				// instantiate/initialize SAPI
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
					using (var d = new InfoDialog("FATAL Error", info))
					{
						d.ShowDialog(this);
					}
					fatality = true;
				}
			}
			else if (headtype != String.Empty && File.Exists(pfe)) // is Console interface ->
			{
#if DEBUG
				logfile.Log(". is Console");
#endif
				// TODO: Fail if a Recognizer is not found.

				// TODO: Ensure that 'head Model/Skeleton type' is a recognized type.
				// Eg. "P_HHM"

				Filelabel = Utility.GetFilelabel(pfe); // NOTE: that will be written into the FXE-file output.

				_sapi = new SapiLipsync(_pfe = pfe);
				if (_sapi.Wavefile != String.Empty)
				{
					isConsole = true;
					_headtype = headtype;

					_sapi.SrStreamEnded += OnSrStreamEnded;
					_sapi.Start(LoadTypedTextFile());
				}
				else
					fatality = true;
			}
			else // is Console error ->
				fatality = true;

			if (fatality)
				Environment.Exit(0);
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
			Version ver = Assembly.GetExecutingAssembly().GetName().Version;
			string version = "version " + ver.Major
						   + "."        + ver.Minor
						   + "."        + ver.Build
						   + "."        + ver.Revision;

//			if (ver.Build != 0 || ver.Revision != 0)
//			{
//				version += "." + ver.Build;
//
//				if (ver.Revision != 0)
//					version += "." + ver.Revision;
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
		/// handle to the file on disk remains open, even though the call that
		/// closes it appears to be correctly implemented. In short, it's
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


		string _dirOpen = String.Empty;

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
				ofd.Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|" // TODO: handle BMU files
						   + "Wave files (*.wav)|*.wav|"
						   + "Mp3 files (*.mp3)|*.mp3|"
						   + "All files (*.*)|*.*";

				if (Directory.Exists(_dirOpen))
				{
					ofd.InitialDirectory = _dirOpen;
				}
				// else let .NET handle it.

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					_dirOpen = Path.GetDirectoryName(ofd.FileName);

					tb_wavefile.Text = _pfe = ofd.FileName;
#if DEBUG
					logfile.Log(". _pfe= " + _pfe);
#endif
					Clear();

					co_headtype .Enabled =
					bu_createfxe.Enabled = false;

					tb_text.Text = LoadTypedTextFile();

					Filelabel = Utility.GetFilelabel(_pfe); // NOTE: that will be written into the FXE-file output.

					string pfe = _pfe.Substring(0, _pfe.Length - 3) + FxeGeneratorF.EXT_FXE;
					if (FxeReader.ReadFile(pfe, _fxedata))
						PopulateDataGrid();

					_sapi.Wavefile = AudioConverter.deterwave(_pfe);
#if DEBUG
					logfile.Log(". _sapi.Wavefile= " + _sapi.Wavefile);
#endif
					bu_generate .Enabled =
					bu_play     .Enabled = (_sapi.Wavefile != String.Empty);
				}
			}
		}

		/// <summary>
		/// Loads a user-prepared file w/ typed-text.
		/// </summary>
		/// <returns></returns>
		string LoadTypedTextFile()
		{
			string file = _pfe.Substring(0, _pfe.Length - 3) + EXT_TXT;
			if (File.Exists(file))
			{
				using (var sr = new StreamReader(file))
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
			Cursor = Cursors.WaitCursor; // shall return to Default cursor either OnSrStreamEnded() or an InfoDialog error.

			tb_text.Text = TypedText.SanitizeTypedText(tb_text.Text);

			Clear();

			_sapi.Start(tb_text.Text);
		}

		/// <summary>
		/// Clears the UI and data-caches.
		/// </summary>
		void Clear()
		{
			tb_expected    .Text =
			tb_def_words   .Text =
			la_def_word_pct.Text =
			tb_def_phons   .Text =
			la_def_phon_pct.Text =
			tb_enh_words   .Text =
			la_enh_word_pct.Text =
			tb_enh_phons   .Text =
			la_enh_phon_pct.Text = String.Empty;

			rb_def.Checked = rb_def.Visible =
			rb_enh.Checked = rb_enh.Visible =
			rb_alt.Checked = rb_alt.Visible = false;

			bu_edit.Enabled = false;

			_dt1.Rows.Clear();
			_dt2.Rows.Clear();

			_fxedata    .Clear();
			_fxedata_def.Clear();
			_fxedata_enh.Clear();
			_fxedata_alt.Clear();
		}


		string _dirCreate = String.Empty;

		/// <summary>
		/// Writes an FXE file to a user-chosen filepath.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_CreateFxe(object sender, EventArgs e)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("click_CreateFxe()");
#endif
			using (var sfd = new SaveFileDialog())
			{
//				sfd.Title = "Save as ...";
				sfd.Filter = "FXE files (*.fxe)|*.fxe|All files (*.*)|*.*";

				if (Directory.Exists(_dirCreate))
				{
					sfd.InitialDirectory = _dirCreate;
				}
				else
				{
					string dir = Path.GetDirectoryName(_pfe);
					if (Directory.Exists(dir))
						sfd.InitialDirectory = dir;
				}
				// else let .NET handle it.

				sfd.FileName = Utility.GetFilelabel(_pfe) + "." + EXT_FXE;

				if (sfd.ShowDialog(this) == DialogResult.OK)
				{
					_dirCreate = Path.GetDirectoryName(sfd.FileName);
					FxeWriter.WriteFile(sfd.FileName, co_headtype.Text, _fxedata);
				}
			}
		}

		/// <summary>
		/// Plays the audio-file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Play(object sender, EventArgs e)
		{
			tb_text.Text = TypedText.SanitizeTypedText(tb_text.Text);

			using (var fs = new FileStream(_sapi.Wavefile, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var player = new SoundPlayer(fs))
			{
				player.SoundLocation = _sapi.Wavefile;
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
		/// Determines if the synth-button should be enabled.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void textchanged_Text(object sender, EventArgs e)
		{
			EnableSynth();
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
		/// Switches the data in the datagrids between default and enhanced and
		/// alternate generations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void checkedchanged_Radio(object sender, EventArgs e)
		{
			var rb = sender as RadioButton;
			if (rb.Checked)
			{
				if (rb == rb_def)			// default dictation
				{
					PopulatePhonGrid(_ars_def);
					_fxedata = _fxedata_def;
					PopulateDataGrid();
				}
				else if (rb == rb_enh)		// enhanced w/ typed-text rule
				{
					PopulatePhonGrid(_ars_enh);
					_fxedata = _fxedata_enh;
					PopulateDataGrid();
				}
				else //if (rb == rb_alt)	// user-defined alternate OrthographicResult data
				{
					PopulatePhonGrid(_ars_alt);
					_fxedata = _fxedata_alt;
					PopulateDataGrid();
				}
			}
		}

		/// <summary>
		/// Fires <see cref="checkedchanged_Radio">checkedchanged_Radio()</see>
		/// when a radio-button's label is clicked.
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
			if (grid_phons.Rows[e.RowIndex].Cells[GRID_COL_PHON].Value.ToString() == StaticData.SIL)
			{
				grid_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightSkyBlue;
			}
			else if (grid_phons.Rows[e.RowIndex].Cells[GRID_COL_LEVEL].Value.ToString() == "Low")
			{
				grid_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightPink;
			}
			else if (grid_phons.Rows[e.RowIndex].Cells[GRID_COL_LEVEL].Value.ToString() == "Normal")
			{
				grid_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
			}
			else if (grid_phons.Rows[e.RowIndex].Cells[GRID_COL_LEVEL].Value.ToString() == "High")
			{
				grid_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Aquamarine;
			}
		}


		/// <summary>
		/// Handler for the KeyDown event of the typed-text textbox. [Enter]
		/// invokes generation of fxe-data.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void keydown_TypedText(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Enter:
					e.Handled = e.SuppressKeyPress = true;
					click_Generate(null, EventArgs.Empty);
					break;

				case Keys.Enter | Keys.Shift:
				case Keys.Enter | Keys.Control:
				case Keys.Enter | Keys.Alt:
					e.Handled = e.SuppressKeyPress = true;
					break;
			}
		}


		/// <summary>
		/// Handler for the Click event of the Edit button. Opens the editor.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void click_Edit(object sender, EventArgs e)
		{
			using (var editor = new EditorPhonF(this, _dt1))
			{
				editor.ShowDialog(this);
			}
		}


		/// <summary>
		/// Handler for the KeyDown event on the datagrids. [Tab] and
		/// [Shift+Tab] cycles to the next control and [Enter] does nada.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void keydown_Datagrid(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Tab:
					SelectNextControl(sender as Control, true, true, false, true);
					goto case Keys.Enter;

				case Keys.Tab | Keys.Shift:
					SelectNextControl(sender as Control, false, true, false, true);
					goto case Keys.Enter;

				case Keys.Enter:
					e.Handled = e.SuppressKeyPress = true;
					break;
			}
		}

		/// <summary>
		/// Clears sort-order of the DataBlocks grid when its viseme-header is
		/// clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void dgblocs_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex == 0) _dt2.DefaultView.Sort = String.Empty;
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
				string pfe = _pfe.Substring(0, _pfe.Length - 3).ToLower() + FxeGeneratorF.EXT_FXE;
				FxeWriter.WriteFile(pfe, _headtype, _fxedata);
				Application.Exit();
			}
			else
			{
				bu_edit.Enabled = (_dt1 != null && _dt1.Rows.Count != 0);
				Cursor = Cursors.Default;
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
			string or;

			for (int i = 0; i != ars.Count; ++i)
			{
				ar = ars[i];
				or = ar.Orthography;

				if (or != String.Empty) // safety (i think).
				{
					if (words != String.Empty) words += " ";
					words += or;

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
		/// Populates the PHONEMES datagrid via checkedchanged_Radio().
		/// </summary>
		/// <param name="ars"></param>
		void PopulatePhonGrid(List<OrthographicResult> ars)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("PopulatePhonGrid() ars.Count= " + ars.Count);
#endif
//			int col = -1, row = -1;
//			if (grid_phons.CurrentCell != null)
//			{
//				col = grid_phons.CurrentCell.ColumnIndex;
//				row = grid_phons.CurrentCell.RowIndex;
//			}

			_dt1.Rows.Clear();

			int j = -1;
			string start, truth, level, phon, vis;
			foreach (OrthographicResult ar in ars)
			{
				++j;

				start = ar.Start.ToString("F3");
				truth = ar.Confi.ToString("F3");
				level = ar.Level;
#if DEBUG
				logfile.Log(". word= " + ar.Orthography);
#endif
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
					phon = ar.Phons[i];

					if (phon == StaticData.SIL)
					{
						vis = truth = String.Empty;
					}
					else if (StaticData.Vices.ContainsKey(phon))
					{
						vis = StaticData.Vices[phon];
					}
					else
					{
						vis = "INVALID";
						truth = String.Empty;
					}
#if DEBUG
					logfile.Log(". . " + phon + " -> "+ vis);
#endif
					_dt1.Rows.Add(new object[] { j + "." + i, // pos
												 phon,
												 start,
												 ar.phStops[i].ToString("F3"),
												 vis,
												 truth,
												 level });
					if (i == 0)
					{
						start = truth = level = String.Empty;
					}
				}
			}
//			grid_phons.Sort(dg_phons.Columns[1], ListSortDirection.Ascending);

//			if (row != -1 && grid_phons.Rows.Count > row)
//			{
//				grid_phons.CurrentCell = grid_phons[col,row];
//			}
//			else
//				grid_phons.ClearSelection();
		}

		/// <summary>
		/// Populates the Data Blocks datagrid via checkedchanged_Radio().
		/// </summary>
		void PopulateDataGrid()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("PopulateDataGrid() _fxedata.Count= " + _fxedata.Count);
#endif
//			int col = -1, row = -1;
//			if (grid_blocs.SelectedCells.Count != 0)
//			{
//				col = grid_blocs.SelectedCells[0].ColumnIndex;
//				row = grid_blocs.SelectedCells[0].RowIndex;
//			}

			var datablocks = new List<FxeDataBlock>();
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in _fxedata)
			{
				datablocks.AddRange(pair.Value);
			}

			_dt2.Rows.Clear();


			int digits = datablocks.Count.ToString().Length;

			FxeDataBlock block;
			string j;
			for (int i = 0; i != datablocks.Count; ++i)
			{
				j = "[" + i.ToString().PadLeft(digits, ' ') + "] ";
				block = datablocks[i];
				_dt2.Rows.Add(new object[] { j + block.Label,
												 block.Val1,
												 block.Val2 });
			}
			grid_blocs.Sort(grid_blocs.Columns[1], ListSortDirection.Ascending); // sort by vis-stops

//			if (row != -1 && grid_blocs.Rows.Count > row)
//			{
//				grid_blocs.CurrentCell = grid_blocs[col,row];
//			}
//			else
//				grid_blocs.ClearSelection();
		}
		#endregion lipsync handlers


		/// <summary>
		/// Switches stuff use to alternate/user-edited data. <see cref="EditorPhonF"/>
		/// @note The data in the "PHONEMES" table changes as data is edited in
		/// 'EditorPhonF' so if that is canceled the table needs to revert.
		/// </summary>
		/// <param name="revert">true to revert to previous data</param>
		internal void AlternateData(bool revert = false)
		{
			if (!revert)
			{
				_fxedata_alt.Clear();
	
				FxeData.GenerateData(_ars_alt, _fxedata_alt);
	
				rb_alt.Checked =
				rb_alt.Visible = true; // fire rb_CheckChanged
	
				rb_def.Visible = true; // else default-button won't be visible if there's no typed-text.
			}
			else
			{
				object rb;
				if      (rb_def.Checked) rb = rb_def;
				else if (rb_enh.Checked) rb = rb_enh;
				else                     rb = rb_alt;

				checkedchanged_Radio(rb, EventArgs.Empty);
			}
		}
	}
}
