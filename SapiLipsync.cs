using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using System.Reflection;
using System.Text;
using System.Windows.Forms;

using SpeechLib;


namespace lipsync_editor
{
	internal delegate void TtsParseTextEvent();
	internal delegate void RecognitionEvent(List<AlignmentResult> arListDef, List<AlignmentResult> arListEnh);


	/// <summary>
	/// Sapi stuff
	/// </summary>
	public sealed class SapiLipsync
	{
		#region events
		internal TtsParseTextEvent TtsParseText;
		internal RecognitionEvent Recognition;
		#endregion events


		#region fields (static)
		const string EXT_MP3 = ".mp3";
		const string EXT_WAV = ".wav";
		const string TMP_MP3 = "sapi_lipsync" + EXT_MP3;
		const string TMP_WAV = "sapi_lipsync" + EXT_WAV;

		const string LAME_EXE = "lame.exe";

		const string RULE = "TextLipsync";
		#endregion fields (static)


		#region fields
		SpFileStream _input;
		SpVoice _voice;
		ISpeechRecoContext _recoContext;
		ISpeechRecoGrammar _recoGrammar;
		SpPhoneConverter _phoneConverter = new SpPhoneConverter();
//		SpAudioFormat _pWaveFmt;

				 List<AlignmentResult> _ars_def = new List<AlignmentResult>(); // default
		readonly List<AlignmentResult> _ars_enh = new List<AlignmentResult>(); // enhanced w/ TypedText

		bool _ruler;
		string _results = String.Empty;

		internal List<string> _tts_Expected = new List<string>();
		#endregion fields


		#region properties
		internal string Fullpath
		{ get; private set; }

		string TypedText
		{ get; set; }


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
		internal SapiLipsync(string wavefile)
		{
			//logfile.Log("SapiLipsync() cTor - wavefile= " + wavefile);

			_voice = new SpVoice();
			_voice.Volume = 0;
			_voice.Rate = 1000;
			_voice.Phoneme   += voice_OnPhoneme;
			_voice.EndStream += voice_OnEndStream;

			_phoneConverter.LanguageId = 1033; // US English

			ConvertMp3toWav(wavefile);
		}
		#endregion cTor


