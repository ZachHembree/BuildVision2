using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRageMath;
using VRage.Utils;
using VRage.Game;
using System;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using HUDMessage = DarkHelmet.BuildVision2.HudAPIv2.HUDMessage;

namespace DarkHelmet.BuildVision2
{
    internal sealed class HudUtilities
    {
        public static HudUtilities Instance { get; private set; }
        public UiTestPattern TestPattern { get; private set; }
        public bool Heartbeat { get { return textHudApi.Heartbeat; } }

        private static BvMain Main { get { return BvMain.Instance; } }
        private HudAPIv2 textHudApi;
        private double screenWidth, screenHeight, aspectRatio, bbOriginScale, fov, fovScale;
        private List<HudElement> hudElements;

        private HudUtilities()
        {
            textHudApi = new HudAPIv2();

            screenWidth = (double)MyAPIGateway.Session.Config.ScreenWidth;
            screenHeight = (double)MyAPIGateway.Session.Config.ScreenHeight;
            aspectRatio = screenWidth / screenHeight;
            fov = MyAPIGateway.Session.Camera.FovWithZoom;
            bbOriginScale = 0.1 * Math.Tan(fov / 2d);
            fovScale = GetFovScale(fov);

            hudElements = new List<HudElement>();
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new HudUtilities();
                Instance.TestPattern = new UiTestPattern();
                Instance.TestPattern.Hide();
            }
        }

        public void Close()
        {
            textHudApi?.Close();
            Instance = null;
        }

        public void Draw()
        {
            if (Heartbeat)
            {
                if (screenWidth != MyAPIGateway.Session.Config.ScreenWidth || screenHeight != MyAPIGateway.Session.Config.ScreenHeight)
                {
                    screenWidth = (double)MyAPIGateway.Session.Config.ScreenWidth;
                    screenHeight = (double)MyAPIGateway.Session.Config.ScreenHeight;
                    aspectRatio = screenWidth / screenHeight;
                }

                if (fov != MyAPIGateway.Session.Camera.FovWithZoom)
                {
                    fov = MyAPIGateway.Session.Camera.FovWithZoom;
                    bbOriginScale = 0.1 * Math.Tan(fov / 2d);
                    fovScale = GetFovScale(fov);
                }

                foreach (HudElement element in hudElements)
                    element.Draw();
            }
        }

        /// <summary>
        /// Generates the inverse scale of the billboard at a given fov setting.
        /// </summary>
        public static double GetFovScale(double fov) // because reasons
        {
            double x = fov * (180d / Math.PI);

            if (x <= 50d)
                return (0.0000027484 * Math.Pow(x, 3d)) - (0.00032981 * Math.Pow(x, 2d)) + (0.027853 * x) - 0.23603;
            else if (x > 50d && x <= 60d)
                return (-0.0000054441 * Math.Pow(x, 3d)) + (0.00089907 * Math.Pow(x, 2d)) - (0.033591 * x) + 0.78804;
            else if (x > 60d && x <= 70d)
                return (0.000019739 * Math.Pow(x, 3d)) - (0.003634 * Math.Pow(x, 2d)) + (0.23839 * x) - 4.6516;
            else if (x > 70d && x <= 80d)
                return (-0.000029677 * Math.Pow(x, 3d)) + (0.0067435 * Math.Pow(x, 2d)) - (0.48803 * x) + 12.298;
            else if (x > 80d && x <= 90d)
                return (0.00003567 * Math.Pow(x, 3d)) - (0.0089399 * Math.Pow(x, 2d)) + (0.76664 * x) - 21.160;
            else if (x > 90d && x <= 100d)
                return (-0.000016044 * Math.Pow(x, 3d)) + (0.0050229 * Math.Pow(x, 2d)) - (0.49001 * x) + 16.540;
            else if (x > 100d && x <= 110d)
                return (0.0000056264 * Math.Pow(x, 3d)) - (0.0014781 * Math.Pow(x, 2d)) + (.16009 * x) - 5.1302;
            else
                return (-0.00001262 * Math.Pow(x, 3d)) + (0.004543 * Math.Pow(x, 2d)) - (0.50224 * x) + 19.155;
        }

        /// <summary>
        /// Pattern of textured boxes used to test scaling and positioning.
        /// </summary>
        public class UiTestPattern
        {
            private readonly TexturedBox[] testPattern;
            private bool visible;

