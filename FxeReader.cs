using System;
using System.Collections.Generic;
using System.IO;


namespace lipsync_editor
{
	/// <summary>
	/// Static class that reads data from an FXE file into a list of
	/// FxeDataBlocks.
	/// </summary>
	static class FxeReader
	{
		#region fields (static)
		static BinaryReader _br;

		static bool _d; // debug.
		#endregion fields (static)


		#region write methods (static)
		internal static bool ReadFile(string file, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
			_d = false;
			logfile.Log("FxeReader.ReadFile()");

			file = file.Substring(0, file.Length - 3) + FxeGeneratorF.EXT_FXE;
			logfile.Log(". file= " + file);

			if (File.Exists(file))
			{
				StaticData.AddCodewords(fxedata);

				using (FileStream fs = File.Open(file, FileMode.Open))
				{
					_br = new BinaryReader(fs);
					if (_d) logfile.Log(". _br.length= " + _br.BaseStream.Length);

					fs.Seek(85, SeekOrigin.Begin);
					string headtype = GetString();
					if (headtype == null) return false;
					logfile.Log(". headtype= " + headtype);

					fs.Seek(34, SeekOrigin.Current);
					string wavelabel = GetString();
					if (wavelabel == null) return false;
					logfile.Log(". wavelabel= " + wavelabel);

					fs.Seek(8, SeekOrigin.Current);
					short blockcount = _br.ReadInt16();
					logfile.Log(". blockcount= " + blockcount);

					fs.Seek(8, SeekOrigin.Current);

					for (int i = 0; i != 15; ++i)
					{
						if (_d) logfile.Log(". . i= " + i);

						string codeword = GetString();
						if (codeword == null) return false;
						if (_d) logfile.Log(". . codeword= " + codeword);

						fs.Seek(8, SeekOrigin.Current);							// 8 bytes of zeroes
						short datablockcount = _br.ReadInt16();
						if (_d) logfile.Log(". . datablockcount= " + datablockcount);

						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes

						for (short j = 0; j != datablockcount; ++j)
						{
							if (_d) logfile.Log(". . . j= " + j);

							float val1 = _br.ReadSingle();
							float val2 = _br.ReadSingle();
							if (_d) logfile.Log(". . . val1= " + val1);
							if (_d) logfile.Log(". . . val2= " + val2);

							fs.Seek(10, SeekOrigin.Current);					// 10 bytes of zeroes

							var block = new FxeDataBlock(codeword, val1,val2, 0,0);
							fxedata[codeword].Add(block);
						}
						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes
					}
					_br.Close();
				}
				return true;
			}
			return false;
		}

		static string GetString()
		{
			if (_d) logfile.Log("FxeReader.GetString() pos= " + _br.BaseStream.Position);

			_br.ReadInt16();

			if (_d) logfile.Log(". pos of length  = " + _br.BaseStream.Position);
			int len = _br.ReadInt32();

			if (_d) logfile.Log(". pos of Chars   = " + _br.BaseStream.Position);
			if (_d) logfile.Log(". length of Chars= " + len);

			if (_br.BaseStream.Position + len > _br.BaseStream.Length)
			{
				logfile.Log(". . _br.ReadChars() will overflow. ABORT ReadFxe");
				return null;
			}

			string str = new string(_br.ReadChars(len));
			if (_d) logfile.Log(); //". str= " + str

			return str;
		}
		#endregion write methods (static)
	}
}
