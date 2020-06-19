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


	/// <summary>
	/// Data for a vis.
	/// </summary>
	sealed class FxeDataBlock
		: IComparable
	{
		#region properties
		/// <summary>
		/// The viscode.
		/// </summary>
		internal string Label
		{ get; private set; }

		float _val1;
		/// <summary>
		/// The stop.
		/// </summary>
		internal float Val1
		{
			get { return _val1; }
			set
			{
				int val = (int)(value * 1000);
				_val1 = (float)val / 1000f;
			}
		}

		float _val2;
		/// <summary>
		/// The morph-weight.
		/// </summary>
		internal float Val2
		{
			get { return _val2; }
			private set
			{
				int val = (int)(value * 100);
				_val2 = (float)val / 100f;
			}
		}

		/// <summary>
		/// Type is not written to file and seems to be used only for sorting in
		/// the rare case that two FxeDataBlocks have the same value of Val1.
		/// </summary>
		internal FxeDataType Type
		{ get; private set; }

		/// <summary>
		/// Id is not written to file and seems to be used only by the
		/// <see cref="Smoother"/>.
		/// </summary>
		internal int Id
		{ get; private set; }
		#endregion properties


		#region cTor
		/// <summary>
		/// cTor. 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <param name="type">start, middle, or stop</param>
		/// <param name="id"></param>
		internal FxeDataBlock(string label, float val1, float val2, FxeDataType type, int id)
		{
			Label = label;
			Val1  = val1;
			Val2  = val2;

			Type = type;
			Id   = id;
		}
		#endregion cTor
 

		#region methods (interface)
		/// <summary>
		/// Used by FxeWriter.WriteData() to sort lists of datablocks.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
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
		#endregion methods (interface)


		#region methods (override)
		/// <summary>
		/// Overrides object.ToString() - is used only for log.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "Label= "  + Label + Environment.NewLine
				 + ". Val1= " + Val1  + Environment.NewLine
				 + ". Val2= " + Val2  + Environment.NewLine
				 + ". Type= " + Type  + Environment.NewLine
				 + ". Id= "   + Id;
		}
		#endregion methods (override)
	}
}
