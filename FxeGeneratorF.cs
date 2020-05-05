﻿using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed partial class FxeGeneratorF
		: Form
	{
		#region fields (static)
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
		readonly SapiLipsync _lipsyncer;

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
			logfile.Log("FxeGeneratorF() cTor wavefile= " + wavefile + " headtype= " + headtype);

			StaticData.FillPhon2VisMap();
			FxeData.LoadTrigrams();

			if (wavefile == String.Empty) // is GUI interface ->
			{
				logfile.Log(". is GUI");

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
				dg_blocks.Columns[0].Width =  70;
				dg_blocks.Columns[1].Width = 100;
				dg_blocks.Columns[2].Width = 107;

				dg_phons .RowHeadersVisible =
				dg_blocks.RowHeadersVisible = false;

				printversion();

				_lipsyncer = new SapiLipsync();
				_lipsyncer.TtsParseText += OnTtsParseText;
				_lipsyncer.SpeechRecognitionEnded += OnSpeechRecognitionEnded;

				LanguageLister.AddLanguages(co_langId); // this will set '_lipsyncer._phoneConverter.LanguageId'
			}
			else if (headtype != String.Empty && File.Exists(wavefile)) // is CL interface ->
			{
				logfile.Log(". is Console");

				// TODO: Ensure that 'head Model/Skeleton type' is a recognized type.
				// Eg. "P_HHM"

				isConsole = true;

				_wavefile = wavefile;
				_headtype = headtype;

				_lipsyncer = new SapiLipsync(_wavefile);
				if (_lipsyncer.Audiopath != String.Empty)
				{
					_lipsyncer.SpeechRecognitionEnded += OnSpeechRecognitionEnded;
					_lipsyncer.Start(LoadTypedTextFile());
				}
			}
		}


		void printversion()
		{
			var an = System.Reflection.Assembly.GetExecutingAssembly().GetName();
			string ver = "ver " + an.Version.Major
					   + "."    + an.Version.Minor;

			if (an.Version.Build != 0 || an.Version.Revision != 0)
			{
				ver += "." + an.Version.Build;

				if (an.Version.Revision != 0)
					ver += "." + an.Version.Revision;
			}
#if DEBUG
			ver += ".d";
#else
			ver += ".r";
#endif
			la_version.Text = ver;
		}
		#endregion cTor


		#region control handlers
		void OnLanguageChanged(object sender, EventArgs e)
		{
			var langid = co_langId.SelectedItem as LanguageId;
			_lipsyncer.SetLanguage(langid.Id);
		}


		void click_Open(object sender, EventArgs e)
		{
			logfile.Log();
			logfile.Log("click_Open()");

			// debug ->
//			tb_wavefile.Text = _wavefile = @"C:\GIT\FXE_Generator\bin\Debug\belueth_00.wav";
//			tb_wavefile.Text = _wavefile = @"C:\GIT\FXE_Generator\bin\Debug\ding.wav";

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
					logfile.Log(". _wavefile= " + _wavefile);

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

//					_ars_def.Clear();
//					_ars_enh.Clear();

					if (FxeReader.ReadFile(_wavefile, _fxedata))
						PopulateDataGrid(_fxedata);

					_lipsyncer.Audiopath = AudioConverter.deterAudiopath(_wavefile);
					logfile.Log(". _lipsyncer.Audiopath= " + _lipsyncer.Audiopath);

					bu_generate .Enabled =
					bu_play     .Enabled = (_lipsyncer.Audiopath != String.Empty);
//					bu_synth    .Enabled =
				}
			}
		}

		string LoadTypedTextFile()
		{
			string file = _wavefile.Substring(0, _wavefile.Length - 3) + EXT_TXT;
			if (File.Exists(file))
			{
				using (StreamReader sr = File.OpenText(file))
				{
					return TypedText.SanitizeDialogText(sr.ReadToEnd());
//					return TypedText.ParseText(sr.ReadToEnd());
				}
			}
			return String.Empty;
		}

		void click_Generate(object sender, EventArgs e)
		{
			logfile.Log();
			logfile.Log("click_Generate()");

			Cursor = Cursors.WaitCursor;

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

//			_ars_def.Clear();
//			_ars_enh.Clear();

			_lipsyncer.Start(tb_text.Text);
		}

		void click_CreateFxe(object sender, EventArgs e)
		{
			logfile.Log();
			logfile.Log("click_CreateFxe()");

			FxeWriter.WriteFile(_wavefile, co_headtype.Text, _fxedata);
		}

		void click_Play(object sender, EventArgs e)
		{
			using (var wavefile = new FileStream(_lipsyncer.Audiopath, FileMode.Open))
			using (var player   = new SoundPlayer(wavefile))
			{
				player.SoundLocation = _lipsyncer.Audiopath;
				player.Play();
			}
		}

		void click_Synth(object sender, EventArgs e)
		{
			var synth = new VoiceSynthF(this, tb_text.Text);
			synth.Show(this);
		}

		/// <summary>
		/// Sets the typed-text when Ok is clicked in the VoiceSynthesizer
		/// dialog.
		/// </summary>
		/// <param name="text"></param>
		internal void SetText(string text)
		{
			tb_text.Text = text;
		}

		void textchanged_Text(object sender, EventArgs e)
		{
			bu_synth.Enabled = !String.IsNullOrEmpty(tb_text.Text);
		}


		void checkedchanged_Radio(object sender, EventArgs e)
		{
			//logfile.Log("checkedchanged_Radio()");

			var rb = sender as RadioButton;
			if (rb.Checked)
			{
				if (rb == rb_def)
				{
					//logfile.Log(". rb_def");

					_fxedata = _fxedata_def;

					PopulatePhonGrid(_ars_def);
					PopulateDataGrid(_fxedata_def);
				}
				else //if (rb == rb_enh)
				{
					//logfile.Log(". rb_enh");

					_fxedata = _fxedata_enh;

					PopulatePhonGrid(_ars_enh);
					PopulateDataGrid(_fxedata_enh);
				}
			}
		}

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


		void dgphons_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
		{
			if (dg_phons.Rows[e.RowIndex].Cells[1].Value.ToString() == "x")
			{
				dg_phons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.PowderBlue;
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
		void OnTtsParseText()
		{
			logfile.Log("OnTtsParseText()");

			Cursor = Cursors.Default;

			string expected = String.Empty;
			foreach (var expect in _lipsyncer.Expected)
			{
				if (expected != String.Empty) expected += " ";
				expected += expect;
			}
			logfile.Log(". expected= " + expected);

			tb_expected.Text = expected;
		}

		void OnSpeechRecognitionEnded(List<OrthographicResult> ars_def, List<OrthographicResult> ars_enh)
		{
			logfile.Log();
			logfile.Log("OnSpeechRecognitionEnded() ars_def.Count= " + ars_def.Count + " ars_enh.Count= " + ars_enh.Count);

			_ars_def = ars_def;
			_ars_enh = ars_enh;


			if (!isConsole)
			{
				PrintResults(_ars_def, tb_def_words, tb_def_phons);

				if (tb_text.Text != String.Empty)
				{
					PrintResults(_ars_enh, tb_enh_words, tb_enh_phons);

					la_def_word_pct.Text = _lipsyncer.RatioWords_def.ToString("P1");
					la_enh_word_pct.Text = _lipsyncer.RatioWords_enh.ToString("P1");
				}

				if (_lipsyncer.Expected.Count != 0)
				{
					la_def_phon_pct.Text = _lipsyncer.RatioPhons_def.ToString("P1");
					la_enh_phon_pct.Text = _lipsyncer.RatioPhons_enh.ToString("P1");

					rb_def.Visible =
					rb_enh.Visible = true;
				}

				ColorPercents();

				bu_createfxe.Enabled =
				co_headtype .Enabled = true;
			}


			FxeData.GenerateData(_ars_def, _fxedata_def);
			FxeData.GenerateData(_ars_enh, _fxedata_enh);

			if (tb_text.Text == String.Empty
				|| _lipsyncer.RatioPhons_def > _lipsyncer.RatioPhons_enh)
			{
				_fxedata = _fxedata_def;

				if (!isConsole)
					rb_def.Checked = true; // fire rb_CheckChanged
			}
			else
			{
				_fxedata = _fxedata_enh;

				if (!isConsole)
					rb_enh.Checked = true; // fire rb_CheckChanged
			}


			if (isConsole)
			{
				FxeWriter.WriteFile(_wavefile, _headtype, _fxedata);
				Application.Exit();
			}
		}

		void PrintResults(IList<OrthographicResult> ars, Control tb_words, Control tb_phons)
		{
			logfile.Log();
			logfile.Log("PrintResults() ars.Count= " + ars.Count);

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

			logfile.Log(". words= " + ((words.Length != 0) ? words : "NO WORDS"));
			logfile.Log(". phons= " + ((phons.Length != 0) ? phons : "NO PHONS"));

			tb_words.Text = words;
			tb_phons.Text = phons;
		}

		void ColorPercents()
		{
//			if      (_lipsyncer.RatioWords_def < 0.65) la_def_word_pct.ForeColor = Color.Red;
//			else if (_lipsyncer.RatioWords_def > 0.80) la_def_word_pct.ForeColor = Color.LimeGreen;
//			else                                       la_def_word_pct.ForeColor = SystemColors.ControlText;

			if      (_lipsyncer.RatioPhons_def < 0.65) la_def_phon_pct.ForeColor = Color.Red;
			else if (_lipsyncer.RatioPhons_def > 0.80) la_def_phon_pct.ForeColor = Color.LimeGreen;
			else                                       la_def_phon_pct.ForeColor = SystemColors.ControlText;

//			if      (_lipsyncer.RatioWords_enh < 0.65) la_enh_word_pct.ForeColor = Color.Red;
//			else if (_lipsyncer.RatioWords_enh > 0.80) la_enh_word_pct.ForeColor = Color.LimeGreen;
//			else                                       la_enh_word_pct.ForeColor = SystemColors.ControlText;

			if      (_lipsyncer.RatioPhons_enh < 0.65) la_enh_phon_pct.ForeColor = Color.Red;
			else if (_lipsyncer.RatioPhons_enh > 0.80) la_enh_phon_pct.ForeColor = Color.LimeGreen;
			else                                       la_enh_phon_pct.ForeColor = SystemColors.ControlText;
		}

		void PopulatePhonGrid(List<OrthographicResult> ars)
		{
			logfile.Log();
			logfile.Log("PopulatePhonGrid() ars.Count= " + ars.Count);

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
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
//					decimal strt = (decimal)ar.GetStart(i) / 10000000;
//					decimal stop = (decimal)ar.Stops[i]    / 10000000;

					string phon = ar.Phons[i];

					_dt1.Rows.Add(new object[] { j + "." + i,
												 phon,
												 (decimal)ar.Stops[i] / 10000000,
												 StaticData.PhonToVis[phon],
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

		void PopulateDataGrid(Dictionary<string, List<FxeDataBlock>> fxedata)
		{
			logfile.Log();
			logfile.Log("PopulateDataGrid() fxedata.Count= " + fxedata.Count);

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
				++j;
//				_dt2.Rows.Add(new object[] { block.Viseme, block.Val1, block.Val2 });
				_dt2.Rows.Add(new object[] { "[" + j + "] " + block.Viseme, block.Val1, block.Val2 });
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
