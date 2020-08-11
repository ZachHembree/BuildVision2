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
            public class MaterialFrame
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
                /// Texture coordinate offset
                /// </summary>
                public Vector2 uvOffset;

                public MaterialFrame()
                {
                    alignment = MaterialAlignment.StretchToFit;
                    uvOffset = Vector2.Zero;
                }

                /// <summary>
                /// Calculates the texture coordinates needed to fit the material to the billboard. 
                /// Aspect ratio = Width/Height
                /// </summary>
                public FlatQuad GetMaterialAlignment(float bbAspectRatio)
                {
                    Vector2 matOrigin = material.uvOffset + uvOffset, 
                        matStep = material.uvSize / 2f;

                    if (alignment != MaterialAlignment.StretchToFit)
                    {
                        float matAspectRatio = material.size.X / material.size.Y;
                        Vector2 localUV = new Vector2(1f);

                        if (alignment == MaterialAlignment.FitAuto)
                        {
                            if (matAspectRatio > bbAspectRatio) // If material is too wide, make it shorter
                                localUV = new Vector2(1f, matAspectRatio / bbAspectRatio);
                            else // If the material is too tall, make it narrower
                                localUV = new Vector2(bbAspectRatio /matAspectRatio, 1f);
                        }
                        else if (alignment == MaterialAlignment.FitVertical)
                        {
                            localUV = new Vector2(bbAspectRatio / matAspectRatio, 1f);
                        }
                        else if (alignment == MaterialAlignment.FitHorizontal)
                        {
                            localUV = new Vector2(1f, matAspectRatio / bbAspectRatio);
                        }

                        matStep *= localUV;
                    }

                    Vector2
                        min = matOrigin - matStep,
                        max = matOrigin + matStep;
                   
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