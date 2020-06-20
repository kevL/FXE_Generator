using System;
using System.Collections.Generic;


namespace lipsync_editor
{
	/// <summary>
	/// 
	/// </summary>
	static class Smoother
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pair"></param>
		internal static void Apply(KeyValuePair<string, List<FxeDataBlock>> pair)
		{
			string vis                    = pair.Key;
			List<FxeDataBlock> datablocks = pair.Value;

			FxeDataBlock datablock;

			int   id = datablocks[0].Id;
			float Ax = datablocks[0].Point;
			float Ay = datablocks[0].Weight;

			for (int i = 1; i < datablocks.Count - 1; ++i)
			{
				datablock = datablocks[i];
				if (id != datablock.Id)
				{
					int Cindex = i;

					int Aindex = i - 1;

					float Bx,By;

					int Bindex = GetNextPoint(datablocks, Aindex, out Bx, out By);
					if (Bindex == -1)
					{
						id = datablock.Id;
						Ax = datablock.Point;
						Ay = datablock.Weight;

						continue;
					}

					float Cx = datablock.Point;
					float Cy = datablock.Weight;

					float Dx,Dy;

					int Dindex = GetNextPoint(datablocks, Cindex, out Dx, out Dy);
					if (Dindex == -1
						&& Ay > 0f) // delete C
					{
						datablocks.RemoveAt(i);
						--i;

						continue;
					}

					float x = 0f;
					float y = 0f;

					if (GetLineIntersection(Ax,Ay, Bx,By, Cx,Cy, Dx,Dy, ref x, ref y))
					{
						InsertIntersectionPoint(datablocks,
												new FxeDataBlock(vis, x, y, FxeDataType.Midl, id),
												i + 1);

						if (floatsequal(Ay, 0f) && Cy > y) // delete A
						{
							datablocks.RemoveAt(i - 1);
							--i;

							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;

							continue;
						}

						if (Cy < y) // delete C
						{
							datablocks.RemoveAt(i);
							--i;
						}
						else
						{
							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;
						}
					}
					else // lines do not cross
					{
						if (Ay < Cy && Bx < Dx) // delete A
						{
							datablocks.RemoveAt(i - 1);
							--i;

							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;

							continue;
						}

						if ((Dx < Bx && Dy < By) // kL_note: not sure about that refactor.
							|| floatsequal(Dy, 0f)
							|| floatsequal(By, 0f)) // delete C
						{
							datablocks.RemoveAt(i);
							--i;
						}
						else
						{
							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;
						}
					}
				}
				else // IDs equal
				{
					id = datablock.Id;
					Ax = datablock.Point;
					Ay = datablock.Weight;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="datablocks"></param>
		/// <param name="id"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static int GetNextPoint(IList<FxeDataBlock> datablocks, int id, out float x, out float y)
		{
			if (datablocks[id].Type != FxeDataType.Stop)
			{
				int blockid = datablocks[id].Id;

				++id;
				while (id < datablocks.Count)
				{
					FxeDataBlock datablock = datablocks[id];
					if (datablock.Id == blockid)
					{
						x = datablock.Point;
						y = datablock.Weight;

						return id;
					}
					++id;
				}
			}

			x = y = -1f;
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Ax"></param>
		/// <param name="Ay"></param>
		/// <param name="Bx"></param>
		/// <param name="By"></param>
		/// <param name="Cx"></param>
		/// <param name="Cy"></param>
		/// <param name="Dx"></param>
		/// <param name="Dy"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		static bool GetLineIntersection(float Ax, float Ay,
										float Bx, float By,
										float Cx, float Cy,
										float Dx, float Dy,
										ref float x, ref float y)
		{
			// Fail if either line segment is zero-length.
			if (   (floatsequal(Ax, Bx) && floatsequal(Ay, By))
				|| (floatsequal(Cx, Dx) && floatsequal(Cy, Dy)))
			{
				return false;
			}

			// Fail if the segments share an end-point.
			if (   (floatsequal(Ax, Cx) && floatsequal(Ay, Cy))
				|| (floatsequal(Bx, Cx) && floatsequal(By, Cy))
				|| (floatsequal(Ax, Dx) && floatsequal(Ay, Dy))
				|| (floatsequal(Bx, Dx) && floatsequal(By, Dy)))
			{
				return false;
			}

			// (1) Translate the system so that point A is on the origin.
			Bx -= Ax;
			By -= Ay;
			Cx -= Ax;
			Cy -= Ay;
			Dx -= Ax;
			Dy -= Ay;

			// Discover the length of segment A-B.
			double AB_dist = Math.Sqrt(Bx * Bx + By * By);

			// (2) Rotate the system so that point B is on the positive x-axis.
			double cos = Bx / AB_dist;
			double sin = By / AB_dist;

			double x1 =  Cx * cos + Cy * sin;
			Cy = (float)(Cy * cos - Cx * sin);
			Cx = (float)x1;

			x1 =         Dx * cos + Dy * sin;
			Dy = (float)(Dy * cos - Dx * sin);
			Dx = (float)x1;

			// Fail if segment C-D doesn't cross line A-B.
			if (   (Cy <  0f && Dy <  0f)
				|| (Cy >= 0f && Dy >= 0f))
			{
				return false;
			}

			// (3) Discover the position of the intersection point along line A-B.
			double AB_pos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

			// Fail if segment C-D crosses line A-B outside of segment A-B.
			if (AB_pos < 0f || AB_pos > AB_dist)
			{
				return false;
			}

			// (4) Apply the discovered position to line A-B in the original coordinate system.
			x = (float)(Ax + AB_pos * cos);
			y = (float)(Ay + AB_pos * sin);

			// Success.
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="datablocks"></param>
		/// <param name="datablock"></param>
		/// <param name="id"></param>
		static void InsertIntersectionPoint(IList<FxeDataBlock> datablocks, FxeDataBlock datablock, int id)
		{
			while (datablock.Point > datablocks[id].Point && id < datablocks.Count)
				++id;

			if (id < datablocks.Count)
				datablocks.Insert(id, datablock);
			else
				datablocks.Add(datablock);
		}


		/// <summary>
		/// Generic function that checks if two floats are equal.
		/// </summary>
		/// <param name="f1">1st float</param>
		/// <param name="f2">2nd float</param>
		/// <returns>true if equal</returns>
		static bool floatsequal(float f1, float f2)
		{
			return Math.Abs(f2 - f1) < StaticData.EPSILON;
		}
	}
}
