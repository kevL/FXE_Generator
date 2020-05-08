using System;
using System.Drawing;
using System.Windows.Forms;

using SpeechLib;


namespace lipsync_editor
{
	sealed class VoiceSynthF
		: Form
	{
		#region fields (static)
		static int _x = -1;
		static int _y = -1;
		static int _w = -1;
		static int _h = -1;

		static int _vol = 100;
		static int _rat =  -2; // default 0 standard (range -10..10).
		#endregion fields (static)


		#region fields
		readonly FxeGeneratorF _f;

		readonly SpVoice _voice = new SpVoice();
		#endregion fields


		#region cTor
		internal VoiceSynthF(FxeGeneratorF f, string text)
		{
			InitializeComponent();
			bar_rat.Minimum = -10; // NOTE: The designer doesn't like negative values apparently.

			if (_w != -1)
				ClientSize = new Size(_w, _h);

			tb_text.Height = ClientSize.Height - tb_text.Location.Y;
			tb_text.Width  =
			bar_vol.Width  =
			bar_rat.Width  = ClientSize.Width;

			_f = f;
			tb_text.Text = text;

			if (_x == -1)
			{
				Location = new Point(_f.Location.X + 20, _f.Location.Y + 20);
			}
			else
				Location = new Point(_x,_y);

			_voice.Volume = bar_vol.Value = _vol;
			_voice.Rate   = bar_rat.Value = _rat;

			la_vol.Text = "vol  "  + _vol;
			la_rat.Text = "rate  " + _rat;
		}
		#endregion cTor


		#region handlers
		void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			_x = Math.Max(0, Location.X);
			_y = Math.Max(0, Location.Y);
			_w = ClientSize.Width;
			_h = ClientSize.Height;

			_f.EnableSynth();
		}

		void OnClick_Play(object sender, EventArgs e)
		{
			_voice.Speak(tb_text.Text, SpeechVoiceSpeakFlags.SVSFlagsAsync);
		}

		void OnClick_Ok(object sender, EventArgs e)
		{
			_f.SetText(tb_text.Text);
			Close();
		}

		void OnClick_Cancel(object sender, EventArgs e)
		{
			Close();
		}

		void OnValueChanged_Vol(object sender, EventArgs e)
		{
			_vol = _voice.Volume = bar_vol.Value;
			la_vol.Text = "vol  " + _vol;
		}

		void OnValueChanged_Rate(object sender, EventArgs e)
		{
			_rat = _voice.Rate = bar_rat.Value;
			la_rat.Text = "rate  " + _rat;
		}
		#endregion handlers


		#region designer
		TrackBar bar_vol;
		TrackBar bar_rat;
		Button bu_play;
		Button bu_cancel;
		TextBox tb_text;
		Button bu_ok;
		Label la_vol;
		Label la_rat;

