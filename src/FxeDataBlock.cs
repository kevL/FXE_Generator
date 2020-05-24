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
		: IComparable
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
		/// <param name="type">start, middle, or stop</param>
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
 

		#region methods
		public int CompareTo(object o)
		{
			var other = o as FxeDataBlock;

			if (other == null)
				throw new ArgumentException();


			int result = Val1.CompareTo(other.Val1);
			if (result != 0)
				return result;

			return Type.CompareTo(other.Type);
		}
		#endregion methods
	}
}
