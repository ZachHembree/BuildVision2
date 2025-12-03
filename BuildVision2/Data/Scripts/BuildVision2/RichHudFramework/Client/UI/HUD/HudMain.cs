using RichHudFramework.UI.Rendering;
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
    using Internal;
    using CursorMembers = MyTuple<
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
        Func<ApiMemberAccessor, bool>, // IsCapturing
        Func<ApiMemberAccessor, bool>, // TryCapture
        Func<ApiMemberAccessor, bool>, // TryRelease
        ApiMemberAccessor // GetOrSetMember
    >;
    using TextBuilderMembers = MyTuple<
        MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
        Func<Vector2I, int, object>, // GetCharMember
        ApiMemberAccessor, // GetOrSetMember
        Action<IList<RichStringMembers>, Vector2I>, // Insert
        Action<IList<RichStringMembers>>, // SetText
        Action // Clear
    >;

    namespace UI
    {
        using static NodeConfigIndices;
        using TextBoardMembers = MyTuple<
            TextBuilderMembers,
            FloatProp, // Scale
            Func<Vector2>, // Size
            Func<Vector2>, // TextSize
            Vec2Prop, // FixedSize
            Action<BoundingBox2, BoundingBox2, MatrixD[]> // Draw 
        >;

        namespace Client
        {
            using HudClientMembers = MyTuple<
                CursorMembers, // Cursor
                Func<TextBoardMembers>, // GetNewTextBoard
                ApiMemberAccessor, // GetOrSetMembers
                Action // Unregister
            >;

            /// <summary>
            /// Primary entry point for the client-side UI system.
            /// Provides shared access to root UI nodes, cursor, clipboard, screen metrics, and world-space
            /// projection utilities.
            /// </summary>
            public sealed partial class HudMain : RichHudClient.ApiModule
            {
                /// <summary>
                /// The root parent node for all client-side HUD elements in the framework.
                /// 
                /// <para>All UI elements added by mods should be parented to this node or 
				/// <see cref="HighDpiRoot"/> to function.</para>
                /// </summary>
                public static HudParentBase Root
                {
                    get
                    {
                        if (Instance == null)
                            Init();

                        return Instance._root;
                    }
                }

                /// <summary>
                /// The root node dedicated to high-DPI scaling for resolutions exceeding 1080p. This node
                /// automatically applies a draw matrix that rescales UI elements to compensate for the
                /// reduced apparent size caused by high-DPI displays, maintaining consistent visual sizing.
				/// 
                /// <para>All UI elements added by mods should be parented to this node or 
                /// <see cref="Root"/> to function.</para>
                /// </summary>
                public static HudParentBase HighDpiRoot
                {
                    get
                    {
                        if (Instance == null)
                            Init();

                        return Instance._highDpiRoot;
                    }
                }

                /// <summary>
                /// The shared cursor instance available across all mods. Mods can use this to handle mouse
                /// input, capture/release focus, and query position. It supports both screen-space and 
                /// world-space interactions.
                /// </summary>
                public static ICursor Cursor
                {
                    get
                    {
                        if (Instance == null)
                            Init();

                        return Instance._cursor;
                    }
                }

                /// <summary>
                /// The shared clipboard for rich text operations across mods. This allows copying formatted
                /// text (including colors, scales, and positions) from one mod's UI and pasting it into another.
                /// <para>
                /// The is separate from the system clipboard, as access to the real clipboard is write-only.
                /// </para>
                /// </summary>
                public static RichText ClipBoard
                {
                    get
                    {
                        if (Instance == null)
                            Init();

                        object value = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.ClipBoard);

                        if (value != null)
                            return new RichText(value as List<RichStringMembers>);
                        else
                            return default(RichText);
                    }
                    set
                    {
                        if (Instance == null)
                            Init();

                        Instance.GetOrSetMemberFunc(value.apiData, (int)HudMainAccessors.ClipBoard);
                    }
                }

                /// <summary>
                /// The resolution scaling factor normalized to a 1080p baseline. For resolutions above 1080p,
                /// this provides a multiplier (e.g., 2.0 for 4K) to adjust UI sizing. For resolutions at or
                /// below 1080p, it returns 1.0f to avoid unnecessary scaling.
                /// </summary>
                public static float ResScale { get; private set; }

                /// <summary>
                /// The primary transformation matrix used to convert 2D screen-space coordinates (in pixels)
                /// to 3D world-space positions (in meters). This is required for rendering UI in screen space.
                /// </summary>
                public static MatrixD PixelToWorld => PixelToWorldRef[0];

                /// <summary>
                /// The primary transformation matrix used to convert 2D screen-space coordinates (in pixels)
                /// to 3D world-space positions (in meters). This is required for rendering UI in screen space.
                /// </summary>
                public static MatrixD[] PixelToWorldRef { get; private set; }

                /// <summary>
                /// The current horizontal resolution of the screen in pixels, updated every frame.
                /// </summary>
                public static float ScreenWidth { get; private set; }

                /// <summary>
                /// The current vertical resolution of the screen in pixels, updated every frame.
                /// </summary>
                public static float ScreenHeight { get; private set; }

                /// <summary>
                /// The current screen dimensions as a Vector2 (ScreenWidth x ScreenHeight) in pixels.
                /// </summary>
                public static Vector2 ScreenDim { get; private set; }

                /// <summary>
                /// The current screen dimensions (ScreenWidth x ScreenHeight) adjusted for high-DPI scaling
                /// by dividing by ResScale, providing normalized coordinates for resolution-independent layouts.
                /// </summary>
                public static Vector2 ScreenDimHighDPI { get; private set; }

                /// <summary>
                /// The current aspect ratio of the screen, calculated as ScreenWidth / ScreenHeight (e.g., 1.777f for 16:9).
                /// </summary>
                public static float AspectRatio { get; private set; }

                /// <summary>
                /// The current field of view (FOV) angle in radians, as set by the game's camera settings.
                /// </summary>
                public static float Fov { get; private set; }

                /// <summary>
                /// A scaling factor applied to billboard matrix transforms to compensate for changes
                /// in apparent size and position due to FOV adjustments
                /// </summary>
                public static float FovScale { get; private set; }

                /// <summary>
                /// The current opacity level (0.0f to 1.0f) for in-game UI backgrounds and menus, as configured
                /// in the game's settings. Mods can use this to match native UI transparency.
                /// </summary>
                public static float UiBkOpacity { get; private set; }

                /// <summary>
                /// Enables or disables the shared cursor and switches the input mode accordingly (e.g., from
                /// movement controls to UI interaction). Setting this to true shows the cursor and allows
                /// mouse input for HUD elements.
                /// </summary>
                public static bool EnableCursor { get; set; }

                /// <summary>
                /// The current input mode for the HUD system, indicating whether UI elements should process
                /// cursor (mouse) input, text (keyboard) input, or neither.
                /// </summary>
                public static HudInputMode InputMode { get; private set; }

                /// <summary>
                /// Singleton instance of HudMain
                /// </summary>
                /// <exclude/>
                public static HudMain Instance { get; private set; }

                /// <summary>
                /// Internal root field
                /// </summary>
                /// <exclude/>
                public readonly HudParentBase _root;

                private readonly HudParentBase _highDpiRoot;
                private readonly HudCursor _cursor;
                private bool enableCursorLast, enableCursorTemp;

                private readonly Func<TextBoardMembers> GetTextBoardDataFunc;
                private readonly ApiMemberAccessor GetOrSetMemberFunc;
                private readonly Action UnregisterAction;

                private HudMain() : base(ApiModuleTypes.HudMain, false, true)
                {
                    if (Instance != null)
                        throw new Exception("Only one instance of HudMain can exist at any given time!");

                    Instance = this;
                    var members = (HudClientMembers)GetApiData();

                    _cursor = new HudCursor(members.Item1);
                    GetTextBoardDataFunc = members.Item2;
                    GetOrSetMemberFunc = members.Item3;
                    UnregisterAction = members.Item4;

                    PixelToWorldRef = new MatrixD[1];
                    _root = new HudClientRoot();
                    _highDpiRoot = new HighDpiClientRoot();

                    // Register update handle
                    GetOrSetMemberFunc(_root.DataHandle, (int)HudMainAccessors.ClientRootNode);
                    GetOrSetMemberFunc(new Action(() => ExceptionHandler.Run(BeforeMasterDraw)), (int)HudMainAccessors.SetBeforeDrawCallback);

                    UpdateCache();
                }

                /// <summary>
                /// Initializes the HudMain singleton instance, registering with the RHM Tree Manager,
				/// setting up text utilities, root nodes, and setting up the cursor.
                /// </summary>
                /// <exclude/>
                public static void Init()
                {
                    BillBoardUtils.Init();

                    if (Instance == null)
                        new HudMain();
                }

                /// <summary>
                /// Internal callback invoked before the game's master draw pass. Updates cached screen metrics
                /// and cursor state.
                /// </summary>
                private void BeforeMasterDraw()
                {
                    UpdateCache();
                    _cursor.Update();
                }

                /// <summary>
                /// Closes the HudMain instance by unregistering all API callbacks and releasing resources.
                /// </summary>
                /// <exclude/>
                public override void Close()
                {
                    UnregisterAction?.Invoke();
                    Instance = null;
                }

                /// <summary>
                /// Updates internal caches with the latest values from the RHF API, including screen
                /// resolution, FOV, input mode, and transformation matrices. Computes derived values like
                /// ScreenDim and synchronizes the EnableCursor state.
                /// </summary>
                private void UpdateCache()
                {
                    ScreenWidth = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenWidth);
                    ScreenHeight = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenHeight);
                    AspectRatio = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.AspectRatio);
                    ResScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ResScale);
                    Fov = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.Fov);
                    FovScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.FovScale);
                    PixelToWorldRef[0] = (MatrixD)GetOrSetMemberFunc(null, (int)HudMainAccessors.PixelToWorldTransform);
                    UiBkOpacity = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.UiBkOpacity);
                    InputMode = (HudInputMode)GetOrSetMemberFunc(null, (int)HudMainAccessors.InputMode);

                    ScreenDim = new Vector2(ScreenWidth, ScreenHeight);
                    ScreenDimHighDPI = ScreenDim / ResScale;

                    GetOrSetMemberFunc(EnableCursor | enableCursorTemp, (int)HudMainAccessors.EnableCursor);
                    enableCursorLast = EnableCursor;
                    enableCursorTemp = false;
                }

                /// <summary>
                /// Temporarily enables the cursor until the next frame.
                /// </summary>
                public static void EnableCursorTemp()
                {
                    if (Instance == null)
                        Init();

                    Instance.enableCursorTemp = true;
                }

                /// <summary>
                /// Retrieves a overlay offset for layering a focused UI window and registers
                /// a callback that will be invoked if another element gains focus, passing the new offset.
                /// This helps manage UI overlap and depth sorting in 3D HUD space.
                /// 
                /// <para>Layering updates and callbacks are handled automatically in <see cref="WindowBase"/>.</para>
                /// </summary>
                public static byte GetFocusOffset(Action<byte> LoseFocusCallback)
                {
                    if (Instance == null)
                        Init();

                    return (byte)Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetFocusOffset);
                }

                /// <summary>
                /// Registers a callback for changes in input focus on UI elements. The callback is invoked
                /// whenever another element (e.g., a game menu or different mod UI) takes input focus away
                /// from the registered element.
                /// 
                /// <para>Input focus is typically handled automatically in <see cref="MouseInputElement"/> and 
                /// standard library UI.</para>
                /// </summary>
                public static void GetInputFocus(IFocusHandler handler)
                {
                    if (Instance == null)
                        Init();

                    Instance.GetOrSetMemberFunc(new Action(handler.ReleaseFocus), (int)HudMainAccessors.GetInputFocus);
                }

                /// <summary>
                /// Returns internal accessors for a new TextBoard
                /// </summary>
                /// <exclude/>
                public static TextBoardMembers GetTextBoardData()
                {
                    if (Instance == null)
                        Init();

                    return Instance.GetTextBoardDataFunc();
                }

                /// <summary>
                /// The root UI element for the entire client-side HUD system. This private class implements
                /// the necessary interfaces for screen-space node management and cursor integration.
                /// </summary>
                private class HudClientRoot : HudParentBase, IReadOnlyHudSpaceNode
                {
                    /// <summary>
                    /// Determines whether the shared cursor should be drawn within this HUD space
                    /// </summary>
                    public bool DrawCursorInHudSpace { get; }

                    /// <summary>
                    /// The current position of the cursor in local 3D space (x/y in screen pixels, z=0).
                    /// Updated during layout passes.
                    /// </summary>
                    public Vector3 CursorPos { get; private set; }

                    public HudSpaceDelegate GetHudSpaceFunc { get; }

                    /// <summary>
                    /// The primary transformation matrix for projecting from the HUD plane to world space,
                    /// shared with PixelToWorldRef for consistency.
                    /// </summary>
                    public MatrixD PlaneToWorld => PlaneToWorldRef[0];

                    /// <summary>
                    /// The primary transformation matrix for projecting from the HUD plane to world space,
                    /// shared with PixelToWorldRef for consistency.
                    /// </summary>
                    public MatrixD[] PlaneToWorldRef { get; }

                    /// <summary>
                    /// A delegate that retrieves the world-space origin of this node, used for depth sorting
                    /// relative to the camera
                    /// </summary>
                    public Func<Vector3D> GetNodeOriginFunc
                    {
                        get { return DataHandle[0].Item2[0]; }
                        private set { DataHandle[0].Item2[0] = value; }
                    }

                    /// <summary>
                    /// Indicates whether this node is positioned in front of the camera (always true for the root).
                    /// </summary>
                    public bool IsInFront { get; }

                    /// <summary>
                    /// Indicates whether this node is oriented to face the camera (always true for the root,
                    /// ensuring billboarding behavior).
                    /// </summary>
                    public bool IsFacingCamera { get; }

                    /// <summary>
                    /// Initializes the root node, configuring it as the primary HUD space with cursor support,
                    /// full visibility, and shared transformation references. Registers it with the RHF API
                    /// for pixel-space queries and origins.
                    /// </summary>
                    public HudClientRoot()
                    {
                        DrawCursorInHudSpace = true;
                        HudSpace = this;
                        IsInFront = true;
                        IsFacingCamera = true;
                        PlaneToWorldRef = PixelToWorldRef;

                        GetHudSpaceFunc = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceFunc) as HudSpaceDelegate;
                        GetNodeOriginFunc = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceOriginFunc) as Func<Vector3D>;
                        _config[StateID] |= (uint)(HudElementStates.CanUseCursor | HudElementStates.IsSpaceNode);
                    }

                    protected override void Layout()
                    {
                        CursorPos = new Vector3(Cursor.ScreenPos.X, Cursor.ScreenPos.Y, 0f);
                        HudElementBase.ElementUtils.UpdateRootAnchoring(ScreenDim, children);
                    }
                }

                private class HighDpiClientRoot : ScaledSpaceNode
                {
                    public HighDpiClientRoot() : base(Root)
                    {
                        UpdateScaleFunc = () => ResScale;
                    }

                    protected override void Layout()
                    {
                        base.Layout();
                        HudElementBase.ElementUtils.UpdateRootAnchoring(ScreenDimHighDPI, children);
                    }
                }
            }
        }
    }

    namespace UI.Server
    { }
}