using System;


namespace lipsync_editor
{
	static class Utility
	{
		const decimal SR_TU_FACTOR = 10000000;

		/// <summary>
		/// Converts SpeechRecognitionEngine time-units to seconds.
		/// </summary>
		/// <param name="nanoseconds_100"></param>
		/// <returns></returns>
		internal static decimal SrTustoSecs(ulong nanoseconds_100)
		{
			return ((decimal)nanoseconds_100 / SR_TU_FACTOR);
		}

		/// <summary>
		/// Converts seconds to SpeechRecognitionEngine time-units.
		/// </summary>
		/// <param name="secs"></param>
		/// <returns></returns>
		internal static ulong SecstoSrTus(string secs)
		{
			return (ulong)(Decimal.Parse(secs) * SR_TU_FACTOR);
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


		internal static bool isWordstart(string pos)
		{
			return pos.EndsWith(".0", StringComparison.OrdinalIgnoreCase);
		}
	}
}
