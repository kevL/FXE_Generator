using System;


namespace kL_audio
{
	// credit: Mark Heath et al. - the NAudio libraries

	/// <summary>
	/// Issues exceptions for Windows API fails.
	/// </summary>
	sealed class MultimediaException
		: Exception
	{
		/// <summary>
		/// cTor. Creates a new MultimediaException.
		/// </summary>
		/// <param name="result">the result returned by the Windows API call</param>
		/// <param name="function">the identifier of the Windows API function
		/// that failed</param>
		internal MultimediaException(MultimediaResult result, string function)
			: base(MultimediaException.ErrorMessage(result, function))
		{}

		static string ErrorMessage(MultimediaResult result, string function)
		{
			return String.Format("{0} calling {1}", result, function);
		}

		/// <summary>
		/// Helper function to automatically raise an exception on failure.
		/// </summary>
		/// <param name="result">the result of the API call</param>
		/// <param name="function">the API function identifier</param>
		internal static void Try(MultimediaResult result, string function)
		{
			if (result != MultimediaResult.NoError)
			{
				throw new MultimediaException(result, function);
			}
		}
	}
}
