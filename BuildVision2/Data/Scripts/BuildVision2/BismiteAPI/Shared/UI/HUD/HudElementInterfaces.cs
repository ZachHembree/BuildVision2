using System;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace DarkHelmet
{
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

    namespace UI
    {
        using HudNodeMembers = MyTuple<
            HudParentMembers, // Base members
            Func<object>, // GetParentID
            object, // GetParentData (Func<HudParentMembers?>)
            Action, // GetFocus
            Action<object>, // Register
            Action // Unregister
        >;

        public enum OriginAlignment : int
        {
            Center = 0,
            UpperLeft = 1,
            UpperRight = 2,
            LowerRight = 3,
            LowerLeft = 4,
            Auto = 5
        }

        public enum ParentAlignment : int
        {
            Center = 0,
            Left = 1,
            Top = 2,
            Right = 3,
            Bottom = 4
        }

        public enum TextAlignment : int
        {
            Left = 0,
            Center = 1,
            Right = 2,
        }

        public interface IHudParent
        {
            bool Visible { get; }
            object ID { get; }

            void Add(IHudNode child);
            void RegisterChildren(IEnumerable<IHudNode> newChildren);
            void RemoveChild(IHudNode child);
            void SetFocus(IHudNode child);
            void BeforeInput();
            void BeforeDraw();
            HudParentMembers GetApiData();
        }

        public interface IHudNode : IHudParent
        {
            IHudParent Parent { get; }

            void Register(IHudParent parent);
            void Unregister();
            void GetFocus();
            HudNodeMembers GetApiData();
        }

        public interface IReadonlyHudElement : IHudNode
        {
            float Scale { get; }
            Vector2 Size { get; }

            Vector2 Origin { get; }
            Vector2 NativeOrigin { get; }
            Vector2 Offset { get; }

            OriginAlignment OriginAlignment { get; }
            ParentAlignment ParentAlignment { get; }

            bool CaptureCursor { get; }
            bool ShareCursor { get; }
            bool IsMousedOver { get; }
        }

        public interface IReadonlyResizableElement : IReadonlyHudElement
        {
            float Height { get; }
            float Width { get; }
            Vector2 MinimumSize { get; }
        }

        public interface IReadonlyTextBox : IReadonlyResizableElement
        {
            Vector2 Padding { get; }
            float TextScale { get; }
            Vector2 TextSize { get; }
            Color BgColor { get; }
        }

        public interface IReadonlyClickableElement : IReadonlyResizableElement
        {
            event Action OnCursorEnter;
            event Action OnCursorExit;
            event Action OnLeftClick;
            event Action OnLeftRelease;
            event Action OnRightClick;
            event Action OnRightRelease;
        }
    }
}