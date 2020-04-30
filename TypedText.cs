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
		internal static string SanitizeDialogText(string text)
		{
			text = Spaceout(text);

			text = RemoveComment('<', '>', text);
			text = RemoveComment('{', '}', text);
			text = RemoveComment('[', ']', text);
			text = RemoveComment('|', '|', text);

			return ConflateSpaces(text).Trim();
		}

/*		/// <summary>
		/// Parses a user-typed text string into something better for the TTS
		/// (text-to-speech) Voice/Speech SAPI.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="keepPunctuation"></param>
		/// <returns></returns>
		internal static string ParseText(string text, bool keepPunctuation = false)
		{
			//logfile.Log("ParseText() text= " + text);

			text = Spaceout(text);

			text = RemoveComment('<', '>', text);
			text = RemoveComment('{', '}', text);
			text = RemoveComment('[', ']', text);
			text = RemoveComment('|', '|', text);

			var sb = new StringBuilder(text.Length);
			char c;
			for (int i = 0; i != text.Length; ++i)
			{
				c = text[i];
				if (!Char.IsControl(c))
				{
					if (   Char.IsLetter(c)
						|| Char.IsNumber(c)
						|| Char.IsDigit(c)
						|| Char.IsSymbol(c)
						|| c == ' '
						|| c == '\''
						|| (keepPunctuation && Char.IsPunctuation(c)))
					{
						sb.Append(c);
					}
				}
			}
			text = ConflateSpaces(sb.ToString()).Trim();

			if (!keepPunctuation)
				text = text.ToLower();

			return text;
		} */

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
					sb.Append(' ');
				else
					sb.Append(c);
			}
			return sb.ToString();
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
				c = text[i];
				if (c != ' ' || i == 0 || text[i - 1] != ' ')
					sb.Append(c);
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
	}
}
