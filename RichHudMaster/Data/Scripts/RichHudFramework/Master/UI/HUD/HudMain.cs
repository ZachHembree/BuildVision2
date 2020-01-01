using RichHudFramework.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Utils;
using VRage.Game.ModAPI;
using VRageMath;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;

namespace RichHudFramework
{
    using CursorMembers = MyTuple<
        Func<bool>, // Visible
        Func<bool>, // IsCaptured
        Func<Vector2>, // Origin
        Action<object>, // Capture
        Func<object, bool>, // IsCapturing
        MyTuple<
            Func<object, bool>, // TryCapture
            Func<object, bool> // TryRelease
        >
    >;
    using HudParentMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        object, // Add (Action<HudNodeMembers>)
        Action, // BeforeDraw
        Action, // BeforeInput
        MyTuple<
            Action<object>, // RemoveChild
            Action<object> // SetFocus
        >
    >;
    using TextBoardMembers = MyTuple<
        // TextBuilderMembers
        MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<RichStringMembers, Vector2I>, // Insert
            Action // Clear
        >,
        FloatProp, // Scale
        Func<Vector2>, // Size
        Func<Vector2>, // TextSize
        Vec2Prop, // FixedSize
        Action<Vector2> // Draw 
    >;

    namespace UI.Server
    {
        using Rendering;
        using Rendering.Server;
        using HudMainMembers = MyTuple<
            HudParentMembers,
            CursorMembers,
            Func<float>, // ScreenWidth
            Func<float>, // ScreenHeight
            Func<float>, // AspectRatio
            MyTuple<
                Func<float>, // ResScale
                Func<float>, // Fov
                Func<float>, // FovScale
                MyTuple<Func<IList<RichStringMembers>>, Action<IList<RichStringMembers>>>,
                Func<TextBoardMembers> // GetNewTextBoard
            >
        >;

        public sealed partial class HudMain : ModBase.ComponentBase
        {
            public static IHudParent Root => Instance.root;
            public static ICursor Cursor => Instance.cursor;
            public static RichText ClipBoard { get; set; }
            public static float ResScale { get; private set; }
            public static UiTestPattern TestPattern { get; private set; }
            public static float ScreenWidth => Instance.screenWidth;
            public static float ScreenHeight => Instance.screenHeight;
            public static float Fov => Instance.fov;
            public static float AspectRatio => Instance.aspectRatio;
            public static float InvTextApiScale => Instance.invTextApiScale;
            public static float FovScale => Instance.fovScale;
            public static float UiBkOpacity => Instance.uiBkOpacity;

            private static HudMain Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static HudMain instance;
            private readonly HudRoot root;
            private readonly HudCursor cursor;
            private readonly Utils.Stopwatch cacheTimer;
            private float screenWidth, screenHeight, aspectRatio, invTextApiScale, fov, fovScale, uiBkOpacity;

            private HudMain() : base(false, true)
            {
                UpdateResScaling();
                UpdateFovScaling();

                root = new HudRoot();
                cursor = new HudCursor();

                cacheTimer = new Utils.Stopwatch();
                cacheTimer.Start();
            }

            public static void Init()
            {
                if (instance == null)
                {
                    instance = new HudMain();

                    instance.cursor.Visible = true;
                    TestPattern = new UiTestPattern();
                    TestPattern.Hide();
                }
            }

            public override void Close()
            {
                Instance = null;
            }

            public override void Draw()
            {
                if (cacheTimer.ElapsedMilliseconds > 1000)
                {
                    if (screenHeight != MyAPIGateway.Session.Camera.ViewportSize.Y || screenWidth != MyAPIGateway.Session.Camera.ViewportSize.X)
                        UpdateResScaling();

                    if (fov != MyAPIGateway.Session.Camera.FovWithZoom)
                        UpdateFovScaling();

                    uiBkOpacity = MyAPIGateway.Session.Config.UIBkOpacity;
                    cacheTimer.Reset();
                }

                root.BeforeDraw();
                cursor.Draw();
            }

            public override void HandleInput()
            {
                cursor.Release();
                root.BeforeInput();
                cursor.HandleInput();
            }

            private void UpdateResScaling()
            {
                screenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
                screenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
                aspectRatio = (screenWidth / screenHeight);

                invTextApiScale = 1080f / screenHeight;
                ResScale = (screenHeight > 1080f) ? screenHeight / 1080f : 1f;
            }

            private void UpdateFovScaling()
            {
                fov = MyAPIGateway.Session.Camera.FovWithZoom;
                fovScale = (float)(0.1f * Math.Tan(fov / 2d));
            }

            public static TextBoardMembers GetTextBoardData() =>
                new TextBoard().GetApiData();

