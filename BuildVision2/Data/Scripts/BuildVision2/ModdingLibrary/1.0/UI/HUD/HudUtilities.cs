using DarkHelmet.Game;
using DarkHelmet.UI.TextHudApi;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRageMath;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Determines where the origin of a hud element is located on the element. When set to Center, it's at the center
    /// of the element, when set to UpperLeft the origin is at the element's upper left, etc.
    /// </summary>
    public enum OriginAlignment
    {
        Center,
        UpperLeft,
        UpperRight,
        LowerRight,
        LowerLeft,
        Auto
    }

    /// <summary>
    /// Determines how a hud element is aligned with respect to the parent. If set to Center, its origin starts at its
    /// parent's center, if set to left, it starts with its right side aligned with its parent's left.
    /// </summary>
    public enum ParentAlignment
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
        public const string LineBreak = null;
        public const double LineSpacing = 20d;

        public static double ResScale { get; private set; }
        public static UiTestPattern TestPattern { get; private set; }
        public static double ScreenWidth => Instance.screenWidth;
        public static double ScreenHeight => Instance.screenHeight;
        public static double AspectRatio => Instance.aspectRatio;
        public static double InvTextApiScale => Instance.invTextApiScale;
        public static double Fov => Instance.fov;
        public static double FovScale => Instance.fovScale;
        public static BindManager.Group SharedBinds => Instance.sharedBinds;

        private static HudUtilities Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static HudUtilities instance;

        private readonly List<Action> hudElementsDraw;
        private readonly BindManager.Group sharedBinds;
        private double screenWidth, screenHeight, aspectRatio, invTextApiScale, fov, fovScale;

        private HudUtilities()
        {
            GetResScaling();
            GetFovScaling();

            hudElementsDraw = new List<Action>();
            sharedBinds = new BindManager.Group("HudUtilities");
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new HudUtilities();

                TestPattern = new UiTestPattern();
                TestPattern.Hide();

                SharedBinds.RegisterBinds(new KeyBindData[]
                {
                    new KeyBindData("enter", new string[] { "enter" }),
                    new KeyBindData("back", new string[] { "back" }),
                    new KeyBindData("delete", new string[] { "delete" }),
                    new KeyBindData("escape", new string[] { "escape" }),
                });
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

        /// <summary>
        /// Captures text input and allows for backspacing. Special characters are ignored. Will not prevent key presses for text input from being registered elsewhere.
        /// </summary>
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

            private readonly IBind backspace;

            private TextInput()
            {
                currentText = new StringBuilder(50);
                backspace = SharedBinds["back"];
                backspace.OnPressAndHold += Backspace;
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
        public abstract class ElementBase
        {
            /// <summary>
            /// If set to true, the hud element will be visible. Parented elements will be hidden if the parent is not visible.
            /// </summary>
            public virtual bool Visible
            {
                get { return parent == null ? visible : parent.Visible && visible; }
                set { visible = value; }
            }
            /// <summary>
            /// Hud element size in pixels with scaling applied.
            /// </summary>
            public Vector2D Size
            {
                get { return UnscaledSize * Scale; }
                protected set { UnscaledSize = value / Scale; }
            }
            /// <summary>
            /// Hud element size in pixels without scaling applied.
            /// </summary>
            public virtual Vector2D UnscaledSize
            {
                get { return unscaledSize; }
                protected set { unscaledSize = Utils.Math.Abs(value); }
            }
            /// <summary>
            /// Hud element size using scale based on screen resolution without element scaling applied. 
            /// </summary>
            public Vector2D RelativeSize
            {
                get { return GetRelativeVector(UnscaledSize); }
                protected set { UnscaledSize = GetPixelVector(value); }
            }
            /// <summary>
            /// Scales the area covered by the hud element or each dimension by Sqrt(Scale)
            /// </summary>
            public virtual double Scale
            {
                get { return (parent == null || ignoreParentScale) ? scale : scale * parent.Scale; }
                set { scale = value; }
            }
            /// <summary>
            /// Current position from the center of the screen in pixels.
            /// </summary>
            public Vector2D Origin { get { return GetAlignedOrigin(); } set { origin = value; } }
            /// <summary>
            /// Determines location of the HUD element relative to the origin.
            /// </summary>
            public virtual Vector2D Offset { get; set; }
            /// <summary>
            /// Position using scale based on screen resolution. 
            /// </summary>
            public Vector2D ScaledOrigin { get { return GetRelativeVector(Origin); } set { Origin = GetPixelVector(value); } }

            /// <summary>
            /// If a parent is set, the hud element's scale and origin will be based on that of the parent.
            /// </summary>
            public ElementBase parent;
            /// <summary>
            /// If true, the hud element's scale will vary indepentently of the parent's scale.
            /// </summary>
            public bool ignoreParentScale;
            /// <summary>
            /// Determines how the hud element is centered about the origin. If set to Center, the origin will be located in the element's center,
            /// if set to UpperLeft, it will be at the upper left corner of the element, etc.
            /// </summary>
            public OriginAlignment originAlignment;
            /// <summary>
            /// Determines how the element is oriented with respect to its parent. If set to center, and with an Offset of zero, it will
            /// be centered at the parent's center, if set to top, it will be placed so its bottom lines up with the top of the parent.
            /// </summary>
            public ParentAlignment parentAlignment;

            private double scale;
            private Vector2D unscaledSize, origin;
            private bool visible;

            public ElementBase()
            {
                Scale = 1d;
                Visible = true;
                parent = null;
                ignoreParentScale = false;
                originAlignment = OriginAlignment.Center;
                parentAlignment = ParentAlignment.Center;
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
            /// Updates draws the hud element.
            /// </summary>
            protected virtual void Draw() { }

            private Vector2D GetOriginWithOffset() =>
                (parent == null) ? origin : (origin + parent.Origin + parent.Offset + GetParentAlignment());

            private Vector2D GetAlignedOrigin()
            {
                Vector2D origin = GetOriginWithOffset(), alignment;

                if (originAlignment == OriginAlignment.UpperLeft)
                    alignment = new Vector2D(Size.X / 2d, -Size.Y / 2d);
                else if (originAlignment == OriginAlignment.UpperRight)
                    alignment = new Vector2D(-Size.X / 2d, -Size.Y / 2d);
                else if (originAlignment == OriginAlignment.LowerRight)
                    alignment = new Vector2D(-Size.X / 2d, Size.Y / 2d);
                else if (originAlignment == OriginAlignment.LowerLeft)
                    alignment = new Vector2D(Size.X / 2d, Size.Y / 2d);
                else if (originAlignment == OriginAlignment.Auto)
                {
                    alignment = Vector2D.Zero;
                    alignment.X = origin.X < 0 ? Size.X / 2d : -Size.X / 2d;
                    alignment.Y = origin.Y < 0 ? Size.Y / 2d : -Size.Y / 2d;
                }
                else
                    alignment = Vector2D.Zero;

                return origin + alignment;
            }

            private Vector2D GetParentAlignment()
            {
                Vector2D alignment;

                if (parentAlignment == ParentAlignment.Bottom)
                    alignment = new Vector2D(0, -(parent.Size.Y + Size.Y) / 2d);
                else if (parentAlignment == ParentAlignment.Left)
                    alignment = new Vector2D(-(parent.Size.X + Size.X) / 2d, 0);
                else if (parentAlignment == ParentAlignment.Right)
                    alignment = new Vector2D((parent.Size.X + Size.X) / 2d, 0);
                else if (parentAlignment == ParentAlignment.Top)
                    alignment = new Vector2D(0, (parent.Size.Y + Size.Y) / 2d);
                else
                    alignment = Vector2D.Zero;

                return alignment;
            }
        }

        /// <summary>
        /// Converts from a coordinate in the scaled coordinate system to a concrete coordinate in pixels.
        /// </summary>
        public static Vector2D GetPixelVector(Vector2D scaledVec)
        {
            scaledVec /= 2d;

            return new Vector2D
            (
                Math.Truncate(scaledVec.X * Instance.screenWidth),
                Math.Truncate(scaledVec.Y * Instance.screenHeight)
            );
        }

        /// <summary>
        /// Converts from a coordinate given in pixels to the API's scaled coordinate system.
        /// </summary>
        public static Vector2D GetRelativeVector(Vector2D pixelVec)
        {
            pixelVec *= 2d;

            return new Vector2D
            (
                pixelVec.X / Instance.screenWidth,
                pixelVec.Y / Instance.screenHeight
            );
        }
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
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(300, 0)
                    },
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(-300, 0)
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(0, 300)
                    },
                    new TexturedBox() // red
                    {
                        color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(0, -300)
                    },
                    new TexturedBox() // yellow
                    {
                        color = new Color(210, 190, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(0, 0)
                    },
                    // sqrt(50) x sqrt(50)
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(-200, -200),
                        Scale = .5d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(-200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(200, 200),
                        Scale = .5d
                    },
                    new TexturedBox() // red
                    {
                        color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(200, -200),
                        Scale = .5d
                    },
                    // 50 x 50
                    new TexturedBox() // green
                    {
                        color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(-400, -400),
                        Scale = .25d
                    },
                    new TexturedBox() // blue
                    {
                        color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(-400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // purple
                    {
                        color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(400, 400),
                        Scale = .25d
                    },
                    new TexturedBox() // red
                    {
                        color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Origin = new Vector2D(400, -400),
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
