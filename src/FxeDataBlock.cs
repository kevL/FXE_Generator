using System;


namespace lipsync_editor
{
	enum FxeDataType
		: byte
	{
		Strt,	// 0
		Midl,	// 1
		Stop	// 2
	}


	sealed class FxeDataBlock
	{
		#region properties
		internal string Vis
		{ get; private set; }

		internal float Val1
		{ get; set; }

		internal float Val2
		{ get; private set; }

		internal FxeDataType Type
		{ get; private set; }

		internal int Id
		{ get; private set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor. 
		/// </summary>
		/// <param name="vis"></param>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <param name="type"></param>
		/// <param name="id"></param>
		internal FxeDataBlock(string vis, float val1, float val2, FxeDataType type, int id)
		{
			Vis  = vis;
			Val1 = val1;
			Val2 = val2;

			Type = type;
			Id   = id;
		}
		#endregion cTor
	}
}
