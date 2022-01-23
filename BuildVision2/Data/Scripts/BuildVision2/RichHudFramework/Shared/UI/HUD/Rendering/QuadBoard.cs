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
                public static readonly BoundingBox2 defaultMask =
                    new BoundingBox2(-Vector2.PositiveInfinity, Vector2.PositiveInfinity);

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
                /// Determines material applied to the billboard as well as its alignment, bounding and tint
                /// </summary>
                public BoundedQuadMaterial materialData;

                static QuadBoard()
                {
                    var matFit = new BoundingBox2(new Vector2(0f, 0f), new Vector2(1f, 1f));
                    Default = new QuadBoard(Material.Default.TextureID, matFit, Color.White);
                }

                public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Vector4 bbColor, float skewRatio = 0f)
                {
                    materialData.textureID = textureID;
                    materialData.texBounds = matFit;
                    materialData.bbColor = bbColor;
                    this.skewRatio = skewRatio;
                }

                public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Color color, float skewRatio = 0f)
                {
                    materialData.textureID = textureID;
                    materialData.texBounds = matFit;
                    materialData.bbColor = BillBoardUtils.GetBillBoardBoardColor(color);
                    this.skewRatio = skewRatio;
                }

                /// <summary>
                /// Draws a billboard in world space using the quad specified.
                /// </summary>
                public void Draw(ref MyQuadD quad)
                {
                    BillBoardUtils.AddQuad(ref materialData, ref quad);
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

                    BillBoardUtils.AddQuad(ref materialData, ref quad);
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

                    BillBoardUtils.AddQuad(ref materialData, ref quad);
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

                    BillBoardUtils.AddQuad(ref materialData, ref quad);
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
                    crop.matBounds = materialData.texBounds;

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

                    AddBillboard(ref materialData, ref crop);
                }

                private static void AddBillboard(ref BoundedQuadMaterial matData, ref CroppedQuad crop)
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
                        matData.textureID, 0,
                        Vector3D.Zero,
                        matData.bbColor,
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
                        matData.textureID, 0,
                        Vector3D.Zero,
                        matData.bbColor,
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