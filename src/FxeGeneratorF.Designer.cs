using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed partial class FxeGeneratorF
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		IContainer components = null;

		/// <summary>
		/// Cleans up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();

			base.Dispose(disposing);
		}


		ComboBox co_recognizers;
		Label la_version;

		Button bu_open;
		Button bu_generate;
		Button bu_synth;
		Button bu_createfxe;
		Button bu_edit;

		TextBox tb_wavefile;
		Button bu_play;

		Label la_headtype;
		ComboBox co_headtype;

		Label la_text;
		TextBox tb_text;
		Label la_expected;
		TextBox tb_expected;

		TextBox tb_def_words;
		Label la_def_words;
		Label la_def_phons;
		TextBox tb_def_phons;

		Label la_enh_words;
		TextBox tb_enh_words;
		Label la_enh_phons;
		TextBox tb_enh_phons;

		Label la_def_word_pct;
		Label la_def_phon_pct;
		Label la_enh_word_pct;
		Label la_enh_phon_pct;

		Label la_phons;
		DataGridView grid_phons;
		Label la_blocks;
		DataGridView grid_blocs;

		RadioButton rb_def;
		RadioButton rb_enh;

		StatusStrip ss_bot;
		ToolStripStatusLabel tssl_token;
		ToolStripStatusLabel tssl_langids_;
		ToolStripStatusLabel tssl_langids;


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify the contents of
		/// this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tb_def_words = new System.Windows.Forms.TextBox();
			this.la_def_words = new System.Windows.Forms.Label();
			this.tb_wavefile = new System.Windows.Forms.TextBox();
			this.bu_open = new System.Windows.Forms.Button();
			this.bu_generate = new System.Windows.Forms.Button();
			this.tb_def_phons = new System.Windows.Forms.TextBox();
			this.la_def_phons = new System.Windows.Forms.Label();
			this.tb_text = new System.Windows.Forms.TextBox();
			this.grid_phons = new System.Windows.Forms.DataGridView();
			this.la_text = new System.Windows.Forms.Label();
			this.la_expected = new System.Windows.Forms.Label();
			this.tb_expected = new System.Windows.Forms.TextBox();
			this.la_enh_phons = new System.Windows.Forms.Label();
			this.tb_enh_phons = new System.Windows.Forms.TextBox();
			this.la_enh_words = new System.Windows.Forms.Label();
			this.tb_enh_words = new System.Windows.Forms.TextBox();
			this.la_def_word_pct = new System.Windows.Forms.Label();
			this.la_def_phon_pct = new System.Windows.Forms.Label();
			this.la_enh_word_pct = new System.Windows.Forms.Label();
			this.la_enh_phon_pct = new System.Windows.Forms.Label();
			this.la_phons = new System.Windows.Forms.Label();
			this.grid_blocs = new System.Windows.Forms.DataGridView();
			this.bu_createfxe = new System.Windows.Forms.Button();
			this.co_headtype = new System.Windows.Forms.ComboBox();
			this.la_headtype = new System.Windows.Forms.Label();
			this.bu_play = new System.Windows.Forms.Button();
			this.la_blocks = new System.Windows.Forms.Label();
			this.la_version = new System.Windows.Forms.Label();
			this.co_recognizers = new System.Windows.Forms.ComboBox();
			this.bu_synth = new System.Windows.Forms.Button();
			this.rb_def = new System.Windows.Forms.RadioButton();
			this.rb_enh = new System.Windows.Forms.RadioButton();
			this.ss_bot = new System.Windows.Forms.StatusStrip();
			this.tssl_token = new System.Windows.Forms.ToolStripStatusLabel();
			this.tssl_langids_ = new System.Windows.Forms.ToolStripStatusLabel();
			this.tssl_langids = new System.Windows.Forms.ToolStripStatusLabel();
			this.bu_edit = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.grid_phons)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid_blocs)).BeginInit();
			this.ss_bot.SuspendLayout();
			this.SuspendLayout();
			// 
			// tb_def_words
			// 
			this.tb_def_words.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_def_words.Location = new System.Drawing.Point(5, 225);
			this.tb_def_words.Margin = new System.Windows.Forms.Padding(0);
			this.tb_def_words.Multiline = true;
			this.tb_def_words.Name = "tb_def_words";
			this.tb_def_words.ReadOnly = true;
			this.tb_def_words.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_def_words.Size = new System.Drawing.Size(460, 40);
			this.tb_def_words.TabIndex = 15;
			// 
			// la_def_words
			// 
			this.la_def_words.Location = new System.Drawing.Point(5, 208);
			this.la_def_words.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_words.Name = "la_def_words";
			this.la_def_words.Size = new System.Drawing.Size(300, 15);
			this.la_def_words.TabIndex = 13;
			this.la_def_words.Text = "SpeechRecognition - orthemes";
			this.la_def_words.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_wavefile
			// 
			this.tb_wavefile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_wavefile.Location = new System.Drawing.Point(60, 7);
			this.tb_wavefile.Margin = new System.Windows.Forms.Padding(0);
			this.tb_wavefile.Name = "tb_wavefile";
			this.tb_wavefile.ReadOnly = true;
			this.tb_wavefile.Size = new System.Drawing.Size(665, 22);
			this.tb_wavefile.TabIndex = 1;
			this.tb_wavefile.WordWrap = false;
			// 
			// bu_open
			// 
			this.bu_open.Location = new System.Drawing.Point(5, 5);
			this.bu_open.Margin = new System.Windows.Forms.Padding(0);
			this.bu_open.Name = "bu_open";
			this.bu_open.Size = new System.Drawing.Size(51, 26);
			this.bu_open.TabIndex = 0;
			this.bu_open.Text = "open";
			this.bu_open.UseVisualStyleBackColor = true;
			this.bu_open.Click += new System.EventHandler(this.click_Open);
			// 
			// bu_generate
			// 
			this.bu_generate.Enabled = false;
			this.bu_generate.Location = new System.Drawing.Point(115, 34);
			this.bu_generate.Margin = new System.Windows.Forms.Padding(0);
			this.bu_generate.Name = "bu_generate";
			this.bu_generate.Size = new System.Drawing.Size(66, 25);
			this.bu_generate.TabIndex = 5;
			this.bu_generate.Text = "Generate";
			this.bu_generate.UseVisualStyleBackColor = true;
			this.bu_generate.Click += new System.EventHandler(this.click_Generate);
			// 
			// tb_def_phons
			// 
			this.tb_def_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_def_phons.Location = new System.Drawing.Point(5, 285);
			this.tb_def_phons.Margin = new System.Windows.Forms.Padding(0);
			this.tb_def_phons.Multiline = true;
			this.tb_def_phons.Name = "tb_def_phons";
			this.tb_def_phons.ReadOnly = true;
			this.tb_def_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_def_phons.Size = new System.Drawing.Size(460, 55);
			this.tb_def_phons.TabIndex = 18;
			// 
			// la_def_phons
			// 
			this.la_def_phons.Location = new System.Drawing.Point(5, 268);
			this.la_def_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_phons.Name = "la_def_phons";
			this.la_def_phons.Size = new System.Drawing.Size(300, 15);
			this.la_def_phons.TabIndex = 16;
			this.la_def_phons.Text = "SpeechRecognition - phonemes";
			this.la_def_phons.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_text
			// 
			this.tb_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_text.Location = new System.Drawing.Point(5, 83);
			this.tb_text.Margin = new System.Windows.Forms.Padding(0);
			this.tb_text.Multiline = true;
			this.tb_text.Name = "tb_text";
			this.tb_text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_text.Size = new System.Drawing.Size(460, 40);
			this.tb_text.TabIndex = 10;
			this.tb_text.TextChanged += new System.EventHandler(this.textchanged_Text);
			this.tb_text.KeyDown += new System.Windows.Forms.KeyEventHandler(this.keydown_TypedText);
			// 
			// grid_phons
			// 
			this.grid_phons.AllowUserToAddRows = false;
			this.grid_phons.AllowUserToDeleteRows = false;
			this.grid_phons.AllowUserToResizeColumns = false;
			this.grid_phons.AllowUserToResizeRows = false;
			this.grid_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.grid_phons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.grid_phons.Location = new System.Drawing.Point(470, 60);
			this.grid_phons.Margin = new System.Windows.Forms.Padding(0);
			this.grid_phons.Name = "grid_phons";
			this.grid_phons.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8F);
			this.grid_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.grid_phons.Size = new System.Drawing.Size(486, 420);
			this.grid_phons.TabIndex = 26;
			this.grid_phons.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dgphons_RowPrePaint);
			// 
			// la_text
			// 
			this.la_text.Location = new System.Drawing.Point(5, 66);
			this.la_text.Margin = new System.Windows.Forms.Padding(0);
			this.la_text.Name = "la_text";
			this.la_text.Size = new System.Drawing.Size(300, 15);
			this.la_text.TabIndex = 9;
			this.la_text.Text = "TextToSpeech - typed text";
			this.la_text.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// la_expected
			// 
			this.la_expected.Location = new System.Drawing.Point(5, 126);
			this.la_expected.Margin = new System.Windows.Forms.Padding(0);
			this.la_expected.Name = "la_expected";
			this.la_expected.Size = new System.Drawing.Size(300, 15);
			this.la_expected.TabIndex = 11;
			this.la_expected.Text = "TextToSpeech - phonemes expected";
			this.la_expected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_expected
			// 
			this.tb_expected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_expected.Location = new System.Drawing.Point(5, 143);
			this.tb_expected.Margin = new System.Windows.Forms.Padding(0);
			this.tb_expected.Multiline = true;
			this.tb_expected.Name = "tb_expected";
			this.tb_expected.ReadOnly = true;
			this.tb_expected.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_expected.Size = new System.Drawing.Size(460, 55);
			this.tb_expected.TabIndex = 12;
			// 
			// la_enh_phons
			// 
			this.la_enh_phons.Location = new System.Drawing.Point(5, 407);
			this.la_enh_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_phons.Name = "la_enh_phons";
			this.la_enh_phons.Size = new System.Drawing.Size(300, 15);
			this.la_enh_phons.TabIndex = 22;
			this.la_enh_phons.Text = "SpeechRecognition - phonemes enhanced w/ TTS";
			this.la_enh_phons.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_enh_phons
			// 
			this.tb_enh_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_enh_phons.Location = new System.Drawing.Point(5, 424);
			this.tb_enh_phons.Margin = new System.Windows.Forms.Padding(0);
			this.tb_enh_phons.Multiline = true;
			this.tb_enh_phons.Name = "tb_enh_phons";
			this.tb_enh_phons.ReadOnly = true;
			this.tb_enh_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_enh_phons.Size = new System.Drawing.Size(460, 55);
			this.tb_enh_phons.TabIndex = 24;
			// 
			// la_enh_words
			// 
			this.la_enh_words.Location = new System.Drawing.Point(5, 347);
			this.la_enh_words.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_words.Name = "la_enh_words";
			this.la_enh_words.Size = new System.Drawing.Size(300, 15);
			this.la_enh_words.TabIndex = 19;
			this.la_enh_words.Text = "SpeechRecognition - orthemes enhanced w/ TTS";
			this.la_enh_words.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_enh_words
			// 
			this.tb_enh_words.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_enh_words.Location = new System.Drawing.Point(5, 364);
			this.tb_enh_words.Margin = new System.Windows.Forms.Padding(0);
			this.tb_enh_words.Multiline = true;
			this.tb_enh_words.Name = "tb_enh_words";
			this.tb_enh_words.ReadOnly = true;
			this.tb_enh_words.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_enh_words.Size = new System.Drawing.Size(460, 40);
			this.tb_enh_words.TabIndex = 21;
			// 
			// la_def_word_pct
			// 
			this.la_def_word_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_def_word_pct.Location = new System.Drawing.Point(415, 208);
			this.la_def_word_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_word_pct.Name = "la_def_word_pct";
			this.la_def_word_pct.Size = new System.Drawing.Size(50, 15);
			this.la_def_word_pct.TabIndex = 14;
			this.la_def_word_pct.Text = "pct";
			this.la_def_word_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_def_phon_pct
			// 
			this.la_def_phon_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_def_phon_pct.Location = new System.Drawing.Point(415, 268);
			this.la_def_phon_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_phon_pct.Name = "la_def_phon_pct";
			this.la_def_phon_pct.Size = new System.Drawing.Size(50, 15);
			this.la_def_phon_pct.TabIndex = 17;
			this.la_def_phon_pct.Text = "pct";
			this.la_def_phon_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.la_def_phon_pct.Click += new System.EventHandler(this.click_pct);
			// 
			// la_enh_word_pct
			// 
			this.la_enh_word_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_enh_word_pct.Location = new System.Drawing.Point(415, 347);
			this.la_enh_word_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_word_pct.Name = "la_enh_word_pct";
			this.la_enh_word_pct.Size = new System.Drawing.Size(50, 15);
			this.la_enh_word_pct.TabIndex = 20;
			this.la_enh_word_pct.Text = "pct";
			this.la_enh_word_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_enh_phon_pct
			// 
			this.la_enh_phon_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_enh_phon_pct.Location = new System.Drawing.Point(415, 407);
			this.la_enh_phon_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_phon_pct.Name = "la_enh_phon_pct";
			this.la_enh_phon_pct.Size = new System.Drawing.Size(50, 15);
			this.la_enh_phon_pct.TabIndex = 23;
			this.la_enh_phon_pct.Text = "pct";
			this.la_enh_phon_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.la_enh_phon_pct.Click += new System.EventHandler(this.click_pct);
			// 
			// la_phons
			// 
			this.la_phons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_phons.Location = new System.Drawing.Point(570, 40);
			this.la_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_phons.Name = "la_phons";
			this.la_phons.Size = new System.Drawing.Size(286, 15);
			this.la_phons.TabIndex = 25;
			this.la_phons.Text = "PHONEMES";
			this.la_phons.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// grid_blocs
			// 
			this.grid_blocs.AllowUserToAddRows = false;
			this.grid_blocs.AllowUserToDeleteRows = false;
			this.grid_blocs.AllowUserToResizeColumns = false;
			this.grid_blocs.AllowUserToResizeRows = false;
			this.grid_blocs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.grid_blocs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.grid_blocs.Location = new System.Drawing.Point(962, 60);
			this.grid_blocs.Margin = new System.Windows.Forms.Padding(0);
			this.grid_blocs.Name = "grid_blocs";
			this.grid_blocs.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 8F);
			this.grid_blocs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.grid_blocs.Size = new System.Drawing.Size(290, 420);
			this.grid_blocs.TabIndex = 28;
			// 
			// bu_createfxe
			// 
			this.bu_createfxe.Enabled = false;
			this.bu_createfxe.Location = new System.Drawing.Point(379, 34);
			this.bu_createfxe.Margin = new System.Windows.Forms.Padding(0);
			this.bu_createfxe.Name = "bu_createfxe";
			this.bu_createfxe.Size = new System.Drawing.Size(86, 25);
			this.bu_createfxe.TabIndex = 8;
			this.bu_createfxe.Text = "Create FXE";
			this.bu_createfxe.UseVisualStyleBackColor = true;
			this.bu_createfxe.Click += new System.EventHandler(this.click_CreateFxe);
			// 
			// co_headtype
			// 
			this.co_headtype.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.co_headtype.Enabled = false;
			this.co_headtype.FormattingEnabled = true;
			this.co_headtype.Items.AddRange(new object[] {
			"P_HHM",
			"P_HHF",
			"P_DDM",
			"P_DDF",
			"P_EEF",
			"P_GGM",
			"P_GGF",
			"P_HTF",
			"P_OOM",
			"P_OOF",
			"c_bugbear",
			"c_devil",
			"c_dogleg",
			"c_dragon",
			"c_ghoul",
			"c_giantfire",
			"c_giantfrost",
			"c_hag",
			"c_heavy",
			"c_hezrou",
			"c_imp",
			"c_ogre",
			"c_orc",
			"c_small",
			"c_succubus",
			"c_troll",
			"c_uthraki",
			"c_werewolf",
			"c_wingleg",
			"c_zombie",
			"n_okku",
			"N_SReaver"});
			this.co_headtype.Location = new System.Drawing.Point(269, 35);
			this.co_headtype.Margin = new System.Windows.Forms.Padding(0);
			this.co_headtype.Name = "co_headtype";
			this.co_headtype.Size = new System.Drawing.Size(106, 23);
			this.co_headtype.TabIndex = 7;
			// 
			// la_headtype
			// 
			this.la_headtype.Location = new System.Drawing.Point(195, 39);
			this.la_headtype.Margin = new System.Windows.Forms.Padding(0);
			this.la_headtype.Name = "la_headtype";
			this.la_headtype.Size = new System.Drawing.Size(70, 15);
			this.la_headtype.TabIndex = 6;
			this.la_headtype.Text = "Head Type";
			this.la_headtype.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// bu_play
			// 
			this.bu_play.Enabled = false;
			this.bu_play.Location = new System.Drawing.Point(5, 34);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(51, 25);
			this.bu_play.TabIndex = 3;
			this.bu_play.Text = "play";
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.click_Play);
			// 
			// la_blocks
			// 
			this.la_blocks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_blocks.Location = new System.Drawing.Point(962, 40);
			this.la_blocks.Margin = new System.Windows.Forms.Padding(0);
			this.la_blocks.Name = "la_blocks";
			this.la_blocks.Size = new System.Drawing.Size(290, 15);
			this.la_blocks.TabIndex = 27;
			this.la_blocks.Text = "Data Blocks";
			this.la_blocks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_version
			// 
			this.la_version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_version.Location = new System.Drawing.Point(361, 66);
			this.la_version.Margin = new System.Windows.Forms.Padding(0);
			this.la_version.Name = "la_version";
			this.la_version.Size = new System.Drawing.Size(104, 15);
			this.la_version.TabIndex = 29;
			this.la_version.Text = "version";
			this.la_version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// co_recognizers
			// 
			this.co_recognizers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.co_recognizers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.co_recognizers.DropDownWidth = 340;
			this.co_recognizers.FormattingEnabled = true;
			this.co_recognizers.Location = new System.Drawing.Point(731, 7);
			this.co_recognizers.Margin = new System.Windows.Forms.Padding(0);
			this.co_recognizers.Name = "co_recognizers";
			this.co_recognizers.Size = new System.Drawing.Size(435, 23);
			this.co_recognizers.TabIndex = 2;
			this.co_recognizers.SelectedIndexChanged += new System.EventHandler(this.OnRecognizerChanged);
			// 
			// bu_synth
			// 
			this.bu_synth.Enabled = false;
			this.bu_synth.Location = new System.Drawing.Point(60, 34);
			this.bu_synth.Margin = new System.Windows.Forms.Padding(0);
			this.bu_synth.Name = "bu_synth";
			this.bu_synth.Size = new System.Drawing.Size(51, 25);
			this.bu_synth.TabIndex = 4;
			this.bu_synth.Text = "voice";
			this.bu_synth.UseVisualStyleBackColor = true;
			this.bu_synth.Click += new System.EventHandler(this.click_Synth);
			// 
			// rb_def
			// 
			this.rb_def.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.rb_def.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.rb_def.Location = new System.Drawing.Point(396, 268);
			this.rb_def.Margin = new System.Windows.Forms.Padding(0);
			this.rb_def.Name = "rb_def";
			this.rb_def.Size = new System.Drawing.Size(20, 15);
			this.rb_def.TabIndex = 30;
			this.rb_def.TabStop = true;
			this.rb_def.UseVisualStyleBackColor = true;
			this.rb_def.Visible = false;
			this.rb_def.CheckedChanged += new System.EventHandler(this.checkedchanged_Radio);
			// 
			// rb_enh
			// 
			this.rb_enh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.rb_enh.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.rb_enh.Location = new System.Drawing.Point(396, 407);
			this.rb_enh.Margin = new System.Windows.Forms.Padding(0);
			this.rb_enh.Name = "rb_enh";
			this.rb_enh.Size = new System.Drawing.Size(20, 15);
			this.rb_enh.TabIndex = 31;
			this.rb_enh.TabStop = true;
			this.rb_enh.UseVisualStyleBackColor = true;
			this.rb_enh.Visible = false;
			this.rb_enh.CheckedChanged += new System.EventHandler(this.checkedchanged_Radio);
			// 
			// ss_bot
			// 
			this.ss_bot.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.ss_bot.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tssl_token,
			this.tssl_langids_,
			this.tssl_langids});
			this.ss_bot.Location = new System.Drawing.Point(0, 484);
			this.ss_bot.Name = "ss_bot";
			this.ss_bot.Size = new System.Drawing.Size(1256, 22);
			this.ss_bot.TabIndex = 32;
			// 
			// tssl_token
			// 
			this.tssl_token.AutoSize = false;
			this.tssl_token.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tssl_token.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
			this.tssl_token.Name = "tssl_token";
			this.tssl_token.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.tssl_token.Size = new System.Drawing.Size(600, 22);
			this.tssl_token.Text = "tssl_token";
			this.tssl_token.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tssl_langids_
			// 
			this.tssl_langids_.AutoSize = false;
			this.tssl_langids_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tssl_langids_.Margin = new System.Windows.Forms.Padding(0);
			this.tssl_langids_.Name = "tssl_langids_";
			this.tssl_langids_.Size = new System.Drawing.Size(55, 22);
			this.tssl_langids_.Text = "languages";
			// 
			// tssl_langids
			// 
			this.tssl_langids.AutoSize = false;
			this.tssl_langids.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tssl_langids.Margin = new System.Windows.Forms.Padding(0);
			this.tssl_langids.Name = "tssl_langids";
			this.tssl_langids.Size = new System.Drawing.Size(584, 22);
			this.tssl_langids.Spring = true;
			this.tssl_langids.Text = "tssl_langids";
			this.tssl_langids.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// bu_edit
			// 
			this.bu_edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_edit.Enabled = false;
			this.bu_edit.Location = new System.Drawing.Point(871, 34);
			this.bu_edit.Margin = new System.Windows.Forms.Padding(0);
			this.bu_edit.Name = "bu_edit";
			this.bu_edit.Size = new System.Drawing.Size(86, 25);
			this.bu_edit.TabIndex = 33;
			this.bu_edit.Text = "Edit ...";
			this.bu_edit.UseVisualStyleBackColor = true;
			this.bu_edit.Click += new System.EventHandler(this.click_PhonLabel);
			// 
			// FxeGeneratorF
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1256, 506);
			this.Controls.Add(this.bu_edit);
			this.Controls.Add(this.ss_bot);
			this.Controls.Add(this.rb_enh);
			this.Controls.Add(this.rb_def);
			this.Controls.Add(this.bu_synth);
			this.Controls.Add(this.co_recognizers);
			this.Controls.Add(this.bu_createfxe);
			this.Controls.Add(this.co_headtype);
			this.Controls.Add(this.la_headtype);
			this.Controls.Add(this.bu_generate);
			this.Controls.Add(this.bu_play);
			this.Controls.Add(this.tb_wavefile);
			this.Controls.Add(this.grid_phons);
			this.Controls.Add(this.grid_blocs);
			this.Controls.Add(this.la_blocks);
			this.Controls.Add(this.la_phons);
			this.Controls.Add(this.la_version);
			this.Controls.Add(this.bu_open);
			this.Controls.Add(this.la_text);
			this.Controls.Add(this.tb_text);
			this.Controls.Add(this.la_expected);
			this.Controls.Add(this.tb_expected);
			this.Controls.Add(this.la_def_words);
			this.Controls.Add(this.la_def_word_pct);
			this.Controls.Add(this.tb_def_words);
			this.Controls.Add(this.la_def_phons);
			this.Controls.Add(this.la_def_phon_pct);
			this.Controls.Add(this.tb_def_phons);
			this.Controls.Add(this.la_enh_words);
			this.Controls.Add(this.la_enh_word_pct);
			this.Controls.Add(this.tb_enh_words);
			this.Controls.Add(this.la_enh_phons);
			this.Controls.Add(this.la_enh_phon_pct);
			this.Controls.Add(this.tb_enh_phons);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = global::FXE_Generator.Properties.Resource.lips;
			this.Name = "FxeGeneratorF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "0x22 - FXE LipSyncer";
			((System.ComponentModel.ISupportInitialize)(this.grid_phons)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid_blocs)).EndInit();
			this.ss_bot.ResumeLayout(false);
			this.ss_bot.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Windows Form Designer generated code
	}
}

