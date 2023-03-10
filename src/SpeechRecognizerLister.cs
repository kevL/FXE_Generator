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
		/// <returns><c>true</c> if a Recognizer was found and added to the
		/// list; false if user is SoL</returns>
		internal static bool AddSpeechRecognizers(ComboBox co)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("SpeechRecognizerLister.AddSpeechRecognizers()");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
#endif
			// NOTE: This is your SAPI5.4 SpeechRecognizer (aka SpeechRecognitionEngine) interface.
			// good luck!
			var sr_default = new SpInprocRecognizer();
#if DEBUG
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");
#endif
			if (sr_default != null)
			{
#if DEBUG
				logfile.Log(". operating system default:");
				logfile.Log("Recognizer.Id= "               + sr_default.Recognizer.Id);
				logfile.Log("Recognizer.GetDescription()= " + sr_default.Recognizer.GetDescription());
				logfile.Log();
#endif
				ISpeechObjectTokens toks = sr_default.GetRecognizers();

				int  id_default = -1;
				bool id_default_found = false;

				var recognizers = new List<Recognizer>();
				foreach (ISpeechObjectToken tok in toks)
				{
#if DEBUG
					logfile.Log(". installed.Id= "               + tok.Id);
					logfile.Log(". installed.GetDescription()= " + tok.GetDescription());
#endif
					// 8.0+ TODO: other SAPI-compliant speech engines incl/ 3rd-party
					if (   tok.GetDescription().Contains("Microsoft Speech Recognizer 8.0")  // english-ish OS
						|| tok.GetDescription().Contains("Microsoft Speech Recogniser 8.0")) // german-ish OS
					{
#if DEBUG
						logfile.Log(". . found a Microsoft Speech Recognizer");
#endif
						recognizers.Add(new Recognizer(tok));

						if (!id_default_found)
						{
							++id_default;
#if DEBUG
							logfile.Log(". . id_default= " + id_default);
#endif
							if (tok.Id == sr_default.Recognizer.Id)
							{
#if DEBUG
								logfile.Log(". . . (tok.Id == sr_default.Recognizer.Id) default found.");
#endif
								id_default_found = true;
							}
						}
					}
#if DEBUG
					logfile.Log();
#endif
				}

				if (recognizers.Count != 0)
				{
					co.DataSource = recognizers;
					co.SelectedIndex = id_default;

					return true;
				}
			}
#if DEBUG
			logfile.Log(". RECOGNIZER NOT FOUND");
#endif
			return false;
		}
	}
}
