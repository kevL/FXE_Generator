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

		float _dur;
		/// <summary>
		/// The duration in seconds truncated to millisecond precision.
		/// </summary>
		internal float Duration
		{
			get { return _dur; }
			set
			{
				int val = (int)(value * 1000);
				_dur = (float)val / 1000f;
			}
		}

		float _weight;
		/// <summary>
		/// The morph-weight truncated to hundredths precision.
		/// </summary>
		internal float Weight
		{
			get { return _weight; }
			private set
			{
				int val = (int)(value * 100);
				_weight = (float)val / 100f;
			}
		}

		/// <summary>
		/// Type is not written to file and seems to be used only for sorting in
		/// the rare case that two FxeDataBlocks have the same value of Duration.
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
		/// <param name="label">viscode</param>
		/// <param name="duration">duration in seconds</param>
		/// <param name="weight">morph-weight</param>
		/// <param name="type">start, middle, or stop</param>
		/// <param name="id">group identifier</param>
		internal FxeDataBlock(string label, float duration, float weight, FxeDataType type, int id)
		{
			Label    = label;
			Duration = duration;
			Weight   = weight;

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


			int result = Duration.CompareTo(other.Duration);
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
			return "Label= "      + Label    + Environment.NewLine
				 + ". Duration= " + Duration + Environment.NewLine
				 + ". Weight= "   + Weight   + Environment.NewLine
				 + ". Type= "     + Type     + Environment.NewLine
				 + ". Id= "       + Id;
		}
		#endregion methods (override)


		internal string GetTypeString()
		{
			switch (Type)
			{
				case FxeDataType.Strt: return "a";
				case FxeDataType.Midl: return "b";
				case FxeDataType.Stop: return "c";
			}
			return "-"; // error.
		}
	}
}
