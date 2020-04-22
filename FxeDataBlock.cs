using System;
using System.Linq;


namespace lipsync_editor
{
	public class FxeDataBlock
		: IComparable
	{
		#region fields
		#endregion fields


		#region properties
		public string Viseme
		{ get; private set; }

		public float Val1
		{ get; set; }

		public float Val2
		{ get; private set; }

		public byte Type
		{ get; private set; }

		public int Id
		{ get; private set; }
		#endregion properties


		#region methods
		public FxeDataBlock(string vis, float val1, float val2, byte type, int id)
		{
			Viseme = vis;
			Val1   = val1;
			Val2   = val2;

			Type = type;
			Id = id;
		}

		public int CompareTo(object o)
		{
			var other = o as FxeDataBlock;

			if (o == null)
				throw new ArgumentException();


			int result = Val1.CompareTo(other.Val1);
			if (result != 0)
				return result;

			return Type.CompareTo(other.Type);
		}
		#endregion methods
	}
}
