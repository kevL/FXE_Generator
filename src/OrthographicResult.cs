using System;
using System.Collections.Generic;


namespace lipsync_editor
{
	/// <summary>
	/// A word basically.
	/// </summary>
	sealed class OrthographicResult
	{
		/// <summary>
		/// The word.
		/// </summary>
		internal string Orthography;

		/// <summary>
		/// The list of phonemes. These are pulled by SAPI.
		/// </summary>
		internal List<string> Phons;

		/// <summary>
		/// The engine-confidence (0.001..0.999).
		/// </summary>
		internal float Confidence;

		/// <summary>
		/// The confidence-level (Low, Normal, High).
		/// </summary>
		internal string Level;

		/// <summary>
		/// t=Start in seconds.
		/// </summary>
		internal decimal Start;

		/// <summary>
		/// t=Stop in seconds.
		/// </summary>
		internal decimal Stop;

		/// <summary>
		/// t=Stop of each phoneme of the word in seconds. SAPI does not
		/// generate this information - instead SapiLipsync.EstimatePhonStops()
		/// estimates the stops.
		/// </summary>
		internal List<decimal> phStops = new List<decimal>();
	}
}
