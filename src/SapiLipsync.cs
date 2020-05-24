using System;
using System.Collections.Generic;

using SpeechLib;


namespace lipsync_editor
{
	internal delegate void TtsStreamEndedEvent();
	internal delegate void SrStreamEndedEvent(List<OrthographicResult> ars_def,
											  List<OrthographicResult> ars_enh);

	/// <summary>
	/// Sapi stuff
	/// </summary>
	public sealed class SapiLipsync
	{
		#region events
		internal TtsStreamEndedEvent TtsStreamEnded;
		internal SrStreamEndedEvent SrStreamEnded;
		#endregion events


		#region enums
		enum Generator
		{
			Dictati, // 1st pass over the audiostream (use dictation grammar)
			Dialogi  // 2nd pass over the audiostream (use dialog-rule derived from any TTS typed-text)
		}
		#endregion enums


		#region fields (static)
		const string RULE = "Text"; // identifier used by the grammar of TTS typed-text
		#endregion fields (static)


		#region fields
		SpInprocRecognizer  _recognizer;		// basic SAPI object
		SpInProcRecoContext _recoContext;		// this does the heavy work here; is tied to the recognizer
		ISpeechRecoGrammar  _recoGrammar;		// the current grammar for '_recoContext'
		SpFileStream        _fs;				// the audiofile stream to analyze
		SpVoice             _voice;				// TTS object for typed-text
		SpPhoneConverter    _phoneConverter;	// object that converts phone-ids (integers) to phonemes (string-literals) and vice versa

		string _text = String.Empty; // the typed-text sanitized

		Generator _generato;

		readonly List<OrthographicResult> _ars_def = new List<OrthographicResult>(); // default
		readonly List<OrthographicResult> _ars_enh = new List<OrthographicResult>(); // enhanced w/ TypedText

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


		/// <summary>
		/// The engine confidence on recognition after a default-type pass (ie.
		/// without typed-text).
		/// @note If user provides typed-text then a comparison will be done
		/// against the typed-text's TTS-phonemes instead.
		/// </summary>
		internal float Confidence_def
		{ get; private set; }

