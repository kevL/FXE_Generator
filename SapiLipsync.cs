using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SpeechLib;


namespace lipsync_editor
{
	internal delegate void TtsParseTextEvent();
	internal delegate void SpeechRecognitionEndedEvent(List<OrthographicResult> ars_def,
													   List<OrthographicResult> ars_enh);


	/// <summary>
	/// Sapi stuff
	/// </summary>
	public sealed class SapiLipsync
	{
		#region events
		internal TtsParseTextEvent TtsParseText;
		internal SpeechRecognitionEndedEvent SpeechRecognitionEnded;
		#endregion events


		#region fields (static)
		const string RULE = "TextLipsync";
		#endregion fields (static)


		#region fields
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public SpInprocRecognizer _recognizer;
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public SpInProcRecoContext _recoContext;
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public SpFileStream _input;
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public SpVoice _voice;
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public ISpeechRecoGrammar _recoGrammar;
//		[System.Runtime.InteropServices.ComVisible(true)]
//		public SpPhoneConverter _phoneConverter;

		SpInprocRecognizer _recognizer;
		SpInProcRecoContext _recoContext;
		SpFileStream _input;
		SpVoice _voice;
		ISpeechRecoGrammar _recoGrammar;
		SpPhoneConverter _phoneConverter;


		string _text = String.Empty;

		readonly List<OrthographicResult> _ars_def = new List<OrthographicResult>(); // default
		readonly List<OrthographicResult> _ars_enh = new List<OrthographicResult>(); // enhanced w/ TypedText

		bool _ruler;
		string _results = String.Empty;
		ulong _offset;
		#endregion fields


		#region properties
		internal string Audiopath
		{ get; set; }

		List<string> _expected = new List<string>();
		internal List<string> Expected
		{
			get { return _expected; }
		}


		internal double RatioWords_def // default
		{ get; private set; }

		internal double RatioPhons_def // default
		{ get; private set; }

		internal double RatioWords_enh // enhanced w/ TypedText
		{ get; private set; }

