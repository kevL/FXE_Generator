using System;
using System.Windows.Forms;


namespace lipsync_editor
{
	static class Program
	{
		/// <summary>
		/// You are here.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			logfile.CreateLog(); // NOTE: The logfile works in debug-builds only.
			// To write a line to the logfile:
			// logfile.Log("what you want to know here");
			//
			// The logfile ought appear in the directory with the executable.


			if (args.Length == 2)
			{
				var fxeGen = new FxeGeneratorF(args[0], args[1]);
				Application.Run();
			}
			else
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FxeGeneratorF());
			}
		}
	}
}