		void InitializeComponent()
		{
			this.bar_vol = new System.Windows.Forms.TrackBar();
			this.bar_rat = new System.Windows.Forms.TrackBar();
			this.bu_play = new System.Windows.Forms.Button();
			this.bu_cancel = new System.Windows.Forms.Button();
			this.tb_text = new System.Windows.Forms.TextBox();
			this.bu_ok = new System.Windows.Forms.Button();
			this.la_vol = new System.Windows.Forms.Label();
			this.la_rat = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.bar_vol)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bar_rat)).BeginInit();
			this.SuspendLayout();
			// 
			// bar_vol
			// 
			this.bar_vol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bar_vol.LargeChange = 10;
			this.bar_vol.Location = new System.Drawing.Point(0, 14);
			this.bar_vol.Margin = new System.Windows.Forms.Padding(0);
			this.bar_vol.Maximum = 100;
			this.bar_vol.Name = "bar_vol";
			this.bar_vol.Size = new System.Drawing.Size(325, 40);
			this.bar_vol.TabIndex = 1;
			this.bar_vol.TickFrequency = 10;
			this.bar_vol.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.bar_vol.Value = 100;
			this.bar_vol.ValueChanged += new System.EventHandler(this.OnValueChanged_Vol);
			// 
			// bar_rat
			// 
			this.bar_rat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bar_rat.LargeChange = 1;
			this.bar_rat.Location = new System.Drawing.Point(0, 63);
			this.bar_rat.Margin = new System.Windows.Forms.Padding(0);
			this.bar_rat.Name = "bar_rat";
			this.bar_rat.Size = new System.Drawing.Size(325, 40);
			this.bar_rat.TabIndex = 3;
			this.bar_rat.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.bar_rat.ValueChanged += new System.EventHandler(this.OnValueChanged_Rate);
			// 
			// bu_play
			// 
			this.bu_play.Location = new System.Drawing.Point(17, 104);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(75, 25);
			this.bu_play.TabIndex = 4;
			this.bu_play.Text = "play";
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.OnClick_Play);
			// 
			// bu_cancel
			// 
			this.bu_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.bu_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bu_cancel.Location = new System.Drawing.Point(241, 104);
			this.bu_cancel.Margin = new System.Windows.Forms.Padding(0);
			this.bu_cancel.Name = "bu_cancel";
			this.bu_cancel.Size = new System.Drawing.Size(75, 25);
			this.bu_cancel.TabIndex = 6;
			this.bu_cancel.Text = "cancel";
			this.bu_cancel.UseVisualStyleBackColor = true;
			this.bu_cancel.Click += new System.EventHandler(this.OnClick_Cancel);
			// 
			// tb_text
			// 
			this.tb_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tb_text.Location = new System.Drawing.Point(0, 133);
			this.tb_text.Margin = new System.Windows.Forms.Padding(0);
			this.tb_text.Multiline = true;
			this.tb_text.Name = "tb_text";
			this.tb_text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.tb_text.Size = new System.Drawing.Size(327, 59);
			this.tb_text.TabIndex = 7;
			// 
			// bu_ok
			// 
			this.bu_ok.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.bu_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bu_ok.Location = new System.Drawing.Point(109, 104);
			this.bu_ok.Margin = new System.Windows.Forms.Padding(0);
			this.bu_ok.Name = "bu_ok";
			this.bu_ok.Size = new System.Drawing.Size(115, 25);
			this.bu_ok.TabIndex = 5;
			this.bu_ok.Text = "ok";
			this.bu_ok.UseVisualStyleBackColor = true;
			this.bu_ok.Click += new System.EventHandler(this.OnClick_Ok);
			// 
			// la_vol
			// 
			this.la_vol.Location = new System.Drawing.Point(5, 2);
			this.la_vol.Margin = new System.Windows.Forms.Padding(0);
			this.la_vol.Name = "la_vol";
			this.la_vol.Size = new System.Drawing.Size(75, 15);
			this.la_vol.TabIndex = 0;
			this.la_vol.Text = "vol";
			// 
			// la_rat
			// 
			this.la_rat.Location = new System.Drawing.Point(5, 50);
			this.la_rat.Margin = new System.Windows.Forms.Padding(0);
			this.la_rat.Name = "la_rat";
			this.la_rat.Size = new System.Drawing.Size(75, 15);
			this.la_rat.TabIndex = 2;
			this.la_rat.Text = "rate";
			// 
			// VoiceSynthF
			// 
			this.AcceptButton = this.bu_ok;
			this.CancelButton = this.bu_cancel;
			this.ClientSize = new System.Drawing.Size(332, 199);
			this.Controls.Add(this.la_rat);
			this.Controls.Add(this.la_vol);
			this.Controls.Add(this.bu_ok);
			this.Controls.Add(this.tb_text);
			this.Controls.Add(this.bu_cancel);
			this.Controls.Add(this.bu_play);
			this.Controls.Add(this.bar_rat);
			this.Controls.Add(this.bar_vol);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(340, 225);
			this.Name = "VoiceSynthF";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Voice synthesizer";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
			((System.ComponentModel.ISupportInitialize)(this.bar_vol)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bar_rat)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