            public UiTestPattern()
            {
                visible = false;

                testPattern = new TexturedBox[]
                {
                    new TexturedBox() // red
                    {
                        color = new Color(255, 0, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(300, 0)
                    },
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-300, 0)
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, 300)
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, -300)
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(0, 0)
                    },
                    // sqrt(50) x sqrt(50)
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-200, -200),
                        Scale = .5d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(200, -200),
                        Scale = .5d
                    },
                    // 50 x 50
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-400, -400),
                        Scale = .25d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(-400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Size = new Vector2I(100, 100),
                        Origin = new Vector2I(400, -400),
                        Scale = .25d
                    },
                };
            }

            public void Toggle()
            {
                if (visible)
                    Hide();
                else
                    Show();
            }

            public void Show()
            {
                foreach (TexturedBox box in testPattern)
                    box.Visible = true;

                visible = true;
            }

            public void Hide()
            {
                foreach (TexturedBox box in testPattern)
                    box.Visible = false;

                visible = false;
            }
        }

        public enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        /// <summary>
        /// Abstract base for hud elements
        /// </summary>
        public abstract class HudElement
        {
            public HudElement()
            {
                Instance.hudElements.Add(this);
            }

            /// <summary>
            /// If a parent is set, the hud element's position will be centered around it.
            /// </summary>
            public virtual HudElement Parent { get; set; } = null;

            /// <summary>
            /// Position using scaled coordinate system
            /// </summary>
            public virtual Vector2D ScaledPos { get; set; } = Vector2D.Zero;

            /// <summary>
            /// If set to true, the hud element will be visible. Parented elements will be hidden if the parent is not visible.
            /// </summary>
            public virtual bool Visible
            {
                get { return Parent == null ? visible : Parent.Visible && visible; }
                set { visible = value; }
            }

            protected bool visible = true;

            /// <summary>
            /// Sizing scale corrected to be consistent across both HUD Message based elements and Billboard based elements.
            /// </summary>
            public virtual double Scale { get; set; } = 1d;

            /// <summary>
            /// Current position from the center of the screen or parent in pixels. Includes parented positions.
            /// </summary>
            public virtual Vector2I Origin
            {
                get { return Parent == null ? origin : (origin + Parent.Origin + Parent.Offset); }
                set { origin = value; }
            }

            protected Vector2I origin = Vector2I.Zero;

            /// <summary>
            /// Determines location of the HUD element relative to the origin.
            /// </summary>
            public virtual Vector2I Offset { get; set; } = Vector2I.Zero;

            /// <summary>
            /// Updates settings and draws hud element.
            /// </summary>
            public abstract void Draw();
        }

        /// <summary>
        /// Scrollable list menu; the selection box position is based on the selction index.
        /// </summary>
        public class ScrollMenu : HudElement
        {
            public StringBuilder HeaderText { get { return header.Message; } set { header.Message = value; } }
            public StringBuilder FooterLeftText { get { return footerLeft.Message; } set { footerLeft.Message = value; } }
            public StringBuilder FooterRightText { get { return footerRight.Message; } set { footerRight.Message = value; } }
            public StringBuilder[] ListText
            {
                get { return listText; }
                set
                {
                    listText = value;
                    
                    for (int n = 0; n < listText.Length; n++)
                        list[n].Message = listText[n];
                }
            }

            public int selectionIndex;
            public Vector2D Size { get; private set; }
            public Color BodyColor { get { return background.color; } set { background.color = value; } }
            public Color HeaderColor
            {
                get { return headerColor; }
                set
                {
                    headerBg.color = value;
                    footerBg.color = value;
                    headerColor = value;
                }
            }
            public Color SelectionBoxColor { get { return highlightBox.color; } set { highlightBox.color = value; } }

            private static readonly Vector2I padding = new Vector2I(72, 32);
            private StringBuilder[] listText;
            private Color headerColor;

            private readonly TexturedBox headerBg, footerBg, background, highlightBox, tab;
            private readonly TextHudMessage header, footerLeft, footerRight;
            private readonly TextHudMessage[] list;
            private double currentScale = 0d;

            public ScrollMenu(int maxListLength)
            {
                background = new TexturedBox(this);

                headerBg = new TexturedBox(background);
                header = new TextHudMessage(headerBg);                

                footerBg = new TexturedBox(background);
                footerLeft = new TextHudMessage(footerBg, TextAlignment.Left);
                footerRight = new TextHudMessage(footerBg, TextAlignment.Right);

                highlightBox = new TexturedBox(background);
                tab = new TexturedBox(highlightBox, new Color(225, 225, 240, 255));

                list = new TextHudMessage[maxListLength];

                for (int n = 0; n < list.Length; n++)
                    list[n] = new TextHudMessage(background, TextAlignment.Left);
            }

            public override void Draw()
            {
                if (Visible)
                {
                    Vector2I listSize = GetListSize(), textOffset = listSize / 2, pos;
                    Origin = Instance.GetPixelPos(Utilities.Round(ScaledPos, 3));

                    if (Scale != currentScale)
                        SetScale(Scale);

                    background.Size = listSize + padding;

                    headerBg.Size = new Vector2I(background.Width, header.TextSize.Y + (int)(28d * Scale));
                    headerBg.Offset = new Vector2I(0, (headerBg.Height + background.Height) / 2 - 1);

                    pos = new Vector2I(-textOffset.X, textOffset.Y - list[0].TextSize.Y / 2);

                    for (int n = 0; n < listText.Length; n++)
                    {
                        list[n].Offset = pos;
                        list[n].Visible = true;
                        pos.Y -= list[n].TextSize.Y;
                    }

                    for (int n = listText.Length; n < list.Length; n++)
                        list[n].Visible = false;

                    highlightBox.Size = new Vector2I(listSize.X + 16, (int)(24d * Scale));
                    highlightBox.Offset = new Vector2I(0, list[selectionIndex].Offset.Y);

                    tab.Size = new Vector2I(4, highlightBox.Height);
                    tab.Offset = new Vector2I(-highlightBox.Width / 2, 0);

                    footerBg.Size = new Vector2I(background.Width, footerLeft.TextSize.Y + (int)(12d * Scale));
                    footerBg.Offset = new Vector2I(0, -(background.Height + footerBg.Height) / 2 + 1);
                    footerLeft.Offset = new Vector2I((-footerBg.Width + padding.X) / 2, 0);
                    footerRight.Offset = new Vector2I((footerBg.Width - padding.X) / 2, 0);

                    Offset = -new Vector2I(0, headerBg.Height - footerBg.Height) / 2;
                    Size = Instance.GetScaledPos(background.Size + new Vector2I(0, headerBg.Height + footerBg.Height));
                }
            }

            private Vector2I GetListSize()
            {
                Vector2I listSize, lineSize;
                int maxLineWidth = 0, footerWidth;
                listSize = Vector2I.Zero;

                for (int n = 0; n < listText.Length; n++)
                {
                    lineSize = list[n].TextSize;
                    listSize.Y += lineSize.Y;

                    if (lineSize.X > maxLineWidth)
                        maxLineWidth = lineSize.X;
                }

                if (header.TextSize.X > maxLineWidth)
                    maxLineWidth = header.TextSize.X;

                footerWidth = footerLeft.TextSize.X + footerRight.TextSize.X + padding.X;

                if (footerWidth > maxLineWidth)
                    maxLineWidth = footerWidth;

                listSize.X = maxLineWidth;
                return listSize;
            }

            private void SetScale(double scale)
            {
                header.Scale = scale * 1.1;
                footerLeft.Scale = scale;
                footerRight.Scale = scale;

                foreach (TextHudMessage element in list)
                    element.Scale = scale;

                currentScale = scale;
            }
        }

        /// <summary>
        /// Wrapper used to make precise pixel-level manipluation of Text HUD API messages easier.
        /// </summary>
        public class TextHudMessage : HudElement
        {
            public StringBuilder Message { get { return hudMessage.Message; } set { SetMessage(value); } }
            public Vector2D ScaledTextSize { get; private set; }
            public Vector2I TextSize { get { return textSize; } set { textSize = Utilities.Abs(value); } }
            public TextAlignment alignment;

            public override Vector2D ScaledPos { get { return hudMessage.Origin; } set { hudMessage.Origin = value; } }
            public override bool Visible
            {
                get
                {
                    hudMessage.Visible = Parent == null ? visible : Parent.Visible && visible;
                    return hudMessage.Visible;
                }
                set { visible = value; }
            }
            public override double Scale
            {
                get { return scale; }
                set
                {
                    scale = value;
                    hudMessage.Scale = scale * (278d / (500d - 138.75 * Instance.aspectRatio));
                }
            }

            private HUDMessage hudMessage;
            private Vector2I textSize;
            private Vector2I alignmentOffset;
            private double scale;

            public TextHudMessage(HudElement Parent = null, TextAlignment alignment = TextAlignment.Center)
            {
                this.Parent = Parent;
                this.alignment = alignment;

                hudMessage = new HUDMessage
                {
                    Blend = BlendTypeEnum.LDR,
                    Scale = Instance.aspectRatio / 2d,
                    Options = HudAPIv2.Options.Fixed,
                    Visible = false
                };
            }

            /// <summary>
            /// Updates settings of underlying Text HUD API type.
            /// </summary>
            public override void Draw()
            {
                if (Visible)
                {
                    hudMessage.Origin = Instance.GetScaledPos(Origin + Offset + alignmentOffset);
                }
            }

            private void SetMessage(StringBuilder message)
            {
                Vector2D length;

                hudMessage.Message = message;
                length = hudMessage.GetTextLength();
                ScaledTextSize = length;
                TextSize = Instance.GetPixelPos(ScaledTextSize);// the scaling on text length is a bit off
                GetAlignmentOffset();
            }

            private void GetAlignmentOffset()
            {
                alignmentOffset = textSize / 2;
                alignmentOffset.X *= -1;

                if (alignment == TextAlignment.Right)
                {
                    alignmentOffset.X -= textSize.X / 2;
                }
                else if (alignment == TextAlignment.Left)
                {
                    alignmentOffset.X += textSize.X / 2;
                }
            }
        }

        /// <summary>
        /// Creates a colored box of a given width and height with a given mateiral. The default material is just a plain color.
        /// </summary>
        public class TexturedBox : HudElement
        {
            public Vector2D scaledSize;
            public Vector2I Size { get { return size; } set { size = Utilities.Abs(value); } }
            public int Height { get { return Size.Y; } set { Size = new Vector2I(value, Size.Y); } }
            public int Width { get { return Size.X; } set { Size = new Vector2I(Size.X, value); ; } }

            public MyStringId material;
            public Color color;
            private Vector2I size;

            public TexturedBox(HudElement Parent = null, Color color = default(Color), MyStringId material = default(MyStringId))
            {
                this.color = color;
                this.Parent = Parent;

                if (material == default(MyStringId))
                    this.material = MyStringId.GetOrCompute("Square");
                else
                    this.material = material;
            }

            public override void Draw()
            {
                if (Visible)
                {
                    scaledSize = Instance.GetScaledSize(Size, Scale);
                    ScaledPos = Instance.GetScaledPos(Origin + Offset);

                    Vector2D localOrigin = ScaledPos, boardSize = scaledSize * (10d / 9d) * Instance.fovScale;

                    localOrigin.X *= Instance.bbOriginScale * Instance.aspectRatio;
                    localOrigin.Y *= Instance.bbOriginScale;

                    MatrixD cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    Vector3D boardPos = Vector3D.Transform(new Vector3D(localOrigin.X, localOrigin.Y, -0.1), cameraMatrix);

                    Quaternion rotquad = Quaternion.CreateFromAxisAngle(cameraMatrix.Forward, 0f);
                    cameraMatrix = MatrixD.Transform(cameraMatrix, rotquad);

                    MyTransparentGeometry.AddBillboardOriented
                    (
                        material,
                        color,
                        boardPos,
                        cameraMatrix.Left,
                        cameraMatrix.Up,
                        (float)boardSize.X,
                        (float)boardSize.Y,
                        Vector2.Zero,
                        BlendTypeEnum.LDR
                    );
                }
            }
        }

        /// <summary>
        /// Converts text Hud API sizing scale to pixels
        /// </summary>
        public Vector2I GetPixelSize(Vector2D scaledSize, double scale = 1d)
        {
            return new Vector2I
            (
                (int)(scaledSize.X * screenHeight * 16d / Math.Sqrt(scale)),
                (int)(scaledSize.Y * screenHeight * 16d / Math.Sqrt(scale))
            );
        }

        /// <summary>
        /// Converts from a size given in pixels to the scale used by the text Hud API
        /// </summary>
        public Vector2D GetScaledSize(Vector2I pixelSize, double scale = 1d)
        {
            return new Vector2D
            (
                pixelSize.X / screenHeight / 16d,
                pixelSize.Y / screenHeight / 16d
            ) * Math.Sqrt(scale);
        }

        /// <summary>
        /// Converts from a coordinate in the API's scaled coordinate system to a concrete coordinate in pixels.
        /// Also useful for converting text block sizes to pixels for some reason.
        /// </summary>
        public Vector2I GetPixelPos(Vector2D scaledPos)
        {
            scaledPos /= 2d;

            return new Vector2I
            (
                (int)(scaledPos.X * screenWidth),
                (int)(scaledPos.Y * screenHeight)
            );
        }

        /// <summary>
        /// Converts from a coordinate given in pixels to the API's scaled coordinate system.
        /// </summary>
        public Vector2D GetScaledPos(Vector2I pixelPos)
        {
            pixelPos *= 2;

            return new Vector2D
            (
                pixelPos.X / screenWidth,
                pixelPos.Y / screenHeight
            );
        }
    }
}
