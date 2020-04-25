using System;
using System.Text;


namespace lipsync_editor
{
	/// <summary>
	/// Parses a user-typed text string into something better for the TTS
	/// (text-to-speech) Voice/Speech SAPI.
	/// </summary>
	static class TypedText
	{
		/// <summary>
		/// Parses a user-typed text string into something better for the TTS
		/// (text-to-speech) Voice/Speech SAPI.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		internal static string ParseText(string text)
		{
			//logfile.Log("ParseText() text= " + text);

			text = Spaceout(text);

			text = RemoveComment('<', '>', text);
			text = RemoveComment('{', '}', text);
			text = RemoveComment('[', ']', text);
			text = RemoveComment('|', '|', text);

			text = text.Replace("\t", " ");

			for (int i = 0; i != text.Length; ++i)
			{
				if (   !char.IsLetter(text[i])
					&& !char.IsNumber(text[i])
					&& text[i] != ' '
					&& text[i] != '\'')
				{
					text = text.Replace(text[i], ' ');
				}
			}
			return Onespace(text).ToLower().Trim();
		}

		/// <summary>
		/// No tabs, no newlines, no funny stuff - just space.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		static string Spaceout(string text)
		{
			var sb = new StringBuilder(text.Length);
			for (int i = 0; i != text.Length; ++i)
			{
				char c = text[i];
				if (char.IsWhiteSpace(c))
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
		static string Onespace(string text)
		{
			var sb = new StringBuilder(text.Length);
			for (int i = 0; i != text.Length; ++i)
			{
				char c = text[i];
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