		#region methods
		/// <summary>
		/// TODO: Fix this. Check for valid audio format, handle some errors,
		/// etc.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		void ConvertMp3toWav(string file)
		{
			logfile.Log("ConvertMp3toWav() file= " + file);

			Fullpath = String.Empty;

			string pathT = Path.GetTempPath();
			//logfile.Log(". path= " + pathT);

			if (file.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // prep .BMU ->
			{
				var fi = new FileInfo(file);
				var br = new BinaryReader(fi.OpenRead());

				char[] c = br.ReadChars(3);
				br.Close();

				if (   c[0] == 'B' // because .BMUs are .MP3s and NwN2 labels them as .WAVs
					&& c[1] == 'M'
					&& c[2] == 'U')
				{
					file = Path.Combine(pathT, TMP_MP3); // so label it as .MP3 and allow the next block to catch it.
					//logfile.Log(". file= " + file);

					fi.CopyTo(file, true);
				}
			}

			if (file.EndsWith(EXT_MP3, StringComparison.InvariantCultureIgnoreCase)) // convert to .WAV file ->
			{
				string waveT = Path.Combine(pathT, TMP_WAV);
				//logfile.Log(". wave= " + waveT);


				if (File.Exists(waveT))
					File.Delete(waveT);

//				string execpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//				var info = new ProcessStartInfo(Path.Combine(execpath, LAME_EXE));
				var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
				info.Arguments = "--decode \"" + file + "\" \"" + waveT + "\"";
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.UseShellExecute = false;
				info.CreateNoWindow  = true;

				using (Process proc = Process.Start(info))
				{
					proc.WaitForExit();
				}

//				string t = Path.Combine(pathT, TMP_MP3);
//				logfile.Log(". t= " + t);
//				if (File.Exists(t))
//					File.Delete(t);

				file = waveT;
			}

			// http://www.topherlee.com/software/pcm-tut-wavformat.html
//			 1- 4	"RIFF"				Marks the file as a riff file. Characters are each 1 byte long.
//			 5- 8	File size (integer)	Size of the overall file - 8 bytes, in bytes (32-bit integer). Typically, you'd fill this in after creation.
//			 9-12	"WAVE"				File Type Header. For our purposes, it always equals "WAVE".
//			13-16	"fmt "				Format chunk marker. Includes trailing null
//			17-20	16					Length of format data as listed above
//			21-22	1					Type of format (1 is PCM) - 2 byte integer
//			23-24	2					Number of Channels - 2 byte integer
//			25-28	44100				Sample Rate - 32 byte integer. Common values are 44100 (CD), 48000 (DAT). Sample Rate = Number of Samples per second, or Hertz.
//			29-32	176400				(Sample Rate * BitsPerSample * Channels) / 8.
//			33-34	4					(BitsPerSample * Channels) / 8.1 - 8 bit mono2 - 8 bit stereo/16 bit mono4 - 16 bit stereo
//			35-36	16					Bits per sample
//			37-40	"data"				"data" chunk header. Marks the beginning of the data section.
//			41-44	File size (data)	Size of the data section.

			if (file.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // check .WAV ->
			{
				var fi = new FileInfo(file);
				var br = new BinaryReader(fi.OpenRead());

				char[] c = br.ReadChars(16);					// start 0

				if (   c[ 0] == 'R' && c[ 1] == 'I' && c[ 2] == 'F' && c[ 3] == 'F'
					&& c[ 8] == 'W' && c[ 9] == 'A' && c[10] == 'V' && c[11] == 'E'
					&& c[12] == 'f' && c[13] == 'm' && c[14] == 't' && c[15] == ' ')
				{
					br.ReadBytes(4);							// start 16

					short format = br.ReadInt16();				// start 20: is PCM
					//logfile.Log(". format= " + format);
					if (format == 1)
					{
						short channels = br.ReadInt16();		// start 22: is Mono
						//logfile.Log(". channels= " + channels);
						if (channels == 1)
						{
							int rate = br.ReadInt32();			// start 24: is 44.1kHz
							//logfile.Log(". rate= " + rate);
							if (rate == 44100)
							{
								br.ReadBytes(6);				// start 28
								short bits = br.ReadInt16();	// start 34: is 16-bit
								//logfile.Log(". bits= " + bits);
								if (bits == 16)
								{
									Fullpath = file;
									logfile.Log(". Fullpath= " + file);
								}
							}
						}
					}
				}
				br.Close();
			}

			if (!FxeGeneratorF.isConsole && Fullpath == String.Empty)
			{
				MessageBox.Show(" Failed to convert to 44.1kHz 16-bit Mono PCM-wave format.",
								" Conversion failed",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1);
			}
		}


		internal void Start(string text)
		{
			logfile.Log("Start() text= " + text);

			// kL_clearall -> these don't all need to be cleared
			_ruler = false;

			_results = String.Empty;

			_tts_Expected.Clear();

			RatioWords_def =
			RatioPhons_def =
			RatioWords_enh =
			RatioPhons_enh = 0.0;

			_ars_def.Clear();
			_ars_enh.Clear();
			// kL_end.


			TypedText = SapiLipsync.ParseText(text.Trim());
			logfile.Log(". TypedText= " + TypedText);

			if (TypedText != String.Empty)
			{
				logfile.Log(". enhanced - call _voice.Speak()");
//				_tts_PhonIds.Clear();
//				_ars_enh    .Clear();

				_voice.Speak(TypedText);
				_voice.WaitUntilDone(-1);
			}
			else if (TtsParseText != null)
			{
				logfile.Log(". default - call TtsParseText()");
				TtsParseText();
			}

			_recoContext = new SpInProcRecoContext();

			((SpInProcRecoContext)_recoContext).Recognition += Sapi_Lipsync_Recognition;
			((SpInProcRecoContext)_recoContext).Hypothesis  += Sapi_Lipsync_Hypothesis;
			((SpInProcRecoContext)_recoContext).EndStream   += Sapi_Lipsync_EndStream;

			_recoGrammar = _recoContext.CreateGrammar(2);
			_recoGrammar.DictationLoad();

			Generate(false);

			logfile.Log("Start() DONE");
		}


		void Generate(bool ruler)
		{
			logfile.Log("Generate() ruler= " + ruler);

			_ruler = ruler;
			logfile.Log(". _ruler= " + _ruler);

			// kL_NOTE: How absolutely bizarre. DO NOT SET '_ruler=ruler' in the
			// conditional expression below. It causes an infinite loop ...
			// since '_ruler' will NOT be set true despite 'ruler' being true.
			//
			// And that, friends, took all day to figure out.

			if (_ruler
				&& TypedText != String.Empty
				&& _recoGrammar.Rules.FindRule(RULE) == null)
			{
				logfile.Log(". . add TextLipsync rule");
				ISpeechGrammarRule rule = _recoGrammar.Rules.Add(RULE,
																 SpeechRuleAttributes.SRATopLevel
															   | SpeechRuleAttributes.SRADynamic,
																 1);

/*				object PropertyValue = String.Empty;
				rule.InitialState.AddWordTransition(null,
													TypedText,
													" ",
													SpeechGrammarWordType.SGLexical,
													"TextLipSync",						// <- typo looks like
													1,
													ref PropertyValue,
													1f); */
				rule.InitialState.AddWordTransition(null,
													TypedText,
													" ",
													SpeechGrammarWordType.SGLexical,
													RULE,
													1);

				_recoGrammar.Rules.Commit();
				_recoGrammar.CmdSetRuleState(RULE, SpeechRuleState.SGDSActive);
			}

			_input = new SpFileStreamClass();
			_input.Open(Fullpath);
			_recoContext.Recognizer.AudioInputStream = _input;

//			_pWaveFmt = _InputWAV.Format;

			_recoGrammar.DictationSetState(SpeechRuleState.SGDSActive);

			logfile.Log("Generate() DONE");
		}
		#endregion methods


		#region voice handlers
		void voice_OnPhoneme(int StreamNumber,
							 object StreamPosition,
							 int Duration,
							 short NextPhoneId,
							 SpeechVisemeFeature Feature,
							 short CurrentPhoneId)
		{
			logfile.Log("voice_OnPhoneme() CurrentPhoneId= " + CurrentPhoneId);

			if (CurrentPhoneId > 9)
			{
				string phon = _phoneConverter.IdToPhone(CurrentPhoneId);
				logfile.Log(". add id - phon= " + phon);

				_tts_Expected.Add(phon);
			}
		}

		void voice_OnEndStream(int StreamNumber, object StreamPosition)
		{
			logfile.Log("voice_OnEndStream()");

			if (TtsParseText != null)
				TtsParseText();
		}
		#endregion voice handlers


		#region lipsync handlers
		void Sapi_Lipsync_Hypothesis(int StreamNumber, object StreamPosition, ISpeechRecoResult Result)
		{
			logfile.Log("Sapi_Lipsync_Hypothesis() _ruler= " + _ruler);

			if (_ruler)
			{
//				string results = Result.PhraseInfo.GetText(0, -1, true);
				string results = Result.PhraseInfo.GetText();
				if (results.Length > _results.Length)
				{
					logfile.Log(". replace _results");

//					_ars_enh.Clear();
					GenerateResults(Result);
					_results = results;
				}
			}
		}

		void Sapi_Lipsync_Recognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
		{
			logfile.Log("Sapi_Lipsync_Recognition() _ruler= " + _ruler);

			if (_ruler)// && _ars_enh.Count != 0)
				_ars_enh.Clear();

			GenerateResults(Result);
		}

		void GenerateResults(ISpeechRecoResult Result)
		{
			logfile.Log("GenerateResults() _ars_enh.Count= " + _ars_enh.Count);

			if (Result.PhraseInfo != null)
			{
				int wordcount = Result.PhraseInfo.Rule.NumberOfElements;
				logfile.Log(". Result.PhraseInfo VALID - wordcount= " + wordcount);

				for (int i = 0; i != wordcount; ++i)
				{
					var ar = new AlignmentResult();

					ISpeechPhraseElement word = Result.PhraseInfo.Elements.Item(i);
					ar.Orthography = word.DisplayText;

					string phons = _phoneConverter.IdToPhone((ushort[])word.Pronunciation);

					ar.Phons = new List<string>(phons.Split(' '));
					ar.Start = (ulong)(word.AudioTimeOffset);						// starttime of the ortheme
					ar.Stop  = (ulong)(word.AudioTimeOffset + word.AudioSizeTime);	// stop time of the ortheme

					_ars_enh.Add(ar);
				}
			}

			logfile.Log("GenerateResults() DONE");
		}

		void Sapi_Lipsync_EndStream(int StreamNumber, object StreamPosition, bool StreamReleased)
		{
			logfile.Log("Sapi_Lipsync_EndStream() _ruler= " + _ruler);

			_recoGrammar.DictationSetState(SpeechRuleState.SGDSInactive);
			_input.Close();

			FinalizeAlignments(); // NOTE: This is only for ars_enh.

			if (!_ruler)
			{
				_ars_def = new List<AlignmentResult>(_ars_enh);

				logfile.Log(". call Generate() w/ ruler");
				Generate(true);
			}
			else
			{
				logfile.Log(". call calculate ratios");

				CalculateWordRatio();
				CalculatePhonRatio();

				if (Recognition != null)
					Recognition(_ars_def, _ars_enh);
			}
		}

		void FinalizeAlignments()
		{
			logfile.Log("FinalizeAlignments()");

			AlignmentResult ar;
			ulong stop = 0;

			for (int i = 0; i < _ars_enh.Count; ++i)
			{
				ar = _ars_enh[i];

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

					_ars_enh.Insert(i, silence);

					stop = ar.Start;
					++i;
				}

				TabulateStops(ar);
				stop = ar.Stop;
			}
		}

		void TabulateStops(AlignmentResult ar)
		{
			//logfile.Log("TabulateStops()");

			var stops = new List<decimal>();
			decimal stop = 0;
			foreach (var phon in ar.Phons)
			{
				switch (phon)
				{
					case "aa":
					case "ae":
					case "ah":
					case "ax":
					case "ay":
					case  "b":
					case "eh":
					case  "l":
					case  "r":
					case  "w":
						stops.Add(stop += 50);
						break;

					case "ao":
					case "aw":
					case "er":
					case "ey":
					case "ow":
					case "oy":
					case "uh":
					case "uw":
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
					decimal dur = (decimal)stops[i] * factor;
					ar.Stops.Add(ar.Start + (ulong)dur);
				}
			}
		}

		void CalculateWordRatio()
		{
			logfile.Log("CalculateWordRatio()");

			var words = new List<string>(TypedText.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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

		void CalculatePhonRatio()
		{
			if (_tts_Expected.Count != 0)
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

				foreach (string phon in _tts_Expected)
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

				RatioPhons_def = (double)count_def / _tts_Expected.Count;
				RatioPhons_enh = (double)count_enh / _tts_Expected.Count;
			}
		}
		#endregion lipsync handlers


		internal static string ParseText(string text)
		{
			//logfile.Log("ParseText() text= " + text);

			text = Spaceout(text);

			text = RemoveComment('<', '>', text);
			text = RemoveComment('{', '}', text);
			text = RemoveComment('[', ']', text);
			text = RemoveComment('|', '|', text);

			text = text.Replace("\t", " ");

			for (int i = 0; i != text.Length; ++i)
			{
				if (   !char.IsLetter(text[i])
					&& !char.IsNumber(text[i])
					&& text[i] != ' '
					&& text[i] != '\'')
				{
					text = text.Replace(text[i], ' ');
				}
			}
			return Onespace(text).ToLower().Trim();
		}

		/// <summary>
		/// No tabs, no newlines, no funny stuff - just space.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static string Spaceout(string text)
		{
			var sb = new StringBuilder(text.Length);
			for (int i = 0; i != text.Length; ++i)
			{
				char c = text[i];
				if (char.IsWhiteSpace(c))
					sb.Append(' ');
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static string Onespace(string text)
		{
			var sb = new StringBuilder(text.Length);
			for (int i = 0; i != text.Length; ++i)
			{
				char c = text[i];
				if (c != ' ' || i == 0 || text[i - 1] != ' ')
					sb.Append(c);
			}
			return sb.ToString();
		}

		/// <summary>
		/// helper for ParseText()
		/// </summary>
		/// <param name="start"></param>
		/// <param name="stop"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		static string RemoveComment(char start, char stop, string text)
		{
			int i,j;
			while ((i = text.IndexOf(start)) != -1)
			{
				if ((j = text.IndexOf(stop, i + 1)) != -1)
				{
					text = text.Remove(i, j - i + 1);
					text = text.Insert(i, " ");
				}
			}
			return text;
		}
	}
}
