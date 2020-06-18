using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace lipsync_editor
{
	#region structs (global)
	/// <summary>
	/// Holds a value derived in 'TriGramTable.dat'.
	/// </summary>
	struct Trival
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

		static readonly Dictionary<string, Dictionary<string, Dictionary<string, Trival>>> TriGramTable =
					new Dictionary<string, Dictionary<string, Dictionary<string, Trival>>>();
		#endregion fields (static)


		#region methods (static)
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
							log += vis;
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
				string vis0 = visual.Key;

				float stop = (float)visual.Value;

				trival = GetTrival(vis2, vis1, vis0);
				float strt = stop - trival.length;
				float midl = strt + trival.length / 2f;

				datablocks.Add(new FxeDataBlock(vis0, strt,         0f, FxeDataType.Strt, id));
				datablocks.Add(new FxeDataBlock(vis0, midl, trival.val, FxeDataType.Midl, id));
				datablocks.Add(new FxeDataBlock(vis0, stop,         0f, FxeDataType.Stop, id));
				++id;

				vis2 = vis1;
				vis1 = vis0;
			}
			datablocks.Sort();

			AddDatablocks(datablocks, fxedata);
			SmoothTransitions(fxedata);
		}

		static Trival GetTrival(string vis2, string vis1, string vis0)
		{
#if DEBUG
			logfile.Log("FxeData.GetTrival() vis2= " + vis2 + " vis1= " + vis1 + " vis0= " + vis0);
#endif
			Trival trival = TriGramTable[vis2][vis1][vis0];
			if (Math.Abs(trival.length) < StaticData.EPSILON)
			{
				Trival trival0;
				foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, Trival>>> pair in TriGramTable)
				{
					trival0 = pair.Value[vis1][vis0];
					if (trival0.count > trival.count)
						trival = trival0;
				}
			}
			return trival;
		}

		static void AddDatablocks(IList<FxeDataBlock> datablocks, Dictionary<string, List<FxeDataBlock>> fxedata)
		{
#if DEBUG
			logfile.Log("FxeData.AddDatablocks()");
#endif
			StaticData.MapFxeViscodes(fxedata);

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
													   (datablocks[i + 1].Val1 - datablock.Val2) / 2f); // TODO: wtf.
						}
						else
							datablock.Val1 += StaticData.STOP_INCR;
					}
				}

				fxedata[datablock.Label].Add(datablock);
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

			string pfe = Path.Combine(Application.StartupPath, TRIGRAMTABLE);
			using (var fs = new FileStream(pfe, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var br = new BinaryReader(fs);
				while (br.BaseStream.Position < br.BaseStream.Length)
				{
					string[] vices = br.ReadString().Split(',');

					var trival = new Trival();
					trival.length = br.ReadSingle();
					trival.val    = br.ReadSingle();
					trival.count  = br.ReadInt16();

					TriGramTable[vices[0]][vices[1]][vices[2]] = trival;
				}
				br.Close();
			}
		}

		static void InitTrigrams()
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
		#endregion methods (trigram table)
	}
}
