using System;
using System.Runtime.InteropServices;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	static class WaveInterop
	{
		#region WaveOutEvent
		[DllImport("winmm.dll", EntryPoint="waveOutOpen")]
		public static extern MultimediaResult waveOutOpenWindow(out IntPtr hWaveOut,
																IntPtr uDeviceID,
																WaveFormat lpFormat,
																IntPtr callbackWindowHandle,
																IntPtr dwInstance,
																WaveInOutOpenFlags dwFlags);
		[Flags]
		public enum WaveInOutOpenFlags
		{
			/// <summary>
			/// CALLBACK_NULL - No callback
			/// </summary>
			CallbackNull     = 0x00000,
			/// <summary>
			/// CALLBACK_WINDOW - dwCallback is a HWND 
			/// </summary>
			CallbackWindow   = 0x10000,
			/// <summary>
			/// CALLBACK_THREAD - callback is a thread ID 
			/// </summary>
			CallbackThread   = 0x20000,
			/// <summary>
			/// CALLBACK_FUNCTION - dwCallback is a FARPROC 
			/// </summary>
			CallbackFunction = 0x30000,
			/// <summary>
			/// CALLBACK_EVENT - dwCallback is an EVENT handle 
			/// </summary>
			CallbackEvent    = 0x50000,
	
//			WAVE_FORMAT_QUERY  = 1,
//			WAVE_MAPPED        = 4,
//			WAVE_FORMAT_DIRECT = 8
		}

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutRestart(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutReset(IntPtr hWaveOut);

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutPause(IntPtr hWaveOut);

		// http://msdn.microsoft.com/en-us/library/dd743863%28VS.85%29.aspx
		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutGetPosition(IntPtr hWaveOut,
																 ref MultimediaTime mmTime,
																 int uSize);

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutClose(IntPtr hWaveOut);
		#endregion WaveOutEvent


		#region WaveOutBuffer
		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutPrepareHeader(IntPtr hWaveOut,
																   WaveHeader lpWaveOutHdr,
																   int uSize);

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutWrite(IntPtr hWaveOut,
														   WaveHeader lpWaveOutHdr,
														   int uSize);

		[DllImport("winmm.dll")]
		public static extern MultimediaResult waveOutUnprepareHeader(IntPtr hWaveOut,
																	 WaveHeader lpWaveOutHdr,
																	 int uSize);
		#endregion WaveOutBuffer
	}
}