		int Confidence_def_count
		{ get; set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="wavefile">blank string if '!isConsole'</param>
		internal SapiLipsync(string wavefile = "")
		{
#if DEBUG
			logfile.Log();
			logfile.Log("SapiLipsync() cTor wavefile= " + wavefile);

			logfile.Log(". create (SpVoice)_voice");
#endif
			_voice = new SpVoice();
#if DEBUG
			logfile.Log(". (SpVoice)_voice CREATED");
#endif
			_voice.Volume = 0;
			_voice.Rate   = 10;
			_voice.Phoneme   += tts_Phoneme;
			_voice.EndStream += tts_EndStream;

			/*
			https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee125220%28v%3dvs.85%29
			enum SpeechVoiceEvents
			SVEStartInputStream = 2
			SVEEndInputStream   = 4
			SVEVoiceChange      = 8
			SVEBookmark         = 16
			SVEWordBoundary     = 32
			SVEPhoneme          = 64
			SVESentenceBoundary = 128 <-
			SVEViseme           = 256 <--
			SVEAudioLevel       = 512
			SVEPrivate          = 32768
			SVEAllEvents        = 33790
			*/
			_voice.EventInterests = (SpeechVoiceEvents)(int)SpeechVoiceEvents.SVEPhoneme
													 + (int)SpeechVoiceEvents.SVEEndInputStream;
#if DEBUG
			logfile.Log(". _voice.EventInterests= " + _voice.EventInterests);
#endif

#if DEBUG
			logfile.Log(". create (SpPhoneConverter)_phoneConverter");
#endif
			_phoneConverter = new SpPhoneConverter();
#if DEBUG
			logfile.Log(". (SpPhoneConverter)_phoneConverter CREATED");
//			PrintPhons(1036); // test fr-FR
#endif

			if (FxeGeneratorF.isConsole)
			{
				_phoneConverter.LanguageId = 1033; // EnglishUS (default) // TODO: <--
				Audiopath = AudioConverter.deterAudiopath(wavefile);
#if DEBUG
				logfile.Log(". Audiopath= " + Audiopath);
#endif
			}
		}
		#endregion cTor


//		/// <summary>
//		/// Get this. I only have EnglishUS and EnglishGB recognizers for my OS
//		/// (win7 pro) but can output the phonemes of French and perhaps other
//		/// languages.
//		/// PS. My EnglishGB phonemes appear to be utterly borked.
//		/// </summary>
//		/// <param name="langid"></param>
//		void PrintPhons(int langid)
//		{
//			_phoneConverter.LanguageId = langid;
//			for (int i = 0; i != 100; ++i)
//				logfile.Log(i + " - " + _phoneConverter.IdToPhone(i));
//		}


		#region methods
		/// <summary>
		/// Sets the Recognizer and LanguageId when the Recognizers combobox
		/// selection changes.
		/// @note The LanguageId is used by both TTS and SpeechRecognition.
		/// </summary>
		/// <param name="recognizer"></param>
		/// <returns>true if the language-id is set successfully</returns>
		bool SetRecognizer(Recognizer recognizer)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("SetRecognizer()");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
#endif
			_recognizer = new SpInprocRecognizer();
#if DEBUG
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");
#endif
			_recognizer.Recognizer = (SpObjectToken)recognizer.Tok;
#if DEBUG
			logfile.Log(". recognizer.Tok.Id= " + recognizer.Tok.Id);
			logfile.Log(". recognizer.Description= " + recognizer.Tok.GetDescription());

			logfile.Log(". recognizer.Langids= " + recognizer.Langids);
#endif
			string langid = recognizer.Langids;
			int pos = recognizer.Langids.IndexOf(' ');
			if (pos != -1)
				langid = langid.Substring(0, pos); // use 1st langid

			// TODO: ComboBox dropdown for user to choose from if 2+ languages
			// are supported by the current Recognizer.

			int id;
			if (!Int32.TryParse(langid, out id)	// safety - unless the token has "n/a" Languages.
				|| id < 0)						// TODO: check id against valid SAPI language-ids
			{
				if (!FxeGeneratorF.isConsole)
				{
					var d = new InfoDialog("Error", "Did not find a Language in the Recognizer's token.");
					d.ShowDialog(FxeGeneratorF.That);
				}
				return false;
			}

			_phoneConverter.LanguageId = id;
#if DEBUG
			logfile.Log(". _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);
			logfile.Log();
#endif
			StaticData.viceroy(_phoneConverter.LanguageId);

			return true;
		}
			// THESE MAKE ABSOLUTELY NO DIFFERENCE WHATSOEVER TO AUDIOFILE INPUT ->
			// apparently.
			// CFGConfidenceRejectionThreshold
			// HighConfidenceThreshold
			// NormalConfidenceThreshold
			// LowConfidenceThreshold

//			int val = 0;
//			_recognizer.GetPropertyNumber("CFGConfidenceRejectionThreshold", ref val);	// default 60-
//			logfile.Log(". CFGConfidenceRejectionThreshold= " + val);
//			_recognizer.GetPropertyNumber("HighConfidenceThreshold", ref val);			// default 80+
//			logfile.Log(". HighConfidenceThreshold= " + val);
//			_recognizer.GetPropertyNumber("NormalConfidenceThreshold", ref val);		// default 50+
//			logfile.Log(". NormalConfidenceThreshold= " + val);
//			_recognizer.GetPropertyNumber("LowConfidenceThreshold", ref val);			// default 20+
//			logfile.Log(". LowConfidenceThreshold= " + val);
//			logfile.Log();
//
//			_recognizer.SetPropertyNumber("CFGConfidenceRejectionThreshold", 0); // tried 100 ... results are identical to 0.
//			_recognizer.GetPropertyNumber("CFGConfidenceRejectionThreshold", ref val);
//			logfile.Log(". CFGConfidenceRejectionThreshold= " + val);
//
//			_recognizer.SetPropertyNumber("HighConfidenceThreshold", 0);
//			_recognizer.GetPropertyNumber("HighConfidenceThreshold", ref val);
//			logfile.Log(". HighConfidenceThreshold= " + val);
//
//			_recognizer.SetPropertyNumber("NormalConfidenceThreshold", 0);
//			_recognizer.GetPropertyNumber("NormalConfidenceThreshold", ref val);
//			logfile.Log(". NormalConfidenceThreshold= " + val);
//
//			_recognizer.SetPropertyNumber("LowConfidenceThreshold", 0);
//			_recognizer.GetPropertyNumber("LowConfidenceThreshold", ref val);
//			logfile.Log(". LowConfidenceThreshold= " + val);
//			logfile.Log();


		internal void Start(string text)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("Start()");
#endif
			// these don't all need to be cleared ->
			Expected.Clear();

			RatioWords_def =
			RatioPhons_def =
			RatioWords_enh =
			RatioPhons_enh = 0.0;

			_ars_def.Clear();
			_ars_enh.Clear();

			Confidence_def = 0f;
			Confidence_def_count = 0;


			if (SetRecognizer(FxeGeneratorF.That.GetRecognizer())) // <- workaround. See FxeGeneratorF.GetRecognizer()
			{
				_text = text;
#if DEBUG
				logfile.Log(". _text= " + _text);
#endif
				if (_text == String.Empty)
				{
#if DEBUG
					logfile.Log(". . DEFAULT - fire TtsStreamEnded event");
#endif
//					if (TtsStreamEnded != null)
					TtsStreamEnded();
				}
				else
				{
#if DEBUG
					logfile.Log(". . ENHANCED - call SpVoice.Speak()");
					logfile.Log(". . _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);
#endif
					_voice.Speak(_text); // -> fire TtsEndStream when the TTS-stream ends.
					_voice.WaitUntilDone(-1);
				}

#if DEBUG
				logfile.Log(". create (SpInProcRecoContext)_recoContext");
#endif
				_recoContext = (SpInProcRecoContext)_recognizer.CreateRecoContext();
#if DEBUG
				logfile.Log(". (SpInProcRecoContext)_recoContext CREATED");
#endif
				_recoContext.FalseRecognition += rc_FalseRecognition;
				_recoContext.Recognition      += rc_Recognition;
				_recoContext.EndStream        += rc_EndStream;
#if DEBUG
				_recoContext.Hypothesis       += rc_Hypothesis;
				_recoContext.StartStream      += rc_StartStream;
				_recoContext.SoundStart       += rc_SoundStart;
				_recoContext.SoundEnd         += rc_SoundEnd;
				_recoContext.PhraseStart      += rc_PhraseStart;
				_recoContext.Interference     += rc_Interference;

				_recoContext.EventInterests = (SpeechRecoEvents)(int)SpeechRecoEvents.SREFalseRecognition
															  + (int)SpeechRecoEvents.SRERecognition
															  + (int)SpeechRecoEvents.SREStreamEnd
															  + (int)SpeechRecoEvents.SREHypothesis
															  + (int)SpeechRecoEvents.SREStreamStart
															  + (int)SpeechRecoEvents.SRESoundStart
															  + (int)SpeechRecoEvents.SRESoundEnd
															  + (int)SpeechRecoEvents.SREPhraseStart
															  + (int)SpeechRecoEvents.SREInterference;
				logfile.Log(". _recoContext.EventInterests= " + _recoContext.EventInterests);
#else
				_recoContext.EventInterests = (SpeechRecoEvents)(int)SpeechRecoEvents.SREFalseRecognition
															  + (int)SpeechRecoEvents.SRERecognition
															  + (int)SpeechRecoEvents.SREStreamEnd;
#endif
/*				https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee125206%28v%3dvs.85%29
				enum SpeechRecoEvents
				SREStreamEnd            = 1
				SRESoundStart           = 2
				SRESoundEnd             = 4
				SREPhraseStart          = 8
				SRERecognition          = 16
				SREHypothesis           = 32
				SREBookmark             = 64
				SREPropertyNumChange    = 128
				SREPropertyStringChange = 256
				SREFalseRecognition     = 512
				SREInterference         = 1024
				SRERequestUI            = 2048
				SREStateChange          = 4096
				SREAdaptation           = 8192
				SREStreamStart          = 16384
				SRERecoOtherContext     = 32768
				SREAudioLevel           = 65536
				SREPrivate              = 262144
				SREAllEvents            = 393215
*/
				_generato = Generator.Dictati;
				Generate();
#if DEBUG
				logfile.Log("Start() DONE");
				logfile.Log();
#endif
			}
		}

		/// <summary>
		/// Generate() will be called only once if there is no typed-text; it
		/// should use dictation. Generate() will be called a second time if
		/// there is typed-text; the second pass should use both dictation and
		/// context-free-grammar (ie, Command and Control: a Rule that's based
		/// on the typed-text).
		/// </summary>
		void Generate()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("Generate() _generato= " + _generato);
#endif
			_offset = 0uL;
			Confidence_def_count = 0;

			// was "2" but MS doc says not needed on its end.
			// and I don't see grammar id #2 defined on this end either.
			_recoGrammar = _recoContext.CreateGrammar();
//			_recoGrammar.DictationLoad(); // ("Pronunciation") <- causes orthemes to print as phonemes instead of words

			switch (_generato)
			{
				case Generator.Dictati:
					if (_recoGrammar.Rules.FindRule(RULE) != null)
					{
#if DEBUG
						logfile.Log(". set Rule INACTIVE");
#endif
						_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSInactive);
					}
#if DEBUG
					logfile.Log(". set Dictation ACTIVE");
#endif
					_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);
					break;

