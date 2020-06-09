using Sandbox.ModAPI;
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
            /// <summary>
            /// Defines a rectangular billboard drawn on the HUD using a <see cref="Material"/> positioned and framed by
            /// a <see cref="MaterialFrame"/>.
            /// </summary>
            public struct QuadBoard
            {
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
                public FlatQuad matFit;

                public QuadBoard(MyStringId textureID, FlatQuad matFit, Vector4 bbColor)
                {
                    this.textureID = textureID;
                    this.matFit = matFit;
                    this.bbColor = bbColor;
                }

                public QuadBoard(MyStringId textureID, FlatQuad matFit, Color color)
                {
                    this.textureID = textureID;
                    this.matFit = matFit;
                    bbColor = GetQuadBoardColor(color);
                }

                public void Draw(Vector2 size, Vector2 origin, ref MatrixD matrix)
                {
                    Vector3D worldPos = new Vector3D(origin.X, origin.Y, 1d);
                    MyQuadD quad;

                    Vector3D.Transform(ref worldPos, ref matrix, out worldPos);
                    MyUtils.GenerateQuad(out quad, ref worldPos, size.X / 2f, size.Y / 2f, ref matrix);

                    AddBillboard(worldPos, ref quad, matrix.Forward, textureID, ref matFit, bbColor);
                }

                public void Draw(Vector2 size, Vector2 origin)
                {
                    MatrixD ptw = HudMain.PixelToWorld;
                    Vector3D worldPos = new Vector3D(origin.X, origin.Y, 1d);
                    MyQuadD quad;

                    Vector3D.Transform(ref worldPos, ref ptw, out worldPos);
                    MyUtils.GenerateQuad(out quad, ref worldPos, size.X / 2f, size.Y / 2f, ref ptw);

                    AddBillboard(worldPos, ref quad, ptw.Forward, textureID, ref matFit, bbColor);
                }

                public static Vector4 GetQuadBoardColor(Color color)
                {
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }

                private static void AddBillboard(Vector3D pos, ref MyQuadD quad, Vector3 normal, MyStringId matID, ref FlatQuad matFit, Vector4 color)
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