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
        using Client;
        using Server;
        using Internal;
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        /// <summary>
        /// Base class for hud elements that can be parented to other elements.
        /// </summary>
        public abstract partial class HudNodeBase : HudParentBase, IReadOnlyHudNode
        {
            protected const HudElementStates nodeVisible = HudElementStates.IsVisible | HudElementStates.WasParentVisible | HudElementStates.IsRegistered;
            protected const int maxPreloadDepth = 5;

            /// <summary>
            /// Read-only parent object of the node.
            /// </summary>
            IReadOnlyHudParent IReadOnlyHudNode.Parent => _parent;

            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public virtual HudParentBase Parent { get { return _parent; } protected set { _parent = value; } }

            /// <summary>
            /// Determines whether or not an element will be drawn or process input. Visible by default.
            /// </summary>
            public override bool Visible => (State & nodeVisible) == nodeVisible;

            /// <summary>
            /// Indicates whether or not the element has been registered to a parent.
            /// </summary>
            public bool Registered => (State & HudElementStates.IsRegistered) > 0;

            protected bool ParentVisible
            {
                get { return (State & HudElementStates.WasParentVisible) > 0; }
                set 
                { 
                    if (value)
                        State |= HudElementStates.WasParentVisible; 
                    else
                        State &= ~HudElementStates.WasParentVisible;
                }
            }

            protected HudParentBase _parent, reregParent;

            public HudNodeBase(HudParentBase parent)
            {
                State &= ~HudElementStates.IsRegistered;
                ParentVisible = true;

                Register(parent);
            }

            /// <summary>
            /// Updates layout for the element and its children. Overriding this method is rarely necessary. 
            /// If you need to update layout, use Layout().
            /// </summary>
            public override void BeginLayout(bool refresh)
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData, _parent);

                        if (_parent == null)
                        {
                            ParentVisible = false;
                        }
                        else
                        {
                            ParentVisible = _parent.Visible;
                        }

                        if (Visible || refresh)
                            Layout();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            public override void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte preloadDepth)
            {
                bool wasSetVisible = (State & HudElementStates.IsVisible) > 0;
                State |= HudElementStates.WasParentVisible;

                if (!wasSetVisible && (State & HudElementStates.CanPreload) > 0)
                    preloadDepth++;

                if (preloadDepth < maxPreloadDepth && (State & HudElementStates.CanPreload) > 0)
                    State |= HudElementStates.IsVisible;

                if (Visible)
                {
                    HudSpace = _parent?.HudSpace ?? reregParent?.HudSpace;
                    layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData, _parent);

                    UpdateActions.EnsureCapacity(UpdateActions.Count + children.Count + 1);
                    accessorDelegates.Item2.Item2 = HudSpace.GetNodeOriginFunc;

                    UpdateActions.Add(accessorDelegates); ;

                    for (int n = 0; n < children.Count; n++)
                        children[n].GetUpdateAccessors(UpdateActions, preloadDepth);
                }

                if (!wasSetVisible)
                    State &= ~HudElementStates.IsVisible;
            }

            /// <summary>
            /// Registers the element to the given parent object.
            /// </summary>
            /// <param name="preregister">Adds the element to the update tree without registering.</param>
            /// <param name="canPreload">Indicates whether or not the element's accessors can be loaded into the update tree
            /// before the element is visible. Useful for preventing flicker in scrolling lists.</param>
            public virtual bool Register(HudParentBase newParent, bool preregister = false, bool canPreload = false)
            {
                if (newParent == this)
                    throw new Exception("Types of HudNodeBase cannot be parented to themselves!");

                // Complete unregistration from previous parent if being registered to a different node
                if ((State & HudElementStates.WasFastUnregistered) > 0 && newParent != reregParent)
                {
                    reregParent.RemoveChild(this);
                    State &= ~HudElementStates.WasFastUnregistered;
                    reregParent = null;
                }

                if (newParent != null && (reregParent == null || (State & HudElementStates.WasFastUnregistered) > 0))
                {
                    reregParent = null;

                    if ((State & HudElementStates.WasFastUnregistered) > 0)
                    {
                        Parent = newParent;
                        State |= HudElementStates.IsRegistered;
                    }
                    else
                    {
                        Parent = newParent;

                        if (_parent.RegisterChild(this))
                            State |= HudElementStates.IsRegistered;
                        else
                            State &= ~HudElementStates.IsRegistered;
                    }

                    if ((State & HudElementStates.IsRegistered) > 0)
                    {
                        if (preregister)
                        {
                            reregParent = newParent;
                            Parent = null;
                            State &= ~HudElementStates.IsRegistered;
                            State |= HudElementStates.WasFastUnregistered;
                        }
                        else
                        {
                            ParentVisible = _parent.Visible;
                            State &= ~HudElementStates.WasFastUnregistered;
                        }

                        if (canPreload)
                            State |= HudElementStates.CanPreload;
                        else
                            State &= ~HudElementStates.CanPreload;

                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public virtual bool Unregister(bool fast = false)
            {
                if (Parent != null || ((State & HudElementStates.WasFastUnregistered) > 0 && !fast))
                {
                    reregParent = _parent;
                    Parent = null;

                    if (!fast)
                    {
                        if (!reregParent.RemoveChild(this, false))
                            State |= HudElementStates.IsRegistered;
                        else
                            State &= ~HudElementStates.IsRegistered;

                        if ((State & HudElementStates.IsRegistered) > 0)
                            Parent = reregParent;

                        reregParent = null;
                    }
                    else
                    {
                        State &= ~HudElementStates.IsRegistered;
                        State |= HudElementStates.WasFastUnregistered;
                    }

                    State &= ~HudElementStates.WasParentVisible;
                }

                return !((State & HudElementStates.IsRegistered) > 0);
            }
        }
    }
}