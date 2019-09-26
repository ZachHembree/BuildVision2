using DarkHelmet.Game;
using DarkHelmet.UI.Rendering;
using DarkHelmet.IO;
using DarkHelmet.UI.TextHudApi;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Collections;
using VRage.Utils;
using VRageMath;

namespace DarkHelmet.UI
{
    public interface ICursor
    {
        bool Visible { get; set; }
        bool IsCaptured { get; }
        Vector2 Origin { get; }

        void Capture(HudElementBase capturedElement);
        bool IsCapturing(HudElementBase capturedElement);
        bool TryCapture(HudElementBase capturedElement);
        bool TryRelease(HudElementBase capturedElement);
    }

    /// <summary>
    /// Collection of tools used to make working with the Text Hud API and general GUI stuff easier; singleton.
    /// </summary>
    public sealed class HudMain : ModBase.ComponentBase
    {
        public static HudNodeBase Root => Instance.root;
        public static ICursor Cursor => Instance.cursor;
        public static float ResScale { get; private set; }
        public static UiTestPattern TestPattern { get; private set; }
        public static float ScreenWidth => Instance.screenWidth;
        public static float ScreenHeight => Instance.screenHeight;
        public static float Fov => Instance.fov;
        public static float AspectRatio => Instance.aspectRatio;
        public static float InvTextApiScale => Instance.invTextApiScale;
        public static float FovScale => Instance.fovScale;

        private static HudMain Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static HudMain instance;
        private readonly HudRoot root;
        private readonly HudCursor cursor;
        private float screenWidth, screenHeight, aspectRatio, invTextApiScale, fov, fovScale;

        private HudMain()
        {
            GetResScaling();
            GetFovScaling();            

            root = new HudRoot();
            cursor = new HudCursor();
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new HudMain();

                Cursor.Visible = true;
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
            if (HudAPIv2.Heartbeat)
            {
                if (screenHeight != MyAPIGateway.Session.Camera.ViewportSize.Y || screenWidth != MyAPIGateway.Session.Camera.ViewportSize.X)
                    GetResScaling();

                if (fov != MyAPIGateway.Session.Camera.FovWithZoom)
                    GetFovScaling();

                root.BeforeDraw();
                cursor.BeforeDraw();
            }
        }

        public override void HandleInput()
        {
            cursor.Release();
            root.BeforeInput();
            cursor.BeforeInput();           
        }

        private void GetResScaling()
        {
            screenWidth = MyAPIGateway.Session.Camera.ViewportSize.X;
            screenHeight = MyAPIGateway.Session.Camera.ViewportSize.Y;
            aspectRatio = (screenWidth / screenHeight);

            invTextApiScale = 1080f / screenHeight;
            ResScale = (screenHeight > 1080f) ? screenHeight / 1080f : 1f;
        }

        private void GetFovScaling()
        {
            fov = MyAPIGateway.Session.Camera.FovWithZoom;
            fovScale = (float)(0.1f * Math.Tan(fov / 2d));
        }

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

        /// <summary>
        /// Root parent for all hud elements.
        /// </summary>
        private sealed class HudRoot : HudNodeBase
        {
            public override bool Visible => true;

            public HudRoot() : base(null)
            { }
        }

        /// <summary>
        /// Draws cursor.
        /// </summary>
        private sealed class HudCursor : HudElementBase, ICursor
        {
            public override bool Visible { get { return base.Visible && MyAPIGateway.Gui.ChatEntryVisible; } set { base.Visible = value; } }
            public bool IsCaptured => capturedElement != null;

            private readonly TexturedBox cursorShadow, cursorBox;
            private HudNodeBase capturedElement;

            public HudCursor() : base(null)
            {
                cursorShadow = new TexturedBox(this)
                {
                    Material = new Material(MyStringId.GetOrCompute("RadialShadow"), new Vector2(32f, 32f)),
                    Color = new Color(0, 0, 0, 96),
                    Offset = new Vector2(12f, -12f),
                    Width = 64f,
                    Height = 64f,
                    Visible = true
                };

                cursorBox = new TexturedBox(this)
                {
                    Material = new Material(MyStringId.GetOrCompute("MouseCursor"), new Vector2(64f, 64f)),
                    Width = 64f,
                    Height = 64f,
                    Visible = true
                };
            }

            public bool IsCapturing(HudElementBase possibleCaptive) =>
                Visible && possibleCaptive == capturedElement;

            public void Capture(HudElementBase capturedElement)
            {
                if (this.capturedElement == null)
                    this.capturedElement = capturedElement;
            }

            public bool TryCapture(HudElementBase capturedElement)
            {
                if (this.capturedElement == null)
                {
                    this.capturedElement = capturedElement;
                    return true;
                }
                else
                    return false;
            }

            public bool TryRelease(HudElementBase capturedElement)
            {
                if (this.capturedElement == capturedElement)
                {
                    Release();
                    return true;
                }
                else
                    return false;
            }

            public void Release()
            {
                capturedElement = null;
            }

            protected override void HandleInput()
            {
                if (Visible)
                {
                    Vector2 pos = MyAPIGateway.Input.GetMousePosition();
                    Origin = new Vector2(pos.X - ScreenWidth / 2f, -(pos.Y - ScreenHeight / 2f));
                }
            }
        }

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
            backspace = SharedBinds.Back;
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
}
