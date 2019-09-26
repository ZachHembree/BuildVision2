using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI
{
    public enum OriginAlignment
    {
        Center,
        UpperLeft,
        UpperRight,
        LowerRight,
        LowerLeft,
        Auto
    }

    public enum ParentAlignment
    {
        Center,
        Left,
        Top,
        Right,
        Bottom
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right,
    }

    public interface IReadonlyHudNode
    {
        bool Visible { get; }
        HudNodeBase Parent { get; }
        ReadOnlyCollection<HudNodeBase> Children { get; }
    }

    public interface IReadonlyHudElement : IReadonlyHudNode
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