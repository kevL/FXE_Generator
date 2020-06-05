using System;
using System.Runtime.InteropServices;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	enum WaveFormatEncoding
		: short
	{
		Unknown   = 0x0, // WAVE_FORMAT_UNKNOWN    - Microsoft Corporation
		Pcm       = 0x1, // WAVE_FORMAT_PCM        - Microsoft Corporation
		Adpcm     = 0x2, // WAVE_FORMAT_ADPCM      - Microsoft Corporation
		IeeeFloat = 0x3  // WAVE_FORMAT_IEEE_FLOAT - Microsoft Corporation

		// etc.
	}

	/// <summary>
	/// Represents a wavefile format.
	/// typedef struct {
	///   WORD  wFormatTag;
	///   WORD  nChannels;
	///   DWORD nSamplesPerSec;
	///   DWORD nAvgBytesPerSec;
	///   WORD  nBlockAlign;
	///   WORD  wBitsPerSample;
	///   WORD  cbSize;
	/// } WAVEFORMATEX;
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack=2)]
	class WaveFormat
	{
		public WaveFormatEncoding format;
		public short channels;
		public int   samplerate;
		public int   bytespersec;
		public short blockalign;
		public short bitspersample;
		public short extrasize;


//		public override string ToString()
//		{
//			return format        + Environment.NewLine
//				 + channels      + Environment.NewLine
//				 + samplerate    + Environment.NewLine
//				 + bytespersec   + Environment.NewLine
//				 + blockalign    + Environment.NewLine
//				 + bitspersample + Environment.NewLine
//				 + extrasize;
//		}
	}
}

/*
"fmt "
DWORD dwRiffSize;
WORD  wFormatTag;         // Format category
WORD  wChannels;          // Number of channels
DWORD dwSamplesPerSec;    // Sampling rate
DWORD dwAvgBytesPerSec;   // For buffer estimation
WORD  wBlockAlign;        // Data block size
WORD  wBitsPerSample;
*/
