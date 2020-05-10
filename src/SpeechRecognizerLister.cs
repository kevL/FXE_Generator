using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SpeechLib;


namespace lipsync_editor
{
	/// <summary>
	/// A static object that determines the user's available SpeechRecognizers.
	/// </summary>
	static class SpeechRecognizerLister
	{
		/// <summary>
		/// Adds the user's installed SAPI-compliant SpeechRecognizers (speech
		/// recognition engines) to a specified combobox.
		/// </summary>
		/// <param name="co"></param>
		/// <returns>true if a Recognizer was found and added to the list;
		/// false if user is SoL</returns>
		internal static bool AddSpeechRecognizers(ComboBox co)
		{
			logfile.Log();
			logfile.Log("SpeechRecognizerLister.AddSpeechRecognizers()");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
			// NOTE: This is your SAPI5.4 SpeechRecognizer (aka SpeechRecognitionEngine) interface.
			// good luck!
			var sr_default = new SpInprocRecognizer();
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");

			if (sr_default != null)
			{
				logfile.Log();
				logfile.Log("Recognizer.Id= "               + sr_default.Recognizer.Id);
				logfile.Log("Recognizer.GetDescription()= " + sr_default.Recognizer.GetDescription());
				logfile.Log();

				ISpeechObjectTokens toks = sr_default.GetRecognizers();
				foreach (ISpeechObjectToken tok in toks)
				{
					logfile.Log(". installed.Id= "               + tok.Id);
					logfile.Log(". installed.GetDescription()= " + tok.GetDescription());
				}
				logfile.Log();


				int  id_default = -1;
				bool id_default_found = false;

				var recognizers = new List<Recognizer>();
				foreach (ISpeechObjectToken tok in toks)
				{
					if (tok.GetDescription().Contains("Microsoft Speech Recognizer")) // 8.0+ TODO: other SAPI-compliant speech engines incl/ 3rd-party
					{
						recognizers.Add(new Recognizer(tok));

						if (!id_default_found)
						{
							++id_default;

							if (tok.Id == sr_default.Recognizer.Id)
								id_default_found = true;
						}
					}
				}

				if (recognizers.Count != 0)
				{
					co.DataSource = recognizers;
					co.SelectedIndex = id_default;

					return true;
				}
			}

			logfile.Log(". RECOGNIZER NOT FOUND");
			return false;
		}
	}
}
