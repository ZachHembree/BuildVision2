using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for the cursor rendered by the Rich HUD Framework
        /// </summary>
        public interface ICursor
        {
            /// <summary>
            /// Indicates whether the cursor is currently visible
            /// </summary>
            bool Visible { get; }

            /// <summary>
            /// Returns true if the cursor has been captured by a UI element
            /// </summary>
            bool IsCaptured { get; }

            /// <summary>
            /// The position of the cursor in pixels in screen space
            /// </summary>
            Vector2 ScreenPos { get; }

            /// <summary>
            /// Position of the cursor in world space.
            /// </summary>
            Vector3D WorldPos { get; }

            /// <summary>
            /// Returns true if the given HUD space is being captured by the cursor
            /// </summary>
            bool IsCapturingSpace(HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            bool TryCaptureHudSpace(float depth, HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor at the given depth with the given HUD space. If drawInHudSpace
            /// is true, then the cursor will be drawn in the given space.
            /// </summary>
            void CaptureHudSpace(float depth, HudSpaceDelegate GetHudSpaceFunc);

            /// <summary>
            /// Attempts to capture the cursor with the given object
            /// </summary>
            void Capture(object capturedElement);

            /// <summary>
            /// Indicates whether the cursor is being captured by the given element.
            /// </summary>
            bool IsCapturing(object capturedElement);

            /// <summary>
            /// Attempts to capture the cursor using the given object. Returns true on success.
            /// </summary>
            bool TryCapture(object capturedElement);

            /// <summary>
            /// Attempts to release the cursor from the given element. Returns false if
            /// not capture or if not captured by the object given.
            /// </summary>
            bool TryRelease(object capturedElement);
        }
    }
}
