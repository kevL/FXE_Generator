using System;
using System.Collections.Generic;
using System.IO;


namespace lipsync_editor
{
	static class FxeWriter
	{
		#region fields (static)
		static BinaryWriter _bw;
		#endregion fields (static)


		#region write methods (static)
		internal static void WriteFile(string wavefile, string headtype, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeWriter.WriteFile()");
#endif
			string file = wavefile.Substring(0, wavefile.Length - 3).ToLower() + FxeGeneratorF.EXT_FXE;
#if DEBUG
			logfile.Log(". file= " + file);
#endif
			using (FileStream fs = File.Open(file, FileMode.Create))
			{
				_bw = new BinaryWriter(fs);

				WriteHeader();
				WriteString(headtype);

				_bw.Write((short)0);
				_bw.Write(0);

				WriteString("exported");

				_bw.Write(0);

				WriteString(String.Empty); // wtf

				_bw.Write(0);

				int pos = wavefile.LastIndexOf('\\') + 1;
				string filelabel = wavefile.Substring(pos, wavefile.Length - pos - 4).ToLower();
				WriteString(filelabel);

				_bw.Write((short)3);

				long fileLengthOffsetLocation = _bw.BaseStream.Position;

				_bw.Write(0);
				_bw.Write((short)0);
				_bw.Write((short)25);
				_bw.Write(0L);

				WriteData(fxedata);
				WriteFooter(fileLengthOffsetLocation);

				_bw.Close();
			}


			if (!FxeGeneratorF.isConsole)
			{
				string titl, info;
				if (FxeReader.TestFile(file))
				{
					titl = "Write SUCCESS";
					info = file;
				}
				else
				{
					titl = "Write FAILED";
					info = "Borked file" + Environment.NewLine + file;
				}

				var d = new InfoDialog(titl, info);
				d.ShowDialog(FxeGeneratorF.That);
			}
		}

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

		static void WriteData(Dictionary<string, List<FxeDataBlock>> fxedata)
		{
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in fxedata)
			{
				WriteString(pair.Key); // key=codeword

				_bw.Write(0L);

				List<FxeDataBlock> data = pair.Value;
				_bw.Write((short)data.Count);

				_bw.Write(0);

				data.Sort();

				foreach (FxeDataBlock datablock in data)
				{
					_bw.Write(datablock.Val1);
					_bw.Write(datablock.Val2);
					_bw.Write((short)0);
					_bw.Write(0L);
				}
				_bw.Write(0);
			}
		}

		static void WriteFooter(long fileLengthOffsetLocation)
		{
			_bw.Write(4494952716784883466L);
			_bw.Write((short)0);
			_bw.Write(0);

			WriteString(String.Empty);
			WriteString(String.Empty);

			_bw.Write(-1);

			_bw.BaseStream.Seek(fileLengthOffsetLocation, SeekOrigin.Begin);

			int length = (int)_bw.BaseStream.Length;
			_bw.Write(length);
		}

		static void WriteString(string str)
		{
			_bw.Write((short)1);

			_bw.Write(str.Length);

			if (str.Length > 0)
				_bw.Write(str.ToCharArray());
		}
		#endregion write methods (static)
	}
}
