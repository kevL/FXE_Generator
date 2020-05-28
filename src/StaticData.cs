using System;
using System.Collections.Generic;


namespace lipsync_editor
{
	static class StaticData
	{
		#region fields (static)
		internal const float EPSILON   = 0.000005f;
		internal const float STOP_INCR = 0.0000001f;
		#endregion fields (static)


		#region properties (static)
		static readonly Dictionary<string, string> _vices =
					new Dictionary<string, string>();

		/// <summary>
		/// A dictionary of viseme-strings keyed by phoneme-strings.
		/// </summary>
		internal static Dictionary<string, string> Vices
		{
			get { return _vices; }
		}
		#endregion properties (static)


		#region methods (static)
		internal static decimal millisecs(ulong nanoseconds_100)
		{
			return ((decimal)nanoseconds_100 / 10000000);
		}

		internal static string GetFilelabel(string file)
		{
			int pos = file.LastIndexOf('\\') + 1;
			if (pos != 0)
				file = file.Substring(pos, file.Length - pos);

			if ((pos = file.LastIndexOf('.')) != -1)
				file = file.Substring(0, pos);

			return file;
		}


		/// <summary>
		/// Fills the 'Vices' dictionary that maps phoneme-strings to
		/// viseme-strings.
		/// </summary>
		/// <param name="langid">the SpPhoneConverter's current language</param>
		internal static void viceroy(int langid)
		{
#if DEBUG
			logfile.Log("StaticData.viceroy() langid= " + langid);
#endif
			Vices.Clear();

			Vices.Add( "x", String.Empty); // silence

			switch (langid)
			{
				default:
//				case 1033: // This is for English US (en-US)
/*					 Output of PhoneConverter.IdToPhon() ->
					 0 - 
					 1 - -
					 2 - !
					 3 - &
					 4 - ,
					 5 - .
					 6 - ?
					 7 - _
					 8 - 1
					 9 - 2
					10 - aa
					11 - ae
					12 - ah
					13 - ao
					14 - aw
					15 - ax
					16 - ay
					17 - b
					18 - ch
					19 - d
					20 - dh
					21 - eh
					22 - er
					23 - ey
					24 - f
					25 - g
					26 - h
					27 - ih
					28 - iy
					29 - jh
					30 - k
					31 - l
					32 - m
					33 - n
					34 - ng
					35 - ow
					36 - oy
					37 - p
					38 - r
					39 - s
					40 - sh
					41 - t
					42 - th
					43 - uh
					44 - uw
					45 - v
					46 - w
					47 - y
					48 - z
					49 - zh */

					Vices.Add("aa", "Ox");
					Vices.Add("ae", "If");
					Vices.Add("ah", "If");
					Vices.Add("ao", "Ox");
					Vices.Add("aw", "If");
					Vices.Add("ax", "If");
					Vices.Add("ay", "If");
					Vices.Add( "b", "Bump");
					Vices.Add("ch", "Church");
					Vices.Add( "d", "Told");
					Vices.Add("dh", "Though");
					Vices.Add("eh", "If");
					Vices.Add("er", "Earth");
					Vices.Add("ey", "If");
					Vices.Add( "f", "Fave");
					Vices.Add( "g", "Cage");
					Vices.Add( "h", "If");
					Vices.Add("ih", "If");
					Vices.Add("iy", "Eat");
					Vices.Add("jh", "Church");
					Vices.Add( "k", "Cage");
					Vices.Add( "l", "Told");
					Vices.Add( "m", "Bump");
					Vices.Add( "n", "New");
					Vices.Add("ng", "New");
					Vices.Add("ow", "Oat");
					Vices.Add("oy", "Oat");
					Vices.Add( "p", "Bump");
					Vices.Add( "r", "Roar");
					Vices.Add( "s", "Size");
					Vices.Add("sh", "Church");
					Vices.Add( "t", "Told");
					Vices.Add("th", "Though");
					Vices.Add("uh", "Oat");
					Vices.Add("uw", "Oat");
					Vices.Add( "v", "Fave");
					Vices.Add( "w", "Wet");
					Vices.Add( "y", "Wet");
					Vices.Add( "z", "Size");
					Vices.Add("zh", "Church");
					break;

//				case 2057: // This is for English GB (en-GB)
/*					 Output of PhoneConverter.IdToPhon() ->
					 0 -
					 1 - _!
					 2 - _&
					 3 - _,
					 4 - _s */
					 // wtf is that

//					break;

				case 1036: // This is for French (fr-FR)
/*					 Output of PhoneConverter.IdToPhon() ->
					 0 - 
					 1 - -
					 2 - !
					 3 - &
					 4 - ,
					 5 - .
					 6 - ?
					 7 - _
					 8 - 1
					 9 - ~
					10 - aa
					11 - a
					12 - oh
					13 - ax
					14 - b
					15 - d
					16 - eh
					17 - ey
					18 - f
					19 - g
					20 - hy
					21 - uy
					22 - iy
					23 - k
					24 - l
					25 - m
					26 - n
					27 - ng
					28 - nj
					29 - oe
					30 - eu
					31 - ow
					32 - p
					33 - r
					34 - s
					35 - sh
					36 - t
					37 - uw
					38 - v
					39 - w
					40 - y
					41 - z
					42 - zh */

					Vices.Add( "~", String.Empty); // nasalvowel signifier

					// kL_note: I based these on the English transcriptions
					// insofar as is possible but i don't speak French and don't
					// even have a fr-FR SpeechRecognizer for my OS (win7 pro).
					Vices.Add("aa", "Ox");
					Vices.Add( "a", "Ox");		// <- not in English phons
					Vices.Add("oh", "Oat");		// <- not in English phons
					Vices.Add("ax", "If");
					Vices.Add( "b", "Bump");
					Vices.Add( "d", "Told");
					Vices.Add("eh", "If");
					Vices.Add("ey", "If");
					Vices.Add( "f", "Fave");
					Vices.Add( "g", "Cage");
					Vices.Add("hy", "Oat");		// <- not in English phons
					Vices.Add("uy", "Oat");		// <- not in English phons
					Vices.Add("iy", "Eat");
					Vices.Add( "k", "Cage");
					Vices.Add( "l", "Told");
					Vices.Add( "m", "Bump");
					Vices.Add( "n", "New");
					Vices.Add("ng", "New");
					Vices.Add("nj", "New");		// <- not in English phons
					Vices.Add("oe", "Oat");		// <- not in English phons
					Vices.Add("eu", "Earth");	// <- not in English phons
					Vices.Add("ow", "Oat");
					Vices.Add( "p", "Bump");
					Vices.Add( "r", "Roar");
					Vices.Add( "s", "Size");
					Vices.Add("sh", "Church");
					Vices.Add( "t", "Told");
					Vices.Add("uw", "Oat");
					Vices.Add( "v", "Fave");
					Vices.Add( "w", "Wet");
					Vices.Add( "y", "Wet");
					Vices.Add( "z", "Size");
					Vices.Add("zh", "Church");
					break;
			}
		}

