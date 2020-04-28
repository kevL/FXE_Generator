using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace lipsync_editor
{
	sealed partial class FxeGeneratorF
	{
		/// <summary>
		/// (un)Required designer variable.
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


		ComboBox co_langId;
		Label la_version;

		Button bu_open;
		Button bu_generate;
		Button bu_synth;
		Button bu_createfxe;

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
		DataGridView dg_phons;
		Label la_blocks;
		DataGridView dg_blocks;


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
			this.dg_phons = new System.Windows.Forms.DataGridView();
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
			this.dg_blocks = new System.Windows.Forms.DataGridView();
			this.bu_createfxe = new System.Windows.Forms.Button();
			this.co_headtype = new System.Windows.Forms.ComboBox();
			this.la_headtype = new System.Windows.Forms.Label();
			this.bu_play = new System.Windows.Forms.Button();
			this.la_blocks = new System.Windows.Forms.Label();
			this.la_version = new System.Windows.Forms.Label();
			this.co_langId = new System.Windows.Forms.ComboBox();
			this.bu_synth = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dg_phons)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dg_blocks)).BeginInit();
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
			this.tb_def_words.TabIndex = 16;
			// 
			// la_def_words
			// 
			this.la_def_words.Location = new System.Drawing.Point(5, 209);
			this.la_def_words.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_words.Name = "la_def_words";
			this.la_def_words.Size = new System.Drawing.Size(305, 15);
			this.la_def_words.TabIndex = 14;
			this.la_def_words.Text = "Result - recognized speech";
			this.la_def_words.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_wavefile
			// 
			this.tb_wavefile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_wavefile.Location = new System.Drawing.Point(85, 7);
			this.tb_wavefile.Margin = new System.Windows.Forms.Padding(0);
			this.tb_wavefile.Name = "tb_wavefile";
			this.tb_wavefile.ReadOnly = true;
			this.tb_wavefile.Size = new System.Drawing.Size(730, 22);
			this.tb_wavefile.TabIndex = 1;
			this.tb_wavefile.WordWrap = false;
			// 
			// bu_open
			// 
			this.bu_open.Location = new System.Drawing.Point(5, 5);
			this.bu_open.Margin = new System.Windows.Forms.Padding(0);
			this.bu_open.Name = "bu_open";
			this.bu_open.Size = new System.Drawing.Size(75, 26);
			this.bu_open.TabIndex = 0;
			this.bu_open.Text = "open";
			this.bu_open.UseVisualStyleBackColor = true;
			this.bu_open.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// bu_generate
			// 
			this.bu_generate.Enabled = false;
			this.bu_generate.Location = new System.Drawing.Point(85, 35);
			this.bu_generate.Margin = new System.Windows.Forms.Padding(0);
			this.bu_generate.Name = "bu_generate";
			this.bu_generate.Size = new System.Drawing.Size(65, 25);
			this.bu_generate.TabIndex = 5;
			this.bu_generate.Text = "generate";
			this.bu_generate.UseVisualStyleBackColor = true;
			this.bu_generate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// tb_def_phons
			// 
			this.tb_def_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_def_phons.Location = new System.Drawing.Point(5, 286);
			this.tb_def_phons.Margin = new System.Windows.Forms.Padding(0);
			this.tb_def_phons.Multiline = true;
			this.tb_def_phons.Name = "tb_def_phons";
			this.tb_def_phons.ReadOnly = true;
			this.tb_def_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_def_phons.Size = new System.Drawing.Size(460, 55);
			this.tb_def_phons.TabIndex = 19;
			// 
			// la_def_phons
			// 
			this.la_def_phons.Location = new System.Drawing.Point(5, 270);
			this.la_def_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_phons.Name = "la_def_phons";
			this.la_def_phons.Size = new System.Drawing.Size(305, 15);
			this.la_def_phons.TabIndex = 17;
			this.la_def_phons.Text = "Result - phonemes";
			this.la_def_phons.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_text
			// 
			this.tb_text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_text.Location = new System.Drawing.Point(5, 80);
			this.tb_text.Margin = new System.Windows.Forms.Padding(0);
			this.tb_text.Multiline = true;
			this.tb_text.Name = "tb_text";
			this.tb_text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_text.Size = new System.Drawing.Size(460, 40);
			this.tb_text.TabIndex = 11;
			// 
			// dg_phons
			// 
			this.dg_phons.AllowUserToAddRows = false;
			this.dg_phons.AllowUserToDeleteRows = false;
			this.dg_phons.AllowUserToResizeColumns = false;
			this.dg_phons.AllowUserToResizeRows = false;
			this.dg_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.dg_phons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dg_phons.Location = new System.Drawing.Point(475, 60);
			this.dg_phons.Margin = new System.Windows.Forms.Padding(0);
			this.dg_phons.Name = "dg_phons";
			this.dg_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dg_phons.Size = new System.Drawing.Size(280, 420);
			this.dg_phons.TabIndex = 27;
			// 
			// la_text
			// 
			this.la_text.Location = new System.Drawing.Point(5, 64);
			this.la_text.Margin = new System.Windows.Forms.Padding(0);
			this.la_text.Name = "la_text";
			this.la_text.Size = new System.Drawing.Size(460, 15);
			this.la_text.TabIndex = 10;
			this.la_text.Text = "Typed text";
			this.la_text.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// la_expected
			// 
			this.la_expected.Location = new System.Drawing.Point(5, 124);
			this.la_expected.Margin = new System.Windows.Forms.Padding(0);
			this.la_expected.Name = "la_expected";
			this.la_expected.Size = new System.Drawing.Size(460, 15);
			this.la_expected.TabIndex = 12;
			this.la_expected.Text = "expected phonemes";
			this.la_expected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_expected
			// 
			this.tb_expected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_expected.Location = new System.Drawing.Point(5, 140);
			this.tb_expected.Margin = new System.Windows.Forms.Padding(0);
			this.tb_expected.Multiline = true;
			this.tb_expected.Name = "tb_expected";
			this.tb_expected.ReadOnly = true;
			this.tb_expected.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_expected.Size = new System.Drawing.Size(460, 55);
			this.tb_expected.TabIndex = 13;
			// 
			// la_enh_phons
			// 
			this.la_enh_phons.Location = new System.Drawing.Point(5, 410);
			this.la_enh_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_phons.Name = "la_enh_phons";
			this.la_enh_phons.Size = new System.Drawing.Size(305, 15);
			this.la_enh_phons.TabIndex = 23;
			this.la_enh_phons.Text = "Result - phonemes enhanced w/ Typed text";
			this.la_enh_phons.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_enh_phons
			// 
			this.tb_enh_phons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_enh_phons.Location = new System.Drawing.Point(5, 426);
			this.tb_enh_phons.Margin = new System.Windows.Forms.Padding(0);
			this.tb_enh_phons.Multiline = true;
			this.tb_enh_phons.Name = "tb_enh_phons";
			this.tb_enh_phons.ReadOnly = true;
			this.tb_enh_phons.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_enh_phons.Size = new System.Drawing.Size(460, 55);
			this.tb_enh_phons.TabIndex = 25;
			// 
			// la_enh_words
			// 
			this.la_enh_words.Location = new System.Drawing.Point(5, 349);
			this.la_enh_words.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_words.Name = "la_enh_words";
			this.la_enh_words.Size = new System.Drawing.Size(305, 15);
			this.la_enh_words.TabIndex = 20;
			this.la_enh_words.Text = "Result - recognized speech enhanced w/ Typed text";
			this.la_enh_words.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tb_enh_words
			// 
			this.tb_enh_words.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_enh_words.Location = new System.Drawing.Point(5, 365);
			this.tb_enh_words.Margin = new System.Windows.Forms.Padding(0);
			this.tb_enh_words.Multiline = true;
			this.tb_enh_words.Name = "tb_enh_words";
			this.tb_enh_words.ReadOnly = true;
			this.tb_enh_words.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_enh_words.Size = new System.Drawing.Size(460, 40);
			this.tb_enh_words.TabIndex = 22;
			// 
			// la_def_word_pct
			// 
			this.la_def_word_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_def_word_pct.Location = new System.Drawing.Point(410, 209);
			this.la_def_word_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_word_pct.Name = "la_def_word_pct";
			this.la_def_word_pct.Size = new System.Drawing.Size(55, 15);
			this.la_def_word_pct.TabIndex = 15;
			this.la_def_word_pct.Text = "pct";
			this.la_def_word_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_def_phon_pct
			// 
			this.la_def_phon_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_def_phon_pct.Location = new System.Drawing.Point(410, 270);
			this.la_def_phon_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_def_phon_pct.Name = "la_def_phon_pct";
			this.la_def_phon_pct.Size = new System.Drawing.Size(55, 15);
			this.la_def_phon_pct.TabIndex = 18;
			this.la_def_phon_pct.Text = "pct";
			this.la_def_phon_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_enh_word_pct
			// 
			this.la_enh_word_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_enh_word_pct.Location = new System.Drawing.Point(410, 349);
			this.la_enh_word_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_word_pct.Name = "la_enh_word_pct";
			this.la_enh_word_pct.Size = new System.Drawing.Size(55, 15);
			this.la_enh_word_pct.TabIndex = 21;
			this.la_enh_word_pct.Text = "pct";
			this.la_enh_word_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_enh_phon_pct
			// 
			this.la_enh_phon_pct.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_enh_phon_pct.Location = new System.Drawing.Point(410, 410);
			this.la_enh_phon_pct.Margin = new System.Windows.Forms.Padding(0);
			this.la_enh_phon_pct.Name = "la_enh_phon_pct";
			this.la_enh_phon_pct.Size = new System.Drawing.Size(55, 15);
			this.la_enh_phon_pct.TabIndex = 24;
			this.la_enh_phon_pct.Text = "pct";
			this.la_enh_phon_pct.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_phons
			// 
			this.la_phons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_phons.Location = new System.Drawing.Point(475, 40);
			this.la_phons.Margin = new System.Windows.Forms.Padding(0);
			this.la_phons.Name = "la_phons";
			this.la_phons.Size = new System.Drawing.Size(280, 15);
			this.la_phons.TabIndex = 26;
			this.la_phons.Text = "PHONEMES";
			this.la_phons.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dg_blocks
			// 
			this.dg_blocks.AllowUserToAddRows = false;
			this.dg_blocks.AllowUserToDeleteRows = false;
			this.dg_blocks.AllowUserToResizeColumns = false;
			this.dg_blocks.AllowUserToResizeRows = false;
			this.dg_blocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.dg_blocks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dg_blocks.Location = new System.Drawing.Point(765, 60);
			this.dg_blocks.Margin = new System.Windows.Forms.Padding(0);
			this.dg_blocks.Name = "dg_blocks";
			this.dg_blocks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.dg_blocks.Size = new System.Drawing.Size(280, 420);
			this.dg_blocks.TabIndex = 29;
			// 
			// bu_createfxe
			// 
			this.bu_createfxe.Enabled = false;
			this.bu_createfxe.Location = new System.Drawing.Point(380, 35);
			this.bu_createfxe.Margin = new System.Windows.Forms.Padding(0);
			this.bu_createfxe.Name = "bu_createfxe";
			this.bu_createfxe.Size = new System.Drawing.Size(85, 25);
			this.bu_createfxe.TabIndex = 9;
			this.bu_createfxe.Text = "Create FXE";
			this.bu_createfxe.UseVisualStyleBackColor = true;
			this.bu_createfxe.Click += new System.EventHandler(this.btnCreateFxe_Click);
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
			this.co_headtype.Location = new System.Drawing.Point(265, 36);
			this.co_headtype.Margin = new System.Windows.Forms.Padding(0);
			this.co_headtype.Name = "co_headtype";
			this.co_headtype.Size = new System.Drawing.Size(105, 23);
			this.co_headtype.TabIndex = 8;
			// 
			// la_headtype
			// 
			this.la_headtype.Location = new System.Drawing.Point(190, 40);
			this.la_headtype.Margin = new System.Windows.Forms.Padding(0);
			this.la_headtype.Name = "la_headtype";
			this.la_headtype.Size = new System.Drawing.Size(70, 15);
			this.la_headtype.TabIndex = 7;
			this.la_headtype.Text = "Head Type";
			this.la_headtype.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// bu_play
			// 
			this.bu_play.Enabled = false;
			this.bu_play.Location = new System.Drawing.Point(5, 35);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(75, 25);
			this.bu_play.TabIndex = 4;
			this.bu_play.Text = "play";
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.btnPlay_Click);
			// 
			// la_blocks
			// 
			this.la_blocks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_blocks.Location = new System.Drawing.Point(765, 40);
			this.la_blocks.Margin = new System.Windows.Forms.Padding(0);
			this.la_blocks.Name = "la_blocks";
			this.la_blocks.Size = new System.Drawing.Size(280, 15);
			this.la_blocks.TabIndex = 28;
			this.la_blocks.Text = "Data Blocks";
			this.la_blocks.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_version
			// 
			this.la_version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.la_version.Location = new System.Drawing.Point(975, 10);
			this.la_version.Margin = new System.Windows.Forms.Padding(0);
			this.la_version.Name = "la_version";
			this.la_version.Size = new System.Drawing.Size(70, 20);
			this.la_version.TabIndex = 3;
			this.la_version.Text = "ver";
			this.la_version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// co_langId
			// 
			this.co_langId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.co_langId.FormattingEnabled = true;
			this.co_langId.Location = new System.Drawing.Point(820, 6);
			this.co_langId.Margin = new System.Windows.Forms.Padding(0);
			this.co_langId.Name = "co_langId";
			this.co_langId.Size = new System.Drawing.Size(145, 23);
			this.co_langId.TabIndex = 2;
			this.co_langId.SelectedIndexChanged += new System.EventHandler(this.OnLanguageChanged);
			// 
			// bu_synth
			// 
			this.bu_synth.Enabled = false;
			this.bu_synth.Location = new System.Drawing.Point(155, 35);
			this.bu_synth.Margin = new System.Windows.Forms.Padding(0);
			this.bu_synth.Name = "bu_synth";
			this.bu_synth.Size = new System.Drawing.Size(30, 25);
			this.bu_synth.TabIndex = 6;
			this.bu_synth.Text = "S";
			this.bu_synth.UseVisualStyleBackColor = true;
			this.bu_synth.Click += new System.EventHandler(this.btnSynth_Click);
			// 
			// FxeGeneratorF
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1052, 486);
			this.Controls.Add(this.bu_synth);
			this.Controls.Add(this.co_langId);
			this.Controls.Add(this.bu_createfxe);
			this.Controls.Add(this.co_headtype);
			this.Controls.Add(this.la_headtype);
			this.Controls.Add(this.bu_generate);
			this.Controls.Add(this.bu_play);
			this.Controls.Add(this.tb_wavefile);
			this.Controls.Add(this.dg_phons);
			this.Controls.Add(this.dg_blocks);
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
			this.Name = "FxeGeneratorF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "0x22 - FXE LipSyncer";
			((System.ComponentModel.ISupportInitialize)(this.dg_phons)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dg_blocks)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Windows Form Designer generated code
	}
}

