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
		internal static bool LoadFxeFile(string file, Dictionary<string, List<FxeDataBlock>> fxeData)
		{
			logfile.Log("LoadFxeFile()");

			file = file.Substring(0, file.Length - 3) + FxeGeneratorF.EXT_FXE;
			if (File.Exists(file))
			{
				StaticData.AddFxeCodewords(fxeData);

				using (FileStream fs = File.Open(file, FileMode.Open))
				{
					var br = new BinaryReader(fs);
					logfile.Log(". br.length= " + br.BaseStream.Length);

					fs.Seek(85, SeekOrigin.Begin);
					string headtype = GetString(br);
					logfile.Log(". headtype= " + headtype);

					fs.Seek(34, SeekOrigin.Current);
					string wavelabel = GetString(br);
					logfile.Log(". wavelabel= " + wavelabel);

					fs.Seek(8, SeekOrigin.Current);
					short blockcount = br.ReadInt16();
					logfile.Log(". blockcount= " + blockcount);

					fs.Seek(8, SeekOrigin.Current);

					for (short i = 0; i != (short)15; ++i)
					{
						logfile.Log(". . i= " + i);

						string codeword = GetString(br);
						logfile.Log(". . codeword= " + codeword);

						fs.Seek(8, SeekOrigin.Current);							// 8 bytes of zeroes
						short datablockcount = br.ReadInt16();
						logfile.Log(". . datablockcount= " + datablockcount);

						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes

						for (short j = 0; j != datablockcount; ++j)
						{
							logfile.Log(". . . j= " + j);

							float val1 = br.ReadSingle();
							float val2 = br.ReadSingle();
							logfile.Log(". . . val1= " + val1);
							logfile.Log(". . . val2= " + val2);

							fs.Seek(10, SeekOrigin.Current);					// 10 bytes of zeroes

							var block = new FxeDataBlock(codeword, val1, val2, 0, 0);
							fxeData[codeword].Add(block);
						}
						fs.Seek(4, SeekOrigin.Current);							// 4 bytes of zeroes
					}
					br.Close();
				}
//				PopulateDataGrid();
				return true;
			}
			return false;
		}

		static string GetString(BinaryReader br)
		{
			logfile.Log("GetString() pos= " + br.BaseStream.Position);

			br.ReadInt16();
			logfile.Log(". pos of Int32= " + br.BaseStream.Position);
			int len = br.ReadInt32();
			logfile.Log(". pos of Chars= " + br.BaseStream.Position);
			logfile.Log(". length of Chars= " + len);
			string str = new string(br.ReadChars(len));
			logfile.Log(". str= " + str);
			return str;
		}
	}
}
