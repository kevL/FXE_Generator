﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		#region structs
		/// <summary>
		/// For values of 'TriGramTable.dat'
		/// </summary>
		struct DataVal
		{
			internal float length;
			internal float val;
			internal short count;
		}
		#endregion structs


		#region fields (static)
		internal const string EXT_FXE = "fxe";
				 const string EXT_TXT = "txt";

		const string HEAD_PHONS_0 = "phoneme"; // headers for the phonemes table ->
		const string HEAD_PHONS_1 = "stop (secs)";
		const string HEAD_PHONS_2 = "viseme";

		const string HEAD_BLOCKS_0 = "viseme"; // headers for the datablocks table ->
		const string HEAD_BLOCKS_1 = "frame stop";
		const string HEAD_BLOCKS_2 = "morph weight";

		internal static bool isConsole;
		#endregion fields (static)


		#region fields
		SapiLipsync _lipsyncer;

		string _wavefile = String.Empty;
		string _headtype = String.Empty;

			Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>> TriGramTable =
		new Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>>();

		Dictionary<string, string> _phon2vis = new Dictionary<string, string>();

			Dictionary<string, List<FxeDataBlock>> _fxeData =
		new Dictionary<string, List<FxeDataBlock>>();

		DataTable _dt1 = new DataTable();
		DataTable _dt2 = new DataTable();
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

			StaticData.AddPhon2VisMap(_phon2vis);
			LoadTrigramTable();

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

				col = new DataColumn(HEAD_PHONS_1, typeof(decimal));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				col = new DataColumn(HEAD_PHONS_2, typeof(string));
				col.ReadOnly = true;
				_dt1.Columns.Add(col);

				dg_phons.DataSource = _dt1;
				dg_phons.Columns[0].Width =  80;
				dg_phons.Columns[1].Width = 115;
				dg_phons.Columns[2].Width =  82;

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

		void btnOpen_Click(object sender, EventArgs e)
		{
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
					_dt1.Clear();
					_dt2.Clear();

					tb_wavefile.Text = _wavefile = ofd.FileName;

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

					if (FxeReader.ReadFile(_wavefile, _fxeData))
						PopulateDataGrid();

					_lipsyncer.Audiopath = AudioConverter.deterAudiopath(_wavefile);
					logfile.Log("btnOpen_Click() _lipsyncer.Audiopath= " + _lipsyncer.Audiopath);

					bu_generate .Enabled =
					bu_play     .Enabled =
					bu_synth    .Enabled = (_lipsyncer.Audiopath != String.Empty);
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

		void btnGenerate_Click(object sender, EventArgs e)
		{
			logfile.Log("btnGenerate_Click()");

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

			_dt1.Clear();
			_dt2.Clear();

			_lipsyncer.Start(tb_text.Text);
		}

		void btnCreateFxe_Click(object sender, EventArgs e)
		{
			logfile.Log("btnCreateFxe_Click()");

			_headtype = co_headtype.Text;
			FxeWriter.WriteFile(_wavefile, _headtype, _fxeData);
		}

		void btnPlay_Click(object sender, EventArgs e)
		{
			using (var wavefile = new FileStream(_lipsyncer.Audiopath, FileMode.Open))
			using (var player   = new SoundPlayer(wavefile))
			{
				player.SoundLocation = _lipsyncer.Audiopath;
				player.Play();
			}
		}

		void btnSynth_Click(object sender, EventArgs e)
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
//			bu_generate.Enabled = true;
		}

		void OnSpeechRecognitionEnded(List<AlignmentResult> ars_def, List<AlignmentResult> ars_enh)
		{
			logfile.Log("OnRecognitionResult() ars_def.Count= " + ars_def.Count + " ars_enh.Count= " + ars_enh.Count);

			if (!isConsole)
			{
				PrintResults(ars_def, tb_def_words, tb_def_phons);

				if (tb_text.Text != String.Empty)
				{
					PrintResults(ars_enh, tb_enh_words, tb_enh_phons);

					la_def_word_pct.Text = _lipsyncer.RatioWords_def.ToString("P1");
					la_enh_word_pct.Text = _lipsyncer.RatioWords_enh.ToString("P1");
				}

				if (_lipsyncer.Expected.Count != 0)
				{
					la_def_phon_pct.Text = _lipsyncer.RatioPhons_def.ToString("P1");
					la_enh_phon_pct.Text = _lipsyncer.RatioPhons_enh.ToString("P1");
				}

				ColorPercents();

				bu_createfxe.Enabled =
				co_headtype .Enabled = true;
			}

			List<AlignmentResult> ars;
			if (_lipsyncer.RatioPhons_def > _lipsyncer.RatioPhons_enh)
				ars = ars_def;
			else
				ars = ars_enh;

			GenerateFxeData(ars);

			if (!isConsole)
			{
				PopulatePhonGrid(ars);
				PopulateDataGrid();
			}
			else
			{
				FxeWriter.WriteFile(_wavefile, _headtype, _fxeData);
				Application.Exit();
			}
		}

		void PrintResults(IList<AlignmentResult> ars, Control tb_words, Control tb_phons)
		{
			logfile.Log("PrintResults() ars.Count= " + ars.Count);

			string words = String.Empty;
			string phons = String.Empty;

			AlignmentResult ar;
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

//			tb_words.Text = SapiLipsync.ParseText(words); // TODO: Parse should be unnecessary.
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

		void PopulatePhonGrid(List<AlignmentResult> ars)
		{
			logfile.Log("PopulatePhonGrid()");

			_dt1.Clear();

			foreach (AlignmentResult ar in ars)
			{
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
//					decimal strt = (decimal)ar.GetStart(i) / 10000000;
					decimal stop = (decimal)ar.Stops[i]    / 10000000;

					string phon = ar.Phons[i];

					_dt1.Rows.Add(new object[] { phon, stop, _phon2vis[phon] });
				}
			}
			dg_phons.Sort(dg_phons.Columns[1], ListSortDirection.Ascending);
			dg_phons.ClearSelection();
		}

		void PopulateDataGrid()
		{
			logfile.Log("PopulateDataGrid()");

			var blocks = new List<FxeDataBlock>();
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in _fxeData)
			{
				blocks.AddRange(pair.Value);
			}

			_dt2.Clear();

			foreach (FxeDataBlock block in blocks)
			{
				_dt2.Rows.Add(new object[] { block.Viseme, block.Val1, block.Val2 });
			}
			dg_blocks.Sort(dg_blocks.Columns[1], ListSortDirection.Ascending);
			dg_blocks.ClearSelection();
		}
		#endregion lipsync handlers


		#region methods (load trigram table)
		void LoadTrigramTable()
		{
			InitTrigramTable();

			using (FileStream fs = File.OpenRead("TriGramTable.dat"))
			{
				var br = new BinaryReader(fs);
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					string[] codewords = br.ReadString().Split(',');

					var dataval = new DataVal();
					dataval.length = br.ReadSingle();
					dataval.val    = br.ReadSingle();
					dataval.count  = br.ReadInt16();

					TriGramTable[codewords[0]][codewords[1]][codewords[2]] = dataval;
				}
				br.Close();
			}
		}

		void InitTrigramTable()
		{
			List<string> codewords = StaticData.AddCodewords();
			foreach (string c2 in codewords)
			{
				var bigram = new Dictionary<string, Dictionary<string, DataVal>>();
				TriGramTable.Add(c2, bigram);

				foreach (string c1 in codewords)
				{
					if (c1 != "S" || c2 == "S")
					{
						var unigram = new Dictionary<string, DataVal>();
						bigram.Add(c1, unigram);

						foreach (string c0 in codewords)
						{
							if (c0 != "S")
							{
								unigram.Add(c0, new DataVal());
							}
						}
					}
				}
			}
		}
		#endregion methods (load trigram table)


		#region methods (generate FXE data)
		void GenerateFxeData(List<AlignmentResult> arList)
		{
			var vices = new List<KeyValuePair<string, decimal>>();

			string phon;
			foreach (AlignmentResult ar in arList)
			{
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
					if ((phon = ar.Phons[i]) != "x")
					{
						decimal stop = (decimal)ar.Stops[i] / 10000000;
						vices.Add(new KeyValuePair<string, decimal>(_phon2vis[phon], stop));
					}
				}
			}


			var blocks = new List<FxeDataBlock>(); // viseme start, mid, end points

			DataVal dataval;
			string c2 = "S";
			string c1 = "S";
			int id = 0;

			foreach (KeyValuePair<string, decimal> vis in vices)
			{
				string c0 = vis.Key;

				float stop = (float)vis.Value;

				dataval = GetTrigramValue(c2, c1, c0);
				float strt = stop - dataval.length;
				float midl = strt + dataval.length / 2f;

				blocks.Add(new FxeDataBlock(c0, strt,          0f, (byte)0, id));
				blocks.Add(new FxeDataBlock(c0, midl, dataval.val, (byte)1, id));
				blocks.Add(new FxeDataBlock(c0, stop,          0f, (byte)2, id));
				++id;

				c2 = c1;
				c1 = c0;
			}

			blocks.Sort();
			AddDatablocks(blocks);
			SmoothFxeData();
		}

		DataVal GetTrigramValue(string c2, string c1, string c0)
		{
			DataVal dataval = TriGramTable[c2][c1][c0];
			if (Math.Abs(dataval.length) < StaticData.EPSILON)
			{
				DataVal dataval0;
				foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, DataVal>>> pair in TriGramTable)
				{
					dataval0 = pair.Value[c1][c0];
					if (dataval0.count > dataval.count)
						dataval = dataval0;
				}
			}
			return dataval;
		}

		void AddDatablocks(IList<FxeDataBlock> datablocks)
		{
			StaticData.AddFxeCodewords(_fxeData);

			FxeDataBlock datablock0 = null;

			for (int i = 0; i != datablocks.Count; ++i)
			{
				FxeDataBlock datablock = datablocks[i];

				if (datablock0 != null)
				{
					if (Math.Abs(datablock.Val1 - datablock0.Val1) < StaticData.EPSILON)
					{
						// force the x-values (stop values) to never be equal
						if (i + 1 < datablocks.Count)
						{
							datablock.Val1 += Math.Min(StaticData.STOP_INCR,
													  (datablocks[i + 1].Val1 - datablock.Val2) / 2f);
						}
						else
							datablock.Val1 += StaticData.STOP_INCR;
					}
				}

				_fxeData[datablock.Viseme].Add(datablock);
				datablock0 = datablock;
			}
		}

		void SmoothFxeData()
		{
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in _fxeData)
			{
				if (pair.Value.Count > 0)
					VisemeSmoother.Smooth(pair.Key, pair.Value);
			}
		}
		#endregion methods (generate FXE data)
	}
}
