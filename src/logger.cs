#if DEBUG
using System;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	public static class logfile
	{
		const string Logfile = "logfile.txt";

		/// <summary>
		/// Creates a logfile (overwrites the previous logfile if it exists).
		/// </summary>
		public static void CreateLog()
		{
			string pfe = Path.Combine(Application.StartupPath, Logfile);
//			string pfe = Path.Combine(Application.StartupPath, "logfile" + System.Diagnostics.Process.GetCurrentProcess().Id + ".txt");
			using (var sw = new StreamWriter(File.Open(pfe,
													   FileMode.Create,
													   FileAccess.Write,
													   FileShare.None)))
			{}
		}

		/// <summary>
		/// Writes a line to the logfile.
		/// </summary>
		/// <param name="line">the line to write</param>
		public static void Log(string line = "")
		{
			string pfe = Path.Combine(Application.StartupPath, Logfile);
//			string pfe = Path.Combine(Application.StartupPath, "logfile" + System.Diagnostics.Process.GetCurrentProcess().Id + ".txt");
			using (var sw = new StreamWriter(File.Open(pfe,
													   FileMode.Append,
													   FileAccess.Write,
													   FileShare.None)))
			{
				sw.WriteLine(line);
			}
		}
	}
}
#endif
