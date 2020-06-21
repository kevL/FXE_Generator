using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;
using System.Windows.Forms;


namespace lipsync_editor
{
	#region structs (global)
	/// <summary>
	/// Holds a value derived in 'TriGramTable.dat'.
	/// </summary>
	struct Trival
	{
		internal float duration;
		internal float weight;
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

		static readonly Dictionary<string, Dictionary<string, Dictionary<string, Trival>>> TriGramTable =
					new Dictionary<string, Dictionary<string, Dictionary<string, Trival>>>();
		#endregion fields (static)


		#region methods (static)
		/// <summary>
		/// Generates FXE-data.
		/// </summary>
		/// <param name="ars">a list of <see cref="OrthographicResult"/></param>
		/// <param name="fxedata">a dictionary of lists of <see cref="FxeDataBlock"/>'s
		/// mapped to viscodes</param>
		internal static void GenerateData(List<OrthographicResult> ars, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log();
			logfile.Log("FxeData.GenerateData() ars.Count= " + ars.Count);
#endif
			var visuals = new List<KeyValuePair<string, decimal>>();

			string phon;
			foreach (OrthographicResult ar in ars)
			{
#if DEBUG
				logfile.Log(". word= " + ar.Orthography);
#endif
				for (int i = 0; i != ar.Phons.Count; ++i)
				{
					if ((phon = ar.Phons[i]) != StaticData.SIL
						&& phon != "~") // nasalvowel signifier
					{
#if DEBUG
						string log = ". . " + phon + " -> ";
#endif
						if (StaticData.Vices.ContainsKey(phon)) // fudge ->
						{
							string vis = StaticData.Vices[phon];
#if DEBUG
							log += vis + " " + ar.phStops[i];
#endif
							visuals.Add(new KeyValuePair<string, decimal>(vis, ar.phStops[i]));
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

			var datablocks = new List<FxeDataBlock>();

			Trival trival;
			string vis2 = "S";
			string vis1 = "S";
			int id = 0;

			foreach (KeyValuePair<string, decimal> visual in visuals)
			{
				string vis0 =        visual.Key;	// viscode
				float stop  = (float)visual.Value;	// phStop

				trival = TriGramTable[vis2][vis1][vis0]; //GetTrival(vis2, vis1, vis0)
				float strt = Math.Max(0f, stop - trival.duration);
				float midl = Math.Max(0f, stop - trival.duration / 2f); // TODO: this could be t=0 -> should be deleted.

				var blocka = new FxeDataBlock(vis0, strt,            0f, FxeDataType.Strt, id);
				var blockb = new FxeDataBlock(vis0, midl, trival.weight, FxeDataType.Midl, id);
				var blockc = new FxeDataBlock(vis0, stop,            0f, FxeDataType.Stop, id);
#if DEBUG
				logfile.Log("viscode= [" + vis2 + "][" + vis1 + "][" + vis0 + "] trival.duration= " + trival.duration);
				logfile.Log("BLOCK A");
				logfile.Log(blocka.ToString());
				logfile.Log("BLOCK B");
				logfile.Log(blockb.ToString());
				logfile.Log("BLOCK C");
				logfile.Log(blockc.ToString());
#endif
				datablocks.Add(blocka);
				datablocks.Add(blockb);
				datablocks.Add(blockc);
				++id;

				vis2 = vis1;
				vis1 = vis0;
			}
			datablocks.Sort();

			AddDatablocks(datablocks, fxedata);
			SmoothTransitions(fxedata); // TODO - test
		}

		// The trivals are hardcoded in TrigramTable.dat and none of them check
		// true for the condition specified in this function.
/*		static Trival GetTrival(string vis2, string vis1, string vis0)
		{
#if DEBUG
			logfile.Log("FxeData.GetTrival() vis2= " + vis2 + " vis1= " + vis1 + " vis0= " + vis0);
			return TriGramTable[vis2][vis1][vis0];
#endif
			Trival trival = TriGramTable[vis2][vis1][vis0];
			if (Math.Abs(trival.length) < StaticData.EPSILON)
			{
				Trival trival0;
				foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, Trival>>> pair in TriGramTable)
				{
					trival0 = pair.Value[vis1][vis0];
					if (trival0.count > trival.count)
					{
#if DEBUG
						logfile.Log(". Remap TRIVAL");
#endif
						trival = trival0;
					}
				}
			}
			return trival;
		} */

		/// <summary>
		/// Adds lists of datablocks to an FXE-data dictionary.
		/// </summary>
		/// <param name="datablocks"></param>
		/// <param name="fxedata"></param>
		static void AddDatablocks(IList<FxeDataBlock> datablocks, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeData.AddDatablocks()");
#endif
			StaticData.CreateViscodeLists(fxedata);

			for (int i = 0; i != datablocks.Count; ++i)
			{
				FxeDataBlock datablock = datablocks[i];

				if (i != 0 && Math.Abs(datablock.Point - datablocks[i - 1].Point) < StaticData.EPSILON)
				{
					// force the x-values (stop values) to never be equal
//					if (i + 1 < datablocks.Count)
//					{
//						datablock.Point += Math.Min(StaticData.STOP_INCR,
//													(datablocks[i + 1].Point - datablock.Weight) / 2f); // TODO: wtf.
//					}
//					else
					datablock.Point += StaticData.STOP_INCR;
				}

				fxedata[datablock.Label].Add(datablock);
			}
		}

		/// <summary>
		/// Smooths viseme-transitions in a given FXE-data dictionary.
		/// </summary>
		/// <param name="fxedata"></param>
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
		/// <summary>
		/// Loads 'TriGramTable.dat'.
		/// </summary>
		internal static void LoadTrigrams()
		{
#if DEBUG
			logfile.Log();
			logfile.Log("LoadTrigrams()");
#endif
			BuildTable();
			FillValues();
//#if DEBUG
//			LogTrigramTable(); // log.
//#endif
		}

//#if DEBUG
//		/// <summary>
//		/// log funct.
//		/// @note Holy batshit what circle of hell did this table spawn ...
//		/// </summary>
//		static void LogTrigramTable()
//		{
//			Trival trival;
//			                                      Dictionary<string, Trival>   level0;
//			                   Dictionary<string, Dictionary<string, Trival>>  level1;
//			Dictionary<string, Dictionary<string, Dictionary<string, Trival>>> level2;
//
//			level2 = TriGramTable;
//			for (int i = 0; i != level2.Count; ++i)
//			{
//				logfile.Log(". level2 vis[" + i + "]= " + level2.Keys.ElementAt(i));
//
//				level1 = level2.Values.ElementAt(i);
//				for (int j = 0; j != level1.Count; ++j)
//				{
//					logfile.Log(". . level1 vis[" + j + "]= " + level1.Keys.ElementAt(j));
//
//					level0 = level1.Values.ElementAt(j);
//					for (int k = 0; k != level0.Count; ++k)
//					{
//						logfile.Log(". . . level0 vis[" + k + "]= " + level0.Keys.ElementAt(k));
//
//						trival = level0.Values.ElementAt(k);
//						logfile.Log(". . . . duration= " + trival.duration);
//						logfile.Log(". . . . weight  = " + trival.weight);
//						logfile.Log(". . . . count   = " + trival.count);
//					}
//				}
//			}
//		}
//#endif

		/// <summary>
		/// Builds the trigram-table.
		/// </summary>
		static void BuildTable()
		{
			List<string> vices = StaticData.GetStandardViscodes();
			foreach (string vis2 in vices)
			{
				var bilateral = new Dictionary<string, Dictionary<string, Trival>>();
				TriGramTable.Add(vis2, bilateral);

				foreach (string vis1 in vices)
				{
					if (vis1 != "S" || vis2 == "S")
					{
						var unilateral = new Dictionary<string, Trival>();
						bilateral.Add(vis1, unilateral);

						foreach (string vis0 in vices)
						{
							if (vis0 != "S") unilateral.Add(vis0, new Trival());
						}
					}
				}
			}
		}

		/// <summary>
		/// Fills the trigram-table with Trivals.
		/// </summary>
		static void FillValues()
		{
			string pfe = Path.Combine(Application.StartupPath, TRIGRAMTABLE);
			using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var br = new BinaryReader(fs);
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					string vices = br.ReadString();
					string[] a = vices.Split(',');

					var trival = new Trival();
					trival.duration = br.ReadSingle();
					trival.weight   = br.ReadSingle();
					trival.count    = br.ReadInt16(); // TODO: <- is suspect.
//#if DEBUG
//					logfile.Log(". vices= " + vices);
//					logfile.Log(". . length= " + trival.duration);
//					logfile.Log(". . val   = " + trival.weight);
//					logfile.Log(". . count = " + trival.count);
//#endif
					TriGramTable[a[0]][a[1]][a[2]] = trival;
				}
				br.Close();
			}
		}
		#endregion methods (trigram table)
	}
}
