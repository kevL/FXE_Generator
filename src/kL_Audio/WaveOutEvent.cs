using System;
using System.Runtime.InteropServices;
using System.Threading;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	sealed class WaveOutEvent
		: IDisposable
	{
		#region events
		internal event EventHandler<StoppedEventArgs> PlaybackStopped;
		#endregion events


		#region fields (static)
		const int BUFFERS = 2;
		const int LATENCY = 100; // buffer duration in millisecs (ie. not latency per se)

		static IntPtr DEVICEID = (IntPtr)(-1);
		#endregion fields (static)


		#region fields
		readonly SynchronizationContext _syncContext; // a thread to run this lowlevel crap on.

		WaveFileReader _waveProvider;
		IntPtr _hWaveOut;
		readonly object _waveOutLock;

		WaveOutBuffer[] _buffers;

		AutoResetEvent _callbackEvent; // <- the device magic.
		#endregion fields


		#region properties
		volatile PlaybackState _playbackState; // TODO: Use lock() on reads and dismiss volatile.
		internal PlaybackState PlaybackState
		{
			get { return _playbackState; }
			set { _playbackState = value; }
		}
		#endregion properties


		#region cTor
		public WaveOutEvent(WaveFileReader waveProvider)
		{
			//lipsync_editor.logfile.Log("WaveOutEvent()");

			_waveProvider = waveProvider;
			//lipsync_editor.logfile.Log(". format= " + _waveProvider.WaveFormat);

			_syncContext = SynchronizationContext.Current; // TODO: what is that - looks like if !null another thread starts
			if (_syncContext != null)
			{
				string label = _syncContext.GetType().Name;
				if (   label == "LegacyAspNetSynchronizationContext"
					|| label == "AspNetSynchronizationContext")
				{
					//lipsync_editor.logfile.Log(". syncContext= " + label);
					//lipsync_editor.logfile.Log(". set syncContext NULL");
					_syncContext = null;
				}
			}

			_callbackEvent = new AutoResetEvent(false);

			_waveOutLock = new object();

			MultimediaResult result;
			lock (_waveOutLock)
			{
				result = WaveInterop.waveOutOpenWindow(out _hWaveOut,
													   DEVICEID,
													   _waveProvider.WaveFormat,
													   _callbackEvent.SafeWaitHandle.DangerousGetHandle(),
													   IntPtr.Zero,
													   WaveInterop.WaveInOutOpenFlags.CallbackEvent);
			}
			MultimediaException.Try(result, "waveOutOpen");

			PlaybackState = PlaybackState.Stopped;

			_buffers = new WaveOutBuffer[BUFFERS];

			int bufferSize = calcBytesPerLatency((LATENCY * 2 + BUFFERS - 1) / BUFFERS); // round up.
			//lipsync_editor.logfile.Log(". bufferSize= " + bufferSize);

			for (var i = 0; i != BUFFERS; ++i)
			{
				_buffers[i] = new WaveOutBuffer(_hWaveOut, bufferSize, _waveProvider, _waveOutLock);
			}
		}

		/// <summary>
		/// Gets the size of a wave-buffer equivalent to a specified latency in
		/// milliseconds.
		/// </summary>
		/// <param name="millisecs">the milliseconds of latency</param>
		/// <returns>count of bytes per latency-duration</returns>
		static int calcBytesPerLatency(int millisecs)
		{
			int bytes = 88200 * millisecs / 1000;
			if (bytes % 2 != 0)
				++bytes;

			return bytes;
		}
		#endregion cTor


		#region methods
		internal void Play()
		{
			switch (PlaybackState)
			{
				case PlaybackState.Stopped:
					PlaybackState = PlaybackState.Playing;
					_callbackEvent.Set(); // give the thread a kick

					ThreadPool.QueueUserWorkItem(state => PlaybackThread(), null);
					break;

				case PlaybackState.Paused:
					Resume();
					_callbackEvent.Set(); // give the thread a kick
					break;
			}
		}

		void PlaybackThread()
		{
			Exception exception = null;
			try
			{
				DoPlayback();
			}
			catch (Exception e)
			{
				exception = e;
			}
			finally
			{
				PlaybackState = PlaybackState.Stopped;
				FirePlaybackStopped(exception); // exit the background thread
			}
		}

		void DoPlayback()
		{
			while (PlaybackState != PlaybackState.Stopped)
			{
//				if (!callbackEvent.WaitOne(DesiredLatency)
//					&& playbackState == PlaybackState.Playing)
//				{
//					Debug.WriteLine("WARNING: WaveOutEvent callback event timeout");
//				}

				if (PlaybackState == PlaybackState.Playing) // requeue any buffers returned ->
				{
					int queued = 0;
					foreach (var buffer in _buffers)
					{
						if (buffer.InQueue || buffer.done())
							++queued;
					}

					if (queued == 0)
					{
						PlaybackState = PlaybackState.Stopped;
						_callbackEvent.Set(); // give the thread a kick
					}
				}
			}
		}

		void FirePlaybackStopped(Exception e)
		{
			var handler = PlaybackStopped;
			if (handler != null) // safety. It will be there.
			{
				if (_syncContext != null)
				{
					_syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
				}
				else
					handler(this, new StoppedEventArgs(e));
			}
		}

		/// <summary>
		/// Resumes playing after a pause from the same position.
		/// </summary>
		void Resume()
		{
			if (PlaybackState == PlaybackState.Paused)
			{
				MultimediaResult result;
				lock (_waveOutLock)
				{
					result = WaveInterop.waveOutRestart(_hWaveOut);
				}

				if (result != MultimediaResult.NoError)
				{
					throw new MultimediaException(result, "waveOutRestart");
				}
				PlaybackState = PlaybackState.Playing;
			}
		}


		/// <summary>
		/// Stops and resets the WaveOut device.
		/// </summary>
		internal void Stop()
		{
			if (PlaybackState != PlaybackState.Stopped)
			{
				// in the call to waveOutReset with function callbacks some drivers
				// will block here until done() is called for every buffer
				PlaybackState = PlaybackState.Stopped; // set this here to avoid a problem with some drivers

				MultimediaResult result;
				lock (_waveOutLock)
				{
					result = WaveInterop.waveOutReset(_hWaveOut);
				}

				if (result != MultimediaResult.NoError)
				{
					throw new MultimediaException(result, "waveOutReset");
				}
				_callbackEvent.Set(); // give the thread a kick to force it to exit.
			}
		}


		/// <summary>
		/// Pauses the audio.
		/// </summary>
		internal void Pause()
		{
			if (PlaybackState == PlaybackState.Playing)
			{
				PlaybackState = PlaybackState.Paused; // set this here to avoid a deadlock problem with some drivers

				MultimediaResult result;
				lock (_waveOutLock)
				{
					result = WaveInterop.waveOutPause(_hWaveOut);
				}

				if (result != MultimediaResult.NoError)
				{
					throw new MultimediaException(result, "waveOutPause");
				}
			}
		}


		/// <summary>
		/// Gets the position of the waveout stream from the WindowsAPI.
		/// </summary>
		/// <returns></returns>
		internal uint GetPosition()
		{
			lock (_waveOutLock)
			{
				var multimediaTime = new MultimediaTime();
				multimediaTime.wType = MultimediaTime.TIME_BYTES;

				MultimediaException.Try(WaveInterop.waveOutGetPosition(_hWaveOut,
																	   ref multimediaTime,
																	   Marshal.SizeOf(multimediaTime)),
										"waveOutGetPosition");

//				if (multimediaTime.wType != MultimediaTime.TIME_BYTES)
//					throw new Exception(string.Format("waveOutGetPosition: wType -> Expected {0}, Received {1}",
//													  MultimediaTime.TIME_BYTES, multimediaTime.wType));

				return multimediaTime.cb;
			}
		}
		#endregion methods


		#region IDisposable
		/// <summary>
		/// Stops, closes, and disposes this WaveOutEvent and its buffers.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true); // TODO: why true
		}
		#endregion IDisposable


		#region dispose
		/// <summary>
		/// Stops, closes, and disposes this WaveOutEvent.
		/// </summary>
		/// <param name="disposing">true disposes the buffers</param>
		void Dispose(bool disposing)
		{
			Stop();

			if (disposing && _buffers != null)
			{
				foreach (var buffer in _buffers)
				{
					buffer.Dispose();
				}
				_buffers = null;
			}

			if (_callbackEvent != null)
			{
				_callbackEvent.Close();
				_callbackEvent = null;
			}

			lock (_waveOutLock)
			{
				if (_hWaveOut != IntPtr.Zero)
				{
					WaveInterop.waveOutClose(_hWaveOut);
					_hWaveOut = IntPtr.Zero;
				}
			}
		}

		/// <summary>
		/// Finalizer. Only called when user forgets to call Dispose().
		/// </summary>
		~WaveOutEvent()
		{
			Dispose(false); // TODO: why false
			//Debug.Assert(false, "WaveOutEvent device was not closed properly.");
		}
		#endregion dispose
	}
}
