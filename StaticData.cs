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
		/// A dictionary of visemes keyed by phoneme-strings.
		/// </summary>
		internal static Dictionary<string, string> Vices
		{
			get { return _vices; }
		}
		#endregion properties (static)


		#region methods (static)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="phoneId"></param>
		internal static void AddVices(int phoneId)
		{
			Vices.Clear();

			// TODO: Vices ought rely on PhoneIds.

			switch (phoneId)
			{
				default:
//				case 1033: // This is for English US (en-US)
					Vices.Add( "x", String.Empty);

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
					Vices.Add("oy", "Oat");
					Vices.Add("ow", "Oat");
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
//					break;

//				case 1036: // This is for French
//					break;
			}
		}

		internal static List<string> GetCodewords()
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

		internal static void AddCodewords(IDictionary<string, List<FxeDataBlock>> fxedata)
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
