using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	static class FxeWriter
	{
		#region fields (static)
		static BinaryWriter _bw;
		#endregion fields (static)


		#region write methods (static)
		internal static void WriteFxeFile(string wavefile, string headtype, Dictionary<string, List<FxeDataBlock>> fxeData)
		{
			string file = wavefile.Substring(0, wavefile.Length - 3).ToLower() + FxeGeneratorF.EXT_FXE;
			using (FileStream fs = File.Open(file, FileMode.Create))
			{
				_bw = new BinaryWriter(fs);

				WriteFxeHeader();
				WriteFxeString(headtype);

				_bw.Write((short)0);
				_bw.Write(0);

				WriteFxeString("exported");

				_bw.Write(0);

				WriteFxeString(String.Empty);

				_bw.Write(0);

				int pos = wavefile.LastIndexOf('\\') + 1;
				string filelabel = wavefile.Substring(pos, wavefile.Length - pos - 4).ToLower();
				WriteFxeString(filelabel);

				_bw.Write((short)3);

				long fileLengthOffsetLocation = _bw.BaseStream.Position;

				_bw.Write(0);
				_bw.Write((short)0);
				_bw.Write((short)25);
				_bw.Write(0L);

				WriteFxeData(fxeData);
				WriteFxeFooter(fileLengthOffsetLocation);

				_bw.Close();
			}


			if (!FxeGeneratorF.isConsole)
			{
				string info;
				MessageBoxIcon icon;
				if (File.Exists(file)) // TODO: That could be a 0-length file -> error.
				{
					info = " SUCCESS";
					icon = MessageBoxIcon.Information;
				}
				else
				{
					info = " FAILED";
					icon = MessageBoxIcon.Error;
				}
				MessageBox.Show(info,
								" Write file",
								MessageBoxButtons.OK,
								icon,
								MessageBoxDefaultButton.Button1);
			}
		}

		static void WriteFxeHeader()
		{
			_bw.Write("FACE".ToCharArray());
			_bw.Write(1500);

			WriteFxeString("Obsidian Entertainment");
			WriteFxeString("evaluation only");

			_bw.Write((short)1000);
			_bw.Write(0L);

			WriteFxeString("exported");

			_bw.Write(0);
		}

		static void WriteFxeData(Dictionary<string, List<FxeDataBlock>> fxeData)
		{
			foreach (KeyValuePair<string, List<FxeDataBlock>> keyval in fxeData)
			{
				WriteFxeString(keyval.Key); // key=codeword

				_bw.Write(0L);

				List<FxeDataBlock> dataList = keyval.Value;
				_bw.Write((short)dataList.Count);

				_bw.Write(0);

				dataList.Sort();

				foreach (FxeDataBlock datablock in dataList)
				{
					_bw.Write(datablock.Val1);
					_bw.Write(datablock.Val2);
					_bw.Write((short)0);
					_bw.Write(0L);
				}
				_bw.Write(0);
			}
		}

		static void WriteFxeFooter(long fileLengthOffsetLocation)
		{
			_bw.Write(4494952716784883466L);
			_bw.Write((short)0);
			_bw.Write(0);

			WriteFxeString(String.Empty);
			WriteFxeString(String.Empty);

			_bw.Write(-1);

			_bw.BaseStream.Seek(fileLengthOffsetLocation, SeekOrigin.Begin);

			int length = (int)_bw.BaseStream.Length;
			_bw.Write(length);
		}

		static void WriteFxeString(string str)
		{
			_bw.Write((short)1);

			_bw.Write(str.Length);

			if (str.Length > 0)
				_bw.Write(str.ToCharArray());
		}
		#endregion write methods (static)
	}
}
