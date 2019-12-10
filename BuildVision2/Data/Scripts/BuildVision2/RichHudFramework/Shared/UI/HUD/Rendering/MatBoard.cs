using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet
{
    namespace UI
    {
        using Client;
        using Server;

        namespace Rendering
        {
            /// <summary>
            /// Used to determine how a given <see cref="Material"/> is scaled on a given Billboard.
            /// </summary>
            public enum MaterialAlignment : int
            {
                /// <summary>
                /// Stretches/compresses the material to cover the whole billboard. Default behavior.
                /// </summary>
                StretchToFit = 0,

                /// <summary>
                ///  Rescales the material so that it matches the height of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitVertical = 1,

                /// <summary>
                /// Rescales the material so that it matches the width of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitHorizontal = 2,

                /// <summary>
                /// Maintains the material's aspect ratio and size at the given scale without regard
                /// to the size of the billboard.
                /// </summary>
                Fixed = 3,
            }

            public class Material
            {
                public static readonly Material Default = new Material(MyStringId.GetOrCompute("HudLibDefault"), new Vector2(4f, 4f));

                /// <summary>
                /// ID of the Texture the <see cref="Material"/> is based on.
                /// </summary>
                public readonly MyStringId TextureID;
                /// <summary>
                /// The dimensions, in pixels, of the <see cref="Material"/>.
                /// </summary>
                public readonly Vector2 size;
                /// <summary>
                /// The dimensions of the <see cref="Material"/> relative to the size of the texture its based on.
                /// </summary>
                public readonly Vector2 scaledSize;
                /// <summary>
                /// Center of the <see cref="Material"/> on the texture scaled relative to the size of the texture.
                /// </summary>
                public readonly Vector2 scaledOrigin;

                /// <summary>
                /// Creates a <see cref="Material"/> using the name of the Texture's ID and its size in pixels.
                /// </summary>
                public Material(string TextureName, Vector2 size)
                    : this(MyStringId.GetOrCompute(TextureName), size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> based on a Texture Atlas/Sprite with a given offset and size.
                /// </summary>
                public Material(string TextureName, Vector2 textureSize, Vector2 offset, Vector2 size)
                    : this(MyStringId.GetOrCompute(TextureName), textureSize, offset, size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> using the <see cref="MyStringId"/> of the texture and its size in pixels.
                /// </summary>
                public Material(MyStringId TextureID, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    scaledSize = Vector2.One;
                    scaledOrigin = scaledSize / 2f;
                }

                /// <summary>
                /// Creates a <see cref="Material"/> based on an Atlas/Sprite with a given offset and size.
                /// </summary>
                public Material(MyStringId TextureID, Vector2 textureSize, Vector2 offset, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    size.X /= textureSize.X;
                    size.Y /= textureSize.Y;

                    scaledSize = size;

                    offset.X /= textureSize.X;
                    offset.Y /= textureSize.Y;

                    scaledOrigin = offset + (scaledSize / 2f);
                }
            }

            /// <summary>
            /// Defines a quad comprised of four <see cref="Vector2"/>s.
            /// </summary>
            public struct FlatQuad
            {
                public Vector2 Point0, Point1, Point2, Point3;
            }

            public class MatBoard
            {
                public Color Color { get { return color; } set { color = value; bbColor = GetBillboardColor(value); } }
                public Vector2 Size
                {
                    get { return new Vector2(width, height); }
                    set
                    {
                        if (value != Size)
                            updateMatFit = true;

                        width = value.X; height = value.Y;
                    }
                }

                public float Width
                {
                    get { return width; }
                    set
                    {
                        if (value != width)
                            updateMatFit = true;

                        width = value;
                    }
                }
                public float Height
                {
                    get { return height; }
                    set
                    {
                        if (value != height)
                            updateMatFit = true;

                        height = value;
                    }
                }

                public Vector2 MatOffset
                {
                    get { return matOffset; }
                    set
                    {
                        if (value != matOffset)
                            updateMatFit = true;

                        matOffset = value;
                    }
                }
                public float MatScale
                {
                    get { return matScale; }
                    set
                    {
                        if (value != matScale)
                            updateMatFit = true;

                        matScale = value;
                    }
                }
                public Material Material
                {
                    get { return material; }
                    set
                    {
                        if (value != material)
                            updateMatFit = true;

                        material = value;
                    }
                }
                public MaterialAlignment MatAlignment
                {
                    get { return matAlignment; }
                    set
                    {
                        if (value != matAlignment)
                            updateMatFit = true;

                        matAlignment = value; 
                    }
                }

                public Vector2 offset;

                private Color color;
                private Vector4 bbColor;
                private float width, height, matScale;
                private Vector2 matOffset;
                private Material material;
                private MaterialAlignment matAlignment;
                private FlatQuad matFit;
                private bool updateMatFit;

                public MatBoard()
                {
                    Material = Material.Default;
                    MatAlignment = MaterialAlignment.StretchToFit;
                    Color = Color.White;
                    updateMatFit = true;
                    MatScale = 1f;
                }

                public void Draw(Vector3D origin, MatrixD matrix)
                {
                    if (updateMatFit)
                    {
                        matFit = GetMaterialAlignment();
                        updateMatFit = false;
                    }

                    Vector3D worldPos = Vector3D.Transform(origin + new Vector3D(offset.X, offset.Y, 0d), matrix);

                    MyQuadD quad;
                    Vector3 normal = MyAPIGateway.Session.Camera.ViewMatrix.Forward;
                    MyUtils.GenerateQuad(out quad, ref worldPos, Size.X, Size.Y, ref matrix);

                    RenderUtils.AddBillboard(worldPos, quad, normal, Material.TextureID, matFit, bbColor);
                }

                public void Draw(Vector2 origin)
                {
                    MatrixD cameraMatrix;
                    Vector3D worldPos;
                    Vector2 screenPos, boardSize;

                    if (updateMatFit)
                    {
                        matFit = GetMaterialAlignment();
                        updateMatFit = false;
                    }
                    
                    boardSize = HudMain.GetNativeVector(Size) * HudMain.FovScale / 2f;
                    boardSize.X *= HudMain.AspectRatio;

                    screenPos = HudMain.GetNativeVector(origin + offset) * HudMain.FovScale;
                    screenPos.X *= HudMain.AspectRatio;

                    cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    worldPos = Vector3D.Transform(new Vector3D(screenPos.X, screenPos.Y, -0.1), cameraMatrix);

                    MyQuadD quad;
                    Vector3 normal = MyAPIGateway.Session.Camera.ViewMatrix.Forward;
                    MyUtils.GenerateQuad(out quad, ref worldPos, boardSize.X, boardSize.Y, ref cameraMatrix);

                    RenderUtils.AddBillboard(worldPos, quad, normal, Material.TextureID, matFit, bbColor);
                }

                private FlatQuad GetMaterialAlignment()
                {
                    Vector2 matOrigin = Material.scaledOrigin, matStep = Material.scaledSize / 2f;

                    if (MatAlignment != MaterialAlignment.StretchToFit)
                    {
                        float xScale = Size.X / Material.size.X,
                            yScale = Size.Y / Material.size.Y;

                        if (MatAlignment == MaterialAlignment.FitVertical)
                        {
                            xScale /= yScale;
                            yScale = 1f;
                        }
                        else if (MatAlignment == MaterialAlignment.FitHorizontal)
                        {
                            yScale /= xScale;
                            xScale = 1f;
                        }
                        else if (MatAlignment == MaterialAlignment.Fixed)
                        {
                            xScale *= MatScale;
                            yScale *= MatScale;
                        }

                        matStep = new Vector2(matStep.X * xScale, matStep.Y * yScale);
                    }

                    Vector2 
                        min = matOrigin - matStep, 
                        max = matOrigin + matStep, 
                        scaledOffset = new Vector2()
                        {
                            X = Utils.Math.Clamp(MatOffset.X / Material.size.X, -1f, 1f) * Material.scaledSize.X,
                            Y = Utils.Math.Clamp(MatOffset.Y / Material.size.Y, -1f, 1f) * Material.scaledSize.Y
                        } * MatScale;

                    matOrigin += scaledOffset;

                    return new FlatQuad()
                    {
                        Point0 = Utils.Math.Clamp(matOrigin - matStep, min, max), // Bottom left
                        Point1 = Utils.Math.Clamp(matOrigin + new Vector2(-matStep.X, matStep.Y), min, max), // Upper left
                        Point2 = Utils.Math.Clamp(matOrigin + matStep, min, max), // Upper right
                        Point3 = Utils.Math.Clamp(matOrigin + new Vector2(matStep.X, -matStep.Y), min, max), // Bottom right
                    };
                }

                /// <summary>
                /// Returns the color with alpha premultiplied.
                /// </summary>
                /// <returns></returns>
                private static Vector4 GetBillboardColor(Color color)
                {
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }
            }

            public static class RenderUtils
            {
                public static void AddBillboard(Vector3D pos, MyQuadD quad, Vector3 normal, MyStringId matID, FlatQuad matFit, Vector4 color)
                {
                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point1,
                        quad.Point2,
                        normal, normal, normal,
                        matFit.Point0,
                        matFit.Point1,
                        matFit.Point2,
                        matID, 0,
                        pos,
                        color,
                        BlendTypeEnum.PostPP
                    );

                    MyTransparentGeometry.AddTriangleBillboard
                    (
                        quad.Point0,
                        quad.Point2,
                        quad.Point3,
                        normal, normal, normal,
                        matFit.Point0,
                        matFit.Point2,
                        matFit.Point3,
                        matID, 0,
                        pos,
                        color,
                        BlendTypeEnum.PostPP
                    );
                }
            }
        }
    }
}