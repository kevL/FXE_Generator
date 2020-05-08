﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

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


		#region fields (static)
		const string RULE = "Text";
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
		SpFileStream _fs;
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

			logfile.Log(". create (SpVoice)_voice");
			_voice = new SpVoice();
			logfile.Log(". (SpVoice)_voice CREATED");

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
			_voice.EventInterests = (SpeechVoiceEvents)(int)SpeechVoiceEvents.SVEEndInputStream
													 + (int)SpeechVoiceEvents.SVEPhoneme;
			logfile.Log(". _voice.EventInterests= " + _voice.EventInterests);


			logfile.Log(". create (SpPhoneConverter)_phoneConverter");
			_phoneConverter = new SpPhoneConverter();
			logfile.Log(". (SpPhoneConverter)_phoneConverter CREATED");

//			logfile.Log(". create (SpInprocRecognizer)_recognizer");
//			_recognizer = new SpInprocRecognizer(); // NOTE: This is your SAPI5.4 SpeechRecognizer (aka SpeechRecognitionEngine) interface. good luck!
//			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");

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
			logfile.Log();
			logfile.Log("SetRecognizer()");

			logfile.Log(". create (SpInprocRecognizer)_recognizer");
			_recognizer = new SpInprocRecognizer();
			logfile.Log(". (SpInprocRecognizer)_recognizer CREATED");
			_recognizer.Recognizer = (SpObjectToken)recognizer.Tok;
			logfile.Log(". recognizer.Tok.Id= " + recognizer.Tok.Id);
			logfile.Log(". recognizer.Description= " + recognizer.Tok.GetDescription());

			logfile.Log(". recognizer.Langids= " + recognizer.Langids);
			string langid = recognizer.Langids;
			int pos = recognizer.Langids.IndexOf(' ');
			if (pos != -1)
				langid = langid.Substring(0, pos); // use 1st langid

			// TODO: ComboBox dropdown for user to choose from if 2+ languages
			// are supported by the current Recognizer.

			int id;
			if (!Int32.TryParse(langid, out id))
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

			_phoneConverter.LanguageId = id;// Int32.Parse(langid);
			logfile.Log(". _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);
			logfile.Log();


			// THESE MAKE ABSOLUTELY NO DIFFERENCE WHATSOEVER TO AUDIOFILE INPUT ->
			// apparently.
			// CFGConfidenceRejectionThreshold
			// HighConfidenceThreshold
			// NormalConfidenceThreshold
			// LowConfidenceThreshold

//			int val = 0;
//			_recognizer.GetPropertyNumber("CFGConfidenceRejectionThreshold", ref val);	// default 60
//			logfile.Log(". CFGConfidenceRejectionThreshold= " + val);
//			_recognizer.GetPropertyNumber("HighConfidenceThreshold", ref val);			// default 80
//			logfile.Log(". HighConfidenceThreshold= " + val);
//			_recognizer.GetPropertyNumber("NormalConfidenceThreshold", ref val);		// default 50
//			logfile.Log(". NormalConfidenceThreshold= " + val);
//			_recognizer.GetPropertyNumber("LowConfidenceThreshold", ref val);			// default 20
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


			StaticData.AddVices(_phoneConverter.LanguageId); // TODO: Figure out if different phoneme-sets can actually be implemented.
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
				logfile.Log(". . DEFAULT - fire TtsStreamEnded event");
				if (TtsStreamEnded != null)
					TtsStreamEnded();
			}
			else
			{
				logfile.Log(". . ENHANCED - call SpVoice.Speak()");
				logfile.Log(". . _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);

				_voice.Speak(_text); // -> fire TtsEndStream when the TTS-stream ends.
				_voice.WaitUntilDone(-1);
			}


			logfile.Log(". create (SpInProcRecoContext)_recoContext");
			_recoContext = (SpInProcRecoContext)_recognizer.CreateRecoContext();
			logfile.Log(". (SpInProcRecoContext)_recoContext CREATED");
#if DEBUG
			_recoContext.FalseRecognition += rc_FalseRecognition;
			_recoContext.Hypothesis       += rc_Hypothesis;
#endif
			_recoContext.Recognition += rc_Recognition;
			_recoContext.EndStream   += rc_EndStream;

			// was "2" but MS doc says not needed on its end.
			// and I don't see grammar id #2 defined on this end either.
//			_recoGrammar = _recoContext.CreateGrammar();
//			_recoGrammar.DictationLoad(); //"Pronunciation" <- causes "orthemes expected" to show as phonemes instead of words

/*
			https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee125206%28v%3dvs.85%29
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
//			_recoContext.EventInterests = (SpeechRecoEvents)(int)SpeechRecoEvents.SREStreamEnd
//														  + (int)SpeechRecoEvents.SREPhraseStart
//														  + (int)SpeechRecoEvents.SRERecognition
//														  + (int)SpeechRecoEvents.SREHypothesis
//														  + (int)SpeechRecoEvents.SREFalseRecognition;
			logfile.Log(". _recoContext.EventInterests= " + _recoContext.EventInterests);


			Generate(false);

			logfile.Log("Start() DONE");
			logfile.Log();
		}

		/// <summary>
		/// Generate() will be called only once if there is no typed-text; it
		/// should use dictation. Generate() will be called a second time if
		/// there is typed-text; the second pass should use both dictation and
		/// context-free-grammar (ie, Command and Control: a Rule that's based
		/// on the typed-text).
		/// </summary>
		/// <param name="ruler"></param>
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

			// was "2" but MS doc says not needed on its end.
			// and I don't see grammar id #2 defined on this end either.
			_recoGrammar = _recoContext.CreateGrammar();
//			_recoGrammar.DictationLoad(); //"Pronunciation" <- causes "orthemes" to show as phonemes instead of words

			if (!_ruler)
			{
				if (_recoGrammar.Rules.FindRule(RULE) != null)
				{
					logfile.Log(". set Rule INACTIVE");
					_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSInactive);
				}

				logfile.Log(". set Dictation ACTIVE");
				_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);
			}
			else
			{
				logfile.Log(". set Dictation INACTIVE");
				_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);

				if (_recoGrammar.Rules.FindRule(RULE) == null)
				{
					logfile.Log(". . add \"" + RULE + "\" Rule");

					ISpeechGrammarRule rule = _recoGrammar.Rules.Add(RULE,
																	 SpeechRuleAttributes.SRATopLevel,
//																	(SpeechRuleAttributes)(int)SpeechRuleAttributes.SRATopLevel + (int)SpeechRuleAttributes.SRADynamic,
																	 1);
					rule.InitialState.AddWordTransition(null,
														_text,
														" ",
														SpeechGrammarWordType.SGLexical,
														RULE,
														1);
					_recoGrammar.Rules.Commit();
				}
				logfile.Log(". set Rule ACTIVE");
				_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSActive);
			}
//			logfile.Log(". set Dictation ACTIVE");
//			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);


//			if (_input != null)
//				_input.Close();

			logfile.Log(". create (SpFileStream)_input");
			_fs = new SpFileStream();
			logfile.Log(". (SpFileStream)_input CREATED");

//			_input.Format.Type = SpeechAudioFormatType.SAFT44kHz16BitMono;

			logfile.Log(". Open Audiopath _input for fs");
			_fs.Open(Audiopath, SpeechStreamFileMode.SSFMOpenForRead, true);

//			if (_recognizer.AudioInputStream != null)
//				_recognizer.AudioInputStream.Seek(0);

			logfile.Log(". assign _input fs to _recognizer.AudioInputStream");
			_recognizer.AudioInputStream = _fs;

			logfile.Log("Generate() DONE");
			logfile.Log();
		}
		#endregion methods


		#region voice handlers
		void tts_Phoneme(int StreamNumber,
						 object StreamPosition,
						 int Duration,
						 short NextPhoneId,
						 SpeechVisemeFeature Feature,
						 short CurrentPhoneId)
		{
			string ttsinfo = "tts_Phoneme() PhoneId= " + CurrentPhoneId;
			if (CurrentPhoneId > 9)
			{
				string phon = _phoneConverter.IdToPhone(CurrentPhoneId);
				ttsinfo += " phon= " + phon;

				Expected.Add(phon);
			}
			logfile.Log(ttsinfo);
		}

		void tts_EndStream(int StreamNumber, object StreamPosition)
		{
			logfile.Log();
			logfile.Log("tts_EndStream() - fire TtsStreamEnded event");

			if (TtsStreamEnded != null)
				TtsStreamEnded();
		}
		#endregion voice handlers


		#region lipsync handlers
#if DEBUG
		void rc_Hypothesis(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log("rc_Hypothesis() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());
//			logfile.Log(". " + Result.PhraseInfo.GetText(0, -1, true));

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
				foreach (var id in ids) logfile.Log(". . . id= " + id + " - " + _phoneConverter.IdToPhone(id));
			}

//			logfile.Log(". get Alternates");
//			ISpeechPhraseAlternates alts = Result.Alternates(3); // DOES NOT WORK AS EXPECTED.
//			logfile.Log(". alts.Count= " + alts.Count);
//			logfile.Log(". alt[0]= " + alts.Item(0));
//			foreach (ISpeechPhraseAlternate alt in alts)
//				logfile.Log(". . alt= " + alt.PhraseInfo.GetText());
//			logfile.Log(". got Alternates");

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

		void rc_FalseRecognition(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log();
			logfile.Log("rc_FalseRecognition() _ruler= " + _ruler);
			logfile.Log(". " + Result.PhraseInfo.GetText());

			logfile.Log(". " + Result.PhraseInfo.GetText());
//			logfile.Log(". " + Result.PhraseInfo.GetText(0, -1, true));

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
				foreach (var id in ids) logfile.Log(". . . id= " + id + " - " + _phoneConverter.IdToPhone(id));
			}
		}
#endif

		void rc_Recognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
		{
			logfile.Log();
			logfile.Log("rc_Recognition() _ruler= " + _ruler);

			logfile.Log(". _phoneConverter.LanguageId= " + _phoneConverter.LanguageId);

//			GenerateResults(Result); ->
			//logfile.Log(". Result.PhraseInfo VALID");
			//logfile.Log(". RecognitionType= " + RecognitionType); // <- standard.

			logfile.Log(". " + Result.PhraseInfo.GetText());
//			logfile.Log(". " + Result.PhraseInfo.GetText(0, -1, true));

			logfile.Log(". _offset= " + _offset);
			logfile.Log(". PhraseInfo.AudioSizeTime= " + Result.PhraseInfo.AudioSizeTime);

			logfile.Log(". Result.PhraseInfo.Rule.Name= "             + Result.PhraseInfo.Rule.Name); // <- blank.
			logfile.Log(". Result.PhraseInfo.Rule.Confidence= "       + Result.PhraseInfo.Rule.Confidence);
			logfile.Log(". Result.PhraseInfo.Rule.EngineConfidence= " + Result.PhraseInfo.Rule.EngineConfidence);
			logfile.Log(". Result.PhraseInfo.Rule.Id= "               + Result.PhraseInfo.Rule.Id);

			logfile.Log(". wordcount= " + Result.PhraseInfo.Elements.Count);


			List<OrthographicResult> ars;
			if (!_ruler) ars = _ars_def;
			else         ars = _ars_enh;

			foreach (ISpeechPhraseElement word in Result.PhraseInfo.Elements)
			{
				logfile.Log(". . word= "             + word.DisplayText);
				logfile.Log(". . LexicalForm= "      + word.LexicalForm);
				logfile.Log(". . ActualConfidence= " + word.ActualConfidence);
				logfile.Log(". . EngineConfidence= " + word.EngineConfidence);
				var ids = (ushort[])word.Pronunciation;
				foreach (var id in ids) logfile.Log(". . . id= " + id + " - " + _phoneConverter.IdToPhone(id));


				var ar = new OrthographicResult();
				ar.Orthography = word.DisplayText;

				string phons = _phoneConverter.IdToPhone(word.Pronunciation); // NOTE: object is a ushort[]

				//logfile.Log(". . . word.AudioTimeOffset= " + word.AudioTimeOffset);
				//logfile.Log(". . . word.AudioSizeTime  = " + word.AudioSizeTime);

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
			logfile.Log();
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

		void rc_EndStream(int StreamNumber, object StreamPosition, bool StreamReleased)
		{
			logfile.Log();
			logfile.Log("rc_EndStream() _ruler= " + _ruler);

			logfile.Log(". set Dictation INACTIVE");
			_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);

			if (_recoGrammar.Rules.FindRule(RULE) != null)
			{
				logfile.Log(". set Rule INACTIVE");
				_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSInactive);
			}

//			_recoGrammar.DictationUnload();

			logfile.Log(". close _input");
			_fs.Close();


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

				logfile.Log(". fire SrStreamEnded event");

				if (SrStreamEnded != null)
					SrStreamEnded(_ars_def, _ars_enh);
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

			// give the default pass an honest chance to match its phonemes to any expected phonemes
			string text = TypedText.StripTypedText(_text);
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
				if (_ars_def.Count == 0) logfile.Log(". . _ars_def.Count == 0");

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
				if (_ars_enh.Count == 0) logfile.Log(". . _ars_enh.Count == 0");

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
