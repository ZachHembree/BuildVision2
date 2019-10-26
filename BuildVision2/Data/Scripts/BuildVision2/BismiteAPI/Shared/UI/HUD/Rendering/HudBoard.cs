using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using System;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>;

namespace DarkHelmet
{
    using RichCharMembers = MyTuple<char, GlyphFormatMembers>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI
    {
        using LineMembers = MyTuple<
            Func<int, RichCharMembers>, // GetChar
            Func<int> // Count
        >;

        namespace Rendering
        {
            /// <summary>
            /// Used to determine how a given <see cref="Material"/> is scaled on a given Billboard.
            /// </summary>
            public enum MaterialAlignment : int
            {
                /// <summary>
                /// Stretches/compresses the material to fit on the billboard
                /// </summary>
                StretchToFit = 0,
                /// <summary>
                ///  Resizes the material so that it matches the height of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitVertical = 1,
                /// <summary>
                /// Resizes the material so that it matches the width of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitHorizontal = 2,
                /// <summary>
                /// No resizing, the material is rendered at its native resolution without regard to the size of the Billboard
                /// </summary>
                None = 3
            }

            public class Material
            {
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
                /// The starting point of the <see cref="Material"/> on the texture scaled relative to the size of the texture.
                /// </summary>
                public readonly Vector2 scaledOffset;

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

                    scaledOffset = Vector2.Zero;
                    scaledSize = Vector2.One;
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

                    scaledOffset = offset;
                }
            }

            /// <summary>
            /// Defines a quad comprised of four <see cref="Vector2"/>s.
            /// </summary>
            public struct FlatQuad
            {
                public Vector2 Point0, Point1, Point2, Point3;
            }

            public class HudBoard
            {
                public Color Color { get { return color; } set { color = value; bbColor = GetBillboardColor(value); } }
                public Vector2 Size { get { return new Vector2(width, height); } set { width = value.X; height = value.Y; updateMatFit = true; } }
                public float Width { get { return width; } set { width = value; updateMatFit = true; } }
                public float Height { get { return height; } set { height = value; updateMatFit = true; } }

                public Material Material { get { return material; } set { material = value; updateMatFit = true; } }
                public MaterialAlignment MatAlignment { get { return matAlignment; } set { matAlignment = value; updateMatFit = true; } }
                public Vector2 offset;

                private static readonly Material flatColor = new Material(MyStringId.GetOrCompute("HudLibDefault"), new Vector2(4f, 4f));

                private Color color;
                private Vector4 bbColor;
                private float width, height;
                private Material material;
                private MaterialAlignment matAlignment;
                private FlatQuad matFit;
                private bool updateMatFit;

                public HudBoard()
                {
                    Material = flatColor;
                    MatAlignment = MaterialAlignment.StretchToFit;
                    Color = Color.White;
                    updateMatFit = true;
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
                    float xScale, yScale;

                    if (MatAlignment == MaterialAlignment.StretchToFit)
                    {
                        xScale = 1f;
                        yScale = 1f;
                    }
                    else
                    {
                        xScale = Size.X / Material.size.X;
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
                    }

                    return new FlatQuad()
                    {
                        Point0 = Material.scaledOffset,
                        Point1 = new Vector2(Material.scaledOffset.X, Material.scaledOffset.Y + (Material.scaledSize.Y * yScale)),
                        Point2 = new Vector2(Material.scaledOffset.X + (Material.scaledSize.X * xScale), Material.scaledOffset.Y + (Material.scaledSize.Y * yScale)),
                        Point3 = new Vector2(Material.scaledOffset.X + (Material.scaledSize.X * xScale), Material.scaledOffset.Y),
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