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
		readonly long _length;

		readonly object _lock = new object();
		#endregion fields


		#region properties
		internal WaveFormat WaveFormat
		{ get; private set; }

		/// <summary>
		/// Current position in the byte-array.
		/// </summary>
		internal long Position
		{ private get; set; }
		#endregion properties


		#region cTor
		internal WaveFileReader(string pfe)
		{
			//lipsync_editor.logfile.Log("WaveFileReader()");

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
				_length = br.ReadUInt32();

				_bytes = new byte[_length];

				for (int i = 0; i != _length; ++i)
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
				if (Position + count > _length)
				{
					count = (int)(_length - Position);
				}

				for (int i = 0; i != count; ++i)
				{
					buffer[i] = _bytes[Position++];
				}

				return count;
			}
		}
		#endregion methods
	}
}
