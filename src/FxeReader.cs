using System;
using System.Collections.Generic;
using System.IO;


namespace lipsync_editor
{
	/// <summary>
	/// Static class that reads data from an FXE-file into a list of
	/// FxeDataBlocks.
	/// </summary>
	static class FxeReader
	{
		#region fields (static)
		static BinaryReader _br;
#if DEBUG
		static bool _d; // debug.
#endif
		#endregion fields (static)


		#region write methods (static)
		/// <summary>
		/// Reads an FXE-file and stores its data in a dictionary.
		/// </summary>
		/// <param name="pfe">fullpath of the FXE-file input</param>
		/// <param name="fxedata">pointer to store the data</param>
		/// <returns>true if the FXE-file exists</returns>
		internal static bool ReadFile(string pfe, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			_d = true;
			logfile.Log("FxeReader.ReadFile()");
			logfile.Log(". pfe= " + pfe);
#endif
			if (File.Exists(pfe))
			{
				StaticData.CreateViscodeLists(fxedata);

				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					_br = new BinaryReader(fs);
#if DEBUG
					if (_d) logfile.Log(". _br.length= " + _br.BaseStream.Length);
#endif
					fs.Seek(85, SeekOrigin.Begin);
					string head = GetString();
					if (head == null) return false;
#if DEBUG
					logfile.Log(". head= " + head);
#endif
					fs.Seek(34, SeekOrigin.Current);
					string label = GetString();
					if (label == null) return false;
#if DEBUG
					logfile.Log(". label= " + label);
#endif
					fs.Seek(8, SeekOrigin.Current);
					short viscodecount = _br.ReadInt16();
#if DEBUG
					logfile.Log(". viscodecount= " + viscodecount);
#endif
					fs.Seek(8, SeekOrigin.Current);

					// Each block is identified by a viscode.
					// NOTE: there are 25 blocks but the last 10 are facial
					// gestures that are not used.
					// But they are not necessarily in the order you'd expect
					// ... the OC uses all 25 viscodes but SoZ uses the last 10
					// viscodes only.
					// TODO: bypass facial gestures - blocks #15..#24
//					for (int i = 0; i != 15; ++i)
					for (int i = 0; i != viscodecount; ++i)
					{
#if DEBUG
						if (_d)
						{
							logfile.Log();
							logfile.Log(". . i= " + i);
						}
#endif
						string viscode = GetString();
						if (viscode == null) return false;
#if DEBUG
						if (_d) logfile.Log(". . viscode= " + viscode);
#endif
						fs.Seek(8, SeekOrigin.Current);				// 8 bytes of zeroes
						short blockcount = _br.ReadInt16();
#if DEBUG
						if (_d) logfile.Log(". . blockcount= " + blockcount);
#endif
						fs.Seek(4, SeekOrigin.Current);				// 4 bytes of zeroes

						for (short j = 0; j != blockcount; ++j)
						{
#if DEBUG
							if (_d) logfile.Log(". . . j= " + j);
#endif
							float val1 = _br.ReadSingle();
							float val2 = _br.ReadSingle();
#if DEBUG
							if (_d) logfile.Log(". . . val1= " + val1);
							if (_d) logfile.Log(". . . val2= " + val2);
#endif
							fs.Seek(10, SeekOrigin.Current);		// 10 bytes of zeroes

							var block = new FxeDataBlock(viscode, val1, val2, FxeDataType.Strt, 0);
							fxedata[viscode].Add(block);
						}
						fs.Seek(4, SeekOrigin.Current);				// 4 bytes of zeroes
					}
					_br.Close();
				}
				return true;
			}
			return false;
		}

		static string GetString()
		{
#if DEBUG
			if (_d) logfile.Log("FxeReader.GetString() pos= " + _br.BaseStream.Position);
#endif
			_br.ReadInt16();

#if DEBUG
			if (_d) logfile.Log(". pos of length  = " + _br.BaseStream.Position);
#endif
			int length = _br.ReadInt32();

#if DEBUG
			if (_d) logfile.Log(". pos of Chars   = " + _br.BaseStream.Position);
			if (_d) logfile.Log(". length of Chars= " + length);
#endif
			if (_br.BaseStream.Position + length > _br.BaseStream.Length)
			{
				// TODO: issue an error w/ InfoDialog
#if DEBUG
				logfile.Log(". . _br.ReadChars() will overflow. ABORT ReadFxe");
#endif
				return null;
			}

			string str = new string(_br.ReadChars(length));
//#if DEBUG
//			if (_d) logfile.Log(); //". str= " + str
//#endif
			return str;
		}
		#endregion write methods (static)


		#region test methods (static)
		/// <summary>
		/// Reads an FXE-file as a test of its integrity only.
		/// </summary>
		/// <param name="pfe">fullpath of an FXE-file</param>
		/// <returns>true if it doesn't throw (i think)</returns>
		internal static bool TestFile(string pfe)
		{
#if DEBUG
			_d = false;
			logfile.Log("FxeReader.TestFile()");
			logfile.Log(". pfe= " + pfe);
#endif
			if (File.Exists(pfe))
			{
				using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					_br = new BinaryReader(fs);
#if DEBUG
					if (_d) logfile.Log(". _br.length= " + _br.BaseStream.Length);
#endif
					fs.Seek(85, SeekOrigin.Begin);
					string head = GetString();
					if (head == null) return false;
#if DEBUG
					logfile.Log(". head= " + head);
#endif
					fs.Seek(34, SeekOrigin.Current);
					string wavelabel = GetString();
					if (wavelabel == null) return false;
#if DEBUG
					logfile.Log(". wavelabel= " + wavelabel);
#endif
					fs.Seek(8, SeekOrigin.Current);
					short blockcount = _br.ReadInt16();
#if DEBUG
					logfile.Log(". blockcount= " + blockcount);
#endif
					fs.Seek(8, SeekOrigin.Current);

					for (int i = 0; i != 15; ++i)
					{
#if DEBUG
						if (_d) logfile.Log(". . i= " + i);
#endif
						string viscode = GetString();
						if (viscode == null) return false;
#if DEBUG
						if (_d) logfile.Log(". . viscode= " + viscode);
#endif
						fs.Seek(8, SeekOrigin.Current);				// 8 bytes of zeroes
						short datablockcount = _br.ReadInt16();
#if DEBUG
						if (_d) logfile.Log(". . datablockcount= " + datablockcount);
#endif
						fs.Seek(4, SeekOrigin.Current);				// 4 bytes of zeroes

						for (short j = 0; j != datablockcount; ++j)
						{
#if DEBUG
							if (_d) logfile.Log(". . . j= " + j);
#endif
							float val1 = _br.ReadSingle();
							float val2 = _br.ReadSingle();
#if DEBUG
							if (_d) logfile.Log(". . . val1= " + val1);
							if (_d) logfile.Log(". . . val2= " + val2);
#endif
							fs.Seek(10, SeekOrigin.Current);		// 10 bytes of zeroes
						}
						fs.Seek(4, SeekOrigin.Current);				// 4 bytes of zeroes
					}
					_br.Close();
				}
				return true;
			}
			return false;
		}
		#endregion test methods (static)
	}
}