            /// <summary>
            /// Converts from a coordinate in the scaled coordinate system to a concrete coordinate in pixels.
            /// </summary>
            public static Vector2 GetPixelVector(Vector2 scaledVec)
            {
                scaledVec /= 2f;

                return new Vector2
                (
                    (int)(scaledVec.X * Instance.screenWidth),
                    (int)(scaledVec.Y * Instance.screenHeight)
                );
            }

            /// <summary>
            /// Converts from a coordinate given in pixels to a scaled system based on the screen resolution.
            /// </summary>
            public static Vector2 GetNativeVector(Vector2 pixelVec)
            {
                pixelVec *= 2f;

                return new Vector2
                (
                    pixelVec.X / Instance.screenWidth,
                    pixelVec.Y / Instance.screenHeight
                );
            }

            public static HudMainMembers GetApiData()
            {
                Init();

                return new HudMainMembers()
                {
                    Item1 = instance.root.GetApiData(),
                    Item2 = instance.cursor.GetApiData(),
                    Item3 = () => instance.screenWidth,
                    Item4 = () => instance.screenHeight,
                    Item5 = () => instance.aspectRatio,
                    Item6 = new MyTuple<Func<float>, Func<float>, Func<float>, MyTuple<Func<IList<RichStringMembers>>, Action<IList<RichStringMembers>>>, Func<TextBoardMembers>>
                    {
                        Item1 = () => ResScale,
                        Item2 = () => instance.fov,
                        Item3 = () => instance.fovScale,
                        Item4 = new MyTuple<Func<IList<RichStringMembers>>, Action<IList<RichStringMembers>>>(() => ClipBoard?.GetApiData(), x => ClipBoard = new RichText(x)),
                        Item5 = GetTextBoardData,
                    }
                };
            }

            /// <summary>
            /// Root parent for all hud elements.
            /// </summary>
            private sealed class HudRoot : HudParentBase
            {
                public override bool Visible => true;

                public HudRoot() : base()
                { }
            }

            /// <summary>
            /// Draws cursor.
            /// </summary>
            private sealed class HudCursor : ICursor
            {
                public Vector2 Origin => cursor.Origin + cursor.Offset + new Vector2(-12f, 12f);
                public bool Visible { get { return visible && MyAPIGateway.Gui.ChatEntryVisible; } set { visible = value; } }
                public bool IsCaptured => CapturedElement != null;
                public object CapturedElement { get; private set; }

                private bool visible;
                private readonly TexturedBox cursor;

                public HudCursor()
                {
                    cursor = new TexturedBox()
                    {
                        Material = new Material(MyStringId.GetOrCompute("RadialShadow"), new Vector2(32f, 32f)),
                        Color = new Color(0, 0, 0, 96),
                        Width = 64f,
                        Height = 64f,
                        Visible = true,
                    };

                    var arrow = new TexturedBox(cursor)
                    {
                        Material = new Material(MyStringId.GetOrCompute("MouseCursor"), new Vector2(64f, 64f)),
                        Offset = new Vector2(-12f, 12f),
                        Width = 64f,
                        Height = 64f,
                        Visible = true
                    };
                }

                public bool IsCapturing(object possibleCaptive) =>
                    Visible && possibleCaptive == CapturedElement;

                public void Capture(object capturedElement)
                {
                    if (this.CapturedElement == null)
                        this.CapturedElement = capturedElement;
                }

                public bool TryCapture(object capturedElement)
                {
                    if (this.CapturedElement == null)
                    {
                        this.CapturedElement = capturedElement;
                        return true;
                    }
                    else
                        return false;
                }

                public bool TryRelease(object capturedElement)
                {
                    if (this.CapturedElement == capturedElement)
                    {
                        Release();
                        return true;
                    }
                    else
                        return false;
                }

                public void Release()
                {
                    CapturedElement = null;
                }

                public void Draw()
                {
                    if (Visible)
                        cursor.BeforeDraw();
                }

                public void HandleInput()
                {
                    if (Visible)
                    {
                        Vector2 pos = MyAPIGateway.Input.GetMousePosition();
                        cursor.Offset = new Vector2(pos.X - ScreenWidth / 2f, -(pos.Y - ScreenHeight / 2f));
                    }
                }

                public CursorMembers GetApiData()
                {
                    return new CursorMembers()
                    {
                        Item1 = () => Visible,
                        Item2 = () => IsCaptured,
                        Item3 = () => Origin,
                        Item4 = Capture,
                        Item5 = IsCapturing,
                        Item6 = new MyTuple<Func<object, bool>, Func<object, bool>>()
                        {
                            Item1 = TryCapture,
                            Item2 = TryRelease
                        }
                    };
                }
            }
        }
    }

    namespace UI.Client
    { }
}
