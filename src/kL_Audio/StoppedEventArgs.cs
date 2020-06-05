using System;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	sealed class StoppedEventArgs
		: EventArgs
	{
		internal Exception Exception
		{ get; private set; }

		internal StoppedEventArgs(Exception e = null)
		{
			Exception = e;
		}
	}
}
