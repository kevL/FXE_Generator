using System;


namespace lipsync_editor
{
	static class Utility
	{
		internal static decimal millisecs(ulong nanoseconds_100)
		{
			return ((decimal)nanoseconds_100 / 10000000);
		}

		internal static string GetFilelabel(string file)
		{
			int pos = file.LastIndexOf('\\') + 1;
			if (pos != 0)
				file = file.Substring(pos, file.Length - pos);

			if ((pos = file.LastIndexOf('.')) != -1)
				file = file.Substring(0, pos);

			return file;
		}

		internal static bool isWordstart(string pos)
		{
			return pos.EndsWith(".0", StringComparison.OrdinalIgnoreCase);
		}
	}
}