		internal static List<string> GetStandardViscodes()
		{
			var codewords = new List<string>();

			codewords.Add("S");
			codewords.Add("Eat");
			codewords.Add("If");
			codewords.Add("Ox");
			codewords.Add("Oat");
			codewords.Add("Earth");
			codewords.Add("Size");
			codewords.Add("Church");
			codewords.Add("Though");
			codewords.Add("Bump");
			codewords.Add("New");
			codewords.Add("Told");
			codewords.Add("Roar");
			codewords.Add("Cage");
			codewords.Add("Wet");
			codewords.Add("Fave");

			return codewords;
		}

		internal static void MapFxeViscodes(IDictionary<string, List<FxeDataBlock>> fxedata)
		{
			fxedata.Clear();

			fxedata.Add("Eat",                    new List<FxeDataBlock>());
			fxedata.Add("If",                     new List<FxeDataBlock>());
			fxedata.Add("Ox",                     new List<FxeDataBlock>());
			fxedata.Add("Oat",                    new List<FxeDataBlock>());
			fxedata.Add("Earth",                  new List<FxeDataBlock>());
			fxedata.Add("Size",                   new List<FxeDataBlock>());
			fxedata.Add("Church",                 new List<FxeDataBlock>());
			fxedata.Add("Fave",                   new List<FxeDataBlock>());
			fxedata.Add("Though",                 new List<FxeDataBlock>());
			fxedata.Add("Bump",                   new List<FxeDataBlock>());
			fxedata.Add("New",                    new List<FxeDataBlock>());
			fxedata.Add("Told",                   new List<FxeDataBlock>());
			fxedata.Add("Roar",                   new List<FxeDataBlock>());
			fxedata.Add("Wet",                    new List<FxeDataBlock>());
			fxedata.Add("Cage",                   new List<FxeDataBlock>());
			fxedata.Add("Orientation Head Pitch", new List<FxeDataBlock>());
			fxedata.Add("Orientation Head Roll",  new List<FxeDataBlock>());
			fxedata.Add("Orientation Head Yaw",   new List<FxeDataBlock>());
			fxedata.Add("Gaze Eye Pitch",         new List<FxeDataBlock>());
			fxedata.Add("Gaze Eye Yaw",           new List<FxeDataBlock>());
			fxedata.Add("Emphasis Head Pitch",    new List<FxeDataBlock>());
			fxedata.Add("Emphasis Head Roll",     new List<FxeDataBlock>());
			fxedata.Add("Emphasis Head Yaw",      new List<FxeDataBlock>());
			fxedata.Add("Eyebrow Raise",          new List<FxeDataBlock>());
			fxedata.Add("Blink",                  new List<FxeDataBlock>());
		}
		#endregion methods (static)
	}
}
/*
https://docs.microsoft.com/en-us/previous-versions/office/developer/speech-technologies/jj127450(v=msdn.10)
American English Phoneme Table

Phone Label	Example									PhoneID
-			syllable boundary (hyphen)				1
!			Sentence terminator (exclamation mark)	2
&			word boundary							3
,			Sentence terminator (comma)				4
.			Sentence terminator (period)			5
?			Sentence terminator (question mark)		6
_			Silence (underscore)					7
1			Primary stress							8
2			Secondary stress						9

aa			father		10
ae			cat			11
ah			cut			12
ao			dog			13
aw			foul		14
ax			ago			15
ay			bite		16
b			big			17
ch			chin		18
d			dig			19
dh			then		20
eh			pet			21
er			fur			22
ey			ate			23
f			fork		24
g			gut			25
h			help		26
ih			fill		27
iy			feel		28
jh			joy			29
k			cut			30
l			lid			31
m			mat			32
n			no			33
ng			sing		34
ow			go			35
oy			toy			36
p			put			37
r			red			38
s			sit			39
sh			she			40
t			talk		41
th			thin		42
uh			book		43
uw			too			44
v			vat			45
w			with		46
y			yard		47
z			zap			48
zh			pleasure	49
*/
/*
https://docs.microsoft.com/en-us/dotnet/api/system.speech.synthesis.speechsynthesizer.visemereached?view=netframework-4.8
The following is a list of the 21 SAPI phonemes and phoneme groups that
correspond to a viseme in US English.

Viseme	Phoneme(s)
 0		silence
 1		ae, ax, ah
 2		aa
 3		ao
 4		ey, eh, uh
 5		er
 6		y, iy, ih, ix
 7		w, uw
 8		ow
 9		aw
10		oy
11		ay
12		h
13		r
14		l
15		s, z
16		sh, ch, jh, zh
17		th, dh
18		f, v
19		d, t, n
20		k, g, ng
21		p, b, m
*/
