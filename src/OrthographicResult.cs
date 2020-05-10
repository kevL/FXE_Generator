using System;
using System.Collections.Generic;
using System.Linq;


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
		/// The list of phonemes. These are pulled from SAPI.
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
		/// The stoptime for each listed phoneme in milliseconds. SAPI 5.1 does
		/// not generate this information, instead sapi_lipsync uses the
		/// phoneme_estimator to do this. Applications can also build their own.
		/// </summary>
		internal List<ulong> Stops = new List<ulong>();


/*		/// <summary>
		/// Gets a starttime.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		internal ulong GetStart(int id)
		{
			if (id == 0)
				return Start;

			return Stops[id - 1];
		} */
	}
}
