using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using DarkHelmet.UI.Rendering;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>;
using RichCharMembers = VRage.MyTuple<char, VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;

namespace DarkHelmet
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
    using HudParentMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        object, // Add (Action<HudNodeMembers>)
        Action, // BeforeDraw
        Action, // BeforeInput
        MyTuple<
            Action<object>, // RemoveChild
            Action<object> // SetFocus
        >
    >;
    using TextBoardMembers = MyTuple<
        MyTuple<Func<int, MyTuple<Func<int, RichCharMembers>, Func<int>>>, Func<int>>, // GetLine, GetCount
        FloatProp, // Scale
        Func<Vector2>, // TextSize
        Vec2Prop, // MaxSize
        FloatProp, // MaxLineWidth
        MyTuple<
            Func<bool>, // WordWrapping
            Action<Vector2>, // Draw
            Func<Vector2I, Vector2I, List<RichStringMembers>>, // GetRange
            Action<Vector2I, Vector2I, GlyphFormatMembers>, // SetFormatting
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            MyTuple<
                Action<RichStringMembers, Vector2I>, // Insert
                Action<Vector2I, Vector2I>, // RemoveRange
                Action // Clear
            >
        >
    >;

    namespace UI
    {
        using BismiteClient;
        using HudMainMembers = MyTuple<
            HudParentMembers,
            CursorMembers,
            Func<float>, // ScreenWidth
            Func<float>, // ScreenHeight
            Func<float>, // AspectRatio
            MyTuple<
                Func<float>, // ResScale
                Func<float>, // Fov
                Func<float>,
                Func<bool, TextBoardMembers> // GetNewTextBoard
            >
        >;

        public sealed class HudMain : BismiteClient.ApiComponentBase
        {
            public static IHudParent Root => Instance.root;
            public static ICursor Cursor => Instance.cursor;
            public static float ResScale => Instance.ResScaleFunc();
            public static float ScreenWidth => Instance.ScreenWidthFunc();
            public static float ScreenHeight => Instance.ScreenHeightFunc();
            public static float AspectRatio => Instance.AspectRatioFunc();
            public static float Fov => Instance.FovFunc();
            public static float FovScale => Instance.FovScaleFunc();

            private static HudMain Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static HudMain instance;

            private readonly HudParentData root;
            private readonly ICursor cursor;           
            private readonly Func<float> ScreenWidthFunc;
            private readonly Func<float> ScreenHeightFunc;
            private readonly Func<float> AspectRatioFunc;
            private readonly Func<float> ResScaleFunc;
            private readonly Func<float> FovFunc;
            private readonly Func<float> FovScaleFunc;
            private readonly Func<bool, TextBoardMembers> GetTextBoardDataFunc;

            private HudMain() : base(ApiComponentTypes.HudMain, false, true)
            {
                var members = (HudMainMembers)GetApiData();

                root = new HudParentData(members.Item1);
                cursor = new CursorData(members.Item2);

                ScreenWidthFunc = members.Item3;
                ScreenHeightFunc = members.Item4;
                AspectRatioFunc = members.Item5;

                var data2 = members.Item6;
                ResScaleFunc = data2.Item1;
                FovFunc = data2.Item2;
                FovScaleFunc = data2.Item3;
                GetTextBoardDataFunc = data2.Item4;
            }

            private static void Init()
            {
                if (instance == null)
                {
                    instance = new HudMain();
                }
            }

            public override void Close()
            {
                root.ClearLocalChildren();
                Instance = null;
            }

            public static TextBoardMembers GetTextBoardData(bool wordWrapping) =>
                Instance.GetTextBoardDataFunc(wordWrapping);

            /// <summary>
            /// Converts from a coordinate in the scaled coordinate system to a concrete coordinate in pixels.
            /// </summary>
            public static Vector2 GetPixelVector(Vector2 scaledVec)
            {
                scaledVec /= 2f;

                return new Vector2
                (
                    (int)(scaledVec.X * ScreenWidth),
                    (int)(scaledVec.Y * ScreenHeight)
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
                    pixelVec.X / ScreenWidth,
                    pixelVec.Y / ScreenHeight
                );
            }

            private class CursorData : ICursor
            {
                public bool Visible => IsVisibleFunc();
                public bool IsCaptured => IsCapturedFunc();
                public Vector2 Origin => GetOriginFunc();

                private readonly Func<bool> IsVisibleFunc;
                private readonly Func<bool> IsCapturedFunc;
                private readonly Func<Vector2> GetOriginFunc;
                private readonly Action<object> CaptureFunc;
                private readonly Func<object, bool> IsCapturingFunc;
                private readonly Func<object, bool> TryCaptureFunc;
                private readonly Func<object, bool> TryReleaseFunc;

                public CursorData(CursorMembers members)
                {
                    IsVisibleFunc = members.Item1;
                    IsCapturedFunc = members.Item2;
                    GetOriginFunc = members.Item3;
                    CaptureFunc = members.Item4;
                    IsCapturingFunc = members.Item5;

                    var data2 = members.Item6;
                    TryCaptureFunc = data2.Item1;
                    TryReleaseFunc = data2.Item2;
                }

                public void Capture(object capturedElement) =>
                    CaptureFunc(capturedElement);

                public bool IsCapturing(object capturedElement) =>
                    IsCapturingFunc(capturedElement);

                public bool TryCapture(object capturedElement) =>
                    TryCaptureFunc(capturedElement);

                public bool TryRelease(object capturedElement) =>
                    TryReleaseFunc(capturedElement);

                public CursorMembers GetApiData()
                {
                    return new CursorMembers()
                    {
                        Item1 = IsVisibleFunc,
                        Item2 = IsCapturedFunc,
                        Item3 = GetOriginFunc,
                        Item4 = CaptureFunc,
                        Item5 = IsCapturingFunc,
                        Item6 = new MyTuple<Func<object, bool>, Func<object, bool>>()
                        {
                            Item1 = TryCaptureFunc,
                            Item2 = TryReleaseFunc
                        }
                    };
                }
            }
        }
    }
}