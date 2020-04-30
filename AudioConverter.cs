using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	static class AudioConverter
	{
		#region fields (static)
		const string EXT_MP3 = ".mp3";
		const string EXT_WAV = ".wav";
		const string TMP_MP3 = "sapi_lipsync" + EXT_MP3;
		const string TMP_WAV = "sapi_lipsync" + EXT_WAV;

		const string LAME_EXE = "lame.exe";
		#endregion fields (static)


		#region methods (static)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		internal static string deterAudiopath(string file)
		{
			//logfile.Log("AudioConverter.deterAudiopath() file= " + file);

			string fullpath = String.Empty;

			string pathT = Path.GetTempPath();
			//logfile.Log(". path= " + pathT);

			if (file.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // prep .BMU ->
			{
				var fi = new FileInfo(file);
				var br = new BinaryReader(fi.OpenRead());

				char[] c = br.ReadChars(3);
				br.Close();

				if (   c[0] == 'B' // because .BMUs are .MP3s and NwN2 labels them as .WAVs
					&& c[1] == 'M'
					&& c[2] == 'U')
				{
					file = Path.Combine(pathT, TMP_MP3); // so label it as .MP3 and allow the next block to catch it.
					//logfile.Log(". file= " + file);

					fi.CopyTo(file, true);
				}
			}

			if (file.EndsWith(EXT_MP3, StringComparison.InvariantCultureIgnoreCase)) // convert to .WAV file ->
			{
				string waveT = Path.Combine(pathT, TMP_WAV);
				//logfile.Log(". wave= " + waveT);


				if (File.Exists(waveT))
					File.Delete(waveT);

//				string execpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//				var info = new ProcessStartInfo(Path.Combine(execpath, LAME_EXE));
				var info = new ProcessStartInfo(Path.Combine(Application.StartupPath, LAME_EXE));
				info.Arguments = "--decode \"" + file + "\" \"" + waveT + "\"";
				info.WindowStyle = ProcessWindowStyle.Hidden;
				info.UseShellExecute = false;
				info.CreateNoWindow  = true;

				using (Process proc = Process.Start(info))
				{
					proc.WaitForExit();
				}

//				string t = Path.Combine(pathT, TMP_MP3);
//				logfile.Log(". t= " + t);
//				if (File.Exists(t))
//					File.Delete(t);

				file = waveT;
			}

// http://www.topherlee.com/software/pcm-tut-wavformat.html
//  1- 4	"RIFF"				Marks the file as a riff file. Characters are each 1 byte long.
//  5- 8	File size (integer)	Size of the overall file - 8 bytes, in bytes (32-bit integer). Typically, you'd fill this in after creation.
//  9-12	"WAVE"				File Type Header. For our purposes, it always equals "WAVE".
// 13-16	"fmt "				Format chunk marker. Includes trailing null
// 17-20	16					Length of format data as listed above
// 21-22	1					Type of format (1 is PCM) - 2 byte integer
// 23-24	2					Number of Channels - 2 byte integer
// 25-28	44100				Sample Rate - 32 byte integer. Common values are 44100 (CD), 48000 (DAT). Sample Rate = Number of Samples per second, or Hertz.
// 29-32	176400				(Sample Rate * BitsPerSample * Channels) / 8.
// 33-34	4					(BitsPerSample * Channels) / 8.1 - 8 bit mono2 - 8 bit stereo/16 bit mono4 - 16 bit stereo
// 35-36	16					Bits per sample
// 37-40	"data"				"data" chunk header. Marks the beginning of the data section.
// 41-44	File size (data)	Size of the data section.

			if (file.EndsWith(EXT_WAV, StringComparison.InvariantCultureIgnoreCase)) // check .WAV ->
			{
				var fi = new FileInfo(file);
				var br = new BinaryReader(fi.OpenRead());

				char[] c = br.ReadChars(16);					// start 0

				if (   c[ 0] == 'R' && c[ 1] == 'I' && c[ 2] == 'F' && c[ 3] == 'F'
					&& c[ 8] == 'W' && c[ 9] == 'A' && c[10] == 'V' && c[11] == 'E'
					&& c[12] == 'f' && c[13] == 'm' && c[14] == 't' && c[15] == ' ')
				{
					br.ReadBytes(4);							// start 16

					short format = br.ReadInt16();				// start 20: is PCM
					if (format == 1)
					{
						short channels = br.ReadInt16();		// start 22: is Mono
						if (channels == 1)
						{
							int rate = br.ReadInt32();			// start 24: is 44.1kHz
							if (rate == 44100)
							{
								br.ReadBytes(6);				// start 28
								short bits = br.ReadInt16();	// start 34: is 16-bit
								if (bits == 16)
								{
									fullpath = file;
									//logfile.Log(". AudioConverter.fullpath= " + fullpath);
								}
							}
						}
					}
				}
				br.Close();
			}

			if (!FxeGeneratorF.isConsole && fullpath == String.Empty)
			{
				MessageBox.Show(" Failed to convert to 44.1kHz 16-bit Mono PCM-wave format.",
								" Conversion failed",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1);
			}

			return fullpath;
		}
		#endregion methods (static)
	}
}
