using System;
using System.Collections.Generic;

using SpeechLib;


namespace lipsync_editor
{
	internal delegate void TtsParseTextEvent();
	internal delegate void SpeechRecognitionEndedEvent(List<AlignmentResult> arListDef,
													   List<AlignmentResult> arListEnh);


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
		SpInprocRecognizer _recognizer;
		SpInProcRecoContext _recoContext;

		SpFileStream _input;
		SpVoice _voice;
		ISpeechRecoGrammar _recoGrammar;
		SpPhoneConverter _phoneConverter;

		string _text = String.Empty;

		readonly List<AlignmentResult> _ars_def = new List<AlignmentResult>(); // default
		readonly List<AlignmentResult> _ars_enh = new List<AlignmentResult>(); // enhanced w/ TypedText

		bool _ruler;
		string _results = String.Empty;
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
			logfile.Log("SapiLipsync() cTor - wavefile= " + wavefile);

			_voice = new SpVoice();
//			_voice.Volume =  100;
//			_voice.Rate   =   -2;
			_voice.Volume =    0;
			_voice.Rate   = 1000;
			_voice.Phoneme   += OnSpeechPhoneme;
			_voice.EndStream += OnSpeechEndStream;

			logfile.Log(". create (SpPhoneConverter)_phoneConverter");
			_phoneConverter = new SpPhoneConverter();
			logfile.Log(". (SpPhoneConverter)_phoneConverter CREATED");

			if (wavefile != String.Empty) // is Console ->
			{
				_phoneConverter.LanguageId = 1033; // US English (default)
				Audiopath = AudioConverter.deterAudiopath(wavefile);
			}
		}
		#endregion cTor


		#region methods
		internal void SetLanguage(int id)
		{
			_phoneConverter.LanguageId = id;
		}


		internal void Start(string text)
		{
			logfile.Log("Start()");

			// kL_clearall -> these don't all need to be cleared
			_ruler = false;

			_results = String.Empty;

			Expected.Clear();

			RatioWords_def =
			RatioPhons_def =
			RatioWords_enh =
			RatioPhons_enh = 0.0;

			_ars_def.Clear();
			_ars_enh.Clear();
			// kL_end.


			_text = text;
			logfile.Log(". _text= " + _text);

			if (_text == String.Empty)
			{
				logfile.Log(". . default - call TtsParseText()");
				if (TtsParseText != null)
					TtsParseText();
			}
			else
			{
				logfile.Log(". . enhanced - call SpVoice.Speak()");

				_voice.Speak(_text); // -> fire TtsParseText when the TTS-stream ends.
//				_voice.WaitUntilDone(-1);
			}


//			_recoContext = new SpInProcRecoContext();

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
			_recognizer = new SpInprocRecognizer();
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");
			_recoContext = (SpInProcRecoContext)_recognizer.CreateRecoContext();
			logfile.Log(". (SpInProcRecoContext)_recoContext CREATED");


//			_input = new SpFileStream();
//			_input.Open(Audiopath, SpeechStreamFileMode.SSFMOpenForRead, true);
//			_recognizer.AudioInputStream = _input;

//			_recoContext.Hypothesis  += Sapi_Lipsync_Hypothesis;
			_recoContext.Recognition += Sapi_Lipsync_Recognition;
			_recoContext.EndStream   += Sapi_Lipsync_EndStream;

			_recoGrammar = _recoContext.CreateGrammar(2);	// was "2" but MS doc says not needed on its end.
			_recoGrammar.DictationLoad();					// and I don't see grammar id #2 defined on this end either.
//			_recoGrammar.DictationLoad("Pronunciation");	// Load pronunciation dictation topic into the grammar so that the raw (unfiltered) phonemes may be retrieved.
//			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);

			Generate(false);

			logfile.Log("Start() DONE");
		}


		void Generate(bool ruler)
		{
			logfile.Log("Generate()");

			_ruler = ruler;
			logfile.Log(". _ruler= " + _ruler);

			// kL_NOTE: How absolutely bizarre. DO NOT SET '_ruler=ruler' in the
			// conditional expression below. It causes an infinite loop ...
			// since '_ruler' will NOT be set true despite 'ruler' being true.
			//
			// And that, friends, took all day to figure out.

			if (_ruler
				&& _text != String.Empty
				&& _recoGrammar.Rules.FindRule(RULE) == null)
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


			logfile.Log(". open audiostream and set Dictation ACTIVE");

			_input = new SpFileStream();
//			_input.Format.Type = SpeechAudioFormatType.SAFT44kHz16BitMono;
			_input.Open(Audiopath, SpeechStreamFileMode.SSFMOpenForRead, true);
			_recognizer.AudioInputStream = _input;

			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);

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
				logfile.Log(". add id - phon= " + phon);

				Expected.Add(phon);
			}
		}

		void OnSpeechEndStream(int StreamNumber, object StreamPosition)
		{
			logfile.Log("OnSpeechEndStream()");

			if (TtsParseText != null)
				TtsParseText();
		}
		#endregion voice handlers


		#region lipsync handlers
