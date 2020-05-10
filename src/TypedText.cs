using System;
using System.Text;


namespace lipsync_editor
{
	/// <summary>
	/// Parses a user-typed text string into something better for the TTS
	/// (text-to-speech) Voice/Speech SAPI. And to compare to Recognition output
	/// orthemes against.
	/// </summary>
	static class TypedText
	{
		/// <summary>
		/// Replaces whitespaces, newlines, and comments with spaces; removes
		/// controlchars and comments, then replaces any multiple spaces with
		/// single spaces and trims the specified text.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		internal static string SanitizeTypedText(string text)
		{
			text = Spaceout(text);

			text = RemoveComment('<', '>', text);
			text = RemoveComment('{', '}', text);
			text = RemoveComment('[', ']', text);
			text = RemoveComment('|', '|', text);

			text = ConflateSpaces(text);
			text = ConflatePunctuation(text);

			return text.Trim();
		}

		/// <summary>
		/// Obliterates all chars other than letters, spaces, and single-quotes,
		/// then returns a lowercase string. This should only be passed text
		/// that has already been sanitized w/ SanitizeTypedText(). The return
		/// is used only to match against default-recognition phonemes, which in
		/// English don't have punctuation other than the single-quote char.
		/// TODO: Other languages can have other chars; eg, French appears to
		/// use tilde characters.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		internal static string StripTypedText(string text)
		{
			var sb = new StringBuilder(text.Length);
			char c;
			for (int i = 0; i != text.Length; ++i)
			{
				if ((c = text[i]) == ' ' || c == '\''
					|| Char.IsLetter(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString().ToLower();
		}

		/// <summary>
		/// No tabs, no newlines, no funny stuff - just space.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static string Spaceout(string text)
		{
			var sb = new StringBuilder(text.Length);

			char c;
			for (int i = 0; i != text.Length; ++i)
			{
				c = text[i];
				if (Char.IsWhiteSpace(c)) // this replaces line- and paragraph-separators also
				{
					sb.Append(' ');
				}
//				else if (c != '\'' && Char.IsPunctuation(c))
				else if (c == '.'
					||   c == ','
					||   c == ';'
					||   c == ':'
					||   c == '!'
					||   c == '?')
				{
					sb.Append(c);
					sb.Append(' ');
				}
				else if (!Char.IsControl(c))
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Removes NwN2-style comments from dialog strings.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="stop"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		static string RemoveComment(char start, char stop, string text)
		{
			int i,j;
			while ((i = text.IndexOf(start)) != -1)
			{
				if ((j = text.IndexOf(stop, i + 1)) != -1)
				{
					text = text.Remove(i, j - i + 1);
					text = text.Insert(i, " ");
				}
			}
			return text;
		}

		/// <summary>
		/// Replaces multiple spaces with a single space.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static string ConflateSpaces(string text)
		{
			var sb = new StringBuilder(text.Length);

			char c;
			for (int i = 0; i != text.Length; ++i)
			{
				if ((c = text[i]) != ' '
					|| i == 0 || (text[i - 1] != ' '))
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		static string ConflatePunctuation(string text)
		{
			var sb = new StringBuilder(text.Length);

			char c;
			for (int i = 0; i != text.Length; ++i)
			{
				if ((c = text[i]) != ' '
					|| i == text.Length - 1
					|| !Char.IsPunctuation(text[i + 1]))
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}
	}
}
