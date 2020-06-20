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
		/// The list of phonemes in this word-result. These are pulled by SAPI.
		/// </summary>
		internal List<string> Phons;

		/// <summary>
		/// The engine-confidence (0.001..0.999).
		/// </summary>
		internal float Confi;

		/// <summary>
		/// The confidence-level (Low, Normal, High).
		/// </summary>
		internal string Level;

		decimal _start;
		/// <summary>
		/// t=Start in seconds truncated to millisecond precision.
		/// </summary>
		internal decimal Start
		{
			get { return _start; }
			set { _start = Truncate(value); }
		}

		decimal _stop;
		/// <summary>
		/// t=Stop in seconds truncated to millisecond precision.
		/// </summary>
		internal decimal Stop
		{
			get { return _stop; }
			set { _stop = Truncate(value); }
		}

		/// <summary>
		/// t=Stop of each phoneme of the word in seconds. SAPI does not
		/// generate this information - instead SapiLipsync.phStops() estimates
		/// the stops.
		/// </summary>
		internal List<decimal> phStops = new List<decimal>();


		/// <summary>
		/// Truncates seconds in decimal to millisecond precision.
		/// </summary>
		/// <param name="dec"></param>
		/// <returns></returns>
		internal static decimal Truncate(decimal dec)
		{
			int val = (int)(dec * 1000);
			return (decimal)val / 1000;
		}
	}
}
