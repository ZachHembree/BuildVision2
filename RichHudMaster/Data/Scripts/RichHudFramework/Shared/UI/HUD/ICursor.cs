using System;
using VRage;
using VRageMath;

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

    namespace UI
    {
        public interface ICursor
        {
            bool Visible { get; }
            bool IsCaptured { get; }
            Vector2 Origin { get; }

            void Capture(object capturedElement);
            bool IsCapturing(object capturedElement);
            bool TryCapture(object capturedElement);
            bool TryRelease(object capturedElement);
            CursorMembers GetApiData();
        }
    }
}
