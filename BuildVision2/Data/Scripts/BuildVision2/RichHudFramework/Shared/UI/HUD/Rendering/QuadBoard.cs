using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

        namespace Rendering
        {
            public struct CroppedBox
            {
                public static readonly BoundingBox2 defaultMask = new BoundingBox2(-Vector2.PositiveInfinity, Vector2.PositiveInfinity);

                public BoundingBox2 bounds;
                public BoundingBox2? mask;
            }

            /// <summary>
            /// Defines a rectangular billboard drawn on the HUD using a material with texture coordinates
            /// accessible for each vertex using a FlatQuad.
            /// </summary>
            public struct QuadBoard
            {
                public static readonly QuadBoard Default;

                /// <summary>
                /// Determines the extent to which the quad will be rhombused
                /// </summary>
                public float skewRatio;

                /// <summary>
                /// Material ID used by the billboard.
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Color of the billboard using native formatting
                /// </summary>
                public Vector4 bbColor;

                /// <summary>
                /// Determines the scale and aspect ratio of the texture as rendered.
                /// </summary>
                public BoundingBox2 texCoords;

                static QuadBoard()
                {
                    var matFit = new BoundingBox2(new Vector2(0f, 0f), new Vector2(0f, 1f));
                    Default = new QuadBoard(Material.Default.TextureID, matFit, Color.White);
                }

                public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Vector4 bbColor, float skewRatio = 0f)
                {
                    this.textureID = textureID;
                    this.texCoords = matFit;
                    this.bbColor = bbColor;
                    this.skewRatio = skewRatio;
                }

                public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Color color, float skewRatio = 0f)
                {
                    this.textureID = textureID;
                    this.texCoords = matFit;
                    bbColor = GetQuadBoardColor(color);
                    this.skewRatio = skewRatio;
                }

                /// <summary>
                /// Draws a billboard in world space using the quad specified.
                /// </summary>
                public void Draw(ref MyQuadD quad)
                {
                    AddBillboard(ref this, ref quad);
                }

                /// <summary>
                /// Draws a billboard in world space facing the +Z direction of the matrix specified. Units in meters matrix
                /// transform notwithstanding.
                /// </summary>
                public void Draw(Vector2 size, Vector3D origin, ref MatrixD matrix)
                {
                    MyQuadD quad;

                    Vector3D.TransformNoProjection(ref origin, ref matrix, out origin);
                    MyUtils.GenerateQuad(out quad, ref origin, size.X * .5f, size.Y * .5f, ref matrix);

                    if (skewRatio != 0f)
                    {
                        Vector3D start = quad.Point0, end = quad.Point3,
                            offset = (end - start) * skewRatio * .5;

                        quad.Point0 = Vector3D.Lerp(start, end, skewRatio) - offset;
                        quad.Point3 = Vector3D.Lerp(start, end, 1d + skewRatio) - offset;
                        quad.Point1 -= offset;
                        quad.Point2 -= offset;
                    }

                    AddBillboard(ref this, ref quad);
                }

                /// <summary>
                /// Draws a billboard in world space facing the +Z direction of the matrix specified. Units in meters, matrix
                /// transform notwithstanding.
                /// </summary>
                public void Draw(ref CroppedBox box, ref MatrixD matrix)
                {
                    Vector2 size = box.bounds.Size,
                        pos = box.bounds.Center;
                    Vector3D worldPos = new Vector3D(pos.X, pos.Y, 0d);
                    MyQuadD quad;

                    Vector3D.TransformNoProjection(ref worldPos, ref matrix, out worldPos);
                    MyUtils.GenerateQuad(out quad, ref worldPos, size.X * .5f, size.Y * .5f, ref matrix);

                    if (skewRatio != 0f)
                    {
                        Vector3D start = quad.Point0, end = quad.Point3,
                            offset = (end - start) * skewRatio * .5;

                        quad.Point0 = Vector3D.Lerp(start, end, skewRatio) - offset;
                        quad.Point3 = Vector3D.Lerp(start, end, 1d + skewRatio) - offset;
                        quad.Point1 -= offset;
                        quad.Point2 -= offset;
                    }

                    AddBillboard(ref this, ref quad);
                }

                /// <summary>
                /// Draws a cropped billboard in world space facing the +Z direction of the matrix specified. Cropping is 
                /// performed s.t. any parts outside the box defined by maskMin and maskMax are not rendered. For 
                /// NON-TEXTURED billboards ONLY. This method will warp textures. Units in meters, matrix transform 
                /// notwithstanding.
                /// </summary>
                public void DrawCropped(ref CroppedBox box, ref MatrixD matrix)
                {
                    box.bounds = box.bounds.Intersect(box.mask.Value);
                    Vector2 size = box.bounds.Size,
                        pos = box.bounds.Center;

                    Vector3D worldPos = new Vector3D(pos.X, pos.Y, 0d);
                    MyQuadD quad;

                    Vector3D.TransformNoProjection(ref worldPos, ref matrix, out worldPos);
                    MyUtils.GenerateQuad(out quad, ref worldPos, size.X * .5f, size.Y * .5f, ref matrix);

                    if (skewRatio != 0f)
                    {
                        Vector3D start = quad.Point0, end = quad.Point3,
                            offset = (end - start) * skewRatio * .5;

                        quad.Point0 = Vector3D.Lerp(start, end, skewRatio) - offset;
                        quad.Point3 = Vector3D.Lerp(start, end, 1d + skewRatio) - offset;
                        quad.Point1 -= offset;
                        quad.Point2 -= offset;
                    }

                    AddBillboard(ref this, ref quad);
                }

                /// <summary>
                /// Draws a cropped billboard in world space facing the +Z direction of the matrix specified. Cropping is 
                /// performed s.t. any parts outside the box defined by maskMin and maskMax are not rendered and WITHOUT 
                /// warping the texture or displacing the billboard. Units in meters, matrix transform notwithstanding.
                /// </summary>
                public void DrawCroppedTex(ref CroppedBox box, ref MatrixD matrix)
                {
                    Vector2 size = box.bounds.Size,
                        pos = box.bounds.Center;
                    box.bounds = box.bounds.Intersect(box.mask.Value);

                    Vector2 clipSize = box.bounds.Size;
                    CroppedQuad crop = default(CroppedQuad);
                    crop.matBounds = texCoords;

                    // Normalized cropped size and offset
                    Vector2 clipScale = clipSize / size,
                        clipOffset = (box.bounds.Center - pos) / size,
                        uvScale = crop.matBounds.Size,
                        uvOffset = crop.matBounds.Center;

                    pos += clipOffset * size; // Offset billboard to compensate for changes in size
                    size = clipSize; // Use cropped billboard size
                    clipOffset *= uvScale * new Vector2(1f, -1f); // Scale offset to fit material and flip Y-axis

                    // Recalculate texture coordinates to simulate clipping without affecting material alignment
                    crop.matBounds.Min = ((crop.matBounds.Min - uvOffset) * clipScale) + (uvOffset + clipOffset);
                    crop.matBounds.Max = ((crop.matBounds.Max - uvOffset) * clipScale) + (uvOffset + clipOffset);

                    Vector3D worldPos = new Vector3D(pos.X, pos.Y, 0d);
                    Vector3D.TransformNoProjection(ref worldPos, ref matrix, out worldPos);
                    MyUtils.GenerateQuad(out crop.quad, ref worldPos, size.X * .5f, size.Y * .5f, ref matrix);

                    if (skewRatio != 0f)
                    {
                        Vector3D start = crop.quad.Point0, end = crop.quad.Point3,
                            offset = (end - start) * skewRatio * .5;

                        crop.quad.Point0 = Vector3D.Lerp(start, end, skewRatio) - offset;
                        crop.quad.Point3 = Vector3D.Lerp(start, end, 1d + skewRatio) - offset;
                        crop.quad.Point1 -= offset;
                        crop.quad.Point2 -= offset;
                    }

                    AddBillboard(ref this, ref crop);
                }

                public static Vector4 GetQuadBoardColor(Color color)
                {   
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }

                private static void AddBillboard(ref QuadBoard qb, ref CroppedQuad crop)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        crop.quad.Point0,
                        crop.quad.Point1,
                        crop.quad.Point2,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        crop.matBounds.Min,
                        (crop.matBounds.Min + new Vector2(0f, crop.matBounds.Size.Y)),
                        crop.matBounds.Max,
                        qb.textureID, 0,
                        Vector3D.Zero,
                        qb.bbColor,
                        BlendTypeEnum.PostPP
                    );

                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        crop.quad.Point0,
                        crop.quad.Point2,
                        crop.quad.Point3,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        crop.matBounds.Min,
                        crop.matBounds.Max,
                        (crop.matBounds.Min + new Vector2(crop.matBounds.Size.X, 0f)),
                        qb.textureID, 0,
                        Vector3D.Zero,
                        qb.bbColor,
                        BlendTypeEnum.PostPP
                    );
                }

                private static void AddBillboard(ref QuadBoard qb, ref MyQuadD quad)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point1,
                        quad.Point2,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        qb.texCoords.Min,
                        (qb.texCoords.Min + new Vector2(0f, qb.texCoords.Size.Y)),
                        qb.texCoords.Max,
                        qb.textureID, 0,
                        Vector3D.Zero,
                        qb.bbColor,
                        BlendTypeEnum.PostPP
                    );

                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point2,
                        quad.Point3,
                        Vector3.Zero, Vector3.Zero, Vector3.Zero,
                        qb.texCoords.Min,
                        qb.texCoords.Max,
                        (qb.texCoords.Min + new Vector2(qb.texCoords.Size.X, 0f)),
                        qb.textureID, 0,
                        Vector3D.Zero,
                        qb.bbColor,
                        BlendTypeEnum.PostPP
                    );
                }

                private struct CroppedQuad
                {
                    public BoundingBox2 matBounds;
                    public MyQuadD quad;
                }
            }
        }
    }
}