				case Generator.Dialogi:
#if DEBUG
					logfile.Log(". set Dictation INACTIVE");
#endif
					_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);

					if (_recoGrammar.Rules.FindRule(RULE) == null)
					{
#if DEBUG
						logfile.Log(". . add \"" + RULE + "\" Rule");
#endif
						ISpeechGrammarRule rule = _recoGrammar.Rules.Add(RULE,
																		 SpeechRuleAttributes.SRATopLevel,
																		 1);
						rule.InitialState.AddWordTransition(null,
															_text,
															" ",
															SpeechGrammarWordType.SGLexical,
															RULE,
															1);
						_recoGrammar.Rules.Commit();
					}
#if DEBUG
					logfile.Log(". set Rule ACTIVE");
#endif
					_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSActive);


//					logfile.Log(". max alternates(pre)= " + _recoContext.CmdMaxAlternates);
//					_recoContext.CmdMaxAlternates = 3;
//					logfile.Log(". max alternates(pos)= " + _recoContext.CmdMaxAlternates);
					break;
			}

#if DEBUG
			logfile.Log(". create (SpFileStream)_fs");
#endif
			_fs = new SpFileStream();
#if DEBUG
			logfile.Log(". (SpFileStream)_fs CREATED");
#endif
//			_fs.Format.Type = SpeechAudioFormatType.SAFT44kHz16BitMono;

