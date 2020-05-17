﻿using System;
using System.Collections.Generic;
using System.IO;


namespace lipsync_editor
{
	#region structs (global)
	/// <summary>
	/// For values of 'TriGramTable.dat'
	/// </summary>
	struct DataVal
	{
		internal float length;
		internal float val;
		internal short count;
	}
	#endregion structs (global)


	/// <summary>
	/// Static class for FXE stuff.
	/// </summary>
	static class FxeData
	{
		#region fields (static)
		const string TRIGRAMTABLE = "TriGramTable.dat";

		static readonly Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>> TriGramTable =
					new Dictionary<string, Dictionary<string, Dictionary<string, DataVal>>>();
		#endregion fields (static)


		#region methods (static)
		internal static void GenerateData(List<OrthographicResult> ars, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("FxeData.GenerateData() ars.Count= " + ars.Count);
#endif
			var vices = new List<KeyValuePair<string, decimal>>();

			string phon;
			foreach (OrthographicResult ar in ars)
			{
#if DEBUG
				logfile.Log(". word= " + ar.Orthography);
#endif
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
					if ((phon = ar.Phons[i]) != "x"
						&& phon != "~") // French dohickey
					{
#if DEBUG
						string log = ". . " + phon + " -> ";
#endif
						if (StaticData.Vices.ContainsKey(phon)) // fudge ->
						{
							string vis = StaticData.Vices[phon];
#if DEBUG
							log += vis;
#endif
							decimal stop = (decimal)ar.Stops[i] / 10000000;
							vices.Add(new KeyValuePair<string, decimal>(vis, stop));
						}
#if DEBUG
						else log += "INVALID";
						logfile.Log(log);
#endif
					}
				}
			}

#if DEBUG
			logfile.Log();
#endif

			var datablocks = new List<FxeDataBlock>(); // viseme start, mid, end points

			DataVal dataval;
			string c2 = "S";
			string c1 = "S";
			int id = 0;

			foreach (KeyValuePair<string, decimal> vis in vices)
			{
				string c0 = vis.Key;

				float stop = (float)vis.Value;

				dataval = GetTrigramValue(c2, c1, c0);
				float strt = stop - dataval.length;
				float midl = strt + dataval.length / 2f;

				datablocks.Add(new FxeDataBlock(c0, strt,          0f, (byte)0, id));
				datablocks.Add(new FxeDataBlock(c0, midl, dataval.val, (byte)1, id));
				datablocks.Add(new FxeDataBlock(c0, stop,          0f, (byte)2, id));
				++id;

				c2 = c1;
				c1 = c0;
			}
			datablocks.Sort();

			AddDatablocks(datablocks, fxedata);
			SmoothTransitions(fxedata);
		}

		static DataVal GetTrigramValue(string c2, string c1, string c0)
		{
#if DEBUG
			logfile.Log("FxeData.GetTrigramValue() c2= " + c2 + " c1= " + c1 + " c0= " + c0);
#endif
			DataVal dataval = TriGramTable[c2][c1][c0];
			if (Math.Abs(dataval.length) < StaticData.EPSILON)
			{
				DataVal dataval0;
				foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, DataVal>>> pair in TriGramTable)
				{
					dataval0 = pair.Value[c1][c0];
					if (dataval0.count > dataval.count)
						dataval = dataval0;
				}
			}
			return dataval;
		}

		static void AddDatablocks(IList<FxeDataBlock> datablocks, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeData.AddDatablocks()");
#endif
			StaticData.AddCodewords(fxedata);

			FxeDataBlock datablock0 = null;

			for (int i = 0; i != datablocks.Count; ++i)
			{
				FxeDataBlock datablock = datablocks[i];

				if (datablock0 != null)
				{
					if (Math.Abs(datablock.Val1 - datablock0.Val1) < StaticData.EPSILON)
					{
						// force the x-values (stop values) to never be equal
						if (i + 1 < datablocks.Count)
						{
							datablock.Val1 += Math.Min(StaticData.STOP_INCR,
													  (datablocks[i + 1].Val1 - datablock.Val2) / 2f);
						}
						else
							datablock.Val1 += StaticData.STOP_INCR;
					}
				}

				fxedata[datablock.Viseme].Add(datablock);
				datablock0 = datablock;
			}
		}

		static void SmoothTransitions(Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeData.SmoothTransitions()");
#endif
			foreach (KeyValuePair<string, List<FxeDataBlock>> pair in fxedata)
			{
				if (pair.Value.Count > 0) Smoother.Apply(pair);
			}
		}
		#endregion methods (static)


		#region methods (trigram table)
		internal static void LoadTrigrams()
		{
			InitTrigrams();

			using (FileStream fs = File.OpenRead(TRIGRAMTABLE))
			{
				var br = new BinaryReader(fs);
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					string[] codewords = br.ReadString().Split(',');

					var dataval = new DataVal();
					dataval.length = br.ReadSingle();
					dataval.val    = br.ReadSingle();
					dataval.count  = br.ReadInt16();

					TriGramTable[codewords[0]][codewords[1]][codewords[2]] = dataval;
				}
				br.Close();
			}
		}

		static void InitTrigrams()
		{
			List<string> codewords = StaticData.GetCodewords();
			foreach (string c2 in codewords)
			{
				var bigram = new Dictionary<string, Dictionary<string, DataVal>>();
				TriGramTable.Add(c2, bigram);

				foreach (string c1 in codewords)
				{
					if (c1 != "S" || c2 == "S")
					{
						var unigram = new Dictionary<string, DataVal>();
						bigram.Add(c1, unigram);

						foreach (string c0 in codewords)
						{
							if (c0 != "S")
							{
								unigram.Add(c0, new DataVal());
							}
						}
					}
				}
			}
		}
		#endregion methods (trigram table)
	}
}