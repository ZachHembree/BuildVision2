using RichHudFramework.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.ModAPI;
using VRageMath;
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
            ApiMemberAccessor
        >
    >;
    using HudElementMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        Action, // Draw
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMembers
    >;
    using TextBoardMembers = MyTuple<
        // TextBuilderMembers
        MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            ApiMemberAccessor, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<RichStringMembers, Vector2I>, // Insert
            Action // Clear
        >,
        FloatProp, // Scale
        Func<Vector2>, // Size
        Func<Vector2>, // TextSize
        Vec2Prop, // FixedSize
        Action<Vector2> // Draw 
    >;

    namespace UI.Client
    {
        using RichHudFramework.Client;
        using HudMainMembers = MyTuple<
            HudElementMembers,
            CursorMembers,
            Func<float>, // ScreenWidth
            Func<float>, // ScreenHeight
            Func<float>, // AspectRatio
            MyTuple<
                Func<float>, // ResScale
                Func<float>, // Fov
                Func<float>, // FovScale
                MyTuple<Func<IList<RichStringMembers>>, Action<IList<RichStringMembers>>>,
                Func<TextBoardMembers>, // GetNewTextBoard
                ApiMemberAccessor
            >
        >;

        public sealed class HudMain : RichHudClient.ApiModule<HudMainMembers>
        {
            public static HudParentData Root => Instance.root;
            public static ICursor Cursor => Instance.cursor;
            public static RichText ClipBoard
            {
                get { return new RichText(Instance.ClipboardPropWrapper.Getter()); }
                set { Instance.ClipboardPropWrapper.Setter(value.GetApiData()); }
            }
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
            private readonly PropWrapper<IList<RichStringMembers>> ClipboardPropWrapper;
            private readonly Func<TextBoardMembers> GetTextBoardDataFunc;

            private HudMain() : base(ApiModuleTypes.HudMain, false, true)
            {
                var members = GetApiData();

                root = new HudParentData(members.Item1);
                cursor = new CursorData(members.Item2);

                ScreenWidthFunc = members.Item3;
                ScreenHeightFunc = members.Item4;
                AspectRatioFunc = members.Item5;

                var data2 = members.Item6;
                ResScaleFunc = data2.Item1;
                FovFunc = data2.Item2;
                FovScaleFunc = data2.Item3;
                ClipboardPropWrapper = new PropWrapper<IList<RichStringMembers>>(data2.Item4.Item1, data2.Item4.Item2);
                GetTextBoardDataFunc = data2.Item5;
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

            public static TextBoardMembers GetTextBoardData() =>
                Instance.GetTextBoardDataFunc();

            /// <summary>
            /// Converts from a value in the relative coordinate system to a concrete value in pixels.
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
            /// Converts from a coordinate given in pixels to a scaled system independent of screen resolution.
            /// </summary>
            public static Vector2 GetRelativeVector(Vector2 pixelVec)
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
                        Item6 = new MyTuple<Func<object, bool>, Func<object, bool>, ApiMemberAccessor>()
                        {
                            Item1 = TryCaptureFunc,
                            Item2 = TryReleaseFunc
                        }
                    };
                }
            }
        }
    }

    namespace UI.Server
    { }
}