using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;

namespace RichHudFramework
{
    using Client;
    using CursorMembers = MyTuple<
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
        Func<object, bool>, // IsCapturing
        Func<object, bool>, // TryCapture
        Func<object, bool>, // TryRelease
        ApiMemberAccessor // GetOrSetMember
    >;
    using TextBoardMembers = MyTuple<
        // TextBuilderMembers
        MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            ApiMemberAccessor, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<IList<RichStringMembers>>, // SetText
            Action // Clear
        >,
        FloatProp, // Scale
        Func<Vector2>, // Size
        Func<Vector2>, // TextSize
        Vec2Prop, // FixedSize
        Action<Vector2, MatrixD> // Draw 
    >;

    namespace UI.Client
    {
        using HudClientMembers = MyTuple<
            CursorMembers, // Cursor
            Func<TextBoardMembers>, // GetNewTextBoard
            ApiMemberAccessor, // GetOrSetMembers
            Action // Unregister
        >;
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        public sealed partial class HudMain : RichHudClient.ApiModule<HudClientMembers>
        {
            /// <summary>
            /// Root parent for all HUD elements.
            /// </summary>
            public static HudParentBase Root
            {
                get
                {
                    if (_instance == null)
                        Init();

                    return _instance.root;
                }
            }

            /// <summary>
            /// Cursor shared between mods.
            /// </summary>
            public static ICursor Cursor
            {
                get
                {
                    if (_instance == null)
                        Init();

                    return _instance.cursor;
                }
            }

            /// <summary>
            /// Shared clipboard.
            /// </summary>
            public static RichText ClipBoard
            {
                get
                {
                    object value = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.ClipBoard);

                    if (value != null)
                        return new RichText(value as IList<RichStringMembers>);
                    else
                        return default(RichText);
                }
                set { Instance.GetOrSetMemberFunc(value.ApiData, (int)HudMainAccessors.ClipBoard); }
            }

            /// <summary>
            /// Resolution scale normalized to 1080p for resolutions over 1080p. Returns a scale of 1f
            /// for lower resolutions.
            /// </summary>
            public static float ResScale { get; private set; }

            /// <summary>
            /// Matrix used to convert from 2D pixel-value screen space coordinates to worldspace.
            /// </summary>
            public static MatrixD PixelToWorld { get; private set; }

            /// <summary>
            /// The current horizontal screen resolution in pixels.
            /// </summary>
            public static float ScreenWidth { get; private set; }

            /// <summary>
            /// The current vertical resolution in pixels.
            /// </summary>
            public static float ScreenHeight { get; private set; }

            /// <summary>
            /// The current aspect ratio (ScreenWidth/ScreenHeight).
            /// </summary>
            public static float AspectRatio { get; private set; }

            /// <summary>
            /// The current field of view
            /// </summary>
            public static float Fov { get; private set; }

            /// <summary>
            /// Scaling used by MatBoards to compensate for changes in apparent size and position as a result
            /// of changes to Fov.
            /// </summary>
            public static float FovScale { get; private set; }

            /// <summary>
            /// The current opacity for the in-game menus as configured.
            /// </summary>
            public static float UiBkOpacity { get; private set; }

            /// <summary>
            /// Used to indicate when the draw list should be refreshed. Resets every frame.
            /// </summary>
            public static bool RefreshDrawList { get; set; }

            /// <summary>
            /// If true then the cursor will be visible while chat is open
            /// </summary>
            public static bool EnableCursor { get; set; }

            private static HudMain Instance
            {
                get { Init(); return _instance; }
                set { _instance = value; }
            }
            private static HudMain _instance;

            private readonly HudClientRoot root;
            private readonly HudCursor cursor;
            private bool enableCursorLast, refreshLast;

            private readonly Func<TextBoardMembers> GetTextBoardDataFunc;
            private readonly ApiMemberAccessor GetOrSetMemberFunc;
            private readonly Action UnregisterAction;

            private HudMain() : base(ApiModuleTypes.HudMain, false, true)
            {
                var members = GetApiData();

                cursor = new HudCursor(members.Item1);
                GetTextBoardDataFunc = members.Item2;
                GetOrSetMemberFunc = members.Item3;
                UnregisterAction = members.Item4;

                root = new HudClientRoot();

                // Register update delegate
                GetOrSetMemberFunc(new Action<List<HudUpdateAccessors>, byte>(root.GetUpdateAccessors), (int)HudMainAccessors.GetUpdateAccessors);
            }

