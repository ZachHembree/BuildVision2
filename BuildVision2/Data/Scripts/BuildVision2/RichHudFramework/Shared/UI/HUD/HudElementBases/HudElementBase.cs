using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;
        using Internal;

        /// <summary>
        /// Base type for all hud elements with definite size and position. Inherits from HudParentBase and HudNodeBase.
        /// </summary>
        public abstract class HudElementBase : HudNodeBase, IReadOnlyHudElement
        {
            protected const float minMouseBounds = 8f;

            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public sealed override HudParentBase Parent
            {
                protected set
                {
                    _parent = value;
                    _parentFull = value as HudElementBase;
                }
            }

            /// <summary>
            /// Size of the element. Units in pixels by default.
            /// </summary>
            public Vector2 Size
            {
                get { return UnpaddedSize + Padding; }
                set 
                {
                    if (value.X > Padding.X)
                        value.X -= Padding.X;

                    if (value.Y > Padding.Y)
                        value.Y -= Padding.Y;

                    UnpaddedSize = value;
                }
            }

            /// <summary>
            /// Width of the hud element. Units in pixels by default.
            /// </summary>
            public float Width
            {
                get { return UnpaddedSize.X + Padding.X; }
                set
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    UnpaddedSize = new Vector2(value, UnpaddedSize.Y);
                }
            }

            /// <summary>
            /// Height of the hud element. Units in pixels by default.
            /// </summary>
            public float Height
            {
                get { return UnpaddedSize.Y + Padding.Y; }
                set
                {
                    if (value > Padding.Y)
                        value -= Padding.Y;

                    UnpaddedSize = new Vector2(UnpaddedSize.X, value);
                }
            }

            /// <summary>
            /// Border size. Included in total element size.
            /// </summary>
            public Vector2 Padding { get; set; }

            /// <summary>
            /// Element size without padding
            /// </summary>
            public Vector2 UnpaddedSize { get; set; }

            /// <summary>
            /// Starting position of the hud element.
            /// </summary>
            public Vector2 Origin { get; private set; }

            /// <summary>
            /// Position of the element relative to its origin.
            /// </summary>
            public Vector2 Offset { get; set; }

            /// <summary>
            /// Current position of the hud element. Origin + Offset.
            /// </summary>
            public Vector2 Position { get; private set; }

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            public ParentAlignments ParentAlignment { get; set; }

            /// <summary>
            /// Determines how/if an element will copy its parent's dimensions. 
            /// </summary>
            public DimAlignments DimAlignment { get; set; }

            /// <summary>
            /// If set to true the hud element will be allowed to capture the cursor.
            /// </summary>
            public bool UseCursor
            {
                get { return (State & HudElementStates.CanUseCursor) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanUseCursor;
                    else
                        State &= ~HudElementStates.CanUseCursor;
                }
            }

            /// <summary>
            /// If set to true the hud element will share the cursor with other elements.
            /// </summary>
            public bool ShareCursor
            {
                get { return (State & HudElementStates.CanShareCursor) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanShareCursor;
                    else
                        State &= ~HudElementStates.CanShareCursor;
                }
            }

            /// <summary>
            /// If set to true, the hud element will act as a clipping mask for child elements.
            /// False by default. Masking parent elements can still affect non-masking children.
            /// </summary>
            public bool IsMasking
            {
                get { return (State & HudElementStates.IsMasking) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsMasking;
                    else
                        State &= ~HudElementStates.IsMasking;
                }
            }

            /// <summary>
            /// If set to true, the hud element will treat its parent as a clipping mask, whether
            /// it's configured as a mask or not.
            /// </summary>
            public bool IsSelectivelyMasked
            {
                get { return (State & HudElementStates.IsSelectivelyMasked) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsSelectivelyMasked;
                    else
                        State &= ~HudElementStates.IsSelectivelyMasked;
                }
            }

            /// <summary>
            /// If set to true, then the element can ignore any bounding masks imposed by its parents.
            /// Superceeds selective masking flag.
            /// </summary>
            public bool CanIgnoreMasking
            {
                get { return (State & HudElementStates.CanIgnoreMasking) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.CanIgnoreMasking;
                    else
                        State &= ~HudElementStates.CanIgnoreMasking;
                }
            }

            /// <summary>
            /// Indicates whether or not the element is capturing the cursor.
            /// </summary>
            public virtual bool IsMousedOver => (State & HudElementStates.IsMousedOver) > 0;

            /// <summary>
            /// Values used internally to minimize property calls.
            /// </summary>
            protected Vector2 CachedSize { get; private set; }

            /// <summary>
            /// Origin offset used internally for parent alignment
            /// </summary>
            protected Vector2 OriginAlignment { get; private set; }

            protected BoundingBox2? maskingBox;
            protected HudElementBase _parentFull;

            /// <summary>
            /// Initializes a new hud element with cursor sharing enabled and scaling set to 1f.
            /// </summary>
            public HudElementBase(HudParentBase parent) : base(parent)
            {
                DimAlignment = DimAlignments.None;
                ParentAlignment = ParentAlignments.Center;

                Origin = Vector2.Zero;
                Position = Vector2.Zero;
                OriginAlignment = Vector2.Zero;
            }

            /// <summary>
            /// Used to check whether the cursor is moused over the element and whether its being
            /// obstructed by another element.
            /// </summary>
            protected override void InputDepth()
            {
                State &= ~HudElementStates.IsMouseInBounds;

                if (HudMain.InputMode != HudInputMode.NoInput && (HudSpace?.IsFacingCamera ?? false))
                {
                    Vector3 cursorPos = HudSpace.CursorPos;
                    Vector2 halfSize = Vector2.Max(CachedSize, new Vector2(minMouseBounds)) * .5f;
                    BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);
                    bool mouseInBounds;

                    if (maskingBox == null)
                        mouseInBounds = box.Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;
                    else
                        mouseInBounds = box.Intersect(maskingBox.Value).Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains;

                    if (mouseInBounds)
                    {
                        State |= HudElementStates.IsMouseInBounds;
                        HudMain.Cursor.TryCaptureHudSpace(cursorPos.Z, HudSpace.GetHudSpaceFunc);
                    }
                }
            }

            /// <summary>
            /// Updates input for the element and its children. Overriding this method is rarely necessary.
            /// If you need to update input, use HandleInput().
            /// </summary>
            public sealed override void BeginInput()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if (_parent != null && (_parent.State & _parent.NodeVisibleMask) == _parent.NodeVisibleMask)
                            State |= HudElementStates.WasParentInputEnabled;
                        else
                            State &= ~HudElementStates.WasParentInputEnabled;

                        bool isVisible = (State & NodeVisibleMask) == NodeVisibleMask,
                             isInputEnabled = (State & NodeInputMask) == NodeInputMask,
                             canUseCursor = (State & HudElementStates.CanUseCursor) > 0,
                             canShareCursor = (State & HudElementStates.CanShareCursor) > 0;

                        State &= ~HudElementStates.IsMousedOver;

                        if (isVisible && isInputEnabled)
                        {
                            Vector3 cursorPos = HudSpace.CursorPos;
                            bool mouseInBounds = (State & HudElementStates.IsMouseInBounds) > 0;

                            if (canUseCursor && mouseInBounds && !HudMain.Cursor.IsCaptured && HudMain.Cursor.IsCapturingSpace(HudSpace.GetHudSpaceFunc))
                            {
                                bool isMousedOver = mouseInBounds;

                                if (isMousedOver)
                                    State |= HudElementStates.IsMousedOver;

                                HandleInput(new Vector2(cursorPos.X, cursorPos.Y));

                                if (!canShareCursor)
                                    HudMain.Cursor.Capture(accessorDelegates.Item1);
                            }
                            else
                            {
                                HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Updates layout for the element and its children. Overriding this method is rarely necessary. 
            /// If you need to update layout, use Layout().
            /// </summary>
            public sealed override void BeginLayout(bool refresh)
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if (_parent != null && (_parent.State & _parent.NodeVisibleMask) == _parent.NodeVisibleMask)
                            State |= HudElementStates.WasParentVisible;
                        else
                            State &= ~HudElementStates.WasParentVisible;

                        bool isVisible = (State & NodeVisibleMask) == NodeVisibleMask;

                        if (isVisible)
                        {
                            layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData, _parent);

                            if (_parentFull != null)
                            {
                                Origin = _parentFull.Position + OriginAlignment;
                            }
                            else
                            {
                                Origin = Vector2.Zero;
                                Position = Offset;
                                Padding = Padding;
                                CachedSize = UnpaddedSize + Padding;
                            }

                            Layout();

                            if (children.Count > 0)
                                UpdateChildAlignment();

                            if (_parentFull != null && (_parentFull.State & HudElementStates.IsMasked) > 0 &&
                                (State & HudElementStates.CanIgnoreMasking) == 0
                            )
                                State |= HudElementStates.IsMasked;
                            else
                                State &= ~HudElementStates.IsMasked;

                            if ((State & HudElementStates.IsMasking) > 0 || (_parentFull != null && (State & HudElementStates.IsSelectivelyMasked) > 0))
                                UpdateMasking();
                            else if ((State & HudElementStates.IsMasked) > 0)
                                maskingBox = _parentFull?.maskingBox;
                            else
                                maskingBox = null;
                        }

                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Used to immediately draw billboards. Overriding this method is rarely necessary. 
            /// If you need to draw something, use Draw().
            /// </summary>
            public sealed override void BeginDraw()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        bool isVisible = (State & NodeVisibleMask) == NodeVisibleMask;

                        if (isVisible)
                        {
                            Draw();
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Updates cached values as well as parent and dim alignment.
            /// </summary>
            protected void UpdateChildAlignment()
            {
                // Update size
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i] as HudElementBase;

                    if (child != null && (child.State & (nodeVisible)) == nodeVisible)
                    {
                        child.Padding = child.Padding;

                        Vector2 size = child.UnpaddedSize + child.Padding;
                        DimAlignments sizeFlags = child.DimAlignment;

                        if (sizeFlags != DimAlignments.None)
                        {
                            if ((sizeFlags & DimAlignments.IgnorePadding) == DimAlignments.IgnorePadding)
                            {
                                if ((sizeFlags & DimAlignments.Width) == DimAlignments.Width)
                                    size.X = CachedSize.X - Padding.X;

                                if ((sizeFlags & DimAlignments.Height) == DimAlignments.Height)
                                    size.Y = CachedSize.Y - Padding.Y;
                            }
                            else
                            {
                                if ((sizeFlags & DimAlignments.Width) == DimAlignments.Width)
                                    size.X = CachedSize.X;

                                if ((sizeFlags & DimAlignments.Height) == DimAlignments.Height)
                                    size.Y = CachedSize.Y;
                            }

                            child.UnpaddedSize = size - child.Padding;
                        }

                        child.CachedSize = size;
                    }
                }

                // Update position
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i] as HudElementBase;

                    if (child != null && (child.State & (nodeVisible)) == nodeVisible)
                    {
                        ParentAlignments originFlags = child.ParentAlignment;
                        Vector2 delta = Vector2.Zero,
                            max = (CachedSize + child.CachedSize) * .5f,
                            min = -max;

                        if ((originFlags & ParentAlignments.UsePadding) == ParentAlignments.UsePadding)
                        {
                            min += Padding * .5f;
                            max -= Padding * .5f;
                        }

                        if ((originFlags & ParentAlignments.InnerV) == ParentAlignments.InnerV)
                        {
                            min.Y += child.CachedSize.Y;
                            max.Y -= child.CachedSize.Y;
                        }

                        if ((originFlags & ParentAlignments.InnerH) == ParentAlignments.InnerH)
                        {
                            min.X += child.CachedSize.X;
                            max.X -= child.CachedSize.X;
                        }

                        if ((originFlags & ParentAlignments.Bottom) == ParentAlignments.Bottom)
                            delta.Y = min.Y;
                        else if ((originFlags & ParentAlignments.Top) == ParentAlignments.Top)
                            delta.Y = max.Y;

                        if ((originFlags & ParentAlignments.Left) == ParentAlignments.Left)
                            delta.X = min.X;
                        else if ((originFlags & ParentAlignments.Right) == ParentAlignments.Right)
                            delta.X = max.X;

                        child.OriginAlignment = delta;
                        child.Origin = Position + delta;
                        child.Position = child.Origin + child.Offset;
                    }
                }
            }

            /// <summary>
            /// Updates masking state and bounding boxes used to mask billboards
            /// </summary>
            protected void UpdateMasking()
            {
                State |= HudElementStates.IsMasked;
                BoundingBox2? parentBox, box = null;

                if ((State & HudElementStates.CanIgnoreMasking) > 0)
                {
                    parentBox = null;
                }
                else if (_parentFull != null && (State & HudElementStates.IsSelectivelyMasked) > 0)
                {
                    Vector2 halfParent = .5f * _parentFull.CachedSize;
                    parentBox = new BoundingBox2(
                        -halfParent + _parentFull.Position,
                        halfParent + _parentFull.Position
                    );

                    if (_parentFull.maskingBox != null)
                        parentBox = parentBox.Value.Intersect(_parentFull.maskingBox.Value);
                }
                else
                    parentBox = _parentFull?.maskingBox;

                if ((State & HudElementStates.IsMasking) > 0)
                {
                    Vector2 halfSize = .5f * CachedSize;
                    box = new BoundingBox2(
                        -halfSize + Position,
                        halfSize + Position
                    );
                }

                if (parentBox != null && box != null)
                    box = box.Value.Intersect(parentBox.Value);
                else if (box == null)
                    box = parentBox;

                maskingBox = box;
            }

            protected override object GetOrSetApiMember(object data, int memberEnum)
            {
                switch ((HudElementAccessors)memberEnum)
                {
                    case HudElementAccessors.Position:
                        return Position;
                    case HudElementAccessors.Size:
                        return Size;
                }

                return base.GetOrSetApiMember(data, memberEnum);
            }
        }
    }
}