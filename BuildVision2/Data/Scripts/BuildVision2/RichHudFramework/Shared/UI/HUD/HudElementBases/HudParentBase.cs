using RichHudFramework.Internal;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        /// <summary>
        /// Base class for HUD elements to which other elements are parented. Types deriving from this class cannot be
        /// parented to other elements; only types of <see cref="HudNodeBase"/> can be parented.
        /// </summary>
        public abstract class HudParentBase : IReadOnlyHudParent
        {
            /// <summary>
            /// Node defining the coordinate space used to render the UI element
            /// </summary>
            public virtual IReadOnlyHudSpaceNode HudSpace { get; protected set; }

            /// <summary>
            /// Determines whether or not an element will be drawn or process input. Visible by default.
            /// </summary>
            public virtual bool Visible { get { return _visible; } set { _visible = value; } }

            /// <summary>
            /// Scales the size and offset of an element. Any offset or size set at a given
            /// be increased or decreased with scale. Defaults to 1f.
            /// </summary>
            public virtual float Scale { get; }

            /// <summary>
            /// Determines whether the UI element will be drawn in the Back, Mid or Foreground
            /// </summary>
            public virtual sbyte ZOffset
            {
                get { return _zOffset; }
                set { _zOffset = value; }
            }

            protected readonly List<HudNodeBase> children;

            protected readonly ApiMemberAccessor GetOrSetMemberFunc;
            protected readonly Func<ushort> GetZOffsetFunc;
            protected readonly Action DepthTestAction;
            protected readonly Action InputAction;
            protected readonly Action<bool> LayoutAction;
            protected readonly Action DrawAction;

            protected sbyte _zOffset;
            protected bool _registered;

            /// <summary>
            /// Additional zOffset range used internally; primarily for determining window draw order.
            /// Don't use this unless you have a good reason for it.
            /// </summary>
            protected byte zOffsetInner;
            protected ushort fullZOffset;
            protected bool _visible;

            public HudParentBase()
            {
                Visible = true;
                Scale = 1f;
                _registered = true;
                children = new List<HudNodeBase>();

                GetOrSetMemberFunc = GetOrSetApiMember;
                GetZOffsetFunc = () => fullZOffset;
                DepthTestAction = SafeInputDepth;
                LayoutAction = SafeBeginLayout;
                DrawAction = SafeBeginDraw;
                InputAction = SafeBeginInput;
            }

            /// <summary>
            /// Starts cursor depth check in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side.
            /// </summary>
            public void SafeInputDepth()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        InputDepth();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts layout update in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side.
            /// </summary>
            public void SafeBeginLayout(bool refresh)
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        BeginLayout(refresh);
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts UI draw in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side.
            /// </summary>
            public void SafeBeginDraw()
            {
                if (!ExceptionHandler.ClientsPaused && _registered)
                {
                    try
                    {
                        BeginDraw();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts input update in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side.
            /// </summary>
            public void SafeBeginInput()
            {
                if (!ExceptionHandler.ClientsPaused && _registered)
                {
                    try
                    {
                        BeginInput();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Used to check whether the cursor is moused over the element and whether its being
            /// obstructed by another element.
            /// </summary>
            protected virtual void InputDepth() { }

            /// <summary>
            /// Updates input for the element and its children. Overriding this method is rarely necessary.
            /// If you need to update input, use HandleInput().
            /// </summary>
            protected virtual void BeginInput()
            {
                if (Visible)
                {
                    Vector3 cursorPos = HudSpace.CursorPos;
                    HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
                }
            }

            /// <summary>
            /// Updates the input of this UI element. Invocation order affected by z-Offset and depth sorting.
            /// </summary>
            protected virtual void HandleInput(Vector2 cursorPos) { }

            /// <summary>
            /// Updates layout for the element and its children. Overriding this method is rarely necessary. 
            /// If you need to update layout, use Layout().
            /// </summary>
            protected virtual void BeginLayout(bool refresh)
            {
                fullZOffset = GetFullZOffset(this);

                if (Visible || refresh)
                    Layout();
            }

            /// <summary>
            /// Updates the layout of this UI element. Not affected by depth or z-Offset sorting.
            /// Executes before input and draw.
            /// </summary>
            protected virtual void Layout() { }

            /// <summary>
            /// Used to immediately draw billboards. Overriding this method is rarely necessary. 
            /// If you need to draw something, use Draw().
            /// </summary>
            protected virtual void BeginDraw()
            {
                if (Visible)
                    Draw();
            }

            /// <summary>
            /// Used to immediately draw billboards. Invocation order affected by z-Offset and depth sorting.
            /// </summary>
            protected virtual void Draw() { }

            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            public virtual void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth)
            {
                fullZOffset = GetFullZOffset(this);

                UpdateActions.EnsureCapacity(UpdateActions.Count + children.Count + 1);
                var accessors = new HudUpdateAccessors()
                {
                    Item1 = GetOrSetMemberFunc,
                    Item2 = new MyTuple<Func<ushort>, Func<Vector3D>>(GetZOffsetFunc, HudSpace.GetNodeOriginFunc),
                    Item3 = DepthTestAction,
                    Item4 = InputAction,
                    Item5 = LayoutAction,
                    Item6 = DrawAction
                };

                UpdateActions.Add(accessors);
                treeDepth++;

                for (int n = 0; n < children.Count; n++)
                    children[n].GetUpdateAccessors(UpdateActions, treeDepth);
            }

            /// <summary>
            /// Registers a child node to the object.
            /// </summary>
            /// <param name="preregister">Adds the element to the update tree without registering.</param>
            public virtual bool RegisterChild(HudNodeBase child, bool preregister = false)
            {
                if (child.Parent == this && !child.Registered)
                {
                    children.Add(child);
                    return true;
                }
                else if (child.Parent == null)
                    return child.Register(this, preregister);
                else
                    return false;
            }

            /// <summary>
            /// Unregisters the specified node from the parent.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public virtual bool RemoveChild(HudNodeBase child, bool fast = false)
            {
                if (child.Parent == this)
                    return child.Unregister(fast);
                else if (child.Parent == null)
                    return children.Remove(child);
                else
                    return false;
            }

            /// <summary>
            /// Calculates the full z-offset using the public offset and inner offset.
            /// </summary>
            public static ushort GetFullZOffset(HudParentBase element, HudParentBase parent = null)
            {
                byte outerOffset = (byte)(element._zOffset - sbyte.MinValue);
                ushort innerOffset = (ushort)(element.zOffsetInner << 8);

                if (parent != null)
                {
                    outerOffset += (byte)((parent.fullZOffset & 0x00FF) + sbyte.MinValue);
                    innerOffset += (ushort)(parent.fullZOffset & 0xFF00);
                }

                return (ushort)(innerOffset | outerOffset);
            }

            /// <summary>
            /// Returns the visibility set for the given <see cref="HudParentBase"/> without including
            /// parent visibility.
            /// </summary>
            protected static bool IsSetVisible(HudParentBase node)
            {
                return node._visible && node._registered;
            }

            protected virtual object GetOrSetApiMember(object data, int memberEnum)
            {
                switch ((HudElementAccessors)memberEnum)
                {
                    case HudElementAccessors.GetType:
                        return GetType();
                    case HudElementAccessors.ZOffset:
                        return ZOffset;
                    case HudElementAccessors.FullZOffset:
                        return fullZOffset;
                    case HudElementAccessors.Position:
                        return Vector2.Zero;
                    case HudElementAccessors.Size:
                        return Vector2.Zero;
                    case HudElementAccessors.GetHudSpaceFunc:
                        return HudSpace?.GetHudSpaceFunc;
                    case HudElementAccessors.ModName:
                        return ExceptionHandler.ModName;
                    case HudElementAccessors.LocalCursorPos:
                        return HudSpace?.CursorPos ?? Vector3.Zero;
                    case HudElementAccessors.DrawCursorInHudSpace:
                        return HudSpace?.DrawCursorInHudSpace ?? false;
                    case HudElementAccessors.PlaneToWorld:
                        return HudSpace?.PlaneToWorld ?? default(MatrixD);
                    case HudElementAccessors.IsInFront:
                        return HudSpace?.IsInFront ?? false;
                    case HudElementAccessors.IsFacingCamera:
                        return HudSpace?.IsFacingCamera ?? false;
                    case HudElementAccessors.NodeOrigin:
                        return HudSpace?.PlaneToWorld.Translation ?? Vector3D.Zero;
                }

                return null;
            }

        }
    }
}