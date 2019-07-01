using DarkHelmet.Game;
using DarkHelmet.UI.TextHudApi;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.UI
{
    public enum OriginAlignment
    {
        Center,
        UpperLeft,
        UpperRight,
        LowerRight,
        LowerLeft,
    }

    public enum OffsetAlignment
    {
        Center,
        Left,
        Top,
        Right,
        Bottom
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Collection of tools used to make working with the Text Hud API and general GUI stuff easier; singleton.
    /// </summary>
    public sealed class HudUtilities : ModBase.ComponentBase
    {
        public static double ResScale { get; private set; }
        public static UiTestPattern TestPattern { get; private set; }

        private static HudUtilities Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static HudUtilities instance;

        private readonly List<Action> hudElementsDraw;
        private readonly BindManager.Group hudBinds;
        private IKeyBind backspace;
        private double screenWidth, screenHeight, aspectRatio, invTextApiScale, fov, fovScale;

        private HudUtilities()
        {
            GetResScaling();
            GetFovScaling();

            hudElementsDraw = new List<Action>();
            hudBinds = new BindManager.Group("HudUtilities");
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new HudUtilities();

                TestPattern = new UiTestPattern();
                TestPattern.Hide();

                if (Instance.hudBinds.TryRegisterBind(new KeyBindData("backspace", new string[] { "back" })))
                    Instance.backspace = Instance.hudBinds.GetBindByName("backspace");
            }
        }

        public override void Close()
        {
            Instance = null;
        }

        public override void Draw()
        {
            if (HudAPIv2.Heartbeat)
            {
                if (screenHeight != MyAPIGateway.Session.Camera.ViewportSize.Y || screenWidth != MyAPIGateway.Session.Camera.ViewportSize.X)
                    GetResScaling();

                if (fov != MyAPIGateway.Session.Camera.FovWithZoom)
                    GetFovScaling();

                foreach (Action Draw in hudElementsDraw)
                    Draw();
            }
        }

        private void GetResScaling()
        {
            screenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
            screenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
            aspectRatio = (screenWidth / screenHeight);

            invTextApiScale = 1080d / screenHeight;
            ResScale = (screenHeight > 1080d) ? screenHeight / 1080d : 1d;
        }

        private void GetFovScaling()
        {
            fov = MyAPIGateway.Session.Camera.FovWithZoom;
            fovScale = 0.1 * Math.Tan(fov / 2d);
        }

        public sealed class TextInput : ModBase.ComponentBase
        {
            public static string CurrentText
            {
                get { return Instance.currentText.ToString(); }
                set { Instance.currentText.Clear(); Instance.currentText.Append(value); }
            }
            public static bool Open { get; set; }

            private static TextInput Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static TextInput instance;
            private readonly StringBuilder currentText;

            private TextInput()
            {
                currentText = new StringBuilder(50);
                HudUtilities.Instance.backspace.OnPressAndHold += Backspace;
            }

            private static void Init()
            {
                if (instance == null)
                    instance = new TextInput();
            }

            public static void Clear() =>
                Instance?.currentText.Clear();

            private void Backspace()
            {
                if (Open && currentText.Length > 0)
                    currentText.Remove(CurrentText.Length - 1, 1);
            }

            public override void Close()
            {
                instance = null;
            }

            public override void HandleInput()
            {
                if (Open)
                {
                    ListReader<char> input = MyAPIGateway.Input.TextInput;

                    for (int n = 0; n < input.Count; n++)
                    {
                        if (input[n] >= ' ' && input[n] <= '~')
                            currentText.Append(input[n]);
                    }
                }
            }
        }

        /// <summary>
        /// Abstract base for hud elements
        /// </summary>
        public abstract class HudElementBase
        {
            /// <summary>
            /// If set to true, the hud element will be visible. Parented elements will be hidden if the parent is not visible.
            /// </summary>
            public bool Visible
            {
                get { return parent == null ? visible : parent.Visible && visible; }
                set { visible = value; }
            }
            /// <summary>
            /// Sizing scale corrected to be consistent across both HUD Message based elements and Billboard based elements.
            /// </summary>
            public virtual Vector2I Size { get; protected set; }
            public virtual Vector2D ScaledSize { get { return GetScaledVector(Size); } protected set { Size = Utils.Math.Abs(GetPixelVector(value)); } }
            public virtual double Scale
            {
                get { return (parent == null || ignoreParentScale) ? scale : scale * parent.Scale; }
                set { scale = value; }
            }

            /// <summary>
            /// Current position from the center of the screen in pixels.
            /// </summary>
            public Vector2I Origin
            {
                get { return parent == null ? origin : (origin + parent.Origin + parent.Offset + GetAlignmentOffset()); }
                set { origin = value; }
            }
            /// <summary>
            /// Determines location of the HUD element relative to the origin.
            /// </summary>
            public Vector2I Offset { get; set; }
            /// <summary>
            /// Position using scaled coordinate system
            /// </summary>
            public virtual Vector2D ScaledOrigin { get { return GetScaledVector(Origin); } set { Origin = GetPixelVector(value); } }

            /// <summary>
            /// If a parent is set, the hud element's origin will be centered around it.
            /// </summary>
            public HudElementBase parent;
            public OffsetAlignment offsetAlignment;
            private Vector2I origin;
            private double scale;
            private bool visible;
            private readonly bool ignoreParentScale;

            public HudElementBase(HudElementBase parent, OffsetAlignment offsetAlignment, bool ignoreParentScale)
            {
                Scale = 1d;
                Visible = true;
                this.ignoreParentScale = ignoreParentScale;

                this.parent = parent;
                this.offsetAlignment = offsetAlignment;
                Instance.hudElementsDraw.Add(BeforeDraw);
            }

            protected virtual void BeforeDraw()
            {
                if (Visible)
                {
                    Draw();
                }
            }

            /// <summary>
            /// Updates settings and draws hud element.
            /// </summary>
            protected virtual void Draw() { }

            protected virtual Vector2I GetAlignmentOffset()
            {
                if (offsetAlignment == OffsetAlignment.Bottom)
                    return new Vector2I(0, -(parent.Size.Y + Size.Y) / 2 + parent.Size.Y % 2);
                else if (offsetAlignment == OffsetAlignment.Left)
                    return new Vector2I(-(parent.Size.X + Size.X) / 2 + Size.X % 2, 0);
                else if (offsetAlignment == OffsetAlignment.Right)
                    return new Vector2I(+(parent.Size.X + Size.X) / 2 - parent.Size.X % 2, 0);
                else if (offsetAlignment == OffsetAlignment.Top)
                    return new Vector2I(0, +(parent.Size.Y + Size.Y) / 2 - Size.Y % 2);
                else
                    return Vector2I.Zero;
            }
        }

        public abstract class ResizableElementBase : HudElementBase
        {
            public sealed override Vector2I Size { get { return new Vector2I(Width, Height); } protected set { Width = value.X; Height = value.Y; } }
            public virtual int Width { get { return width; } set { width = Math.Abs(value); } }
            public virtual int Height { get { return height; } set { height = Math.Abs(value); } }

            private int width, height;

            public ResizableElementBase(HudElementBase parent, OffsetAlignment offsetAlignment, bool ignoreParentScale) : base(parent, offsetAlignment, ignoreParentScale)
            { }

            public virtual void SetSize(Vector2I newSize) =>
                Size = newSize;
        }

        public abstract class TextBoxBase : ResizableElementBase
        {
            public Vector2I MinimumSize
            {
                get
                {
                    Vector2I minSize = TextSize;
                    minSize.X += (int)(Padding.X * Scale);
                    minSize.Y += (int)(Padding.Y * Scale);
                    return minSize;
                }
            }
            public Vector2I Padding { get { return padding; } set { padding = Utils.Math.Abs(value); } }
            public abstract Vector2I TextSize { get; }
            public virtual double TextScale { get; set; }

            private Vector2I padding;

            public TextBoxBase(HudElementBase parent, Vector2I padding, OffsetAlignment offsetAlignment, bool ignoreParentScale) : base(parent, offsetAlignment, ignoreParentScale)
            {
                TextScale = 1d;
                Padding = padding;
            }

            protected override void BeforeDraw()
            {
                if (Visible)
                {
                    Vector2I minSize = MinimumSize;

                    if (Width < minSize.X)
                        Width = minSize.X;

                    if (Height < minSize.Y)
                        Height = minSize.Y;

                    Draw();
                }
            }
        }

        /// <summary>
        /// Wrapper used to make precise pixel-level manipluation of <see cref="HudAPIv2.HUDMessage"/> easier.
        /// </summary>
        public class TextHudMessage : HudElementBase
        {
            public TextAlignment alignment;
            private HudAPIv2.HUDMessage hudMessage;
            private Vector2I alignmentOffset;
            private string text;
            private bool updateSize;

            public string Text { get { return text; } set { text = value; UpdateMessage(); } }
            public override double Scale
            {
                get => base.Scale;
                set
                {
                    base.Scale = value;

                    if (hudMessage != null)
                    {
                        hudMessage.Scale = base.Scale * Instance.invTextApiScale;
                        updateSize = true;
                    }
                }
            }

            public TextHudMessage(HudElementBase parent = null, TextAlignment alignment = TextAlignment.Center, 
                OffsetAlignment offsetAlignment = OffsetAlignment.Center, bool ignoreParentScale = false) : base(parent, offsetAlignment, ignoreParentScale)
            {
                this.alignment = alignment;
                updateSize = false;
            }

            protected override void Draw()
            {
                if (hudMessage == null)
                {
                    hudMessage = new HudAPIv2.HUDMessage
                    {
                        Blend = BlendTypeEnum.PostPP,
                        Scale = Scale * Instance.invTextApiScale,
                        Options = HudAPIv2.Options.Fixed,
                        Visible = false,
                    };

                    UpdateMessage();
                    UpdateSize();
                }

                if (updateSize)
                    UpdateSize();

                hudMessage.Origin = GetScaledVector(Origin + Offset + alignmentOffset);
                hudMessage.Draw();
            }

            private void UpdateMessage()
            {
                if (hudMessage != null && Text != null)
                {
                    hudMessage.Message.Clear();
                    hudMessage.Message.Append(Text);
                    updateSize = true;
                }
            }

            private void UpdateSize()
            {
                ScaledSize = hudMessage.GetTextLength();
                UpdateTextOffset();
                updateSize = false;
            }

            private void UpdateTextOffset()
            {
                alignmentOffset = Size / 2;
                alignmentOffset.X *= -1;

                if (alignment == TextAlignment.Right)
                    alignmentOffset.X -= Size.X / 2;
                else if (alignment == TextAlignment.Left)
                    alignmentOffset.X += Size.X / 2;
            }
        }

        /// <summary>
        /// Creates a colored box of a given width and height with a given mateiral. The default material is just a plain color.
        /// </summary>
        public class TexturedBox : ResizableElementBase
        {
            public override double Scale
            {
                get => base.Scale;
                set
                {
                    if (value != base.Scale)
                    {
                        base.Scale = value;
                        scaleSqrt = Math.Sqrt(value);
                    }
                }
            }

            public MyStringId material;
            public Color color;
            private double scaleSqrt;
            private static readonly MyStringId square = MyStringId.GetOrCompute("Square");

            public TexturedBox(HudElementBase parent = null, OffsetAlignment offsetAlignment = OffsetAlignment.Center, Color color = default(Color), MyStringId material = default(MyStringId), bool ignoreParentScale = false)
                : base(parent, offsetAlignment, ignoreParentScale)
            {
                this.color = color;

                if (material == default(MyStringId))
                    this.material = square;
                else
                    this.material = material;
            }

            protected override void Draw()
            {
                if (color.A > 0)
                {
                    MatrixD cameraMatrix;
                    Quaternion rotquad;
                    Vector3D boardPos;
                    Vector2D boardOrigin, boardSize;

                    boardSize = ScaledSize * Instance.fovScale * scaleSqrt / 2d;
                    boardSize.X *= Instance.aspectRatio;

                    boardOrigin = GetScaledVector(Origin + Offset) * Instance.fovScale;
                    boardOrigin.X *= Instance.aspectRatio;

                    cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                    boardPos = Vector3D.Transform(new Vector3D(boardOrigin.X, boardOrigin.Y, -0.1), cameraMatrix);

                    rotquad = Quaternion.CreateFromAxisAngle(cameraMatrix.Forward, 0f);
                    cameraMatrix = MatrixD.Transform(cameraMatrix, rotquad);

                    MyTransparentGeometry.AddBillboardOriented
                    (
                        material,
                        GetBillboardColor(color),
                        boardPos,
                        cameraMatrix.Left,
                        cameraMatrix.Up,
                        (float)boardSize.X,
                        (float)boardSize.Y,
                        Vector2.Zero,
                        BlendTypeEnum.PostPP
                    );
                }
            }

            private static Color GetBillboardColor(Color color)
            {
                double opacity = color.A / 255d;

                color.R = (byte)(color.R * opacity);
                color.G = (byte)(color.G * opacity);
                color.B = (byte)(color.B * opacity);

                return color;
            }
        }

        /// <summary>
        /// Converts from a coordinate in the scaled coordinate system to a concrete coordinate in pixels.
        /// </summary>
        public static Vector2I GetPixelVector(Vector2D scaledVec)
        {
            scaledVec /= 2d;

            return new Vector2I
            (
                (int)(scaledVec.X * Instance.screenWidth),
                (int)(scaledVec.Y * Instance.screenHeight)
            );
        }

        /// <summary>
        /// Converts from a coordinate given in pixels to the API's scaled coordinate system.
        /// </summary>
        public static Vector2D GetScaledVector(Vector2I pixelVec)
        {
            pixelVec *= 2;

            return new Vector2D
            (
                pixelVec.X / Instance.screenWidth,
                pixelVec.Y / Instance.screenHeight
            );
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
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(300, 0)
                    },
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(-300, 0)
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(0, 300)
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(0, -300)
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(0, 0)
                    },
                    // sqrt(50) x sqrt(50)
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(-200, -200),
                        Scale = .5d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(-200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(200, -200),
                        Scale = .5d
                    },
                    // 50 x 50
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(-400, -400),
                        Scale = .25d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(-400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2I(400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Height = 100,
                        Width = 100,
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

    }
}
