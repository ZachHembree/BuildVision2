using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;

        namespace Rendering
        {
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
                            minBoard.bbColor = QuadBoard.GetQuadBoardColor(value);

                        color = value;
                    }
                }

                /// <summary>
                /// Size of the billboard.
                /// </summary>
                public Vector2 Size
                {
                    get { return size; }
                    set
                    {
                        if (value != size)
                            updateMatFit = true;

                        size = value;
                    }
                }

                /// <summary>
                /// Width of the billboard.
                /// </summary>
                public float Width
                {
                    get { return size.X; }
                    set
                    {
                        if (value != size.X)
                            updateMatFit = true;

                        size.X = value;
                    }
                }

                /// <summary>
                /// Height of the billboard.
                /// </summary>
                public float Height
                {
                    get { return size.Y; }
                    set
                    {
                        if (value != size.Y)
                            updateMatFit = true;

                        size.Y = value;
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
                            updateMatFit = true;
                            matFrame.Material = value;
                            minBoard.textureID = value.TextureID;
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
                            updateMatFit = true;
                            matFrame.Alignment = value;
                        }
                    }
                }

                private Vector2 size;
                private Color color;
                private bool updateMatFit;

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
                    updateMatFit = true;
                }

                /// <summary>
                /// Draws a billboard in world space using the quad specified.
                /// </summary>
                public void Draw(ref MyQuadD quad)
                {
                    if (updateMatFit && matFrame.Material != Material.Default)
                    {
                        minBoard.matFit = matFrame.GetMaterialAlignment(size.X / size.Y);
                        updateMatFit = false;
                    }

                    minBoard.Draw(ref quad);
                }

                /// <summary>
                /// Draws a billboard in world space facing the +Z direction of the matrix given. Units in meters,
                /// matrix transform notwithstanding. Dont forget to compensate for perspective scaling!
                /// </summary>
                public void Draw(Vector3D offset, ref MatrixD matrix)
                {
                    if (updateMatFit && matFrame.Material != Material.Default)
                    {
                        minBoard.matFit = matFrame.GetMaterialAlignment(size.X / size.Y);
                        updateMatFit = false;
                    }

                    minBoard.Draw(size, offset, ref matrix);
                }

                /// <summary>
                /// Draws a billboard in world space facing the +Z direction of the matrix given. Units in meters,
                /// matrix transform notwithstanding. Dont forget to compensate for perspective scaling!
                /// </summary>
                public void Draw(Vector2 offset, ref MatrixD matrix)
                {
                    if (updateMatFit && matFrame.Material != Material.Default)
                    {
                        minBoard.matFit = matFrame.GetMaterialAlignment(size.X / size.Y);
                        updateMatFit = false;
                    }

                    minBoard.Draw(size, offset, ref matrix);
                }

                /// <summary>
                /// Draws a billboard in screen space at the given position in pixels.
                /// </summary>
                public void Draw(Vector2 origin)
                {
                    if (updateMatFit && matFrame.Material != Material.Default)
                    {
                        minBoard.matFit = matFrame.GetMaterialAlignment(size.X / size.Y);
                        updateMatFit = false;
                    }

                    minBoard.Draw(size, origin);
                }           
            }
        }
    }
}