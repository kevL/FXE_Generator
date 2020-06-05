using System;
using System.Collections.Generic;
using System.IO;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	sealed class WaveFileReader
	{
		#region fields
		readonly byte[] _bytes;

		readonly object _lock = new object();
		#endregion fields


		#region properties
		internal WaveFormat WaveFormat
		{ get; private set; }

		internal long Position
		{ private get; set; }

		/// <summary>
		/// Length of the byte-array.
		/// </summary>
		long Length
		{ get; set; }

//		public TimeSpan CurrentTime
//		{
//			get { return TimeSpan.FromSeconds((double)Position / WaveFormat.bytespersec); }
//			set { Position = (long)(value.TotalSeconds * WaveFormat.bytespersec); }
//		}
//
//		public TimeSpan TotalTime
//		{
//			get { return TimeSpan.FromSeconds((double)Length / WaveFormat.bytespersec); }
//		}
		#endregion properties


		#region cTor
		internal WaveFileReader(string pfe)
		{
			lipsync_editor.logfile.Log("WaveFileReader()");

			WaveFormat = new WaveFormat();
			WaveFormat.format        = WaveFormatEncoding.Pcm;
			WaveFormat.channels      = (short)1;
			WaveFormat.samplerate    = 44100;
			WaveFormat.bytespersec   = 88200;
			WaveFormat.blockalign    = (short)2;
			WaveFormat.bitspersample = (short)16;


			using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var br = new BinaryReader(fs);

				fs.Seek(40, SeekOrigin.Begin);
				Length = br.ReadUInt32();

				_bytes = new byte[Length];

				for (int i = 0; i != Length; ++i)
				{
					_bytes[i] = br.ReadByte();
				}
				br.Close();
			}
		}
		#endregion cTor


		#region methods
		/// <summary>
		/// Here's your filestream.reader. /enjoy
		/// </summary>
		/// <param name="buffer">pointer to a byte-buffer to fill</param>
		/// <param name="count">count of bytes requested</param>
		/// <returns>count of bytes actually filled</returns>
		internal int Read(IList<byte> buffer, int count)
		{
			lock (_lock)
			{
				if (Position + count > Length)
				{
					count = (int)(Length - Position);
				}

				for (int i = 0; i != count; ++i)
				{
					buffer[i] = _bytes[Position++];
				}

				return count;
			}
		}

//		/// <summary>
//		/// Moves forward or backwards the specified seconds in the stream.
//		/// </summary>
//		/// <param name="seconds">count of seconds to move - can be negative</param>
//		public void Skip(int seconds)
//		{
//			long pos = (long)WaveFormat.bytespersec * seconds + Position;
//			Position = Math.Max(0L, Math.Min(pos, Length));
//		}
		#endregion methods
	}
}
