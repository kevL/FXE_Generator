using System;
using System.Collections.Generic;
using System.Linq;


namespace lipsync_editor
{
	public class AlignmentResult
	{
		/// <summary>
		/// Starttime in milliseconds of this alignment result.
		/// </summary>
		public ulong Start;

		/// <summary>
		/// Stoptime in milliseconds of this alignment result.
		/// </summary>
		public ulong Stop;

		/// <summary>
		/// The text representing this result.
		/// </summary>
		public string Orthography;

		/// <summary>
		/// The phonemes representing this result. These are pulled from SAPI.
		/// Each phoneme is a separate index in the phonemes-array for easier
		/// parsing.
		/// </summary>
		public List<string> Phons;

		/// <summary>
		/// The stoptime for each phoneme in milliseconds. SAPI 5.1 does not
		/// generate this information, instead sapi_lipsync uses the
		/// phoneme_estimator to do this. Applications can also build their own.
		/// </summary>
		public List<ulong> Stops = new List<ulong>();


		/// <summary>
		/// Gets a starttime.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public ulong GetStart(int id)
		{
			if (id == 0)
				return Start;

			return Stops[id - 1];
		}
	}
}