		internal double RatioPhons_enh // enhanced w/ TypedText
		{ get; private set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor for GUI interface.
		/// </summary>
		internal SapiLipsync()
			: this(String.Empty)
		{}

		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="wavefile"></param>
		internal SapiLipsync(string wavefile)
		{
			logfile.Log();
			logfile.Log("SapiLipsync() cTor wavefile= " + wavefile);

			_voice = new SpVoice();
			_voice.Volume = 0;
			_voice.Rate   = 10;
			_voice.Phoneme   += OnSpeechPhoneme;
			_voice.EndStream += OnSpeechEndStream;

			logfile.Log(". create (SpPhoneConverter)_phoneConverter");
			_phoneConverter = new SpPhoneConverter();
			logfile.Log(". (SpPhoneConverter)_phoneConverter CREATED");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
			_recognizer = new SpInprocRecognizer(); // NOTE: This is your SAPI5.4 SpeechRecognizer (aka SpeechRecognitionEngine) interface. good luck!
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");

			if (wavefile != String.Empty) // is Console ->
			{
				_phoneConverter.LanguageId = 1033; // US English (default)
				Audiopath = AudioConverter.deterAudiopath(wavefile);

				logfile.Log(". Audiopath= " + Audiopath);
			}
		}
		#endregion cTor


		#region methods
		/// <summary>
		/// Sets the Recognizer and LanguageId when the Recognizers combobox
		/// selection changes.
		/// @note The LanguageId is used by both TTS and SpeechRecognition.
		/// </summary>
		/// <param name="recognizer"></param>
		internal void SetRecognizer(Recognizer recognizer)
		{
			_recognizer = new SpInprocRecognizer();
			_recognizer.Recognizer = (SpObjectToken)recognizer.Tok;

			// TODO: a better way to do this, try
			// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech\Recognizers\Tokens\MS-1033-80-DESK\Attributes -> Language
			// _recognizer.Tok -> Attributes -> Language (would need to decode those ##)
			int langid;
			if (!Int32.TryParse(recognizer.Id.Substring(3,4), out langid))
			{
				MessageBox.Show(" Did not parse a Language from the registry's token."
								+ Environment.NewLine + Environment.NewLine
								+ " LipSyncer will bork off.",
								" Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1);
				Environment.Exit(0);
			}
			_phoneConverter.LanguageId = langid;
		}


		internal void Start(string text)
		{
			logfile.Log();
			logfile.Log("Start()");

			// these don't all need to be cleared ->
			_ruler = false;

			_results = String.Empty;

			Expected.Clear();

			RatioWords_def =
			RatioPhons_def =
			RatioWords_enh =
			RatioPhons_enh = 0.0;

			_ars_def.Clear();
			_ars_enh.Clear();


			_text = text;
			logfile.Log(". _text= " + _text);

			if (_text == String.Empty)
			{
				logfile.Log(". . default - fire TtsParseText()");
				if (TtsParseText != null)
					TtsParseText();
			}
			else
			{
				logfile.Log(". . enhanced - call SpVoice.Speak()");

				_voice.Speak(_text); // -> fire TtsParseText when the TTS-stream ends.
//				_voice.WaitUntilDone(-1);
			}


			logfile.Log(". create (SpInProcRecoContext)_recoContext");
			_recoContext = (SpInProcRecoContext)_recognizer.CreateRecoContext();
			logfile.Log(". (SpInProcRecoContext)_recoContext CREATED");


//			_input = new SpFileStream();
//			_input.Open(Audiopath, SpeechStreamFileMode.SSFMOpenForRead, true);
//			_recognizer.AudioInputStream = _input;

#if DEBUG
			_recoContext.FalseRecognition += Sapi_Lipsync_FalseRecogntion;
			_recoContext.Hypothesis       += Sapi_Lipsync_Hypothesis;
#endif
			_recoContext.Recognition += Sapi_Lipsync_Recognition;
			_recoContext.EndStream   += Sapi_Lipsync_EndStream;


			logfile.Log("_recoContext.EventInterests= " + _recoContext.EventInterests);


			_recoGrammar = _recoContext.CreateGrammar();	// was "2" but MS doc says not needed on its end.
			_recoGrammar.DictationLoad();					// and I don't see grammar id #2 defined on this end either.
//			_recoGrammar.DictationLoad("Pronunciation");	// Load pronunciation dictation topic into the grammar so that the raw (unfiltered) phonemes may be retrieved.
//			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);

//			DictationGrammar dictationGrammar = new DictationGrammar("grammar:dictation");
//			dictationGrammar.Name = "DictationQuestion";
//			recognizer.LoadGrammar(dictationGrammar);
//			recognizer.RequestRecognizerUpdate();
//			recognizer.SetInputToDefaultAudioDevice();

			Generate(false);

			logfile.Log("Start() DONE");
			logfile.Log();
		}

/*		/// <summary>
		/// Instantiates a SpeechRecognitionEngine based on a specified culture
		/// and id.
		/// </summary>
		/// <param name="requiredCulture"></param>
		/// <param name="requiredId"></param>
		/// <returns>a SpeechRecognitionEngine that meets the criteria (if user
		/// has one installed) else null</returns>
		SpeechRecognitionEngine CreateSre(object requiredCulture, string requiredId)   
		{
			SpeechRecognitionEngine sre = null;

			foreach (RecognizerInfo info in SpeechRecognitionEngine.InstalledRecognizers())
			{
				if (info.Culture.Equals(requiredCulture) && info.Id == requiredId)
				{
					sre = new SpeechRecognitionEngine(info); // TODO: Dispose()
					break;
				}
			}
			return sre;
		} */


		void Generate(bool ruler)
		{
			logfile.Log();
			logfile.Log("Generate()");

			_offset = 0L;

			_ruler = ruler;
			logfile.Log(". _ruler= " + _ruler);

			// kL_NOTE: How absolutely bizarre. DO NOT SET '_ruler=ruler' in the
			// conditional expression below. It causes an infinite loop ...
			// since '_ruler' will NOT be set true despite 'ruler' being true.
			//
			// And that, friends, took a day to figure out.

			if (_ruler)// && _recoGrammar.Rules.FindRule(RULE) == null)
			{
				logfile.Log(". . add TextLipsync rule and set Rule ACTIVE");
				ISpeechGrammarRule rule = _recoGrammar.Rules.Add(RULE,
																 SpeechRuleAttributes.SRATopLevel
															   | SpeechRuleAttributes.SRADynamic,
																 1);

//				object PropertyValue = String.Empty;
//				rule.InitialState.AddWordTransition(null,
//													TypedText,
//													" ",
//													SpeechGrammarWordType.SGLexical,
//													"TextLipSync",						// <- typo looks like
//													1,
//													ref PropertyValue,
//													1f);
				rule.InitialState.AddWordTransition(null,
													_text,
													" ",
													SpeechGrammarWordType.SGLexical,
													RULE,
													1);

				_recoGrammar.Rules.Commit();
				_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSActive);
			}
			else
				_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive); // huh


			logfile.Log(". open audiostream and set Dictation ACTIVE");

			logfile.Log(". create (SpFileStream)_input");
			_input = new SpFileStream();
			logfile.Log(". (SpFileStream)_input CREATED");
//			_input.Format.Type = SpeechAudioFormatType.SAFT44kHz16BitMono;
			_input.Open(Audiopath, SpeechStreamFileMode.SSFMOpenForRead, true);
			_recognizer.AudioInputStream = _input;

//			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);

			logfile.Log("Generate() DONE");
		}
		#endregion methods


