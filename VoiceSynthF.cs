using System;
using System.Windows.Forms;

using SpeechLib;


namespace lipsync_editor
{
	sealed class VoiceSynthF
		: Form
	{
		#region fields
		FxeGeneratorF _f;

		SpVoice _voice = new SpVoice();
		#endregion fields


		#region cTor
		internal VoiceSynthF(FxeGeneratorF f, string text)
		{
			InitializeComponent();

			_f = f;

			tb_text.Text = text;

			_voice.Volume = tb_vol.Value;
			_voice.Rate   = tb_rat.Value;

			la_vol.Text = tb_vol.Value.ToString();
			la_rat.Text = tb_rat.Value.ToString();
		}
		#endregion cTor


		#region handlers
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
			_voice.Volume = tb_vol.Value;
			la_vol.Text = tb_vol.Value.ToString();
		}

		void OnValueChanged_Rate(object sender, EventArgs e)
		{
			_voice.Rate = tb_rat.Value;
			la_rat.Text = tb_rat.Value.ToString();
		}
		#endregion handlers


		#region designer
		TrackBar tb_vol;
		TrackBar tb_rat;
		Button bu_play;
		Button bu_cancel;
		TextBox tb_text;
		Button bu_ok;
		Label la_vol;
		Label la_rat;

		void InitializeComponent()
		{
			this.tb_vol = new System.Windows.Forms.TrackBar();
			this.tb_rat = new System.Windows.Forms.TrackBar();
			this.bu_play = new System.Windows.Forms.Button();
			this.bu_cancel = new System.Windows.Forms.Button();
			this.tb_text = new System.Windows.Forms.TextBox();
			this.bu_ok = new System.Windows.Forms.Button();
			this.la_vol = new System.Windows.Forms.Label();
			this.la_rat = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.tb_vol)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tb_rat)).BeginInit();
			this.SuspendLayout();
			// 
			// tb_vol
			// 
			this.tb_vol.LargeChange = 10;
			this.tb_vol.Location = new System.Drawing.Point(5, 5);
			this.tb_vol.Margin = new System.Windows.Forms.Padding(0);
			this.tb_vol.Maximum = 100;
			this.tb_vol.Name = "tb_vol";
			this.tb_vol.Size = new System.Drawing.Size(285, 40);
			this.tb_vol.TabIndex = 0;
			this.tb_vol.TickFrequency = 10;
			this.tb_vol.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.tb_vol.Value = 100;
			this.tb_vol.ValueChanged += new System.EventHandler(this.OnValueChanged_Vol);
			// 
			// tb_rat
			// 
			this.tb_rat.LargeChange = 1;
			this.tb_rat.Location = new System.Drawing.Point(5, 45);
			this.tb_rat.Margin = new System.Windows.Forms.Padding(0);
			this.tb_rat.Maximum = 5;
			this.tb_rat.Minimum = -5;
			this.tb_rat.Name = "tb_rat";
			this.tb_rat.Size = new System.Drawing.Size(285, 40);
			this.tb_rat.TabIndex = 1;
			this.tb_rat.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.tb_rat.ValueChanged += new System.EventHandler(this.OnValueChanged_Rate);
			// 
			// bu_play
			// 
			this.bu_play.Location = new System.Drawing.Point(20, 120);
			this.bu_play.Margin = new System.Windows.Forms.Padding(0);
			this.bu_play.Name = "bu_play";
			this.bu_play.Size = new System.Drawing.Size(70, 25);
			this.bu_play.TabIndex = 4;
			this.bu_play.Text = "play";
			this.bu_play.UseVisualStyleBackColor = true;
			this.bu_play.Click += new System.EventHandler(this.OnClick_Play);
			// 
			// bu_cancel
			// 
			this.bu_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bu_cancel.Location = new System.Drawing.Point(205, 120);
			this.bu_cancel.Margin = new System.Windows.Forms.Padding(0);
			this.bu_cancel.Name = "bu_cancel";
			this.bu_cancel.Size = new System.Drawing.Size(70, 25);
			this.bu_cancel.TabIndex = 6;
			this.bu_cancel.Text = "cancel";
			this.bu_cancel.UseVisualStyleBackColor = true;
			this.bu_cancel.Click += new System.EventHandler(this.OnClick_Cancel);
			// 
			// tb_text
			// 
			this.tb_text.Location = new System.Drawing.Point(10, 155);
			this.tb_text.Margin = new System.Windows.Forms.Padding(0);
			this.tb_text.Multiline = true;
			this.tb_text.Name = "tb_text";
			this.tb_text.Size = new System.Drawing.Size(275, 115);
			this.tb_text.TabIndex = 7;
			// 
			// bu_ok
			// 
			this.bu_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bu_ok.Location = new System.Drawing.Point(112, 120);
			this.bu_ok.Margin = new System.Windows.Forms.Padding(0);
			this.bu_ok.Name = "bu_ok";
			this.bu_ok.Size = new System.Drawing.Size(70, 25);
			this.bu_ok.TabIndex = 5;
			this.bu_ok.Text = "ok";
			this.bu_ok.UseVisualStyleBackColor = true;
			this.bu_ok.Click += new System.EventHandler(this.OnClick_Ok);
			// 
			// la_vol
			// 
			this.la_vol.Location = new System.Drawing.Point(65, 95);
			this.la_vol.Margin = new System.Windows.Forms.Padding(0);
			this.la_vol.Name = "la_vol";
			this.la_vol.Size = new System.Drawing.Size(75, 20);
			this.la_vol.TabIndex = 2;
			this.la_vol.Text = "vol";
			this.la_vol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// la_rat
			// 
			this.la_rat.Location = new System.Drawing.Point(155, 95);
			this.la_rat.Margin = new System.Windows.Forms.Padding(0);
			this.la_rat.Name = "la_rat";
			this.la_rat.Size = new System.Drawing.Size(75, 20);
			this.la_rat.TabIndex = 3;
			this.la_rat.Text = "rate";
			this.la_rat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// VoiceSynthF
			// 
			this.AcceptButton = this.bu_ok;
			this.CancelButton = this.bu_cancel;
			this.ClientSize = new System.Drawing.Size(294, 276);
			this.Controls.Add(this.la_rat);
			this.Controls.Add(this.la_vol);
			this.Controls.Add(this.bu_ok);
			this.Controls.Add(this.tb_text);
			this.Controls.Add(this.bu_cancel);
			this.Controls.Add(this.bu_play);
			this.Controls.Add(this.tb_rat);
			this.Controls.Add(this.tb_vol);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "VoiceSynthF";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Voice synthesizer";
			((System.ComponentModel.ISupportInitialize)(this.tb_vol)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tb_rat)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion designer
	}
}
