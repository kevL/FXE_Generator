using System;
using System.Runtime.InteropServices;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	/// <summary>
	/// A buffer of wave-samples for streaming to a wave-output device.
	/// </summary>
	sealed class WaveOutBuffer
		: IDisposable
	{
		#region fields
		readonly WaveHeader _header;

		readonly byte[] _buffer;

		readonly WaveFileReader _waveProvider;
		readonly object _waveOutLock;

		IntPtr _hWaveOut;

		readonly GCHandle _hBuffer;
		readonly GCHandle _hHeader;	// need to pin the header structure
		readonly GCHandle _hThis;	// for the user callback
		#endregion fields


		#region properties
		/// <summary>
		/// True if the header's 'InQueue' flag is set.
		/// </summary>
		internal bool InQueue
		{
			get { return (_header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue; }
		}

//		readonly Int32 _bufferSize; // allocated bytes - may not be the same as bytes read
//		/// <summary>
//		/// The buffer-size in bytes.
//		/// </summary>
//		public int BufferSize
//		{
//			get { return _bufferSize; }
//		}
		#endregion properties


		#region cTor
		/// <summary>
		/// Creates a new WaveOutBuffer.
		/// </summary>
		/// <param name="hWaveOut">WaveOut device to write to</param>
		/// <param name="bufferSize">buffer size in bytes</param>
		/// <param name="bufferFillStream">stream to provide more data</param>
		/// <param name="waveOutLock">lock to protect WaveOut API's from being called on 2+ threads</param>
		internal WaveOutBuffer(IntPtr hWaveOut, Int32 bufferSize, WaveFileReader bufferFillStream, object waveOutLock)
		{
//			_bufferSize = bufferSize;

			_buffer  = new byte[bufferSize];
			_hBuffer = GCHandle.Alloc(_buffer, GCHandleType.Pinned);

			_hWaveOut     = hWaveOut;
			_waveProvider = bufferFillStream;
			_waveOutLock  = waveOutLock;

			_header  = new WaveHeader();
			_hHeader = GCHandle.Alloc(_header, GCHandleType.Pinned);
			_header.dataBuffer   = _hBuffer.AddrOfPinnedObject();
			_header.bufferLength = bufferSize;
			_header.loops        = 1;

			_hThis = GCHandle.Alloc(this);
			_header.userData = (IntPtr)_hThis;

			lock (_waveOutLock)
			{
				MultimediaException.Try(WaveInterop.waveOutPrepareHeader(_hWaveOut,
																		 _header,
																		 Marshal.SizeOf(_header)),
										"waveOutPrepareHeader");
			}
		}
		#endregion cTor


		/// <summary>
		/// This is called by the callback and will refill the buffer as needed.
		/// </summary>
		/// <returns>true if more bytes were transfered from the input-buffer to
		/// the output-buffer</returns>
		internal bool done()
		{
			int count;
			lock (_waveProvider)
			{
				count = _waveProvider.Read(_buffer, _buffer.Length);
			}

			if (count != 0)
			{
				for (int i = count; i != _buffer.Length; ++i) // TODO: huh
				{
					_buffer[i] = 0;
				}

				MultimediaResult result;
				lock (_waveOutLock)
				{
					result = WaveInterop.waveOutWrite(_hWaveOut,
													  _header,
													  Marshal.SizeOf(_header));
				}

				if (result != MultimediaResult.NoError)
				{
					throw new MultimediaException(result, "waveOutWrite");
				}

				GC.KeepAlive(this);
				return true;
			}
			return false;
		}


		#region IDisposable
		/// <summary>
		/// Releases resources held by this WaveOutBuffer.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true); // TODO: why true
		}
		#endregion IDisposable


		#region dispose
		/// <summary>
		/// Releases resources held by this WaveOutBuffer.
		/// </summary>
		void Dispose(bool disposing)
		{
//			if (disposing) free managed resources

			// free unmanaged resources
			if (_hHeader.IsAllocated)
				_hHeader.Free();

			if (_hBuffer.IsAllocated)
				_hBuffer.Free();

			if (_hThis.IsAllocated)
				_hThis.Free();

			if (_hWaveOut != IntPtr.Zero)
			{
				lock (_waveOutLock)
				{
					WaveInterop.waveOutUnprepareHeader(_hWaveOut,
													   _header,
													   Marshal.SizeOf(_header));
				}
				_hWaveOut = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Finalizer for this WaveOutBuffer.
		/// </summary>
		~WaveOutBuffer()
		{
			Dispose(false); // TODO: why false
//			System.Diagnostics.Debug.Assert(true, "WaveOutBuffer was not disposed properly.");
		}
		#endregion dispose
	}
}
