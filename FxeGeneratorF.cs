using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;


namespace lipsync_editor
{
	public partial class FxeGeneratorF
		: Form
	{
		#region structs
		/// <summary>
		/// For values of 'TriGramTable.dat'
		/// </summary>
		struct DataVal
		{
			public float length;
			public float val;
			public short count;
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
			Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>> TriGramTable =
		new Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>>();

		string _wavefile = String.Empty;
		string _headtype = String.Empty;

		SapiLipsync _lipsyncer;

		Dictionary<string, string> _phon2vis = new Dictionary<string, string>();

		DataTable _dt1 = new DataTable();
		DataTable _dt2 = new DataTable();

			Dictionary<string, List<FxeDataBlock>> _fxeData =
		new Dictionary<string, List<FxeDataBlock>>();
		#endregion fields


		#region cTor
		internal FxeGeneratorF()
			: this(String.Empty, String.Empty)
		{}

		internal FxeGeneratorF(string wavefile, string headtype)
		{
			LoadVisMap();
			LoadTriGramTable();

			if (wavefile == String.Empty)
			{
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
			}
			else if (headtype != String.Empty && File.Exists(wavefile))
			{
				// TODO: Ensure that 'head Model/Skeleton type' is a recognized type.
				// Eg. "P_HHM"

				isConsole = true;

				_wavefile = wavefile;
				_headtype = headtype;

				_lipsyncer = new SapiLipsync(_wavefile);
				if (_lipsyncer.Fullpath != String.Empty)
				{
					_lipsyncer.Recognition  += OnRecognition;
//					_lipsyncer.TtsParseText += OnTtsParseText;

					_lipsyncer.Start(LoadTypedTextFile());
				}
			}
		}


		void printversion()
		{
			var an = System.Reflection.Assembly.GetExecutingAssembly().GetName();
			string ver = an.Version.Major + "."
					   + an.Version.Minor + "."
					   + an.Version.Build + "."
					   + an.Version.Revision;
#if DEBUG
			ver += "  deb";
#else
			ver += "  rel";
#endif
			la_version.Text = ver;
		}
		#endregion cTor


		#region control handlers
		void btnOpen_Click(object sender, EventArgs e)
		{
			// debug ->
			tb_wavefile.Text = _wavefile = @"C:\GIT\FXE_Generator\bin\Debug\belueth_00.wav";
//			tb_wavefile.Text = _wavefile = @"C:\GIT\FXE_Generator\bin\Debug\ding.wav";

//			using (var ofd = new OpenFileDialog())
//			{
//				ofd.Title  = "Select a WAV or MP3 Audio file";
//				ofd.Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|"
//						   + "Wave files (*.wav)|*.wav|"
//						   + "Mp3 files (*.mp3)|*.mp3|"
//						   + "All files (*.*)|*.*";
//
//				if (ofd.ShowDialog() == DialogResult.OK)
//				{

					_dt1.Clear();
					_dt2.Clear();

//					tb_wavefile.Text = _wavefile = ofd.FileName;

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

					LoadFxeFile();

					bool enabled = false;

					_lipsyncer = new SapiLipsync(_wavefile);
					if (_lipsyncer.Fullpath != String.Empty)
					{
						enabled = true;

						_lipsyncer.Recognition  += OnRecognition;
						_lipsyncer.TtsParseText += OnTtsParseText;
					}

					bu_generate .Enabled =
					bu_play     .Enabled = enabled;
//				}
//			}
		}

		public string LoadTypedTextFile()
		{
			string file = _wavefile.Substring(0, _wavefile.Length - 3) + EXT_TXT;
			if (File.Exists(file))
			{
				using (StreamReader sr = File.OpenText(file))
				{
					return sr.ReadToEnd();
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
			_headtype = co_headtype.Text;
			FxeWriter.WriteFxeFile(_wavefile, _headtype, _fxeData);
		}

		void btnPlay_Click(object sender, EventArgs e)
		{
			using (var wavefile = new FileStream(_lipsyncer.Fullpath, FileMode.Open))
			{
				using (var player = new SoundPlayer(wavefile))
				{
					player.SoundLocation = _lipsyncer.Fullpath;
					player.Play();
				}
			}
		}
		#endregion control handlers


		#region lipsync handlers
		void OnTtsParseText(string expected)
		{
			logfile.Log("OnTtsParseText() expected= " + expected);

			Cursor = Cursors.Default;

			tb_expected.Text = expected;
			bu_generate.Enabled = true;
		}

		void OnRecognition(List<AlignmentResult> ars_def, List<AlignmentResult> ars_enh)
		{
			logfile.Log("OnRecognitionResult() ars_def.Count= " + ars_def.Count + " ars_enh.Count= " + ars_enh.Count);

			if (!isConsole)
			{
				PrintTextResults(ars_def, tb_def_words, tb_def_phons);
				PrintTextResults(ars_enh, tb_enh_words, tb_enh_phons);

				if (tb_text.Text != String.Empty)
				{
					la_def_word_pct.Text = _lipsyncer.RatioWords_def.ToString("P0");
					la_enh_word_pct.Text = _lipsyncer.RatioWords_enh.ToString("P0");
				}

				if (_lipsyncer._tts_Phons.Count != 0)
				{
					la_def_phon_pct.Text = _lipsyncer.RatioPhons_def.ToString("P0");
					la_enh_phon_pct.Text = _lipsyncer.RatioPhons_enh.ToString("P0");
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
				FxeWriter.WriteFxeFile(_wavefile, _headtype, _fxeData);
				Application.Exit();
			}
		}

		void PrintTextResults(IList<AlignmentResult> ars, Control tb_words, Control tb_phons)
		{
			logfile.Log("GetResults() ars.Count= " + ars.Count);

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

			logfile.Log(". words= " + words);
			logfile.Log(". phons= " + phons);

			tb_words.Text = SapiLipsync.ParseText(words); // TODO: Parse should be unnecessary.
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
			_dt1.Clear();

			foreach (AlignmentResult ar in ars)
			{
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
//					ulong strt = ar.GetStart(i);
					ulong stop = ar.Stops[i];

//					decimal dstrt = (decimal)strt / (decimal)10000000;
					decimal dstop = (decimal)stop / (decimal)10000000;

					string phon = ar.Phons[i];

					_dt1.Rows.Add(new object[] { phon, dstop, _phon2vis[phon] });
				}
			}
			dg_phons.Sort(dg_phons.Columns[1], ListSortDirection.Ascending);
			dg_phons.ClearSelection();
		}

		void PopulateDataGrid()
		{
			var blocks = new List<FxeDataBlock>();
			foreach (KeyValuePair<string, List<FxeDataBlock>> keyval in _fxeData)
			{
				blocks.AddRange(keyval.Value);
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


		#region methods
		void LoadVisMap()
		{
			_phon2vis.Add( "x", String.Empty);
			_phon2vis.Add("iy", "Eat");
			_phon2vis.Add("ih", "If");
			_phon2vis.Add("eh", "If");
			_phon2vis.Add("ey", "If");
			_phon2vis.Add("ae", "If");
			_phon2vis.Add("aa", "Ox");
			_phon2vis.Add("aw", "If");
			_phon2vis.Add("ay", "If");
			_phon2vis.Add("ah", "If");
			_phon2vis.Add("ao", "Ox");
			_phon2vis.Add("oy", "Oat");
			_phon2vis.Add("ow", "Oat");
			_phon2vis.Add("uh", "Oat");
			_phon2vis.Add("uw", "Oat");
			_phon2vis.Add("er", "Earth");
			_phon2vis.Add("ax", "If");
			_phon2vis.Add( "s", "Size");
			_phon2vis.Add("sh", "Church");
			_phon2vis.Add( "z", "Size");
			_phon2vis.Add("zh", "Church");
			_phon2vis.Add( "f", "Fave");
			_phon2vis.Add("th", "Though");
			_phon2vis.Add( "v", "Fave");
			_phon2vis.Add("dh", "Though");
			_phon2vis.Add( "m", "Bump");
			_phon2vis.Add( "n", "New");
			_phon2vis.Add("ng", "New");
			_phon2vis.Add( "l", "Told");
			_phon2vis.Add( "r", "Roar");
			_phon2vis.Add( "w", "Wet");
			_phon2vis.Add( "y", "Wet");
			_phon2vis.Add( "h", "If");
			_phon2vis.Add( "b", "Bump");
			_phon2vis.Add( "d", "Told");
			_phon2vis.Add("jh", "Church");
			_phon2vis.Add( "g", "Cage");
			_phon2vis.Add( "p", "Bump");
			_phon2vis.Add( "t", "Told");
			_phon2vis.Add( "k", "Cage");
			_phon2vis.Add("ch", "Church");
		}

		void LoadTriGramTable()
		{
			InitTriGramTable();

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

		void InitTriGramTable()
		{
			List<string> codewords = CreateCodewordList();
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

		List<string> CreateCodewordList()
		{
			var codewords = new List<string>();

			codewords.Add("S");
			codewords.Add("Eat");
			codewords.Add("If");
			codewords.Add("Ox");
			codewords.Add("Oat");
			codewords.Add("Earth");
			codewords.Add("Size");
			codewords.Add("Church");
			codewords.Add("Though");
			codewords.Add("Bump");
			codewords.Add("New");
			codewords.Add("Told");
			codewords.Add("Roar");
			codewords.Add("Cage");
			codewords.Add("Wet");
			codewords.Add("Fave");

			return codewords;
		}


		void LoadFxeFile()
		{
			logfile.Log("LoadFxeFile()");

			string file = _wavefile.Substring(0, _wavefile.Length - 3) + EXT_FXE;
			if (File.Exists(file))
			{
				LoadFxeCodewords();

				using (FileStream fs = File.Open(file, FileMode.Open))
				{
					var br = new BinaryReader(fs);

					fs.Seek(85, SeekOrigin.Begin);
					string headtype = ReadFxeString(br);
					logfile.Log(". headtype= " + headtype);

					fs.Seek(34, SeekOrigin.Current);
					string wavefile = ReadFxeString(br);
					logfile.Log(". wavefile= " + wavefile);

					fs.Seek(8, SeekOrigin.Current);
					short blockcount = br.ReadInt16();
					//logfile.Log(". blockcount= " + blockcount);

					fs.Seek(8, SeekOrigin.Current);

					for (short i = 0; i != (short)15; ++i)
					{
						//logfile.Log(". . i= " + i);

						string codeword = ReadFxeString(br);
						//logfile.Log(". . codeword= " + codeword);

						fs.Seek(8, SeekOrigin.Current);							// 8 bytes of zeroes
						short datablockcount = br.ReadInt16();
						//logfile.Log(". . datablockcount= " + datablockcount);

						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes

						for (short j = 0; j != datablockcount; ++j)
						{
							//logfile.Log(". . j= " + j);

							float val1 = br.ReadSingle();
							float val2 = br.ReadSingle();
							//logfile.Log(". . val1= " + val1);
							//logfile.Log(". . val2= " + val2);

							fs.Seek(10, SeekOrigin.Current);					// 10 bytes of zeroes

							var block = new FxeDataBlock(codeword, val1, val2, 0, 0);
							_fxeData[codeword].Add(block);
						}
						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes
					}
					br.Close();
				}
				PopulateDataGrid();
			}
		}

		void LoadFxeCodewords()
		{
			_fxeData.Clear();

			_fxeData.Add("Eat",                    new List<FxeDataBlock>());
			_fxeData.Add("If",                     new List<FxeDataBlock>());
			_fxeData.Add("Ox",                     new List<FxeDataBlock>());
			_fxeData.Add("Oat",                    new List<FxeDataBlock>());
			_fxeData.Add("Earth",                  new List<FxeDataBlock>());
			_fxeData.Add("Size",                   new List<FxeDataBlock>());
			_fxeData.Add("Church",                 new List<FxeDataBlock>());
			_fxeData.Add("Fave",                   new List<FxeDataBlock>());
			_fxeData.Add("Though",                 new List<FxeDataBlock>());
			_fxeData.Add("Bump",                   new List<FxeDataBlock>());
			_fxeData.Add("New",                    new List<FxeDataBlock>());
			_fxeData.Add("Told",                   new List<FxeDataBlock>());
			_fxeData.Add("Roar",                   new List<FxeDataBlock>());
			_fxeData.Add("Wet",                    new List<FxeDataBlock>());
			_fxeData.Add("Cage",                   new List<FxeDataBlock>());
			_fxeData.Add("Orientation Head Pitch", new List<FxeDataBlock>());
			_fxeData.Add("Orientation Head Roll",  new List<FxeDataBlock>());
			_fxeData.Add("Orientation Head Yaw",   new List<FxeDataBlock>());
			_fxeData.Add("Gaze Eye Pitch",         new List<FxeDataBlock>());
			_fxeData.Add("Gaze Eye Yaw",           new List<FxeDataBlock>());
			_fxeData.Add("Emphasis Head Pitch",    new List<FxeDataBlock>());
			_fxeData.Add("Emphasis Head Roll",     new List<FxeDataBlock>());
			_fxeData.Add("Emphasis Head Yaw",      new List<FxeDataBlock>());
			_fxeData.Add("Eyebrow Raise",          new List<FxeDataBlock>());
			_fxeData.Add("Blink",                  new List<FxeDataBlock>());
		}

		string ReadFxeString(BinaryReader br)
		{
			br.ReadInt16();
			int len = br.ReadInt32();
			return new String(br.ReadChars(len));
		}

		void GenerateFxeData(List<AlignmentResult> arList)
		{
			var vices = new List<KeyValuePair<string, decimal>>();

			string phon;
			foreach (AlignmentResult ar in arList)
			{
				for (int i = 0; i < ar.Phons.Count; ++i)
				{
					if ((phon = ar.Phons[i]) != "x")
					{
						decimal stop = (decimal)ar.Stops[i] / (decimal)10000000;

						var keyval = new KeyValuePair<string, decimal>(_phon2vis[phon], stop);
						vices.Add(keyval);
					}
				}
			}


			var blocks = new List<FxeDataBlock>(); // viseme start, mid, end points

			string c2 = "S";
			string c1 = "S";
			int id = 0;

			foreach (KeyValuePair<string, decimal> vis in vices)
			{
				string c0 = vis.Key;

				float stop = (float)vis.Value;

				DataVal dataval = GetTrigramValues(c2, c1, c0);
				float strt = stop - dataval.length;
				float midl = strt + dataval.length / 2F;

				blocks.Add(new FxeDataBlock(c0, strt,          0F, (byte)0, id));
				blocks.Add(new FxeDataBlock(c0, midl, dataval.val, (byte)1, id));
				blocks.Add(new FxeDataBlock(c0, stop,          0F, (byte)2, id));

				++id;
				c2 = c1;
				c1 = c0;
			}

			blocks.Sort();
			AssignDatablocks(blocks);
			SmoothFxeData();
		}

		DataVal GetTrigramValues(string c2, string c1, string c0)
		{
			DataVal dataval = TriGramTable[c2][c1][c0];
			if (Math.Abs(dataval.length) < 0.000005)
			{
				c2 = String.Empty;

				int count = 0;
				foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, DataVal>>> keyval in TriGramTable)
				{
					DataVal dataval0 = keyval.Value[c1][c0];
					if (dataval0.count > count)
					{
						count = dataval0.count;
						c2 = keyval.Key;
						dataval = dataval0;
					}
				}
			}
			return dataval;
		}

		void AssignDatablocks(IList<FxeDataBlock> datablocks)
		{
			LoadFxeCodewords();

			FxeDataBlock datablock0 = null;

			for (int i = 0; i != datablocks.Count; ++i)
			{
				FxeDataBlock datablock = datablocks[i];

				if (datablock0 != null)
				{
					if (Math.Abs(datablock.Val1 - datablock0.Val1) < 0.000005F)
					{
						// force the x values (time values) to not ever be equal
						if (i + 1 < datablocks.Count)
						{
							datablock.Val1 += Math.Min(0.0000001F, (datablocks[i + 1].Val1 - datablock.Val2) / 2F);
						}
						else
							datablock.Val1 += 0.0000001F;
					}
				}

				_fxeData[datablock.Viseme].Add(datablock);
				datablock0 = datablock;
			}
		}

		void SmoothFxeData()
		{
			foreach (KeyValuePair<string, List<FxeDataBlock>> keyval in _fxeData)
			{
				if (keyval.Value.Count > 0)
					SmoothVis(keyval.Key, keyval.Value);
			}
		}


		static void SmoothVis(string vis, List<FxeDataBlock> datablocks)
		{
			var linesById = new Dictionary<int, List<KeyValuePair<float, float>>>();

			int lastId = datablocks[0].Id;
			float Ax   = datablocks[0].Val1;
			float Ay   = datablocks[0].Val2;

			for (int i = 1; i < datablocks.Count - 1; ++i)
			{
				FxeDataBlock datablock = datablocks[i];
				if (lastId != datablock.Id)
				{
					int Cindex = i;

					int Aindex = i - 1;

					float Bx = 0F;
					float By = 0F;

					int Bindex = GetNextPoint(datablocks, Aindex, ref Bx, ref By);
					if (Bindex == -1)
					{
						lastId = datablock.Id;
						Ax     = datablock.Val1;
						Ay     = datablock.Val2;

						continue;
					}

					float Cx = datablock.Val1;
					float Cy = datablock.Val2;

					float Dx = 0F;
					float Dy = 0F;

					int Dindex = GetNextPoint(datablocks, Cindex, ref Dx, ref Dy);
					if (Dindex == -1)
					{
						if (Ay > 0F) // remove c
						{
							datablocks.RemoveAt(i);
							--i;

							continue;
						}
					}

					float x = 0F;
					float y = 0F;

					if (GetLineIntersection(Ax,Ay, Bx,By, Cx,Cy, Dx,Dy, ref x, ref y))
					{
						InsertIntersectionPoint(datablocks,
												new FxeDataBlock(vis, x, y, 1, lastId),
												i + 1);

						if (floatsequal(Ay, 0F) && Cy > y) // remove a
						{
							datablocks.RemoveAt(i - 1);
							--i;

							lastId = datablock.Id;
							Ax     = Cx;
							Ay     = Cy;

							continue;
						}

						if (Cy < y) // remove c
						{
							datablocks.RemoveAt(i);
							--i;
						}
						else
						{
							lastId = datablock.Id;
							Ax     = Cx;
							Ay     = Cy;
						}
					}
					else // lines do not cross
					{
						if (Ay < Cy && Bx < Dx) // remove a
						{
							datablocks.RemoveAt(i - 1);
							--i;

							lastId = datablock.Id;
							Ax     = Cx;
							Ay     = Cy;

							continue;
						}

						if ((Dx < Bx && Dy < By) // kL_note: not sure about that refactor.
							|| floatsequal(Dy, 0F)
							|| floatsequal(By, 0F)) // remove c
						{
							datablocks.RemoveAt(i);
							--i;
						}
						else
						{
							lastId = datablock.Id;
							Ax     = Cx;
							Ay     = Cy;
						}
					}
				}
				else // IDs equal
				{
					lastId = datablock.Id;
					Ax     = datablock.Val1;
					Ay     = datablock.Val2;
				}
			}
		}

		static int GetNextPoint(IList<FxeDataBlock> datablocks, int startId, ref float x, ref float y)
		{
			x = -1;
			y = -1;

			if (datablocks[startId].Type != 2)
			{
				int id = datablocks[startId].Id;

				++startId;
				while (startId < datablocks.Count)
				{
					FxeDataBlock datablock = datablocks[startId];
					if (datablock.Id == id)
					{
						x = datablock.Val1;
						y = datablock.Val2;

						return startId;
					}
					++startId;
				}
			}
			return -1;
		}

		public static bool GetLineIntersection(float Ax, float Ay,
											   float Bx, float By,
											   float Cx, float Cy,
											   float Dx, float Dy,
											   ref float x, ref float y)
		{
			// Fail if either line segment is zero-length.
			if (   (floatsequal(Ax, Bx) && floatsequal(Ay, By))
				|| (floatsequal(Cx, Dx) && floatsequal(Cy, Dy)))
			{
				return false;
			}

			// Fail if the segments share an end-point.
			if (   (floatsequal(Ax, Cx) && floatsequal(Ay, Cy))
				|| (floatsequal(Bx, Cx) && floatsequal(By, Cy))
				|| (floatsequal(Ax, Dx) && floatsequal(Ay, Dy))
				|| (floatsequal(Bx, Dx) && floatsequal(By, Dy)))
			{
				return false;
			}

			// (1) Translate the system so that point A is on the origin.
			Bx -= Ax;
			By -= Ay;
			Cx -= Ax;
			Cy -= Ay;
			Dx -= Ax;
			Dy -= Ay;

			// Discover the length of segment A-B.
			double AB_dist = Math.Sqrt(Bx * Bx + By * By);

			// (2) Rotate the system so that point B is on the positive x-axis.
			double cos = Bx / AB_dist;
			double sin = By / AB_dist;

			double x1 =  Cx * cos + Cy * sin;
			Cy = (float)(Cy * cos - Cx * sin);
			Cx = (float)x1;

			x1 =         Dx * cos + Dy * sin;
			Dy = (float)(Dy * cos - Dx * sin);
			Dx = (float)x1;

			// Fail if segment C-D doesn't cross line A-B.
			if (   (Cy <  0F && Dy <  0F)
				|| (Cy >= 0F && Dy >= 0F))
			{
				return false;
			}

			// (3) Discover the position of the intersection point along line A-B.
			double AB_pos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

			// Fail if segment C-D crosses line A-B outside of segment A-B.
			if (AB_pos < 0F || AB_pos > AB_dist)
			{
				return false;
			}

			// (4) Apply the discovered position to line A-B in the original coordinate system.
			x = (float)(Ax + AB_pos * cos);
			y = (float)(Ay + AB_pos * sin);

			// Success.
			return true;
		}

		static void InsertIntersectionPoint(IList<FxeDataBlock> datablocks, FxeDataBlock datablock, int id)
		{
			while (datablock.Val1 > datablocks[id].Val1 && id < datablocks.Count)
				++id;

			if (id < datablocks.Count)
				datablocks.Insert(id, datablock);
			else
				datablocks.Add(datablock);
		}
		#endregion methods


		static bool floatsequal(float f1, float f2)
		{
			return Math.Abs(f2 - f1) < 0.000005F;
		}
	}
}
