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
		/// Starttime in milliseconds.
		/// </summary>
		internal ulong Start;

		/// <summary>
		/// Stoptime in milliseconds.
		/// </summary>
		internal ulong Stop;

		/// <summary>
		/// The stoptime of each phoneme in milliseconds. SAPI 5.4 does not
		/// generate this information - instead SapiLipsync.AddStops() estimates
		/// the stops.
		/// </summary>
		internal List<ulong> phStops = new List<ulong>();
	}
}
