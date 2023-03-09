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
		/// <returns>seconds</returns>
		internal static decimal GarpstoSecs(int nanoseconds_100)
		{
			return nanoseconds_100 / SR_TU_FACTOR;
		}

//		/// <summary>
//		/// Converts seconds to SpeechRecognitionEngine time-units.
//		/// </summary>
//		/// <param name="secs"></param>
//		/// <returns></returns>
//		internal static ulong SecstoSrTus(string secs)
//		{
//			return (ulong)(Decimal.Parse(secs) * SR_TU_FACTOR);
//		}


		/// <summary>
		/// Gets the filelabel w/out path or extension.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		/// <remarks>This is redundant w/
		/// <c>System.IO.Path.GetFileNameWithoutExtension()</c>.</remarks>
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
		/// Checks if the phoneme-position in a datatable is a wordstart.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		internal static bool isWordstart(string pos)
		{
			return pos.EndsWith(".0", StringComparison.OrdinalIgnoreCase);
		}
	}
}
