using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Defines a quad comprised of four <see cref="Vector2"/>s.
            /// </summary>
            public struct FlatQuad
            {
                public Vector2 Point0, Point1, Point2, Point3;

                public FlatQuad(Vector2 Point0, Vector2 Point1, Vector2 Point2, Vector2 Point3)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                    this.Point3 = Point3;
                }
            }

            /// <summary>
            /// A set of three vectors defining a triangle
            /// </summary>
            public struct Triangle
            {
                public Vector2 Point0, Point1, Point2;

                public Triangle(Vector2 Point0, Vector2 Point1, Vector2 Point2)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                }
            }

            /// <summary>
            /// Material data for rendering individual triangles.
            /// </summary>
            public struct TriMaterial
            {
                public static readonly TriMaterial Default = new TriMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = BillBoardUtils.GetBillBoardBoardColor(Color.White),
                    texCoords = new Triangle(
                        new Vector2(0f, 0f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 0f)
                    )
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public Triangle texCoords;
            }

            /// <summary>
            /// Material data for rendering quads.
            /// </summary>
            public struct QuadMaterial
            {
                public static readonly QuadMaterial Default = new QuadMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = BillBoardUtils.GetBillBoardBoardColor(Color.White),
                    texCoords = new FlatQuad(
                        new Vector2(0f, 0f),
                        new Vector2(0f, 1f),
                        new Vector2(1f, 0f),
                        new Vector2(1f, 1f)
                    )
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public FlatQuad texCoords;
            }

            /// <summary>
            /// Material data for rendering quads with texture coordinates defined by a bounding box.
            /// </summary>
            public struct BoundedQuadMaterial
            {
                public static readonly BoundedQuadMaterial Default = new BoundedQuadMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = BillBoardUtils.GetBillBoardBoardColor(Color.White),
                    texBounds = new BoundingBox2(new Vector2(0f, 0f), new Vector2(1f, 1f))
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Determines the scale and aspect ratio of the texture as rendered
                /// </summary>
                public BoundingBox2 texBounds;
            }

            /// <summary>
            /// Material data for rendering polygons.
            /// </summary>
            public struct PolyMaterial
            {
                public static readonly PolyMaterial Default = new PolyMaterial()
                {
                    textureID = Material.Default.TextureID,
                    bbColor = BillBoardUtils.GetBillBoardBoardColor(Color.White),
                    texCoords = null
                };

                /// <summary>
                /// Material ID used by billboards
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Normalized Linear RGB color
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Min/max texcoords
                /// </summary>
                public BoundingBox2 texBounds;

                /// <summary>
                /// Normalized texture coordinates
                /// </summary>
                public List<Vector2> texCoords;
            }

            public static class BillBoardUtils
            {
                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices and the tex coords are parallel to the vertex list.
                /// </summary>
                public static void AddTriangles(List<int> indices, List<Vector3D> vertices, ref PolyMaterial mat)
                {
                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        MyTransparentGeometry.AddTriangleBillboard
                        (
                            vertices[indices[i]],
                            vertices[indices[i + 1]],
                            vertices[indices[i + 2]],
                            Vector3.Zero, Vector3.Zero, Vector3.Zero,
                            mat.texCoords[indices[i]],
                            mat.texCoords[indices[i + 1]],
                            mat.texCoords[indices[i + 2]],
                            mat.textureID, 0,
                            Vector3D.Zero,
                            mat.bbColor,
                            BlendTypeEnum.PostPP
                        );
                    }
                }

                /// <summary>
                /// Adds a triangles in the given starting index range
                /// </summary>
                public static void AddTriangleRange(Vector2I range, List<int> indices, List<Vector3D> vertices, ref PolyMaterial mat)
                {
                    for (int i = range.X; i <= range.Y; i += 3)
                    {
                        MyTransparentGeometry.AddTriangleBillboard
                        (
                            vertices[indices[i]],
                            vertices[indices[i + 1]],
                            vertices[indices[i + 2]],
                            Vector3.Zero, Vector3.Zero, Vector3.Zero,
                            mat.texCoords[indices[i]],
                            mat.texCoords[indices[i + 1]],
                            mat.texCoords[indices[i + 2]],
                            mat.textureID, 0,
                            Vector3D.Zero,
                            mat.bbColor,
                            BlendTypeEnum.PostPP
                        );
                    }
                }

                /// <summary>
                /// Renders a polygon from a given set of unique vertex coordinates. Triangles are defined by their
                /// indices.
                /// </summary>
                public static void AddTriangles(List<int> indices, List<Vector3D> vertices, ref TriMaterial mat)
                {
                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        MyTransparentGeometry.AddTriangleBillboard
                        (
                            vertices[indices[i]],
                            vertices[indices[i + 1]],
                            vertices[indices[i + 2]],
                            Vector3.Zero, Vector3.Zero, Vector3.Zero,
                            mat.texCoords.Point0,
                            mat.texCoords.Point1,
                            mat.texCoords.Point2,
                            mat.textureID, 0,
                            Vector3D.Zero,
                            mat.bbColor,
                            BlendTypeEnum.PostPP
                        );
                    }
                }

                /// <summary>
                /// Adds a triangle starting at the given index.
                /// </summary>
                public static void AddTriangle(int start, List<int> indices, List<Vector3D> vertices, ref TriMaterial mat)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        vertices[indices[start]],
                        vertices[indices[start + 1]],
                        vertices[indices[start + 2]],
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texCoords.Point0,
                        mat.texCoords.Point1,
                        mat.texCoords.Point2,
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );
                }

                public static void AddTriangle(ref TriMaterial mat, ref MyQuadD quad)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point1,
                        quad.Point2,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texCoords.Point0,
                        mat.texCoords.Point1,
                        mat.texCoords.Point2,
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );
                }

                public static void AddQuad(ref QuadMaterial mat, ref MyQuadD quad)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point1,
                        quad.Point2,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texCoords.Point0,
                        mat.texCoords.Point1,
                        mat.texCoords.Point2,
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );

                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point2,
                        quad.Point3,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texCoords.Point0,
                        mat.texCoords.Point2,
                        mat.texCoords.Point3,
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );
                }

                public static void AddQuad(ref BoundedQuadMaterial mat, ref MyQuadD quad)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point1,
                        quad.Point2,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texBounds.Min,
                        (mat.texBounds.Min + new Vector2(0f, mat.texBounds.Size.Y)),
                        mat.texBounds.Max,
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );

                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point2,
                        quad.Point3,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        mat.texBounds.Min,
                        mat.texBounds.Max,
                        (mat.texBounds.Min + new Vector2(mat.texBounds.Size.X, 0f)),
                        mat.textureID, 0,
                        Vector3D.Zero,
                        mat.bbColor,
                        BlendTypeEnum.PostPP
                    );
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