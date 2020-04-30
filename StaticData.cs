using System;
using System.Collections.Generic;


namespace lipsync_editor
{
	static class StaticData
	{
		internal static float EPSILON   = 0.000005f;
		internal static float STOP_INCR = 0.0000001f;


		internal static void AddPhon2VisMap(IDictionary<string, string> phon2vis)
		{
			phon2vis.Add( "x", String.Empty);
			phon2vis.Add("iy", "Eat");
			phon2vis.Add("ih", "If");
			phon2vis.Add("eh", "If");
			phon2vis.Add("ey", "If");
			phon2vis.Add("ae", "If");
			phon2vis.Add("aa", "Ox");
			phon2vis.Add("aw", "If");
			phon2vis.Add("ay", "If");
			phon2vis.Add("ah", "If");
			phon2vis.Add("ao", "Ox");
			phon2vis.Add("oy", "Oat");
			phon2vis.Add("ow", "Oat");
			phon2vis.Add("uh", "Oat");
			phon2vis.Add("uw", "Oat");
			phon2vis.Add("er", "Earth");
			phon2vis.Add("ax", "If");
			phon2vis.Add( "s", "Size");
			phon2vis.Add("sh", "Church");
			phon2vis.Add( "z", "Size");
			phon2vis.Add("zh", "Church");
			phon2vis.Add( "f", "Fave");
			phon2vis.Add("th", "Though");
			phon2vis.Add( "v", "Fave");
			phon2vis.Add("dh", "Though");
			phon2vis.Add( "m", "Bump");
			phon2vis.Add( "n", "New");
			phon2vis.Add("ng", "New");
			phon2vis.Add( "l", "Told");
			phon2vis.Add( "r", "Roar");
			phon2vis.Add( "w", "Wet");
			phon2vis.Add( "y", "Wet");
			phon2vis.Add( "h", "If");
			phon2vis.Add( "b", "Bump");
			phon2vis.Add( "d", "Told");
			phon2vis.Add("jh", "Church");
			phon2vis.Add( "g", "Cage");
			phon2vis.Add( "p", "Bump");
			phon2vis.Add( "t", "Told");
			phon2vis.Add( "k", "Cage");
			phon2vis.Add("ch", "Church");
		}

		internal static List<string> AddCodewords()
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

		internal static void AddFxeCodewords(IDictionary<string, List<FxeDataBlock>> fxeData)
		{
			fxeData.Clear();

			fxeData.Add("Eat",                    new List<FxeDataBlock>());
			fxeData.Add("If",                     new List<FxeDataBlock>());
			fxeData.Add("Ox",                     new List<FxeDataBlock>());
			fxeData.Add("Oat",                    new List<FxeDataBlock>());
			fxeData.Add("Earth",                  new List<FxeDataBlock>());
			fxeData.Add("Size",                   new List<FxeDataBlock>());
			fxeData.Add("Church",                 new List<FxeDataBlock>());
			fxeData.Add("Fave",                   new List<FxeDataBlock>());
			fxeData.Add("Though",                 new List<FxeDataBlock>());
			fxeData.Add("Bump",                   new List<FxeDataBlock>());
			fxeData.Add("New",                    new List<FxeDataBlock>());
			fxeData.Add("Told",                   new List<FxeDataBlock>());
			fxeData.Add("Roar",                   new List<FxeDataBlock>());
			fxeData.Add("Wet",                    new List<FxeDataBlock>());
			fxeData.Add("Cage",                   new List<FxeDataBlock>());
			fxeData.Add("Orientation Head Pitch", new List<FxeDataBlock>());
			fxeData.Add("Orientation Head Roll",  new List<FxeDataBlock>());
			fxeData.Add("Orientation Head Yaw",   new List<FxeDataBlock>());
			fxeData.Add("Gaze Eye Pitch",         new List<FxeDataBlock>());
			fxeData.Add("Gaze Eye Yaw",           new List<FxeDataBlock>());
			fxeData.Add("Emphasis Head Pitch",    new List<FxeDataBlock>());
			fxeData.Add("Emphasis Head Roll",     new List<FxeDataBlock>());
			fxeData.Add("Emphasis Head Yaw",      new List<FxeDataBlock>());
			fxeData.Add("Eyebrow Raise",          new List<FxeDataBlock>());
			fxeData.Add("Blink",                  new List<FxeDataBlock>());
		}
	}
}
