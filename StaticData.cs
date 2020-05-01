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
		static readonly Dictionary<string, string> _phon2vis =
					new Dictionary<string, string>();
		internal static Dictionary<string, string> PhonToVis
		{
			get { return _phon2vis; }
		}
		#endregion properties (static)


		// TODO: Phon2Vis etc ought rely on LanguageId. This is for US English.

		#region methods (static)
		internal static void FillPhon2VisMap()
		{
			PhonToVis.Add( "x", String.Empty);
			PhonToVis.Add("iy", "Eat");
			PhonToVis.Add("ih", "If");
			PhonToVis.Add("eh", "If");
			PhonToVis.Add("ey", "If");
			PhonToVis.Add("ae", "If");
			PhonToVis.Add("aa", "Ox");
			PhonToVis.Add("aw", "If");
			PhonToVis.Add("ay", "If");
			PhonToVis.Add("ah", "If");
			PhonToVis.Add("ao", "Ox");
			PhonToVis.Add("oy", "Oat");
			PhonToVis.Add("ow", "Oat");
			PhonToVis.Add("uh", "Oat");
			PhonToVis.Add("uw", "Oat");
			PhonToVis.Add("er", "Earth");
			PhonToVis.Add("ax", "If");
			PhonToVis.Add( "s", "Size");
			PhonToVis.Add("sh", "Church");
			PhonToVis.Add( "z", "Size");
			PhonToVis.Add("zh", "Church");
			PhonToVis.Add( "f", "Fave");
			PhonToVis.Add("th", "Though");
			PhonToVis.Add( "v", "Fave");
			PhonToVis.Add("dh", "Though");
			PhonToVis.Add( "m", "Bump");
			PhonToVis.Add( "n", "New");
			PhonToVis.Add("ng", "New");
			PhonToVis.Add( "l", "Told");
			PhonToVis.Add( "r", "Roar");
			PhonToVis.Add( "w", "Wet");
			PhonToVis.Add( "y", "Wet");
			PhonToVis.Add( "h", "If");
			PhonToVis.Add( "b", "Bump");
			PhonToVis.Add( "d", "Told");
			PhonToVis.Add("jh", "Church");
			PhonToVis.Add( "g", "Cage");
			PhonToVis.Add( "p", "Bump");
			PhonToVis.Add( "t", "Told");
			PhonToVis.Add( "k", "Cage");
			PhonToVis.Add("ch", "Church");
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
