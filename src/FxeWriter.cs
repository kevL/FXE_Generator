using System;
using System.Collections.Generic;
using System.IO;


namespace lipsync_editor
{
	/// <summary>
	/// Writer of FXE-files.
	/// @note I have no idea where 0100010 got this format info - perhaps by
	/// reverse engineering stock FXE-files.
	/// </summary>
	static class FxeWriter
	{
		#region fields (static)
		static BinaryWriter _bw;
		#endregion fields (static)


		#region write methods (static)
		/// <summary>
		/// Writes FXE-data to a file.
		/// </summary>
		/// <param name="pfe">fullpath for the FXE-file output</param>
		/// <param name="head">skeleton head type</param>
		/// <param name="fxedata">pointer to the data</param>
		internal static void WriteFile(string pfe, string head, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeWriter.WriteFile()");
			logfile.Log(". pfe= " + pfe);
#endif
			using (var fs = new FileStream(pfe, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				_bw = new BinaryWriter(fs);

				WriteHeader();
				WriteString(head);

				_bw.Write((short)0);
				_bw.Write(0);

				WriteString("exported");

				_bw.Write(0);

				WriteString(String.Empty); // wtf

				_bw.Write(0);

				// TODO: not sure if the label should be the label of the
				// related wavefile or the label of the fxefile itself.
//				WriteString(Utility.GetFilelabel(pfe).ToLower());	// <- label of this FXE-file
				WriteString(FxeGeneratorF.Filelabel.ToLower());		// <- label of the parent/related wavefile
				// It's probably not even used because either way this label can
				// be seen as redundant.

				_bw.Write((short)3);

				long position = _bw.BaseStream.Position;
				_bw.Write(0); // <- those 4 bytes will be filled w/ '_bw.BaseStream.Length'

				_bw.Write((short)0);
				_bw.Write((short)25);
				_bw.Write(0L);

				WriteData(fxedata);
				WriteFooter(position);

				_bw.Close();
			}

			if (!FxeGeneratorF.isConsole)
				TestOutputFile(pfe);
		}

		/// <summary>
		/// Writes the header block.
		/// </summary>
		static void WriteHeader()
		{
			_bw.Write("FACE".ToCharArray());
			_bw.Write(1500);

			WriteString("Obsidian Entertainment");
			WriteString("evaluation only");

			_bw.Write((short)1000);
			_bw.Write(0L);

			WriteString("exported");

			_bw.Write(0);
		}

		/// <summary>
		/// Writes the data block.
		/// </summary>
		/// <param name="fxedata"></param>
		static void WriteData(Dictionary<string, List<FxeDataBlock>> fxedata)
		{
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in fxedata)
			{
				string viscode = pair.Key;
#if DEBUG
				logfile.Log();
				logfile.Log(". viscode= " + viscode);
#endif
				WriteString(viscode);

				_bw.Write(0L);

				List<FxeDataBlock> data = pair.Value;
//#if DEBUG
//				if (data.Count != 0)
//				{
//					logfile.Log();
//					logfile.Log("- presort");
//					foreach (FxeDataBlock datablock in data)
//						logfile.Log(datablock.ToString());
//				}
//#endif
				data.Sort(); // NOTE: Sorting does not appear to be needed.

				_bw.Write((short)data.Count);
				_bw.Write(0);

				foreach (FxeDataBlock datablock in data)
				{
#if DEBUG
					logfile.Log();
					logfile.Log("- postsort");
					logfile.Log(datablock.ToString());
#endif
					_bw.Write(datablock.Point);
					_bw.Write(datablock.Weight);
					_bw.Write((short)0);
					_bw.Write(0L);
				}
				_bw.Write(0);
			}
		}

		/// <summary>
		/// Writes the footer block.
		/// </summary>
		/// <param name="position"></param>
		static void WriteFooter(long position)
		{
			_bw.Write(4494952716784883466L); // 4,494,952,716,784,883,466 - aka 4.5 quintillion - 3E61 47AE 3E23 D70A Qua
			_bw.Write((short)0);
			_bw.Write(0);

			WriteString(String.Empty);
			WriteString(String.Empty);

			_bw.Write(-1);

			_bw.BaseStream.Seek(position, SeekOrigin.Begin);

			_bw.Write((int)_bw.BaseStream.Length);
		}

		/// <summary>
		/// Writes a string.
		/// </summary>
		/// <param name="str"></param>
		static void WriteString(string str)
		{
			_bw.Write((short)1);

			_bw.Write(str.Length);

			if (str.Length > 0)
				_bw.Write(str.ToCharArray());
		}
		#endregion write methods (static)


		#region read methods (static)
		/// <summary>
		/// Tests an FXE-file by reading it w/ <see cref="FxeReader.TestFile"/>.
		/// </summary>
		/// <param name="pfe">fullpath of FXE-file to test</param>
		static void TestOutputFile(string pfe)
		{
			string titl, info;
			if (FxeReader.TestFile(pfe))
			{
				titl = "Write SUCCESS";
				info = pfe;
			}
			else
			{
				titl = "Write FAILED";
				info = "Borked file" + Environment.NewLine + pfe;
			}

			using (var d = new InfoDialog(titl, info))
			{
				d.ShowDialog(FxeGeneratorF.That);
			}
		}
		#endregion read methods (static)
	}
}
