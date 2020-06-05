using System;
using System.Runtime.InteropServices;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	/// <summary>
	/// WaveHeader interop structure (WAVEHDR).
	/// http://msdn.microsoft.com/en-us/library/dd743837%28VS.85%29.aspx
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	sealed class WaveHeader
	{
		public IntPtr          dataBuffer;    // pointer to locked data buffer (lpData)
		public int             bufferLength;  // length of data buffer (dwBufferLength)
		public int             bytesRecorded; // used for input only (dwBytesRecorded)
		public IntPtr          userData;      // for client's use (dwUser)
		public WaveHeaderFlags flags;         // assorted flags (dwFlags)
		public int             loops;         // loop control counter (dwLoops)
		public IntPtr          next;          // PWaveHdr, reserved for driver (lpNext)
		public IntPtr          reserved;      // reserved for driver
	}

	/// <summary>
	/// WaveHeaderFlags enumeration.
	/// </summary>
	[Flags]
	enum WaveHeaderFlags
	{
		/// <summary>
		/// WHDR_DONE
		/// Set by the device driver to indicate that it is finished with the
		/// buffer and is returning it to the application.
		/// </summary>
		Done      = 0x01,
		/// <summary>
		/// WHDR_PREPARED
		/// Set by Windows to indicate that the buffer has been prepared with
		/// the waveInPrepareHeader or waveOutPrepareHeader function.
		/// </summary>
		Prepared  = 0x02,
		/// <summary>
		/// WHDR_BEGINLOOP
		/// This buffer is the first buffer in a loop. This flag is used only
		/// with output buffers.
		/// </summary>
		BeginLoop = 0x04,
		/// <summary>
		/// WHDR_ENDLOOP
		/// This buffer is the last buffer in a loop. This flag is used only
		/// with output buffers.
		/// </summary>
		EndLoop   = 0x08,
		/// <summary>
		/// WHDR_INQUEUE
		/// Set by Windows to indicate that the buffer is queued for playback.
		/// </summary>
		InQueue   = 0x10
	}
}