            private static void Init()
            {
                if (_instance == null)
                {
                    _instance = new HudMain();
                    _instance.root.CustomDrawAction = _instance.HudMasterDraw;
                    _instance.UpdateCache();
                }
            }

            /// <summary>
            /// Updates cached values used to render UI elements.
            /// </summary>
            private void HudMasterDraw()
            {
                UpdateCache();
            }

            private void UpdateCache()
            {
                cursor.Update();

                ScreenWidth = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenWidth);
                ScreenHeight = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenHeight);
                AspectRatio = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.AspectRatio);
                ResScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ResScale);
                Fov = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.Fov);
                FovScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.FovScale);
                PixelToWorld = (MatrixD)GetOrSetMemberFunc(null, (int)HudMainAccessors.PixelToWorldTransform);
                UiBkOpacity = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.UiBkOpacity);

                if (EnableCursor != enableCursorLast)
                    GetOrSetMemberFunc(EnableCursor, (int)HudMainAccessors.EnableCursor);
                else
                    EnableCursor = (bool)GetOrSetMemberFunc(null, (int)HudMainAccessors.EnableCursor);

                if (RefreshDrawList != refreshLast)
                    GetOrSetMemberFunc(RefreshDrawList, (int)HudMainAccessors.RefreshDrawList);
                else
                    RefreshDrawList = (bool)GetOrSetMemberFunc(null, (int)HudMainAccessors.RefreshDrawList);

                enableCursorLast = EnableCursor;
                refreshLast = RefreshDrawList;
            }

            public override void Close()
            {
                UnregisterAction();
                Instance = null;
            }

            /// <summary>
            /// Returns the ZOffset for focusing a window and registers a callback
            /// for when another object takes focus.
            /// </summary>
            public static byte GetFocusOffset(Action<byte> LoseFocusCallback) =>
                (byte)Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetFocusOffset);

            /// <summary>
            /// Returns accessors for a new TextBoard
            /// </summary>
            public static TextBoardMembers GetTextBoardData() =>
                Instance.GetTextBoardDataFunc();

            /// <summary>
            /// Converts from a position in absolute screen space coordinates to a position in pixels.
            /// </summary>
            public static Vector2 GetPixelVector(Vector2 scaledVec)
            {
                if (_instance == null)
                    Init();

                return new Vector2
                (
                    (int)(scaledVec.X * ScreenWidth),
                    (int)(scaledVec.Y * ScreenHeight)
                );
            }

            /// <summary>
            /// Converts from a coordinate given in pixels to a position in absolute units.
            /// </summary>
            public static Vector2 GetAbsoluteVector(Vector2 pixelVec)
            {
                if (_instance == null)
                    Init();

                return new Vector2
                (
                    pixelVec.X / ScreenWidth,
                    pixelVec.Y / ScreenHeight
                );
            }

            /// <summary>
            /// Root UI element for the client. Registered directly to master root.
            /// </summary>
            private class HudClientRoot : HudParentBase, IReadOnlyHudSpaceNode
            {
                public override bool Visible => true;

                public bool DrawCursorInHudSpace => true;

                public override IReadOnlyHudSpaceNode HudSpace => this;

                public Vector3 CursorPos => new Vector3(Cursor.ScreenPos.X, Cursor.ScreenPos.Y, 0f);

                public HudSpaceDelegate GetHudSpaceFunc { get; }

                public MatrixD PlaneToWorld => PixelToWorld;

                public Func<MatrixD> UpdateMatrixFunc => null;

                public Func<Vector3D> GetNodeOriginFunc { get; }

                public Action CustomDrawAction;

                public HudClientRoot()
                {
                    GetHudSpaceFunc = () => new MyTuple<bool, float, MatrixD>(true, 1f, PixelToWorld);
                    GetNodeOriginFunc = () => PixelToWorld.Translation;
                }

                protected override void Layout()
                {
                    CustomDrawAction?.Invoke();
                }
            }
        }
    }

    namespace UI.Server
    { }
}