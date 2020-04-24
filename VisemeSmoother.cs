using System;
using System.Collections.Generic;


namespace lipsync_editor
{
	static class VisemeSmoother
	{
		internal static void Smooth(string vis, List<FxeDataBlock> datablocks)
		{
//			var linesById = new Dictionary<int, List<KeyValuePair<float,float>>>();

			FxeDataBlock datablock;

			int   id = datablocks[0].Id;
			float Ax = datablocks[0].Val1;
			float Ay = datablocks[0].Val2;

			for (int i = 1; i < datablocks.Count - 1; ++i)
			{
				datablock = datablocks[i];
				if (id != datablock.Id)
				{
					int Cindex = i;

					int Aindex = i - 1;

					float Bx = 0F;
					float By = 0F;

					int Bindex = GetNextPoint(datablocks, Aindex, ref Bx, ref By);
					if (Bindex == -1)
					{
						id = datablock.Id;
						Ax = datablock.Val1;
						Ay = datablock.Val2;

						continue;
					}

					float Cx = datablock.Val1;
					float Cy = datablock.Val2;

					float Dx = 0F;
					float Dy = 0F;

					int Dindex = GetNextPoint(datablocks, Cindex, ref Dx, ref Dy);
					if (Dindex == -1)
					{
						if (Ay > 0F) // remove c
						{
							datablocks.RemoveAt(i);
							--i;

							continue;
						}
					}

					float x = 0F;
					float y = 0F;

					if (GetLineIntersection(Ax,Ay, Bx,By, Cx,Cy, Dx,Dy, ref x, ref y))
					{
						InsertIntersectionPoint(datablocks,
												new FxeDataBlock(vis, x, y, 1, id),
												i + 1);

						if (floatsequal(Ay, 0F) && Cy > y) // remove a
						{
							datablocks.RemoveAt(i - 1);
							--i;

							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;

							continue;
						}

						if (Cy < y) // remove c
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
						if (Ay < Cy && Bx < Dx) // remove a
						{
							datablocks.RemoveAt(i - 1);
							--i;

							id = datablock.Id;
							Ax = Cx;
							Ay = Cy;

							continue;
						}

						if ((Dx < Bx && Dy < By) // kL_note: not sure about that refactor.
							|| floatsequal(Dy, 0F)
							|| floatsequal(By, 0F)) // remove c
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
					Ax = datablock.Val1;
					Ay = datablock.Val2;
				}
			}
		}

		static int GetNextPoint(IList<FxeDataBlock> datablocks, int startId, ref float x, ref float y)
		{
			x = -1;
			y = -1;

			if (datablocks[startId].Type != 2)
			{
				int id = datablocks[startId].Id;

				++startId;
				while (startId < datablocks.Count)
				{
					FxeDataBlock datablock = datablocks[startId];
					if (datablock.Id == id)
					{
						x = datablock.Val1;
						y = datablock.Val2;

						return startId;
					}
					++startId;
				}
			}
			return -1;
		}

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
			if (   (Cy <  0F && Dy <  0F)
				|| (Cy >= 0F && Dy >= 0F))
			{
				return false;
			}

			// (3) Discover the position of the intersection point along line A-B.
			double AB_pos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

			// Fail if segment C-D crosses line A-B outside of segment A-B.
			if (AB_pos < 0F || AB_pos > AB_dist)
			{
				return false;
			}

			// (4) Apply the discovered position to line A-B in the original coordinate system.
			x = (float)(Ax + AB_pos * cos);
			y = (float)(Ay + AB_pos * sin);

			// Success.
			return true;
		}

		static void InsertIntersectionPoint(IList<FxeDataBlock> datablocks, FxeDataBlock datablock, int id)
		{
			while (datablock.Val1 > datablocks[id].Val1 && id < datablocks.Count)
				++id;

			if (id < datablocks.Count)
				datablocks.Insert(id, datablock);
			else
				datablocks.Add(datablock);
		}


		static bool floatsequal(float f1, float f2)
		{
			return Math.Abs(f2 - f1) < 0.000005F;
		}
	}
}