/*		void Sapi_Lipsync_Hypothesis(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log("Sapi_Lipsync_Hypothesis() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());

			if (_ruler)
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
			}
		} */

		void Sapi_Lipsync_Recognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
		{
			logfile.Log("Sapi_Lipsync_Recognition() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());

//			GenerateResults(Result); ->
			if (Result.PhraseInfo != null)
			{
				int wordcount = Result.PhraseInfo.Rule.NumberOfElements;
				logfile.Log(". . Result.PhraseInfo VALID - wordcount= " + wordcount + " langid= " + _phoneConverter.LanguageId);

				List<AlignmentResult> ars;
				if (!_ruler) ars = _ars_def;
				else         ars = _ars_enh;

				for (int i = 0; i != wordcount; ++i)
				{
					var ar = new AlignmentResult();

					ISpeechPhraseElement word = Result.PhraseInfo.Elements.Item(i);
					ar.Orthography = word.DisplayText;

					string phons = _phoneConverter.IdToPhone((ushort[])word.Pronunciation);

					ar.Phons = new List<string>(phons.Split(' '));
					ar.Start = (ulong)(word.AudioTimeOffset);						// start of the ortheme/word
					ar.Stop  = (ulong)(word.AudioTimeOffset + word.AudioSizeTime);	// stop  of the ortheme/word

					ars.Add(ar);
				}
			}
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

				List<AlignmentResult> ars;
				if (!_ruler) ars = _ars_def;
				else         ars = _ars_enh;

				for (int i = 0; i != wordcount; ++i)
				{
					var ar = new AlignmentResult();

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
			logfile.Log("Sapi_Lipsync_EndStream() _ruler= " + _ruler);

			logfile.Log(". set Dictation INACTIVE and close audiostream");
			_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);
			_input.Close();

			FinalizeAlignments();

			if (!_ruler)
			{
				logfile.Log(". call Generate() w/ ruler");
				Generate(true);	// TODO: not a good way to initiate 2nd pass w/ ruler.
			}					// The 2nd pass should be bypassed if there is no typed-text.
			else
			{
				logfile.Log(". call calculate ratios");

				CalculateWordRatios();
				CalculatePhonRatios();

				logfile.Log(". fire Recognition");

				if (SpeechRecognitionEnded != null)
					SpeechRecognitionEnded(_ars_def, _ars_enh);
			}
		}

		void FinalizeAlignments()
		{
			logfile.Log("FinalizeAlignments() _ruler= " + _ruler);

			List<AlignmentResult> ars;
			if (!_ruler) ars = _ars_def;
			else         ars = _ars_enh;

			AlignmentResult ar;
			ulong stop = 0;

			for (int i = 0; i < ars.Count; ++i)
			{
				ar = ars[i];

				logfile.Log(". ar.Orthography= " + ar.Orthography);
				string phons = String.Empty;
				foreach (var phon in ar.Phons)
				{
					if (phons != String.Empty) phons += " ";
					phons += phon;
				}
				logfile.Log(". ar.Phons= " + phons);


				if (ar.Start > stop)
				{
					logfile.Log(". . insert silence");
					var silence = new AlignmentResult();

					silence.Start = stop;
					silence.Stop  = ar.Start;

					silence.Phons = new List<string>();
					silence.Phons.Add("x");

					silence.Orthography = String.Empty;

					silence.Stops.Add(silence.Stop);

					ars.Insert(i, silence);

					stop = ar.Start;
					++i;
				}

				TallyStops(ar);
				stop = ar.Stop;
			}
		}

		void TallyStops(AlignmentResult ar)
		{
			//logfile.Log("TallyStops()");

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

		void CalculateWordRatios()
		{
			logfile.Log("CalculateWordRatio()");

			var words = new List<string>(_text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			if (words.Count != 0)
			{
				var words_def = new List<string>();
				var words_enh = new List<string>();

				logfile.Log(". words_def");
				foreach (var ar in _ars_def)
				{
					if (ar.Orthography != String.Empty)
					{
						logfile.Log(". . add " + ar.Orthography);
						words_def.Add(ar.Orthography);
					}
				}

				logfile.Log(". words_enh");
				foreach (var ar in _ars_enh)
				{
					if (ar.Orthography != String.Empty)
					{
						logfile.Log(". . add " + ar.Orthography);
						words_enh.Add(ar.Orthography);
					}
				}

				int count_def = 0;
				int count_enh = 0;

				foreach (string word in words)
				{
					if (words_def.Contains(word))
					{
						++count_def;
						words_def.Remove(word);
					}

					if (words_enh.Contains(word))
					{
						++count_enh;
						words_enh.Remove(word);
					}
				}

				RatioWords_def = (double)count_def / words.Count;
				RatioWords_enh = (double)count_enh / words.Count;
			}
		}

		void CalculatePhonRatios()
		{
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
