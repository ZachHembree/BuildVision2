using System.Collections.Generic;
using System;
using System.Threading;
using Sandbox.ModAPI;
using VRage.Game;
using VRage;
using VRage.Utils;
using VRageMath;
using VRageRender;
using RichHudFramework.Internal;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            public struct TriangleBillboardData
            {
                public BlendTypeEnum blendType;
                public int bbID;
                public MyStringId material;
                public Vector4 color;
                public Triangle texCoords;
                public TriangleD positions;
            }

            public struct FlatTriangleBillboardData
            {
                public BlendTypeEnum blendType;
                public int bbID;
                public int matrixID;
                public MyStringId material;
                public BoundingBox2? mask;
                public Vector4 color;
                public Triangle texCoords;
                public Triangle positions;
            }

            public sealed class BillBoardUtils : RichHudComponentBase
            {
                private static BillBoardUtils instance;
                private const int statsWindowSize = 240, sampleRateDiv = 5,
                    bufMinResizeThreshold = 4000;

                private readonly List<MyTriangleBillboard> bbBuf;
                private readonly List<MyTriangleBillboard>[] bbSwapPools;
                private List<MyTriangleBillboard> bbPoolBack;
                private int currentPool;

                private readonly List<TriangleBillboardData> triangleList;
                private readonly List<FlatTriangleBillboardData> flatTriangleList;
                private readonly List<MatrixD> matrixBuf;
                private readonly Dictionary<MatrixD[], int> matrixTable;

                private readonly int[] billboardUsage, billboardAlloc, matrixUsage;
                private readonly List<int> billboardUsageStats, billboardAllocStats, matrixUsageStats;
                private readonly Action UpdateBillboardsCallback;
                private int sampleTick, tick;

                private BillBoardUtils() : base(false, true)
                {
                    if (instance != null)
                        throw new Exception($"Only one instance of {GetType().Name} can exist at once.");

                    bbSwapPools = new List<MyTriangleBillboard>[3]
                    {
                        new List<MyTriangleBillboard>(1000),
                        new List<MyTriangleBillboard>(1000),
                        new List<MyTriangleBillboard>(1000)
                    };
                    bbBuf = new List<MyTriangleBillboard>(1000);

                    triangleList = new List<TriangleBillboardData>();
                    flatTriangleList = new List<FlatTriangleBillboardData>(1000);
                    matrixBuf = new List<MatrixD>();
                    matrixTable = new Dictionary<MatrixD[], int>();

                    billboardUsage = new int[statsWindowSize];
                    billboardAlloc = new int[statsWindowSize];
                    matrixUsage = new int[statsWindowSize];

                    billboardUsageStats = new List<int>(statsWindowSize);
                    billboardAllocStats = new List<int>(statsWindowSize);
                    matrixUsageStats = new List<int>(statsWindowSize);

                    UpdateBillboardsCallback = UpdateBillboards;
                }

                public static void Init()
                {
                    if (instance == null)
                    {
                        instance = new BillBoardUtils();
                    }
                }

                public override void Close()
                {
                    instance = null;
                }

                /// <summary>
                /// Returns billboard usage at the given percentile.
                /// </summary>
                public static int GetUsagePercentile(float percentile)
                {
                    if (instance != null)
                        return instance.billboardUsageStats[(int)(statsWindowSize * percentile)];
                    else
                        throw new Exception($"{typeof(BillBoardUtils).Name} not initialized!");
                }

                /// <summary>
                /// Returns billboard usage at the given percentile.
                /// </summary>
                public static int GetAllocPercentile(float percentile)
                {
                    if (instance != null)
                        return instance.billboardAllocStats[(int)(statsWindowSize * percentile)];
                    else
                        throw new Exception($"{typeof(BillBoardUtils).Name} not initialized!");
                }

                /// <summary>
                /// Returns unique matrix usage at the given percentile.
                /// </summary>
                public static int GetMatrixUsagePercentile(float percentile)
                {
                    if (instance != null)
                        return instance.matrixUsageStats[(int)(statsWindowSize * percentile)];
                    else
                        throw new Exception($"{typeof(BillBoardUtils).Name} not initialized!");
                }

                public static void BeginDraw()
                {
                    if (instance != null)
                    {
                        instance.BeginDrawInternal();
                    }
                }

                public static void FinishDraw()
                {
                    if (instance != null)
                    {
                        instance.FinishDrawInternal();
                    }
                }

                private void BeginDrawInternal()
                {
                    bbPoolBack = bbSwapPools[currentPool];                    
                }

                private void FinishDrawInternal()
                {
                    MyTransparentGeometry.ApplyActionOnPersistentBillboards(UpdateBillboardsCallback);
                    currentPool = (currentPool + 1) % bbSwapPools.Length;

                    if (tick == 0)
                    {
                        billboardUsage[sampleTick] = flatTriangleList.Count;
                        billboardUsageStats.Clear();
                        billboardUsageStats.AddRange(billboardUsage);
                        billboardUsageStats.Sort();

                        billboardAlloc[sampleTick] = flatTriangleList.Capacity;
                        billboardAllocStats.Clear();
                        billboardAllocStats.AddRange(billboardAlloc);
                        billboardAllocStats.Sort();

                        matrixUsage[sampleTick] = matrixBuf.Count;
                        matrixUsageStats.Clear();
                        matrixUsageStats.AddRange(matrixUsage);
                        matrixUsageStats.Sort();

                        sampleTick++;
                        sampleTick %= statsWindowSize;

                        int usage99 = GetUsagePercentile(.99f),
                            alloc01 = GetAllocPercentile(0.01f);

                        if (flatTriangleList.Capacity > bufMinResizeThreshold
                            && flatTriangleList.Capacity > 3 * flatTriangleList.Count
                            && flatTriangleList.Capacity > 3 * usage99
                            && alloc01 > 3 * usage99)
                        {
                            int max = Math.Max(2 * flatTriangleList.Count, bufMinResizeThreshold);
                            flatTriangleList.ClearAndTrim(max);
                            bbBuf.ClearAndTrim(max);

                            foreach (List<MyTriangleBillboard> bb in bbSwapPools)
                            {
                                int remStart = Math.Min(max, bb.Count - 1),
                                    remCount = Math.Max(bb.Count - remStart, 0);

                                bb.RemoveRange(remStart, remCount);
                                bb.TrimExcess();
                            }
                        }
                    }

                    triangleList.Clear();
                    flatTriangleList.Clear();
                    matrixBuf.Clear();
                    matrixTable.Clear();

                    tick++;
                    tick %= sampleRateDiv;
                }

                private void UpdateBillboards()
                {
                    Update3dBillboards();
                    UpdateFlatBillboards();
                }

                private void Update3dBillboards()
                {
                    int count = Math.Min(triangleList.Count, bbPoolBack.Count),
                        stride = Math.Max(500, MathHelper.CeilToInt(count / 8d));

                    MyAPIGateway.Parallel.For(0, count, i => 
                    {
                        TriangleBillboardData bbData = triangleList[i];
                        MyTriangleBillboard bb = bbPoolBack[bbData.bbID];

                        bb.BlendType = bbData.blendType;
                        bb.Position0 = bbData.positions.Point0;
                        bb.Position1 = bbData.positions.Point1;
                        bb.Position2 = bbData.positions.Point2;
                        bb.UV0 = bbData.texCoords.Point0;
                        bb.UV1 = bbData.texCoords.Point1;
                        bb.UV2 = bbData.texCoords.Point2;
                        bb.Material = bbData.material;
                        bb.Color = bbData.color;
                    });
                }

                private void UpdateFlatBillboards()
                {
                    int count = Math.Min(flatTriangleList.Count, bbPoolBack.Count),
                        stride = Math.Max(500, MathHelper.CeilToInt(count / 8d));

                    MyAPIGateway.Parallel.For(0, count, i =>
                    {
                        FlatTriangleBillboardData bbData = flatTriangleList[i];
                        Triangle planePos = bbData.positions,
                            texCoords = bbData.texCoords;

                        // Masking/clipping
                        if (bbData.mask != null)
                        {
                            BoundingBox2 mask = bbData.mask.Value,
                                bounds = new BoundingBox2(Vector2.Min(planePos.Point1, planePos.Point2), planePos.Point0),
                                texBounds = new BoundingBox2(Vector2.Min(texCoords.Point0, texCoords.Point2), Vector2.Max(texCoords.Point0, texCoords.Point1));
                            Vector2 size = bounds.Size,
                                pos = bounds.Center;

                            bounds = bounds.Intersect(mask);
                            planePos.Point0 = Vector2.Clamp(planePos.Point0, bounds.Min, bounds.Max);
                            planePos.Point1 = Vector2.Clamp(planePos.Point1, bounds.Min, bounds.Max);
                            planePos.Point2 = Vector2.Clamp(planePos.Point2, bounds.Min, bounds.Max);

                            // Dont bother clipping texcoords for solid colors
                            if (bbData.material != Material.Default.TextureID)
                            {
                                Vector2 clipSize = bounds.Size;

                                // Normalized cropped size and offset
                                Vector2 clipScale = clipSize / size,
                                    clipOffset = (bounds.Center - pos) / size,
                                    uvScale = texBounds.Size,
                                    uvOffset = texBounds.Center;

                                pos += clipOffset * size; // Offset billboard to compensate for changes in size
                                size = clipSize; // Use cropped billboard size
                                clipOffset *= uvScale * new Vector2(1f, -1f); // Scale offset to fit material and flip Y-axis

                                // Recalculate texture coordinates to simulate clipping without affecting material alignment
                                texBounds.Min = ((texBounds.Min - uvOffset) * clipScale) + (uvOffset + clipOffset);
                                texBounds.Max = ((texBounds.Max - uvOffset) * clipScale) + (uvOffset + clipOffset);

                                texCoords.Point0 = Vector2.Clamp(texCoords.Point0, texBounds.Min, texBounds.Max);
                                texCoords.Point1 = Vector2.Clamp(texCoords.Point1, texBounds.Min, texBounds.Max);
                                texCoords.Point2 = Vector2.Clamp(texCoords.Point2, texBounds.Min, texBounds.Max);
                            }
                        }

                        // Transform 2D planar positions into world space
                        MatrixD matrix = matrixBuf[bbData.matrixID];
                        TriangleD worldPos = new TriangleD
                        {
                            Point0 = matrix.Translation + (planePos.Point0.X * matrix.Right) + (planePos.Point0.Y * matrix.Up),
                            Point1 = matrix.Translation + (planePos.Point1.X * matrix.Right) + (planePos.Point1.Y * matrix.Up),
                            Point2 = matrix.Translation + (planePos.Point2.X * matrix.Right) + (planePos.Point2.Y * matrix.Up)
                        };

                        MyTriangleBillboard bb = bbPoolBack[bbData.bbID];
                        bb.BlendType = bbData.blendType;
                        bb.Position0 = worldPos.Point0;
                        bb.Position1 = worldPos.Point1;
                        bb.Position2 = worldPos.Point2;
                        bb.UV0 = texCoords.Point0;
                        bb.UV1 = texCoords.Point1;
                        bb.UV2 = texCoords.Point2;
                        bb.Material = bbData.material;
                        bb.Color = bbData.color;
                    }, stride);
                }

                #region 3D Billboards

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices and the tex coords are parallel to the vertex list.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref PolyMaterial mat)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;

                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            positions = new TriangleD
                            {
                                Point0 = vertices[indices[i]],
                                Point1 = vertices[indices[i + 1]],
                                Point2 = vertices[indices[i + 2]],
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texCoords[indices[i]],
                                Point1 = mat.texCoords[indices[i + 1]],
                                Point2 = mat.texCoords[indices[i + 2]],
                            },
                            material = mat.textureID,
                            color = mat.bbColor,
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a triangles in the given starting index range
                /// </summary>
                public static void AddTriangleRange(Vector2I range, IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref PolyMaterial mat)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;

                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = range.X; i <= range.Y; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            positions = new TriangleD
                            {
                                Point0 = vertices[indices[i]],
                                Point1 = vertices[indices[i + 1]],
                                Point2 = vertices[indices[i + 2]],
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texCoords[indices[i]],
                                Point1 = mat.texCoords[indices[i + 1]],
                                Point2 = mat.texCoords[indices[i + 2]],
                            },
                            material = mat.textureID,
                            color = mat.bbColor,
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref TriMaterial mat)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;

                    int triangleCount = indices.Count / 3,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new TriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            positions = new TriangleD
                            {
                                Point0 = vertices[indices[i]],
                                Point1 = vertices[indices[i + 1]],
                                Point2 = vertices[indices[i + 2]],
                            },
                            texCoords = mat.texCoords,
                            material = mat.textureID,
                            color = mat.bbColor,
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a list of textured quads in one batch using QuadBoard data
                /// </summary>
                public static void AddQuads(IReadOnlyList<QuadBoardData> quads)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    var bbBuf = instance.bbBuf;

                    int triangleCount = quads.Count * 2,
                        bbRemaining = bbPool.Count - bbDataBack.Count,
                        bbToAdd = Math.Max(triangleCount - bbRemaining, 0);

                    instance.AddNewBB(bbToAdd);

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
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            positions = new TriangleD
                            {
                                Point0 = quad.Point0,
                                Point1 = quad.Point1,
                                Point2 = quad.Point2,
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texBounds.Min,
                                Point1 = mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
                                Point2 = mat.texBounds.Max,
                            },
                            material = mat.textureID,
                            color = mat.bbColor,
                        };
                        var bbR = new TriangleBillboardData
                        {
                            bbID = bbDataBack.Count + 1,
                            blendType = BlendTypeEnum.PostPP,
                            positions = new TriangleD
                            {
                                Point0 = quad.Point0,
                                Point1 = quad.Point2,
                                Point2 = quad.Point3,
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texBounds.Min,
                                Point1 = mat.texBounds.Max,
                                Point2 = mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f),
                            },
                            material = mat.textureID,
                            color = mat.bbColor,
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);
                    }
                }

                /// <summary>
                /// Adds a triangle starting at the given index.
                /// </summary>
                public static void AddTriangle(int start, IReadOnlyList<int> indices, IReadOnlyList<Vector3D> vertices, ref TriMaterial mat)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int index = bbDataBack.Count;

                    var bb = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count,
                        blendType = BlendTypeEnum.PostPP,
                        positions = new TriangleD
                        {
                            Point0 = vertices[indices[start]],
                            Point1 = vertices[indices[start + 1]],
                            Point2 = vertices[indices[start + 2]],
                        },
                        texCoords = mat.texCoords,
                        material = mat.textureID,
                        color = mat.bbColor,
                    };
                    bbDataBack.Add(bb);

                    if (index >= bbPool.Count)
                        instance.AddNewBB(index - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[index], false);
                }

                /// <summary>
                /// Queues a single triangle billboard for rendering
                /// </summary>
                public static void AddTriangle(ref TriMaterial mat, ref TriangleD triangle)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int index = bbDataBack.Count;

                    var bb = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count,
                        blendType = BlendTypeEnum.PostPP,
                        positions = triangle,
                        texCoords = mat.texCoords,
                        material = mat.textureID,
                        color = mat.bbColor,
                    };
                    bbDataBack.Add(bb);

                    if (index >= bbPool.Count)
                        instance.AddNewBB(index - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[index], false);
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref QuadMaterial mat, ref MyQuadD quad)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int indexL = bbDataBack.Count,
                        indexR = bbDataBack.Count + 1;

                    var bbL = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count,
                        blendType = BlendTypeEnum.PostPP,
                        positions = new TriangleD
                        {
                            Point0 = quad.Point0,
                            Point1 = quad.Point1,
                            Point2 = quad.Point2,
                        },
                        texCoords = new Triangle
                        {
                            Point0 = mat.texCoords.Point0,
                            Point1 = mat.texCoords.Point1,
                            Point2 = mat.texCoords.Point2,
                        },
                        material = mat.textureID,
                        color = mat.bbColor,
                    };
                    var bbR = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count + 1,
                        blendType = BlendTypeEnum.PostPP,
                        positions = new TriangleD
                        {
                            Point0 = quad.Point0,
                            Point1 = quad.Point2,
                            Point2 = quad.Point3,
                        },
                        texCoords = new Triangle
                        {
                            Point0 = mat.texCoords.Point0,
                            Point1 = mat.texCoords.Point2,
                            Point2 = mat.texCoords.Point3,
                        },
                        material = mat.textureID,
                        color = mat.bbColor,
                    };

                    bbDataBack.Add(bbL);
                    bbDataBack.Add(bbR);

                    if (indexR >= bbPool.Count)
                        instance.AddNewBB(indexR - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                    MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref BoundedQuadMaterial mat, ref MyQuadD quad)
                {
                    var bbPool = instance.bbPoolBack;
                    var bbDataBack = instance.triangleList;
                    int indexL = bbDataBack.Count,
                        indexR = bbDataBack.Count + 1;

                    var bbL = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count,
                        blendType = BlendTypeEnum.PostPP,
                        positions = new TriangleD
                        {
                            Point0 = quad.Point0,
                            Point1 = quad.Point1,
                            Point2 = quad.Point2,
                        },
                        texCoords = new Triangle
                        {
                            Point0 = mat.texBounds.Min,
                            Point1 = mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y),
                            Point2 = mat.texBounds.Max,
                        },
                        material = mat.textureID,
                        color = mat.bbColor,
                    };
                    var bbR = new TriangleBillboardData
                    {
                        bbID = bbDataBack.Count + 1,
                        blendType = BlendTypeEnum.PostPP,
                        positions = new TriangleD
                        {
                            Point0 = quad.Point0,
                            Point1 = quad.Point2,
                            Point2 = quad.Point3,
                        },
                        texCoords = new Triangle
                        {
                            Point0 = mat.texBounds.Min,
                            Point1 = mat.texBounds.Max,
                            Point2 = mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f),
                        },
                        material = mat.textureID,
                        color = mat.bbColor,
                    };

                    bbDataBack.Add(bbL);
                    bbDataBack.Add(bbR);

                    if (indexR >= bbPool.Count)
                        instance.AddNewBB(indexR - (bbPool.Count - 1));

                    MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                    MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                }

                #endregion

                #region 2D Billboards

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices and the tex coords are parallel to the vertex list.
                /// </summary>
                public static void AddTriangles(IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
                {
                    var bbPool = instance.bbPoolBack;
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

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        var bb = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            mask = null,
                            material = mat.textureID,
                            color = mat.bbColor,
                            positions = new Triangle
                            {
                                Point0 = vertices[indices[i]],
                                Point1 = vertices[indices[i + 1]],
                                Point2 = vertices[indices[i + 2]],
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texCoords[indices[i]],
                                Point1 = mat.texCoords[indices[i + 1]],
                                Point2 = mat.texCoords[indices[i + 2]],
                            },
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a triangles in the given starting index range
                /// </summary>
                public static void AddTriangleRange(Vector2I range, IReadOnlyList<int> indices, IReadOnlyList<Vector2> vertices, ref PolyMaterial mat, MatrixD[] matrixRef)
                {
                    var bbPool = instance.bbPoolBack;
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

                    instance.AddNewBB(bbToAdd);

                    for (int i = bbDataBack.Count; i < triangleCount + bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = range.X; i <= range.Y; i += 3)
                    {
                        var bb = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            mask = null,
                            material = mat.textureID,
                            color = mat.bbColor,
                            positions = new Triangle
                            {
                                Point0 = vertices[indices[i]],
                                Point1 = vertices[indices[i + 1]],
                                Point2 = vertices[indices[i + 2]],
                            },
                            texCoords = new Triangle
                            {
                                Point0 = mat.texCoords[indices[i]],
                                Point1 = mat.texCoords[indices[i + 1]],
                                Point2 = mat.texCoords[indices[i + 2]],
                            }
                        };
                        bbDataBack.Add(bb);
                    }
                }

                /// <summary>
                /// Adds a list of textured quads in one batch using QuadBoard data
                /// </summary>
                public static void AddQuads(IReadOnlyList<BoundedQuadBoard> quads, MatrixD[] matrixRef, BoundingBox2? mask = null, 
                    Vector2 offset = default(Vector2), float scale = 1f)
                {
                    var bbPool = instance.bbPoolBack;
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

                    int triangleCount = quads.Count * 2,
                        bbCountStart = bbDataBack.Count;

                    bbDataBack.EnsureCapacity(bbDataBack.Count + triangleCount);

                    for (int i = 0; i < quads.Count; i++)
                    {
                        BoundedQuadBoard boundedQB = quads[i];
                        BoundedQuadMaterial mat = boundedQB.quadBoard.materialData;
                        Vector2 size = boundedQB.bounds.Size * scale,
                            center = offset + boundedQB.bounds.Center * scale;

                        BoundingBox2 bounds = BoundingBox2.CreateFromHalfExtent(center, .5f * size);
                        BoundingBox2? maskBox = mask;
                        ContainmentType containment = ContainmentType.Contains;

                        if (maskBox != null)
                        {
                            maskBox.Value.Contains(ref bounds, out containment);

                            if (containment == ContainmentType.Contains)
                                maskBox = null;
                        }

                        if (containment != ContainmentType.Disjoint)
                        {
                            var bbL = new FlatTriangleBillboardData
                            {
                                bbID = bbDataBack.Count,
                                blendType = BlendTypeEnum.PostPP,
                                matrixID = matrixID,
                                mask = maskBox,
                                material = mat.textureID,
                                color = mat.bbColor,
                                positions = new Triangle
                                {
                                    Point0 = bounds.Max,
                                    Point1 = new Vector2(bounds.Max.X, bounds.Min.Y),
                                    Point2 = bounds.Min,
                                },
                                texCoords = new Triangle
                                {
                                    Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                    Point1 = mat.texBounds.Max, // 0
                                    Point2 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                },
                            };
                            var bbR = new FlatTriangleBillboardData
                            {
                                bbID = bbDataBack.Count + 1,
                                blendType = BlendTypeEnum.PostPP,
                                matrixID = matrixID,
                                mask = maskBox,
                                material = mat.textureID,
                                color = mat.bbColor,
                                positions = new Triangle
                                {
                                    Point0 = bounds.Max,
                                    Point1 = bounds.Min,
                                    Point2 = new Vector2(bounds.Min.X, bounds.Max.Y),
                                },
                                texCoords = new Triangle
                                {
                                    Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                    Point1 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                    Point2 = mat.texBounds.Min, // 2
                                },
                            };

                            bbDataBack.Add(bbL);
                            bbDataBack.Add(bbR);
                        }
                    }

                    // Add more billboards to pool as needed then queue them for rendering
                    int bbToAdd = Math.Max(bbDataBack.Count - bbPool.Count, 0);
                    instance.AddNewBB(bbToAdd);

                    for (int i = bbCountStart; i < bbDataBack.Count; i++)
                        bbBuf.Add(bbPool[i]);

                    MyTransparentGeometry.AddBillboards(bbBuf, false);
                    bbBuf.Clear();
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref BoundedQuadBoard boundedQB, MatrixD[] matrixRef, BoundingBox2? mask = null)
                {
                    var bbPool = instance.bbPoolBack;
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
                    BoundingBox2? maskBox = mask;
                    ContainmentType containment = ContainmentType.Contains;

                    if (maskBox != null)
                    {
                        maskBox.Value.Contains(ref boundedQB.bounds, out containment);

                        if (containment == ContainmentType.Contains)
                            maskBox = null;
                    }

                    if (containment != ContainmentType.Disjoint)
                    {
                        int indexL = bbDataBack.Count,
                            indexR = bbDataBack.Count + 1;
                        BoundedQuadMaterial mat = boundedQB.quadBoard.materialData;

                        var bbL = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            mask = maskBox,
                            material = mat.textureID,
                            color = mat.bbColor,
                            positions = new Triangle
                            {
                                Point0 = boundedQB.bounds.Max,
                                Point1 = new Vector2(boundedQB.bounds.Max.X, boundedQB.bounds.Min.Y),
                                Point2 = boundedQB.bounds.Min,
                            },
                            texCoords = new Triangle
                            {
                                Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                Point1 = mat.texBounds.Max, // 0
                                Point2 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                            },
                        };
                        var bbR = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count + 1,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            mask = maskBox,
                            material = mat.textureID,
                            color = mat.bbColor,
                            positions = new Triangle
                            {
                                Point0 = boundedQB.bounds.Max,
                                Point1 = boundedQB.bounds.Min,
                                Point2 = new Vector2(boundedQB.bounds.Min.X, boundedQB.bounds.Max.Y),
                            },
                            texCoords = new Triangle
                            {
                                Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                Point1 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                Point2 = mat.texBounds.Min, // 2
                            },
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);

                        if (indexR >= bbPool.Count)
                            instance.AddNewBB(indexR - (bbPool.Count - 1));

                        MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                        MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                    }
                }

                /// <summary>
                /// Queues a quad billboard for rendering
                /// </summary>
                public static void AddQuad(ref FlatQuad quad, ref BoundedQuadMaterial mat, MatrixD[] matrixRef, BoundingBox2? mask = null)
                {
                    var bbPool = instance.bbPoolBack;
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
                    BoundingBox2? maskBox = mask;
                    ContainmentType containment = ContainmentType.Contains;

                    if (maskBox != null)
                    {
                        BoundingBox2 bounds = new BoundingBox2(quad.Point2, quad.Point0);
                        maskBox.Value.Contains(ref bounds, out containment);

                        if (containment == ContainmentType.Contains)
                            maskBox = null;
                    }

                    if (containment != ContainmentType.Disjoint)
                    {
                        int indexL = bbDataBack.Count,
                            indexR = bbDataBack.Count + 1;

                        var bbL = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            material = mat.textureID,
                            color = mat.bbColor,
                            mask = maskBox,
                            positions = new Triangle
                            {
                                Point0 = quad.Point0,
                                Point1 = quad.Point1,
                                Point2 = quad.Point2,
                            },
                            texCoords = new Triangle
                            {
                                Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                Point1 = mat.texBounds.Max, // 0
                                Point2 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                            },
                        };
                        var bbR = new FlatTriangleBillboardData
                        {
                            bbID = bbDataBack.Count + 1,
                            blendType = BlendTypeEnum.PostPP,
                            matrixID = matrixID,
                            material = mat.textureID,
                            color = mat.bbColor,
                            mask = maskBox,
                            positions = new Triangle
                            {
                                Point0 = quad.Point0,
                                Point1 = quad.Point2,
                                Point2 = quad.Point3,
                            },
                            texCoords = new Triangle
                            {
                                Point0 = new Vector2(mat.texBounds.Max.X, mat.texBounds.Min.Y), // 1
                                Point1 = new Vector2(mat.texBounds.Min.X, mat.texBounds.Max.Y), // 3
                                Point2 = mat.texBounds.Min, // 2
                            },
                        };

                        bbDataBack.Add(bbL);
                        bbDataBack.Add(bbR);

                        if (indexR >= bbPool.Count)
                            instance.AddNewBB(indexR - (bbPool.Count - 1));

                        MyTransparentGeometry.AddBillboard(bbPool[indexL], false);
                        MyTransparentGeometry.AddBillboard(bbPool[indexR], false);
                    }
                }

                #endregion

                /// <summary>
                /// Adds the given number of <see cref="MyTriangleBillboard"/>s to the pool
                /// </summary>
                private void AddNewBB(int count)
                {
                    bbPoolBack.EnsureCapacity(bbPoolBack.Count + count);

                    for (int i = 0; i < count; i++)
                    {
                        bbPoolBack.Add(new MyTriangleBillboard
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

                /// <summary>
                /// Converts a color to its normalized linear RGB equivalent. Assumes additive blending
                /// with premultiplied alpha.
                /// </summary>
                public static Vector4 GetBillBoardBoardColor(Color color)
                {
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }
            }
        }
    }
}