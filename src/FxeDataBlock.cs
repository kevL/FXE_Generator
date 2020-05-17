using System;


namespace lipsync_editor
{
	sealed class FxeDataBlock
		: IComparable
	{
		#region properties
		internal string Viseme
		{ get; private set; }

		internal float Val1
		{ get; set; }

		internal float Val2
		{ get; private set; }

		internal byte Type
		{ get; private set; }

		internal int Id
		{ get; private set; }
		#endregion properties


		#region cTor
		internal FxeDataBlock(string vis, float val1, float val2, byte type, int id)
		{
			Viseme = vis;
			Val1   = val1;
			Val2   = val2;

			Type = type;
			Id = id;
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