		#region voice handlers
		void OnSpeechPhoneme(int StreamNumber,
							 object StreamPosition,
							 int Duration,
							 short NextPhoneId,
							 SpeechVisemeFeature Feature,
							 short CurrentPhoneId)
		{
			logfile.Log("OnSpeechPhoneme() CurrentPhoneId= " + CurrentPhoneId + " langid= " + _phoneConverter.LanguageId);

			if (CurrentPhoneId > 9)
			{
				string phon = _phoneConverter.IdToPhone(CurrentPhoneId);
				logfile.Log(". phon= " + phon);

				Expected.Add(phon);
			}
		}

		void OnSpeechEndStream(int StreamNumber, object StreamPosition)
		{
			logfile.Log();
			logfile.Log("OnSpeechEndStream()");

			if (TtsParseText != null)
				TtsParseText();
		}
		#endregion voice handlers


		#region lipsync handlers
#if DEBUG
		void Sapi_Lipsync_Hypothesis(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log("Sapi_Lipsync_Hypothesis() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());

/*			if (_ruler)
			{
				logfile.Log("Sapi_Lipsync_Hypothesis()");

//				string results = Result.PhraseInfo.GetText(0, -1, true);
				string results = Result.PhraseInfo.GetText();
				if (results.Length > _results.Length)
				{
					logfile.Log(". replace _results");

					_results = results;
//					GenerateResults(Result);
				}
			} */
		}

		void Sapi_Lipsync_FalseRecogntion(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log();
			logfile.Log("Sapi_Lipsync_FalseRecogntion() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());
		}
#endif

		void Sapi_Lipsync_Recognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
		{
			logfile.Log();
			logfile.Log("Sapi_Lipsync_Recognition() _ruler= " + _ruler);
			logfile.Log(". duration= " + Result.PhraseInfo.AudioSizeTime);
			//logfile.Log(". RecognitionType= " + RecognitionType); // <- standard.
			logfile.Log(". " + Result.PhraseInfo.GetText());

			logfile.Log(". words= " + Result.PhraseInfo.Elements.Count);
			foreach (ISpeechPhraseElement word in Result.PhraseInfo.Elements)
			{
				logfile.Log(". . word= "             + word.DisplayText);
				//logfile.Log(". . LexicalForm= "      + word.LexicalForm);
				//logfile.Log(". . Pronunciation= "    + word.Pronunciation);
				logfile.Log(". . ActualConfidence= " + word.ActualConfidence);
				logfile.Log(". . EngineConfidence= " + word.EngineConfidence);
			}

//			GenerateResults(Result); ->
//			if (Result.PhraseInfo != null) // I have not seen a null PhraseInfo yet.
//			{
			//logfile.Log(". Result.PhraseInfo.Rule.Parent.Name= "      + Result.PhraseInfo.Rule.Parent.Name); // breaks the funct.
			//logfile.Log(". Result.PhraseInfo.Rule.Children.Count= "   + Result.PhraseInfo.Rule.Children.Count); // breaks the funct.
			logfile.Log(". Result.PhraseInfo.Rule.Confidence= "       + Result.PhraseInfo.Rule.Confidence);
			logfile.Log(". Result.PhraseInfo.Rule.EngineConfidence= " + Result.PhraseInfo.Rule.EngineConfidence);
			logfile.Log(". Result.PhraseInfo.Rule.Id= "               + Result.PhraseInfo.Rule.Id);
			//logfile.Log(". Result.PhraseInfo.Rule.Name= "             + Result.PhraseInfo.Rule.Name);
			//logfile.Log(". Result.PhraseInfo.Rule.NumberOfElements= " + Result.PhraseInfo.Rule.NumberOfElements);

//			int wordcount = Result.PhraseInfo.Rule.NumberOfElements;
			int wordcount = Result.PhraseInfo.Elements.Count;
			logfile.Log(". . Result.PhraseInfo VALID langid= " + _phoneConverter.LanguageId);
			logfile.Log(". . _offset= " + _offset);

			List<OrthographicResult> ars;
			if (!_ruler) ars = _ars_def;
			else         ars = _ars_enh;

			logfile.Log(". wordcount= " + wordcount);
			for (int i = 0; i != wordcount; ++i)
			{
				var ar = new OrthographicResult();

				ISpeechPhraseElement word = Result.PhraseInfo.Elements.Item(i);
				ar.Orthography = word.DisplayText;

				string phons = _phoneConverter.IdToPhone((ushort[])word.Pronunciation);

				logfile.Log(". . . word.AudioTimeOffset= " + word.AudioTimeOffset);

				ar.Phons      = new List<string>(phons.Split(' ')); // remove empty entries ...
				ar.Confidence = word.EngineConfidence;
				ar.Level      = word.ActualConfidence.ToString().Replace("SEC", String.Empty).Replace("Confidence", String.Empty);
				ar.Start      = _offset + (ulong)(word.AudioTimeOffset);
				ar.Stop       = _offset + (ulong)(word.AudioTimeOffset + word.AudioSizeTime);

				ars.Add(ar);
			}

			// NOTE: Recognition could be fired before the entire audiofile has
			// completed, which means it's going to fire again but the AudioTimeOffsets
			// will be completely borked obviously. So add this time-offset to any
			// second or subsequent Recognition event that happens on this stream
			_offset += (ulong)Result.PhraseInfo.AudioSizeTime;
//			}
		}

/*		void GenerateResults(ISpeechRecoResult Result)
		{
			logfile.Log("GenerateResults() _ruler= " + _ruler);
			logfile.Log(". _ars_def.Count= " + _ars_def.Count);
			logfile.Log(". _ars_enh.Count= " + _ars_enh.Count);

			if (Result.PhraseInfo != null)
			{
				int wordcount = Result.PhraseInfo.Rule.NumberOfElements;
				logfile.Log(". . Result.PhraseInfo VALID - wordcount= " + wordcount);

				List<OrthographicResult> ars;
				if (!_ruler) ars = _ars_def;
				else         ars = _ars_enh;

				for (int i = 0; i != wordcount; ++i)
				{
					var ar = new OrthographicResult();

					ISpeechPhraseElement word = Result.PhraseInfo.Elements.Item(i);
					ar.Orthography = word.DisplayText;

					string phons = _phoneConverter.IdToPhone((ushort[])word.Pronunciation);

					ar.Phons = new List<string>(phons.Split(' '));
					ar.Start = (ulong)(word.AudioTimeOffset);						// start of the ortheme/word
					ar.Stop  = (ulong)(word.AudioTimeOffset + word.AudioSizeTime);	// stop  of the ortheme/word

					ars.Add(ar);
				}
			}

			logfile.Log("GenerateResults() DONE");
		} */

		void Sapi_Lipsync_EndStream(int StreamNumber, object StreamPosition, bool StreamReleased)
		{
			logfile.Log();
			logfile.Log("Sapi_Lipsync_EndStream() _ruler= " + _ruler);

			logfile.Log(". set Dictation INACTIVE and close audiostream");
			_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);
			_input.Close();

			Orthography();

			if (!_ruler && _text != String.Empty)
			{
				logfile.Log(". call Generate() w/ ruler");
				Generate(true);
			}
			else
			{
				CalculateWordRatio_def();
				CalculateWordRatio_enh();
				CalculatePhonRatios();

				logfile.Log(". fire SpeechRecognitionEnded");

				if (SpeechRecognitionEnded != null)
					SpeechRecognitionEnded(_ars_def, _ars_enh);
			}
		}

		void Orthography()
		{
			logfile.Log();
			logfile.Log("Orthography() _ruler= " + _ruler);

			List<OrthographicResult> ars;
			if (!_ruler) ars = _ars_def;
			else         ars = _ars_enh;

			OrthographicResult ar;
			ulong stop = 0;

			for (int i = 0; i != ars.Count; ++i)
			{
				ar = ars[i];

				if (ar.Start > stop)
				{
					logfile.Log(". . insert silence");
					var silence = new OrthographicResult();

					silence.Orthography = String.Empty;

					silence.Phons = new List<string>();
					silence.Phons.Add("x");

					silence.Confidence = 1f;
					silence.Level = String.Empty;

					silence.Start = stop;
					silence.Stop  = ar.Start;

					silence.Stops.Add(silence.Stop);

					ars.Insert(i, silence);

					++i;
				}

				logfile.Log(". ar.Orthography= " + ar.Orthography);
				string phons = String.Empty;
				foreach (var phon in ar.Phons)
				{
					if (phons != String.Empty) phons += " ";
					phons += phon;
				}
				logfile.Log(". ar.Phons= " + phons);

				AddStops(ar);
				stop = ar.Stop;
			}
		}

		void AddStops(OrthographicResult ar)
		{
			//logfile.Log("AddStops()");

			var stops = new List<decimal>();

			decimal stop = 0;
			foreach (var phon in ar.Phons)
			{
				switch (phon)
				{
					case "aa": case "ae": case "ah": case "ax": case "ay":
					case  "b": case "eh": case  "l": case  "r": case  "w":
						stops.Add(stop += 50);
						break;

					case "ao": case "aw": case "er": case "ey": case "ow":
					case "oy": case "uh": case "uw":
						stops.Add(stop += 60);
						break;

					default:
						stops.Add(stop += 30);
						break;
				}
			}

			if (stop != 0)
			{
				decimal factor = (decimal)(ar.Stop - ar.Start) / stop;

				ar.Stops = new List<ulong>();
				for (int i = 0; i != stops.Count; ++i)
				{
					decimal dur = stops[i] * factor;
					ar.Stops.Add(ar.Start + (ulong)dur);
				}
			}
		}


		void CalculateWordRatio_def()
		{
			logfile.Log();
			logfile.Log("CalculateWordRatio_def() _ars_def.Count= " + _ars_def.Count);

			string text = TypedText.StripDialogText(_text);
			logfile.Log(". text= " + text);

			var words = new List<string>(text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			if (words.Count != 0)
			{
				var words_def = new List<string>();

				foreach (var ar in _ars_def)
				{
					if (ar.Orthography != String.Empty)
					{
						logfile.Log(". . add " + ar.Orthography);
						words_def.Add(ar.Orthography);
					}
				}

				int count_def = 0;
				foreach (string word in words)
				{
					if (words_def.Contains(word))
					{
						++count_def;
						words_def.Remove(word);
					}
				}

				logfile.Log(". " + count_def + " / " + words.Count);
				RatioWords_def = (double)count_def / words.Count;
			}
		}

		void CalculateWordRatio_enh()
		{
			logfile.Log();
			logfile.Log("CalculateWordRatio()_enh _ars_enh.Count= " + _ars_enh.Count);

			logfile.Log(". _text= " + _text);

			var words = new List<string>(_text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			if (words.Count != 0)
			{
				var words_enh = new List<string>();

				foreach (var ar in _ars_enh)
				{
					if (ar.Orthography != String.Empty)
					{
						logfile.Log(". . add " + ar.Orthography);
						words_enh.Add(ar.Orthography);
					}
				}

				int count_enh = 0;
				foreach (string word in words)
				{
					if (words_enh.Contains(word))
					{
						++count_enh;
						words_enh.Remove(word);
					}
				}

				logfile.Log(". " + count_enh + " / " + words.Count);
				RatioWords_enh = (double)count_enh / words.Count;
			}
		}

		void CalculatePhonRatios()
		{
			logfile.Log();
			logfile.Log("CalculatePhonRatios()");

			if (Expected.Count != 0)
			{
				var phon_def = new List<string>();
				var phon_enh = new List<string>();

				logfile.Log(". phon_def");
				foreach (var ar in _ars_def)
				{
					if (ar.Orthography != String.Empty)
					{
						phon_def.AddRange(ar.Phons);

						string phons = String.Empty;
						foreach (var phon in ar.Phons)
						{
							if (phons != String.Empty) phons += " ";
							phons += phon;
						}
						logfile.Log(". . ar.Phons= " + phons);
					}
				}

				logfile.Log(". phon_enh");
				foreach (var ar in _ars_enh)
				{
					if (ar.Orthography != String.Empty)
					{
						phon_enh.AddRange(ar.Phons);

						string phons = String.Empty;
						foreach (var phon in ar.Phons)
						{
							if (phons != String.Empty) phons += " ";
							phons += phon;
						}
						logfile.Log(". . ar.Phons= " + phons);
					}
				}

				int count_def = 0;
				int count_enh = 0;

				foreach (string phon in Expected)
				{
					if (phon_def.Contains(phon))
					{
						++count_def;
						phon_def.Remove(phon);
					}

					if (phon_enh.Contains(phon))
					{
						++count_enh;
						phon_enh.Remove(phon);
					}
				}
/*				string phon0 = null;
				foreach (string phon in _tts_Expected)
				{
					if (phon0 == null)
					{
						if (phon_def.Count != 0 && phon_def[0] == phon)
							++count_def;

						if (phon_enh.Count != 0 && phon_enh[0] == phon)
							++count_enh;
					}
					else
					{
						for (int i = 1; i != phon_def.Count; ++i)
						{
							if (phon_def[i] == phon && phon_def[i - 1] == phon0)
							{
								++count_def;
								break;
							}
						}

						for (int i = 1; i != phon_enh.Count; ++i)
						{
							if (phon_enh[i] == phon && phon_enh[i - 1] == phon0)
							{
								++count_enh;
								break;
							}
						}
					}
					phon0 = phon;
				} */

				RatioPhons_def = (double)count_def / Expected.Count;
				RatioPhons_enh = (double)count_enh / Expected.Count;
			}
		}
		#endregion lipsync handlers
	}
}
