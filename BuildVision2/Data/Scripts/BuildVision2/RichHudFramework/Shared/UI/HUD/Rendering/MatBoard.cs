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
                /// Distance of the material from the MatBoard's center.
                /// </summary>
                public Vector2 MatOffset
                {
                    get { return matFrame.offset; }
                    set
                    {
                        if (value != matFrame.offset)
                        {
                            updateMatFit = true;
                            matFrame.offset = value;
                        }
                    }
                }

                /// <summary>
                /// Material scale.
                /// </summary>
                public float MatScale
                {
                    get { return matFrame.scale; }
                    set
                    {
                        if (value != matFrame.scale)
                        {
                            updateMatFit = true;
                            matFrame.scale = value;
                        }
                    }
                }

                /// <summary>
                /// Texture applied to the billboard.
                /// </summary>
                public Material Material
                {
                    get { return matFrame.material; }
                    set
                    {
                        if (value != matFrame.material)
                        {
                            updateMatFit = true;
                            matFrame.material = value;
                            minBoard.textureID = value.TextureID;
                        }
                    }
                }

                /// <summary>
                /// Determines how the texture scales with the MatBoard's dimensions.
                /// </summary>
                public MaterialAlignment MatAlignment
                {
                    get { return matFrame.alignment; }
                    set
                    {
                        if (value != matFrame.alignment)
                        {
                            updateMatFit = true;
                            matFrame.alignment = value;
                        }
                    }
                }

                public Vector2 offset;

                private Vector2 size;
                private Color color;
                private bool updateMatFit;

                private QuadBoard minBoard;
                private MaterialFrame matFrame;

                public MatBoard()
                {
                    minBoard = new QuadBoard();
                    matFrame = new MaterialFrame();

                    Material = Material.Default;
                    Color = Color.White;
                    updateMatFit = true;
                }

                public void Draw(Vector2 origin)
                {
                    if (updateMatFit)
                    {
                        minBoard.matFit = matFrame.GetMaterialAlignment(size);
                        updateMatFit = false;
                    }

                    minBoard.Draw(size, origin + offset);
                }           
            }

        }
    }
}