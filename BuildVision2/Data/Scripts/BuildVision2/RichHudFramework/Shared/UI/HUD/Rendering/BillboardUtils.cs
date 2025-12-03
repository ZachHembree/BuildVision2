using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using VRageRender;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
	namespace UI
	{
		using FlatTriangleBillboardData = MyTuple<
			BlendTypeEnum, // blendType
			Vector2I, // bbID + matrixID
			MyStringId, // material
			MyTuple<Vector4, BoundingBox2?>, // color + mask
			MyTuple<Vector2, Vector2, Vector2>, // texCoords
			MyTuple<Vector2, Vector2, Vector2> // flat pos
		>;
		using TriangleBillboardData = MyTuple<
			BlendTypeEnum, // blendType
			Vector2I, // bbID + matrixID
			MyStringId, // material
			Vector4, // color
			MyTuple<Vector2, Vector2, Vector2>, // texCoords
			MyTuple<Vector3D, Vector3D, Vector3D> // vertexPos
		>;

		namespace Rendering
		{
			/// <summary>
			/// Internal billboard pool API member access indices
			/// </summary>
			/// <exclude/>
			public enum BillBoardUtilAccessors : int
			{
				/// <summary>
				/// Deprecated
				/// out: List{MyTriangleBillboard}
				/// </summary>
				GetPoolBack = 1
			}

			public sealed partial class BillBoardUtils
			{
				#region 3D Billboards

				/// <summary>
				/// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
				/// indices and the tex coords are parallel to the vertex list.
				/// </summary>
				public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref PolyMaterial mat, MatrixD[] matrixRef = null)
				{
					var bbPool = instance.triPoolBack[0];
					var bbDataBack = instance.triangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID = -1;

					if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					int triangleCount = indices.Count / 3,
						bbRemaining = bbPool.Count - bbDataBack.Count,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNew3DBB(bbToAdd);

					for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();

					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = 0; i < indices.Count; i += 3)
					{
						var bb = new TriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count, matrixID),
							Item3 = mat.textureID,
							Item4 = mat.bbColor,
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texCoords[indices[i]],
								mat.texCoords[indices[i + 1]],
								mat.texCoords[indices[i + 2]]
							),
							Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
							(
								vertices[indices[i]],
								vertices[indices[i + 1]],
								vertices[indices[i + 2]]
							),
						};
						bbDataBack.Add(bb);
					}
				}

				/// <summary>
				/// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
				/// indices.
				/// </summary>
				public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref TriMaterial mat, MatrixD[] matrixRef = null)
				{
					var bbPool = instance.triPoolBack[0];
					var bbDataBack = instance.triangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID = -1;

					if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					int triangleCount = indices.Count / 3,
						bbRemaining = bbPool.Count - bbDataBack.Count,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNew3DBB(bbToAdd);

					for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();

					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = 0; i < indices.Count; i += 3)
					{
						var bb = new TriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count, matrixID),
							Item3 = mat.textureID,
							Item4 = mat.bbColor,
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texCoords.Point0,
								mat.texCoords.Point1,
								mat.texCoords.Point2
							),
							Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
							(
								vertices[indices[i]],
								vertices[indices[i + 1]],
								vertices[indices[i + 2]]
							),
						};
						bbDataBack.Add(bb);

					}
				}

				/// <summary>
				/// Adds a list of textured quads in one batch using QuadBoard data
				/// </summary>
				public static void AddQuads(IReadOnlyList<QuadBoardData> quads, MatrixD[] matrixRef = null)
				{
					var bbPool = instance.triPoolBack[0];
					var bbDataBack = instance.triangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID = -1;

					if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					int triangleCount = quads.Count * 2,
						bbRemaining = bbPool.Count - bbDataBack.Count,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNew3DBB(bbToAdd);

					for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();

					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = 0; i < quads.Count; i++)
					{
						QuadBoardData quadBoard = quads[i];
						MyQuadD quad = quadBoard.positions;
						BoundedQuadMaterial mat = quadBoard.material;

						var bbL = new TriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count, matrixID),
							Item3 = mat.textureID,
							Item4 = mat.bbColor,
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texBounds.Min,
								mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
								mat.texBounds.Max
							),
							Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
							(
								quad.Point0,
								quad.Point1,
								quad.Point2
							),

						};
						var bbR = new TriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
							Item3 = mat.textureID,
							Item4 = mat.bbColor,
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texBounds.Min,
								mat.texBounds.Max,
								mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f)
							),
							Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
							(
								quad.Point0,
								quad.Point2,
								quad.Point3
							),
						};

						bbDataBack.Add(bbL);
						bbDataBack.Add(bbR);
					}
				}

				/// <summary>
				/// Queues a quad billboard for rendering
				/// </summary>
				public static void AddQuad(ref BoundedQuadMaterial mat, ref MyQuadD quad, MatrixD[] matrixRef = null)
				{
					var bbPool = instance.triPoolBack[0];
					var bbDataBack = instance.triangleList;

					int indexL = bbDataBack.Count,
						indexR = indexL + 1;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID = -1;

					if (matrixRef != null && !matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					var bbL = new TriangleBillboardData
					{
						Item1 = BlendTypeEnum.PostPP,
						Item2 = new Vector2I(indexL, matrixID),
						Item3 = mat.textureID,
						Item4 = mat.bbColor,
						Item5 = new MyTuple<Vector2, Vector2, Vector2>
						(
							mat.texBounds.Min,
							mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
							mat.texBounds.Max
						),
						Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
						(
							quad.Point0,
							quad.Point1,
							quad.Point2
						),
					};
					var bbR = new TriangleBillboardData
					{
						Item1 = BlendTypeEnum.PostPP,
						Item2 = new Vector2I(indexR, matrixID),
						Item3 = mat.textureID,
						Item4 = mat.bbColor,
						Item5 = new MyTuple<Vector2, Vector2, Vector2>
						(
							mat.texBounds.Min,
							mat.texBounds.Max,
							mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f)
						),
						Item6 = new MyTuple<Vector3D, Vector3D, Vector3D>
						(
							quad.Point0,
							quad.Point2,
							quad.Point3
						),
					};

					bbDataBack.Add(bbL);
					bbDataBack.Add(bbR);

					if (indexR >= bbPool.Count)
						instance.AddNew3DBB(indexR - (bbPool.Count - 1));

					MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
					MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
				}

				#endregion

				/// <summary>
				/// Adds the given number of <see cref="MyTriangleBillboard"/>s to the pool
				/// </summary>
				private void AddNew3DBB(int count)
				{
					triPoolBack[0].EnsureCapacity(triPoolBack[0].Count + count);

					for (int i = 0; i < count; i++)
					{
						triPoolBack[0].Add(new MyTriangleBillboard
						{
							BlendType = BlendTypeEnum.PostPP,
							Position0 = Vector3D.Zero,
							Position1 = Vector3D.Zero,
							Position2 = Vector3D.Zero,
							UV0 = Vector2.Zero,
							UV1 = Vector2.Zero,
							UV2 = Vector2.Zero,
							Material = Material.Default.TextureID,
							Color = Vector4.One,
							DistanceSquared = float.PositiveInfinity,
							ColorIntensity = 1f,
							CustomViewProjection = -1
						});
					}
				}

				#region 2D Billboards

				public static void AddTriangleData(
					IReadOnlyList<FlatTriangleBillboardData> triangles,
					MatrixD[] matrixRef
				)
				{
					var bbPool = instance.flatTriPoolBack[0];
					var bbDataBack = instance.flatTriangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID;

					if (!matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					int bbCountStart = bbDataBack.Count;
					bbDataBack.EnsureCapacity(bbDataBack.Count + triangles.Count);

					for (int i = 0; i < triangles.Count; i++)
					{
						var tri = triangles[i];
						tri.Item2 = new Vector2I(bbDataBack.Count, matrixID);
						bbDataBack.Add(tri);
					}

					// Add more billboards to pool as needed then queue them for rendering
					int bbToAdd = Math.Max(bbDataBack.Count - bbPool.Count, 0);
					instance.AddNewFlatBB(bbToAdd);

					for (int i = bbCountStart; i < bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();
				}

				public static void GetTriangleData(
					ref QuadBoard qb,
					ref CroppedBox box,
					List<FlatTriangleBillboardData> bbDataOut
				)
				{
					FlatQuad quad = new FlatQuad()
					{
						Point0 = box.bounds.Max,
						Point1 = new Vector2(box.bounds.Max.X, box.bounds.Min.Y),
						Point2 = box.bounds.Min,
						Point3 = new Vector2(box.bounds.Min.X, box.bounds.Max.Y),
					};

					if (qb.skewRatio != 0f)
					{
						Vector2 start = quad.Point0, end = quad.Point3,
							offset = (end - start) * qb.skewRatio * .5f;

						quad.Point0 = Vector2.Lerp(start, end, qb.skewRatio) - offset;
						quad.Point3 = Vector2.Lerp(start, end, 1f + qb.skewRatio) - offset;
						quad.Point1 -= offset;
						quad.Point2 -= offset;
					}

					// Mask bounding check. Null mask if not intersecting.
					bool isDisjoint = false;

					if (box.mask != null)
					{
						BoundingBox2 bounds = new BoundingBox2(quad.Point2, quad.Point0);
						isDisjoint =
							(bounds.Max.X < box.mask.Value.Min.X) ||
							(bounds.Min.X > box.mask.Value.Max.X) ||
							(bounds.Max.Y < box.mask.Value.Min.Y) ||
							(bounds.Min.Y > box.mask.Value.Max.Y);
					}

					if (!isDisjoint)
					{
						var bbL = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item3 = qb.materialData.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(qb.materialData.bbColor, box.mask),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								new Vector2(qb.materialData.texBounds.Max.X, qb.materialData.texBounds.Min.Y), // 1
								qb.materialData.texBounds.Max, // 0
								new Vector2(qb.materialData.texBounds.Min.X, qb.materialData.texBounds.Max.Y) // 3
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								quad.Point0,
								quad.Point1,
								quad.Point2
							),
						};
						var bbR = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item3 = qb.materialData.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(qb.materialData.bbColor, box.mask),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								new Vector2(qb.materialData.texBounds.Max.X, qb.materialData.texBounds.Min.Y), // 1
								new Vector2(qb.materialData.texBounds.Min.X, qb.materialData.texBounds.Max.Y), // 3
								qb.materialData.texBounds.Min // 2
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								quad.Point0,
								quad.Point2,
								quad.Point3
							),
						};

						bbDataOut.Add(bbL);
						bbDataOut.Add(bbR);
					}
				}

				public static void GetTriangleData(
					IReadOnlyList<BoundedQuadBoard> quads,
					List<FlatTriangleBillboardData> bbDataOut,
					BoundingBox2? mask = null,
					Vector2 offset = default(Vector2),
					float scale = 1
				)
				{
					for (int i = 0; i < quads.Count; i++)
					{
						BoundedQuadBoard bqb = quads[i];
						BoundedQuadMaterial mat = bqb.quadBoard.materialData;
						Vector2 size = bqb.bounds.Size * scale,
							center = offset + bqb.bounds.Center * scale;
						BoundingBox2 bounds = BoundingBox2.CreateFromHalfExtent(center, .5f * size);

						FlatQuad quad = new FlatQuad()
						{
							Point0 = bounds.Max,
							Point1 = new Vector2(bounds.Max.X, bounds.Min.Y),
							Point2 = bounds.Min,
							Point3 = new Vector2(bounds.Min.X, bounds.Max.Y),
						};

						if (bqb.quadBoard.skewRatio != 0f)
						{
							Vector2 start = quad.Point0, end = quad.Point3,
								delta = (end - start) * bqb.quadBoard.skewRatio * .5f;

							quad.Point0 = Vector2.Lerp(start, end, bqb.quadBoard.skewRatio) - delta;
							quad.Point3 = Vector2.Lerp(start, end, 1f + bqb.quadBoard.skewRatio) - delta;
							quad.Point1 -= delta;
							quad.Point2 -= delta;
						}

						bool isDisjoint = false;

						if (mask != null)
						{
							isDisjoint =
								(bounds.Max.X < mask.Value.Min.X) ||
								(bounds.Min.X > mask.Value.Max.X) ||
								(bounds.Max.Y < mask.Value.Min.Y) ||
								(bounds.Min.Y > mask.Value.Max.Y);
						}

						if (!isDisjoint)
						{
							var bbL = new FlatTriangleBillboardData
							{
								Item1 = BlendTypeEnum.PostPP,
								Item3 = mat.textureID,
								Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, mask),
								Item5 = new MyTuple<Vector2, Vector2, Vector2>
								(
									new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
									mat.texBounds.Max, // 0
									new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y) // 3
								),
								Item6 = new MyTuple<Vector2, Vector2, Vector2>
								(
									quad.Point0,
									quad.Point1,
									quad.Point2
								),
							};
							var bbR = new FlatTriangleBillboardData
							{
								Item1 = BlendTypeEnum.PostPP,
								Item3 = mat.textureID,
								Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, mask),
								Item5 = new MyTuple<Vector2, Vector2, Vector2>
								(
									new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
									new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
									mat.texBounds.Min // 2
								),
								Item6 = new MyTuple<Vector2, Vector2, Vector2>
								(
									quad.Point0,
									quad.Point2,
									quad.Point3
								),
							};

							bbDataOut.Add(bbL);
							bbDataOut.Add(bbR);
						}
					}
				}

				/// <summary>
				/// Adds a group of adjacent quads in continuous a strip with a shared material, and each quad sharing a side with the last
				/// </summary>
				public static void AddQuadStrip(IReadOnlyList<Vector2> vertices, BoundedQuadMaterial material, MatrixD[] matrixRef, BoundingBox2? mask = null)
				{
					var bbPool = instance.flatTriPoolBack[0];
					var bbDataBack = instance.flatTriangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID;

					if (!matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					// Calculate maximum triangle count and preallocate
					int triangleCount = 2 * ((vertices.Count - 2) / 2);
					int bbDataStart = bbDataBack.Count;
					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = 0; i < vertices.Count - 2; i += 2)
					{
						bool isDisjoint = false;

						if (mask != null)
						{
							Vector2 min = new Vector2
							{
								X = Math.Min(Math.Min(Math.Min(vertices[i].X, vertices[i + 1].X), vertices[i + 2].X), vertices[i + 3].X),
								Y = Math.Min(Math.Min(Math.Min(vertices[i].Y, vertices[i + 1].Y), vertices[i + 2].Y), vertices[i + 3].Y)
							};
							Vector2 max = new Vector2
							{
								X = Math.Max(Math.Max(Math.Max(vertices[i].X, vertices[i + 1].X), vertices[i + 2].X), vertices[i + 3].X),
								Y = Math.Max(Math.Max(Math.Max(vertices[i].Y, vertices[i + 1].Y), vertices[i + 2].Y), vertices[i + 3].Y)
							};

							isDisjoint =
								(max.X < mask.Value.Min.X) ||
								(min.X > mask.Value.Max.X) ||
								(max.Y < mask.Value.Min.Y) ||
								(min.Y > mask.Value.Max.Y);
						}

						if (!isDisjoint)
						{
							var bbL = new FlatTriangleBillboardData
							{
								Item1 = BlendTypeEnum.PostPP,
								Item2 = new Vector2I(bbDataBack.Count, matrixID),
								Item3 = material.textureID,
								Item4 = new MyTuple<Vector4, BoundingBox2?>(material.bbColor, mask),
								Item5 = new MyTuple<Vector2, Vector2, Vector2>
								(
									material.texBounds.Max, // 0
									new Vector2(material.texBounds.Max.X, material.texBounds.Min.Y), // 1
									material.texBounds.Min // 2
								),
								Item6 = new MyTuple<Vector2, Vector2, Vector2>
								(
									vertices[i],
									vertices[i + 1],
									vertices[i + 3]
								),
							};
							var bbR = new FlatTriangleBillboardData
							{
								Item1 = BlendTypeEnum.PostPP,
								Item2 = new Vector2I(bbDataBack.Count + 1, matrixID),
								Item3 = material.textureID,
								Item4 = new MyTuple<Vector4, BoundingBox2?>(material.bbColor, mask),
								Item5 = new MyTuple<Vector2, Vector2, Vector2>
								(
									material.texBounds.Max, // 0
									material.texBounds.Min, // 2
									new Vector2(material.texBounds.Min.X, material.texBounds.Max.Y) // 3
								),
								Item6 = new MyTuple<Vector2, Vector2, Vector2>
								(
									vertices[i],
									vertices[i + 3],
									vertices[i + 2]
								),
							};

							bbDataBack.Add(bbL);
							bbDataBack.Add(bbR);
						}
					}

					// Get actual triangles used
					triangleCount = bbDataBack.Count - bbDataStart;
					int bbRemaining = bbPool.Count - bbDataStart,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNewFlatBB(bbToAdd);
					bbBuf.Clear();

					for (int i = bbDataStart; i < bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
				}

				/// <summary>
				/// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
				/// indices and the tex coords are parallel to the vertex list.
				/// </summary>
				public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
				{
					var bbPool = instance.flatTriPoolBack[0];
					var bbDataBack = instance.flatTriangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID;

					if (!matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					// Get triangle count, ensure enough billboards are in the pool and add them to the
					// render queue before writing QB data to buffer
					int triangleCount = indices.Count / 3,
						bbRemaining = bbPool.Count - bbDataBack.Count,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNewFlatBB(bbToAdd);

					for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();

					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = 0; i < indices.Count; i += 3)
					{
						var bb = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count, matrixID),
							Item3 = mat.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, null),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texCoords[indices[i]],
								mat.texCoords[indices[i + 1]],
								mat.texCoords[indices[i + 2]]
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								vertices[indices[i]],
								vertices[indices[i + 1]],
								vertices[indices[i + 2]]
							),
						};
						bbDataBack.Add(bb);
					}
				}

				/// <summary>
				/// Adds a triangles in the given starting index range
				/// </summary>
				public static void AddTriangleRange(Vector2I range, IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
				{
					var bbPool = instance.flatTriPoolBack[0];
					var bbDataBack = instance.flatTriangleList;
					var bbBuf = instance.bbBuf;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID;

					if (!matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					// Get triangle count, ensure enough billboards are in the pool and add them to the
					// render queue before writing QB data to buffer
					int iMax = indices.Count,
						triangleCount = (range.Y - range.X) / 3,
						bbRemaining = bbPool.Count - bbDataBack.Count,
						bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

					instance.AddNewFlatBB(bbToAdd);

					for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
						bbBuf.Add(bbPool[i]);

					MyTransparentGeometry.AddBillboards(bbBuf, false);
					bbBuf.Clear();

					bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

					for (int i = range.X; i <= range.Y; i += 3)
					{
						var bb = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(bbDataBack.Count, matrixID),
							Item3 = mat.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, null),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								mat.texCoords[indices[i % iMax]],
								mat.texCoords[indices[(i + 1) % iMax]],
								mat.texCoords[indices[(i + 2) % iMax]]
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								vertices[indices[i % iMax]],
								vertices[indices[(i + 1) % iMax]],
								vertices[indices[(i + 2) % iMax]]
							),
						};
						bbDataBack.Add(bb);

					}
				}

				/// <summary>
				/// Queues a quad billboard for rendering
				/// </summary>
				public static void AddQuad(ref FlatQuad quad, ref BoundedQuadMaterial mat, MatrixD[] matrixRef, BoundingBox2? mask = null)
				{
					var bbPool = instance.flatTriPoolBack[0];
					var bbDataBack = instance.flatTriangleList;
					var matList = instance.matrixBuf;
					var matTable = instance.matrixTable;

					// Find matrix index in table or add it
					int matrixID;

					if (!matTable.TryGetValue(matrixRef, out matrixID))
					{
						matrixID = matList.Count;
						matList.Add(matrixRef[0]);
						matTable.Add(matrixRef, matrixID);
					}

					// Mask bounding check. Null mask if not intersecting.
					bool isDisjoint = false;

					if (mask != null)
					{
						BoundingBox2 bounds = new BoundingBox2(quad.Point2, quad.Point0);
						isDisjoint =
							(bounds.Max.X < mask.Value.Min.X) ||
							(bounds.Min.X > mask.Value.Max.X) ||
							(bounds.Max.Y < mask.Value.Min.Y) ||
							(bounds.Min.Y > mask.Value.Max.Y);
					}

					if (!isDisjoint)
					{
						int indexL = bbDataBack.Count,
							indexR = bbDataBack.Count + 1;

						var bbL = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(indexL, matrixID),
							Item3 = mat.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, mask),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
								mat.texBounds.Max, // 0
								new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y) // 3
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								quad.Point0,
								quad.Point1,
								quad.Point2
							),
						};
						var bbR = new FlatTriangleBillboardData
						{
							Item1 = BlendTypeEnum.PostPP,
							Item2 = new Vector2I(indexR, matrixID),
							Item3 = mat.textureID,
							Item4 = new MyTuple<Vector4, BoundingBox2?>(mat.bbColor, mask),
							Item5 = new MyTuple<Vector2, Vector2, Vector2>
							(
								new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
								new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
								mat.texBounds.Min // 2
							),
							Item6 = new MyTuple<Vector2, Vector2, Vector2>
							(
								quad.Point0,
								quad.Point2,
								quad.Point3
							),
						};

						bbDataBack.Add(bbL);
						bbDataBack.Add(bbR);

						if (indexR >= bbPool.Count)
							instance.AddNewFlatBB(indexR - (bbPool.Count - 1));

						MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
						MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
					}
				}

				#endregion

				/// <summary>
				/// Adds the given number of <see cref="MyTriangleBillboard"/>s to the pool
				/// </summary>
				private void AddNewFlatBB(int count)
				{
					flatTriPoolBack[0].EnsureCapacity(flatTriPoolBack[0].Count + count);

					for (int i = 0; i < count; i++)
					{
						flatTriPoolBack[0].Add(new MyTriangleBillboard
						{
							BlendType = BlendTypeEnum.PostPP,
							Position0 = Vector3D.Zero,
							Position1 = Vector3D.Zero,
							Position2 = Vector3D.Zero,
							UV0 = Vector2.Zero,
							UV1 = Vector2.Zero,
							UV2 = Vector2.Zero,
							Material = Material.Default.TextureID,
							Color = Vector4.One,
							DistanceSquared = float.PositiveInfinity,
							ColorIntensity = 1f,
							CustomViewProjection = -1
						});
					}
				}
			}
		}
	}
}