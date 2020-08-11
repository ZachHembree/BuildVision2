using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    using CursorMembers = MyTuple<
        Func<bool>, // Visible
        Func<bool>, // IsCaptured
        Func<Vector2>, // Position
        Func<Vector3D>, // WorldPos
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        MyTuple<
            Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
            Func<object, bool>, // IsCapturing
            Func<object, bool>, // TryCapture
            Func<object, bool>, // TryRelease
            ApiMemberAccessor // GetOrSetMember
        >
    >;

    namespace UI.Client
    {
        /// <summary>
        /// Wrapper for the cursor rendered by the Rich HUD Framework
        /// </summary>
        public class HudCursor : ICursor
        {
            /// <summary>
            /// Indicates whether the cursor is currently visible
            /// </summary>
            public bool Visible => IsVisibleFunc();

            /// <summary>
            /// Returns true if the cursor has been captured by a UI element
            /// </summary>
            public bool IsCaptured => IsCapturedFunc();

            /// <summary>
            /// The position of the cursor in pixels in screen space
            /// </summary>
            public Vector2 ScreenPos => GetScreenPosFunc();

            /// <summary>
            /// Position of the cursor in world space.
            /// </summary>
            public Vector3D WorldPos => GetWorldPosFunc();

            private readonly Func<bool> IsVisibleFunc;
            private readonly Func<bool> IsCapturedFunc;
            private readonly Func<Vector2> GetScreenPosFunc;
            private readonly Func<Vector3D> GetWorldPosFunc;
            private readonly Func<HudSpaceDelegate, bool> IsCapturingSpaceFunc;
            private readonly Func<float, HudSpaceDelegate, bool> TryCaptureHudSpaceFunc;
            private readonly Func<object, bool> IsCapturingFunc;
            private readonly Func<object, bool> TryCaptureFunc;
            private readonly Func<object, bool> TryReleaseFunc;

            public HudCursor(CursorMembers members)
            {
                IsVisibleFunc = members.Item1;
                IsCapturedFunc = members.Item2;
                GetScreenPosFunc = members.Item3;
                GetWorldPosFunc = members.Item4;
                IsCapturingSpaceFunc = members.Item5;

                var members2 = members.Item6;

                TryCaptureHudSpaceFunc = members2.Item1;
                IsCapturingFunc = members2.Item2;
                TryCaptureFunc = members2.Item3;
                TryReleaseFunc = members2.Item4;
            }

            /// <summary>
            /// Returns true if the given HUD space is being captured by the cursor
            /// </summary>
            public bool IsCapturingSpace(HudSpaceDelegate GetHudSpaceFunc) =>
                IsCapturingSpaceFunc(GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            public bool TryCaptureHudSpace(float depth, HudSpaceDelegate GetHudSpaceFunc) =>
                TryCaptureHudSpaceFunc(depth, GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            public void CaptureHudSpace(float depth, HudSpaceDelegate GetHudSpaceFunc) =>
                TryCaptureHudSpaceFunc(depth, GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor with the given object
            /// </summary>
            public void Capture(object capturedElement) =>
                TryCaptureFunc(capturedElement);

            /// <summary>
            /// Indicates whether the cursor is being captured by the given element.
            /// </summary>
            public bool IsCapturing(object capturedElement) =>
                IsCapturingFunc(capturedElement);

            /// <summary>
            /// Attempts to capture the cursor using the given object. Returns true on success.
            /// </summary>
            public bool TryCapture(object capturedElement) =>
                TryCaptureFunc(capturedElement);

            /// <summary>
            /// Attempts to release the cursor from the given element. Returns false if
            /// not capture or if not captured by the object given.
            /// </summary>
            public bool TryRelease(object capturedElement) =>
                TryRelease(capturedElement);
        }
    }
}