#if DEBUG
			logfile.Log(". Open Audiopath _fs");
#endif
			_fs.Open(Audiopath);
#if DEBUG
			logfile.Log(". assign _fs to _recognizer.AudioInputStream");
#endif
			_recognizer.AudioInputStream = _fs;
#if DEBUG
			logfile.Log("Generate() DONE");
			logfile.Log();
#endif
		}
		#endregion methods


		#region voice handlers
		/// <summary>
		/// Handles the TTS-stream of a typed-text. Adds its phonemes to the
		/// 'Expected' list.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="Duration"></param>
		/// <param name="NextPhoneId"></param>
		/// <param name="Feature"></param>
		/// <param name="CurrentPhoneId"></param>
		void tts_Phoneme(int StreamNumber,
						 object StreamPosition,
						 int Duration,
						 short NextPhoneId,
						 SpeechVisemeFeature Feature,
						 short CurrentPhoneId)
		{
#if DEBUG
			string ttsinfo = "tts_Phoneme() PhoneId= " + CurrentPhoneId;
#endif
			if (CurrentPhoneId > 9) // NOTE: This causes signifiers like silence #7 "_" and nasalvowel #9 "~" to bypass.
			{
				string phon = _phoneConverter.IdToPhone(CurrentPhoneId);
#if DEBUG
				logfile.Log(ttsinfo + " - " + phon);
				logfile.Log(". ADD to Expected");
#endif
				Expected.Add(phon);
			}
#if DEBUG
			else
			{
				logfile.Log(ttsinfo + " - " + _phoneConverter.IdToPhone(CurrentPhoneId));
				logfile.Log(". BYPASS Expected");
			}
			logfile.Log(". Duration= "    + Duration);
			logfile.Log(". Feature= "     + Feature);
			logfile.Log(". NextPhoneId= " + NextPhoneId);
			logfile.Log(". nextphone= "   + _phoneConverter.IdToPhone(NextPhoneId)); // iffy. Can fail silently.
#endif
		}

		/// <summary>
		/// Handles the TTS-stream of a typed-text finish. Fires the
		/// 'TtsStreamEnded' event in 'FxeGeneratorF' to print the expected-
		/// phonemes.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		void tts_EndStream(int StreamNumber, object StreamPosition)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("tts_EndStream() - fire TtsStreamEnded event");
#endif
//			if (TtsStreamEnded != null)
			TtsStreamEnded();
		}
		#endregion voice handlers


		#region lipsync handlers
