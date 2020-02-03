using VRageMath;

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
                public readonly Vector2 Point0, Point1, Point2, Point3;

                public FlatQuad(Vector2 Point0, Vector2 Point1, Vector2 Point2, Vector2 Point3)
                {
                    this.Point0 = Point0;
                    this.Point1 = Point1;
                    this.Point2 = Point2;
                    this.Point3 = Point3;
                }
            }

            /// <summary>
            /// Defines the positioning and alignment of a Material on a QuadBoard.
            /// </summary>
            internal class MaterialFrame
            {
                /// <summary>
                /// Texture associated with the frame
                /// </summary>
                public Material material;

                /// <summary>
                /// Determines how or if the material is scaled w/respect to its aspect ratio.
                /// </summary>
                public MaterialAlignment alignment;

                /// <summary>
                /// Material scale. Applied after alignment scaling.
                /// </summary>
                public float scale;

                /// <summary>
                /// Determines the material's distance from the center of the billboard.
                /// </summary>
                public Vector2 offset;

                public MaterialFrame()
                {
                    alignment = MaterialAlignment.StretchToFit;
                    scale = 1f;
                    offset = Vector2.Zero;
                }

                /// <summary>
                /// Calculates the UV alignment needed to fit the billboard.
                /// </summary>
                public FlatQuad GetMaterialAlignment(Vector2 bbSize)
                {
                    Vector2 matOrigin = material.scaledOrigin, matStep = material.scaledSize / 2f;

                    if (alignment != MaterialAlignment.StretchToFit)
                    {
                        float xScale = bbSize.X / material.size.X,
                            yScale = bbSize.Y / material.size.Y;

                        if (alignment == MaterialAlignment.FitVertical)
                        {
                            xScale /= yScale;
                            yScale = 1f;
                        }
                        else if (alignment == MaterialAlignment.FitHorizontal)
                        {
                            yScale /= xScale;
                            xScale = 1f;
                        }
                        else if (alignment == MaterialAlignment.Fixed)
                        {
                            xScale *= scale;
                            yScale *= scale;
                        }

                        matStep = new Vector2(matStep.X * xScale, matStep.Y * yScale);
                    }

                    Vector2
                        min = matOrigin - matStep,
                        max = matOrigin + matStep,
                        scaledOffset = new Vector2()
                        {
                            X = MathHelper.Clamp(offset.X / material.size.X, -1f, 1f) * material.scaledSize.X,
                            Y = MathHelper.Clamp(offset.Y / material.size.Y, -1f, 1f) * material.scaledSize.Y
                        } * scale;

                    matOrigin += scaledOffset;
                   
                    return new FlatQuad
                    (
                        Vector2.Clamp(matOrigin - matStep, min, max), // Bottom left
                        Vector2.Clamp(matOrigin + new Vector2(-matStep.X, matStep.Y), min, max), // Upper left
                        Vector2.Clamp(matOrigin + matStep, min, max), // Upper right
                        Vector2.Clamp(matOrigin + new Vector2(matStep.X, -matStep.Y), min, max) // Bottom right
                    );
                }
            }

        }
    }
}