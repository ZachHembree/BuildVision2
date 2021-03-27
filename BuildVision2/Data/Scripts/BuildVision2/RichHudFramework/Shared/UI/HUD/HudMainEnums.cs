namespace RichHudFramework
{
    namespace UI
    {
        public enum HudMainAccessors : int
        {
            /// <summary>
            /// out: float
            /// </summary>
            ScreenWidth = 1,

            /// <summary>
            /// out: float
            /// </summary>
            ScreenHeight = 2,

            /// <summary>
            /// out: float
            /// </summary>
            AspectRatio = 3,

            /// <summary>
            /// out: float
            /// </summary>
            ResScale = 4,

            /// <summary>
            /// out: float
            /// </summary>
            Fov = 5,

            /// <summary>
            /// out: float
            /// </summary>
            FovScale = 6,

            /// <summary>
            /// out: MatrixD
            /// </summary>
            PixelToWorldTransform = 7,

            /// <summary>
            /// in/out: RichText
            /// </summary>
            ClipBoard = 8,

            /// <summary>
            /// out: float
            /// </summary>
            UiBkOpacity = 9,

            /// <summary>
            /// in/out: bool
            /// </summary>
            EnableCursor = 10,

            /// <summary>
            /// in/out: bool
            /// </summary>
            RefreshDrawList = 11,

            /// <summary>
            /// in/out: Action<List<HudUpdateAccessors>, byte>
            /// </summary>
            GetUpdateAccessors = 12,

            /// <summary>
            /// out: byte, in: Action{byte}
            /// </summary>
            GetFocusOffset = 13,

            /// <summary>
            /// out: HudSpaceDelegate
            /// </summary>
            GetPixelSpaceFunc = 14,

            /// <summary>
            /// out: Func{Vector3D}
            /// </summary>
            GetPixelSpaceOriginFunc = 15,

            /// <summary>
            /// in: Action
            /// </summary>
            GetInputFocus = 16,

            /// <summary>
            /// out: int
            /// </summary>
            TreeRefreshRate = 17
        }
    }
}
