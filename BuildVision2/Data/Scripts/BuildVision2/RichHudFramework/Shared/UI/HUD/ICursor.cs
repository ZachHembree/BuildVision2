using System.Collections.Generic;
using VRage;
using VRage.Utils;
using VRageMath;
using System;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;
using ApiMemberAccessor = System.Func<object, int, object>;

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
            Func<object, bool>, // TryRelease
            ApiMemberAccessor // GetOrSetMember
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
