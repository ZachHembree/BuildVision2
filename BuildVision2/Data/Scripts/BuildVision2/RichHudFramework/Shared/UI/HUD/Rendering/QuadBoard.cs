using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;

        namespace Rendering
        {
            /// <summary>
            /// Defines a rectangular billboard drawn on the HUD using a <see cref="Material"/> positioned and framed by
            /// a <see cref="MaterialFrame"/>.
            /// </summary>
            internal struct QuadBoard
            {
                /// <summary>
                /// Material ID used by the billboard.
                /// </summary>
                public MyStringId textureID;

                /// <summary>
                /// Determines the scale and aspect ratio of the texture as rendered.
                /// </summary>
                public FlatQuad matFit;

                /// <summary>
                /// Color of the billboard using native formatting
                /// </summary>
                public Vector4 bbColor;

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

                public void Draw(Vector2 size, Vector2 origin)
                {
                    MatrixD cameraMatrix;
                    Vector3D worldPos;
                    Vector2 screenPos, boardSize;

                    boardSize = HudMain.GetRelativeVector(size) * HudMain.FovScale / 2f;
                    boardSize.X *= HudMain.AspectRatio;

                    screenPos = HudMain.GetRelativeVector(origin) * HudMain.FovScale;
                    screenPos.X *= HudMain.AspectRatio;

                    cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    worldPos = Vector3D.Transform(new Vector3D(screenPos.X, screenPos.Y, -0.1), cameraMatrix);

                    MyQuadD quad;
                    Vector3 normal = MyAPIGateway.Session.Camera.ViewMatrix.Forward;
                    MyUtils.GenerateQuad(out quad, ref worldPos, boardSize.X, boardSize.Y, ref cameraMatrix);

                    AddBillboard(worldPos, quad, normal, textureID, matFit, bbColor);
                }

                public static Vector4 GetQuadBoardColor(Color color)
                {
                    float opacity = color.A / 255f;

                    color.R = (byte)(color.R * opacity);
                    color.G = (byte)(color.G * opacity);
                    color.B = (byte)(color.B * opacity);

                    return ((Vector4)color).ToLinearRGB();
                }

                private static void AddBillboard(Vector3D pos, MyQuadD quad, Vector3 normal, MyStringId matID, FlatQuad matFit, Vector4 color)
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