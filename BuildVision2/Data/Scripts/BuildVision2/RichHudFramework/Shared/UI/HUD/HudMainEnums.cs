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
            /// out: RichText, in: RichText
            /// </summary>
            ClipBoard = 8
        }
    }
}
