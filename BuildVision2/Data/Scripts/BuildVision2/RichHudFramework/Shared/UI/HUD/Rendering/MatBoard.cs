using VRageMath;
using System;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;

        namespace Rendering
        {
            using Client;
            using Server;

            public class MatBoard
            {
                /// <summary>
                /// Coloring applied to the material.
                /// </summary>
                public Color Color
                {
                    get { return color; }
                    set
                    {
                        if (value != color)
                            minBoard.materialData.bbColor = value.GetBbColor();

                        color = value;
                    }
                }

                /// <summary>
                /// Texture applied to the billboard.
                /// </summary>
                public Material Material
                {
                    get { return matFrame.Material; }
                    set
                    {
                        if (value != matFrame.Material)
                        {
                            bbAspect = -1f;
                            matFrame.Material = value;
                            minBoard.materialData.textureID = value.TextureID;
                        }
                    }
                }

                /// <summary>
                /// Determines how the texture scales with the MatBoard's dimensions.
                /// </summary>
                public MaterialAlignment MatAlignment
                {
                    get { return matFrame.Alignment; }
                    set
                    {
                        if (value != matFrame.Alignment)
                        {
                            bbAspect = -1f;
                            matFrame.Alignment = value;
                        }
                    }
                }

                private Color color;
                private float bbAspect;

                private QuadBoard minBoard;
                private readonly MaterialFrame matFrame;

                /// <summary>
                /// Initializes a new matboard with a size of 0 and a blank, white material.
                /// </summary>
                public MatBoard()
                {
                    matFrame = new MaterialFrame();
                    minBoard = QuadBoard.Default;

                    color = Color.White;
                    bbAspect = -1f;
                }

                /// <summary>
                /// Draws a billboard in world space using the quad specified.
                /// </summary>
                public void Draw(ref MyQuadD quad)
                {
                    minBoard.Draw(ref quad);
                }

                /// <summary>
                /// Draws a billboard in world space facing the +Z direction of the matrix given. Units in meters,
                /// matrix transform notwithstanding. Dont forget to compensate for perspective scaling!
                /// </summary
                public void Draw(ref CroppedBox box, MatrixD[] matrixRef)
                {
                    ContainmentType containment = ContainmentType.Contains;

                    if (box.mask != null)
                        box.mask.Value.Contains(ref box.bounds, out containment);

                    if (containment != ContainmentType.Disjoint)
                    {
                        if (matFrame.Material != Material.Default)
                        {
                            Vector2 boxSize = box.bounds.Size;
                            float newAspect = (boxSize.X / boxSize.Y);

                            if (Math.Abs(bbAspect - newAspect) > 1E-5f)
                            {
                                bbAspect = newAspect;
                                minBoard.materialData.texBounds = matFrame.GetMaterialAlignment(bbAspect);
                            }
                        }

                        minBoard.Draw(ref box, matrixRef);
                    }
                }     
            }
        }
    }
}