#if DEBUG
		/// <summary>
		/// Handles 'SpInProcRecoContext.StartStream' event.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		void rc_StartStream(int StreamNumber, object StreamPosition)
		{
			logfile.Log("rc_StartStream() StreamPosition= " + StreamPosition);
		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.SoundStart' event.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		void rc_SoundStart(int StreamNumber, object StreamPosition)
		{
			logfile.Log("rc_SoundStart() StreamPosition= " + StreamPosition);
		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.SoundEnd' event.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		void rc_SoundEnd(int StreamNumber, object StreamPosition)
		{
			logfile.Log("rc_SoundEnd() StreamPosition= " + StreamPosition);
		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.PhraseStart' event.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		void rc_PhraseStart(int StreamNumber, object StreamPosition)
		{
			logfile.Log("rc_PhraseStart() StreamPosition= " + StreamPosition);
		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.Interference' event.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="Interference"></param>
		void rc_Interference(int StreamNumber, object StreamPosition, SpeechInterference Interference)
		{
			logfile.Log("rc_Interference() StreamPosition= " + StreamPosition + " Interference= " + Interference);
		}


		/// <summary>
		/// Handles 'SpInProcRecoContext.Hypothesis' event. Fires each time the
		/// engine performs a hypothesis.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="Result"></param>
		void rc_Hypothesis(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log("rc_Hypothesis() _generato= " + _generato);
			logfile.Log(". " + Result.PhraseInfo.GetText()); // (0, -1, true)

//			logfile.Log(". Result.PhraseInfo.Rule.Name= "             + Result.PhraseInfo.Rule.Name); // <- blank.
//			logfile.Log(". Result.PhraseInfo.Rule.Id= "               + Result.PhraseInfo.Rule.Id);
//			logfile.Log(". Result.PhraseInfo.Rule.EngineConfidence= " + Result.PhraseInfo.Rule.EngineConfidence);
//			logfile.Log(". Result.PhraseInfo.Rule.Confidence= "       + Result.PhraseInfo.Rule.Confidence);
//
//			logfile.Log(". wordcount= " + Result.PhraseInfo.Elements.Count);
//			foreach (ISpeechPhraseElement word in Result.PhraseInfo.Elements)
//			{
//				logfile.Log(". . word= "              + word.DisplayText);
//				logfile.Log(". . LexicalForm= "       + word.LexicalForm);
//				logfile.Log(". . DisplayAttributes= " + word.DisplayAttributes);
//				logfile.Log(". . EngineConfidence= "  + word.EngineConfidence);
//				logfile.Log(". . ActualConfidence= "  + word.ActualConfidence);
//				var ids = (ushort[])word.Pronunciation;
//				foreach (var id in ids) logfile.Log(". . . PhoneId= " + id + " - " + _phoneConverter.IdToPhone(id));
//			}

//			logfile.Log(". get Alternates");
//			ISpeechPhraseAlternates alts = Result.Alternates(3);	// DOES NOT WORK AS EXPECTED.
//			logfile.Log(". alts.Count= " + alts.Count);				// NOTE: for CC only - SpeechRecoContext.CmdMaxAlternates() def 0
//			logfile.Log(". alt[0]= " + alts.Item(0));				// This fails silently regardless of CmdMaxAlternates value and/or isCC.
//			foreach (ISpeechPhraseAlternate alt in alts)
//				logfile.Log(". . alt= " + alt.PhraseInfo.GetText());
//			logfile.Log(". got Alternates");
		}
#endif

		/// <summary>
		/// Handles 'SpInProcRecoContext.FalseRecognition' event. Fires rarely,
		/// supposedly if engine-confidence-level is too low or perhaps when a
		/// word or phrase is dropped due to interference - but the event is
		/// unfortunately not that consistent in my experience.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="Result"></param>
		void rc_FalseRecognition(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("rc_FalseRecognition() _generato= " + _generato);
#endif
			rc_Recognition(StreamNumber, StreamPosition, SpeechRecognitionType.SRTStandard, Result); // force Recognition.

/*			logfile.Log(". " + Result.PhraseInfo.GetText()); // (0, -1, true)

			logfile.Log(". Result.PhraseInfo.Rule.Name= "             + Result.PhraseInfo.Rule.Name); // <- blank.
			logfile.Log(". Result.PhraseInfo.Rule.Confidence= "       + Result.PhraseInfo.Rule.Confidence);
			logfile.Log(". Result.PhraseInfo.Rule.EngineConfidence= " + Result.PhraseInfo.Rule.EngineConfidence);
			logfile.Log(". Result.PhraseInfo.Rule.Id= "               + Result.PhraseInfo.Rule.Id);

			logfile.Log(". wordcount= " + Result.PhraseInfo.Elements.Count);
			foreach (ISpeechPhraseElement word in Result.PhraseInfo.Elements)
			{
				logfile.Log(". . word= "             + word.DisplayText);
				logfile.Log(". . LexicalForm= "      + word.LexicalForm);
				logfile.Log(". . ActualConfidence= " + word.ActualConfidence);
				logfile.Log(". . EngineConfidence= " + word.EngineConfidence);
				var ids = (ushort[])word.Pronunciation;
				foreach (var id in ids) logfile.Log(". . . PhoneId= " + id + " - " + _phoneConverter.IdToPhone(id));
			} */
		}

//		ulong GetAudioStreamPositionSeconds(string pos)
//		{
//			ulong sec = UInt64.Parse(pos);
//
//			sec /= 2uL;		// bytes per sample (16-bit)
//			sec /= 44100;	// samples per second
//
//			return sec;
//		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.Recognition' event. Fires as the final
		/// hypothesis for a phrase. Each word will be added to a list of
		/// 'OrthographicResult's for the phrase.
		/// WARNING: This can fire 2+ on the same file-stream causing the engine
		/// to drop/reset important variables like 'PhraseInfo.StartTime' and
		/// 'word.AudioStreamOffset' and 'word.AudioTimeOffset'
		/// TODO: a fact that is exceedingly annoying to try to compensate for.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="RecognitionType"></param>
		/// <param name="Result"></param>
		void rc_Recognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("rc_Recognition() _generato= " + _generato);
			logfile.Log(". StreamNumber= " + StreamNumber);
			logfile.Log(". RecognitionType= " + RecognitionType); // <- standard.

			logfile.Log(". _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);

			logfile.Log(". " + Result.PhraseInfo.GetText()); // (0, -1, true)

			logfile.Log(". _offset                       = " + _offset);
			logfile.Log(". StreamPosition                = " + StreamPosition);
			logfile.Log(". PhraseInfo.AudioStreamPosition= " + Result.PhraseInfo.AudioStreamPosition);
//			logfile.Log(". . sec= " + GetAudioStreamPositionSeconds(Result.PhraseInfo.AudioStreamPosition.ToString()));

			logfile.Log(". PhraseInfo.AudioSizeBytes     = " + Result.PhraseInfo.AudioSizeBytes);
			logfile.Log(". PhraseInfo.StartTime          = " + Result.PhraseInfo.StartTime);
			logfile.Log(". PhraseInfo.AudioSizeTime      = " + Result.PhraseInfo.AudioSizeTime);

			logfile.Log(". Result.PhraseInfo.Rule.Name= "             + Result.PhraseInfo.Rule.Name); // <- blank.
			logfile.Log(". Result.PhraseInfo.Rule.Id= "               + Result.PhraseInfo.Rule.Id);
			logfile.Log(". Result.PhraseInfo.Rule.EngineConfidence= " + Result.PhraseInfo.Rule.EngineConfidence);
			logfile.Log(". Result.PhraseInfo.Rule.Confidence= "       + Result.PhraseInfo.Rule.Confidence);

			logfile.Log(". wordcount= " + Result.PhraseInfo.Elements.Count);
#endif

			List<OrthographicResult> ars = null;
			switch (_generato)
			{
				case Generator.Dictati: ars = _ars_def; break;
				case Generator.Dialogi: ars = _ars_enh; break;
			}

			foreach (ISpeechPhraseElement word in Result.PhraseInfo.Elements)
			{
#if DEBUG
				logfile.Log(". . word= "              + word.DisplayText);
				logfile.Log(". . LexicalForm= "       + word.LexicalForm);
				logfile.Log(". . DisplayAttributes= " + word.DisplayAttributes);
				logfile.Log(". . EngineConfidence= "  + word.EngineConfidence);
				logfile.Log(". . ActualConfidence= "  + word.ActualConfidence);
				var ids = (ushort[])word.Pronunciation;
				foreach (var id in ids) logfile.Log(". . . PhoneId= " + id + " - " + _phoneConverter.IdToPhone(id));

				logfile.Log(". . word.AudioStreamOffset= " + word.AudioStreamOffset);
				logfile.Log(". . word.AudioSizeBytes   = " + word.AudioSizeBytes);
				logfile.Log(". . word.AudioTimeOffset  = " + word.AudioTimeOffset);
				logfile.Log(". . word.AudioSizeTime    = " + word.AudioSizeTime);
#endif

				var ar = new OrthographicResult();
				ar.Orthography = word.DisplayText;

				string phons = _phoneConverter.IdToPhone(word.Pronunciation); // NOTE: object is a ushort or ushort[]

				ar.Phons      = new List<string>(phons.Split(' '));
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
			_offset += (ulong)Result.PhraseInfo.AudioSizeTime; // TODO. is not accurate.

			if (_text == String.Empty)
			{
				++Confidence_def_count;
				Confidence_def += Result.PhraseInfo.Rule.EngineConfidence;
			}
#if DEBUG
			logfile.Log();
#endif
		}

		/// <summary>
		/// Handles 'SpInProcRecoContext.EndStream' event. Fires after
		/// Recognition(s) completes itself, closes the filestream, and either
		/// (a) calls Generate() for a 2nd pass or (b) calculates word/phoneme
		/// ratios and fires SrStreamEnded -> FxeGenerator.OnSrStreamEnded()
		/// to print results and generate FXE data/datablocks.
		/// </summary>
		/// <param name="StreamNumber"></param>
		/// <param name="StreamPosition"></param>
		/// <param name="StreamReleased"></param>
		void rc_EndStream(int StreamNumber, object StreamPosition, bool StreamReleased)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("rc_EndStream() _generato= " + _generato);
			//logfile.Log(". StreamReleased= " + StreamReleased);
#endif
			switch (_generato)
			{
				case Generator.Dictati:
#if DEBUG
					logfile.Log(". set Dictation INACTIVE");
#endif
					_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);
					break;

				case Generator.Dialogi:
					if (_recoGrammar.Rules.FindRule(RULE) != null)
					{
#if DEBUG
						logfile.Log(". set Rule INACTIVE");
#endif
						_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSInactive);
					}
					break;
			}
//			_recoGrammar.DictationUnload();
#if DEBUG
			logfile.Log(". close _fs");
#endif
			_fs.Close();
			_fs = null;


			Orthography();

			switch (_generato)
			{
				case Generator.Dictati:
					if (_text != String.Empty)
					{
						_generato = Generator.Dialogi;
						Generate();
					}
					else
					{
						Confidence_def /= (float)Confidence_def_count;
						goto case Generator.Dialogi;
					}
					break;

				case Generator.Dialogi:
					CalculateRatios_word();
					CalculateRatios_phon();

//					if (SrStreamEnded != null)
					SrStreamEnded(_ars_def, _ars_enh);
					break;
			}
		}

		void Orthography()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("Orthography() _generato= " + _generato);
#endif
			List<OrthographicResult> ars = null;
			switch (_generato)
			{
				case Generator.Dictati: ars = _ars_def; break;
				case Generator.Dialogi: ars = _ars_enh; break;
			}

			OrthographicResult ar;
			ulong stop = 0uL;

			for (int i = 0; i != ars.Count; ++i)
			{
				if ((ar = ars[i]).Start > stop)
				{
#if DEBUG
					logfile.Log(". . insert silence");
#endif
					var silence = new OrthographicResult();

					silence.Orthography = String.Empty;

					silence.Phons = new List<string>();
					silence.Phons.Add("x");

					silence.Confidence = 1f;
					silence.Level = String.Empty;

					silence.Start = stop;
					silence.Stop  = ar.Start;

					silence.phStops.Add(silence.Stop);

					ars.Insert(i, silence);

					++i;
				}

#if DEBUG
				logfile.Log(". ar.Orthography= " + ar.Orthography);
				string phons = String.Empty;
				foreach (var phon in ar.Phons)
				{
					if (phons != String.Empty) phons += " ";
					phons += phon;
				}
				logfile.Log(". ar.Phons= " + phons);
#endif

				CalculatePhonStops(ar);

				stop = ar.Stop;
			}
		}

		static void CalculatePhonStops(OrthographicResult ar)
		{
			//logfile.Log("AddStops()");

			var stops = new List<decimal>();

			decimal stop = 0;
			foreach (var phon in ar.Phons)
			{
				switch (phon)
				{
					// curious where 0100010 got these - intuition perhaps.

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

					// TODO: French etc.
				}
			}

			if (stop != 0)
			{
				decimal factor = (decimal)(ar.Stop - ar.Start) / stop;

				for (int i = 0; i != stops.Count; ++i)
				{
					ar.phStops.Add(ar.Start + (ulong)(stops[i] * factor));
				}
			}
		}


		void CalculateRatios_word()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("CalculateRatios_word()");
#endif
			// give the default pass an honest chance to match its words to a typed-text
			string text = TypedText.StripTypedText(_text);
#if DEBUG
			logfile.Log(". _ars_def.Count= " + _ars_def.Count);
			logfile.Log(". text(stripped)= " + text);
#endif
			var words = new List<string>(text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			if (words.Count != 0)
			{
				var words_def = new List<string>();
				foreach (var ar in _ars_def)
				{
					if (ar.Orthography != String.Empty)
					{
#if DEBUG
						logfile.Log(". . add " + ar.Orthography);
#endif
						words_def.Add(ar.Orthography.ToLower());
					}
				}

				int count_def_over = Math.Max(0, words_def.Count - words.Count);
				int count_def = 0;

				foreach (string word in words)
				{
					if (words_def.Contains(word))
					{
						words_def.Remove(word);
						++count_def;
					}
				}
#if DEBUG
				logfile.Log(". (" + count_def + " - " + count_def_over + ") / " + words.Count);
#endif
				RatioWords_def = Math.Max(0.0, (double)(count_def - count_def_over) / words.Count);
			}


#if DEBUG
			logfile.Log(".  _ars_enh.Count= " + _ars_enh.Count);
			logfile.Log(". _text= " + _text);
#endif
			words = new List<string>(_text.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
			if (words.Count != 0)
			{
				var words_enh = new List<string>();
				foreach (var ar in _ars_enh)
				{
					if (ar.Orthography != String.Empty)
					{
#if DEBUG
						logfile.Log(". . add " + ar.Orthography);
#endif
						words_enh.Add(ar.Orthography);
					}
				}

				int count_enh_over = Math.Max(0, words_enh.Count - words.Count);

				int count_enh = 0;
				foreach (string word in words)
				{
					if (words_enh.Contains(word))
					{
						words_enh.Remove(word);
						++count_enh;
					}
				}
#if DEBUG
				logfile.Log(". (" + count_enh + " - " + count_enh_over + ") / " + words.Count);
#endif
				RatioWords_enh = Math.Max(0.0, (double)(count_enh - count_enh_over) / words.Count);
			}
		}

		void CalculateRatios_phon()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("CalculateRatios_phon()");
#endif
			if (Expected.Count != 0)
			{
				var phon_def = new List<string>();
				var phon_enh = new List<string>();
#if DEBUG
				logfile.Log(". phon_def");
				if (_ars_def.Count == 0) logfile.Log(". . _ars_def.Count == 0");

				string phons;
#endif

				foreach (var ar in _ars_def)
				{
					if (ar.Orthography != String.Empty)
					{
						foreach (var phon in ar.Phons)
						{
							if (phon != "~") // IGNORE nasalvowel signifier TODO: All signifiers.
								phon_def.Add(phon);
						}
//						foreach (var phon in ar.Phons)
//						{
//							if ((ushort)_phoneConverter.PhoneToId(phon) > 9) // ignore signifiers
//								phon_def.Add(phon);
//						}
#if DEBUG
						phons = String.Empty;
						foreach (var phon in ar.Phons)
						{
							if (phons != String.Empty) phons += " ";
							phons += phon;
						}
						logfile.Log(". . ar.Phons= " + phons);
#endif
					}
				}
#if DEBUG
				phons = String.Empty;
				foreach (var phon in phon_def)
				{
					if (phons != String.Empty) phons += " ";
					phons += phon;
				}
				logfile.Log(". . phon_def= " + phons);
#endif
#if DEBUG
				logfile.Log(". phon_enh");
				if (_ars_enh.Count == 0) logfile.Log(". . _ars_enh.Count == 0");
#endif
				foreach (var ar in _ars_enh)
				{
					if (ar.Orthography != String.Empty)
					{
						foreach (var phon in ar.Phons)
						{
							if (phon != "~") // IGNORE nasalvowel signifier TODO: All signifiers.
								phon_enh.Add(phon);
						}
//						foreach (var phon in ar.Phons)
//						{
//							if ((ushort)_phoneConverter.PhoneToId(phon) > 9) // ignore signifiers
//								phon_enh.Add(phon);
//						}
#if DEBUG
						phons = String.Empty;
						foreach (var phon in ar.Phons)
						{
							if (phons != String.Empty) phons += " ";
							phons += phon;
						}
						logfile.Log(". . ar.Phons= " + phons);
#endif
					}
				}
#if DEBUG
				phons = String.Empty;
				foreach (var phon in phon_enh)
				{
					if (phons != String.Empty) phons += " ";
					phons += phon;
				}
				logfile.Log(". . phon_enh= " + phons);
#endif

				int count_def_over = Math.Max(0, phon_def.Count - Expected.Count);
				int count_enh_over = Math.Max(0, phon_enh.Count - Expected.Count);

				int count_def = 0;
				int count_enh = 0;

				foreach (string expect in Expected)
				{
					if (phon_def.Contains(expect))
					{
						phon_def.Remove(expect);
						++count_def;
					}

					if (phon_enh.Contains(expect))
					{
						phon_enh.Remove(expect);
						++count_enh;
					}
				}

#if DEBUG
				logfile.Log(". (" + count_def + " - " + count_def_over + ") / " + Expected.Count);
				logfile.Log(". (" + count_enh + " - " + count_enh_over + ") / " + Expected.Count);
#endif
				RatioPhons_def = Math.Max(0.0, (double)(count_def - count_def_over) / Expected.Count);
				RatioPhons_enh = Math.Max(0.0, (double)(count_enh - count_enh_over) / Expected.Count);

//				string phon0 = null;
//				foreach (string phon in Expected)
//				{
//					if (phon0 == null)
//					{
//						if (phon_def.Count != 0 && phon_def[0] == phon)
//							++count_def;
//
//						if (phon_enh.Count != 0 && phon_enh[0] == phon)
//							++count_enh;
//					}
//					else
//					{
//						for (int i = 1; i != phon_def.Count; ++i)
//						{
//							if (phon_def[i] == phon && phon_def[i - 1] == phon0)
//							{
//								++count_def;
//								break;
//							}
//						}
//						for (int i = 1; i != phon_enh.Count; ++i)
//						{
//							if (phon_enh[i] == phon && phon_enh[i - 1] == phon0)
//							{
//								++count_enh;
//								break;
//							}
//						}
//					}
//					phon0 = phon;
//				}
			}
		}
		#endregion lipsync handlers
	}
}
