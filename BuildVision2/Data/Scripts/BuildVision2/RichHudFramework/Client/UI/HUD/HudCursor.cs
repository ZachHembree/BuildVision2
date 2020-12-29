using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    using CursorMembers = MyTuple<
        Func<HudSpaceDelegate, bool>, // IsCapturingSpace
        Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
        Func<object, bool>, // IsCapturing
        Func<object, bool>, // TryCapture
        Func<object, bool>, // TryRelease
        ApiMemberAccessor // GetOrSetMember
    >;

    namespace UI.Client
    {
        public sealed partial class HudMain
        {
            /// <summary>
            /// Wrapper for the cursor rendered by the Rich HUD Framework
            /// </summary>
            private class HudCursor : ICursor
            {
                /// <summary>
                /// Indicates whether the cursor is currently visible
                /// </summary>
                public bool Visible { get; private set; }

                /// <summary>
                /// Returns true if the cursor has been captured by a UI element
                /// </summary>
                public bool IsCaptured { get; private set; }

                /// <summary>
                /// The position of the cursor in pixels in screen space
                /// </summary>
                public Vector2 ScreenPos { get; private set; }

                /// <summary>
                /// Position of the cursor in world space.
                /// </summary>
                public Vector3D WorldPos { get; private set; }

                public LineD WorldLine { get; private set; }

                private readonly Func<HudSpaceDelegate, bool> IsCapturingSpaceFunc;
                private readonly Func<float, HudSpaceDelegate, bool> TryCaptureHudSpaceFunc;
                private readonly Func<object, bool> IsCapturingFunc;
                private readonly Func<object, bool> TryCaptureFunc;
                private readonly Func<object, bool> TryReleaseFunc;
                private readonly ApiMemberAccessor GetOrSetMemberFunc;

                public HudCursor(CursorMembers members)
                {
                    IsCapturingSpaceFunc = members.Item1;
                    TryCaptureHudSpaceFunc = members.Item2;
                    IsCapturingFunc = members.Item3;
                    TryCaptureFunc = members.Item4;
                    TryReleaseFunc = members.Item5;
                    GetOrSetMemberFunc = members.Item6;
                }

                public void Update()
                {
                    Visible = (bool)GetOrSetMemberFunc(null, (int)HudCursorAccessors.Visible);
                    IsCaptured = (bool)GetOrSetMemberFunc(null, (int)HudCursorAccessors.IsCaptured);
                    ScreenPos = (Vector2)GetOrSetMemberFunc(null, (int)HudCursorAccessors.ScreenPos);
                    WorldPos = (Vector3D)GetOrSetMemberFunc(null, (int)HudCursorAccessors.WorldPos);
                    WorldLine = (LineD)GetOrSetMemberFunc(null, (int)HudCursorAccessors.WorldLine);
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
                public bool TryCaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc) =>
                    TryCaptureHudSpaceFunc(depthSquared, GetHudSpaceFunc);

                /// <summary>
                /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
                /// is true, then the cursor will be drawn in the given space.
                /// </summary>
                public void CaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc) =>
                    TryCaptureHudSpaceFunc(depthSquared, GetHudSpaceFunc);

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
                    TryReleaseFunc(capturedElement);
            }
        }
    }
}
