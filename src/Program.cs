﻿using System;
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
#if DEBUG
			logfile.CreateLog(); // NOTE: The logfile works in debug-builds only.
			// To write a line to the logfile:
			// logfile.Log("what you want to know string");
			//
			// The logfile ought appear in the directory with the executable.
#endif

			if (args.Length == 2) // TODO: test that.
			{
				var kittencuddles = new FxeGeneratorF(args[0], args[1]);
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
