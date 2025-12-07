using System;

namespace RichHudFramework
{
    namespace UI
    {
        using static RichHudFramework.UI.NodeConfigIndices;

        /// <summary>
        /// Abstract base for hud elements that can be parented to other elements.
        /// </summary>
        public abstract partial class HudNodeBase : HudParentBase, IReadOnlyHudNode
        {
            /// <summary>
            /// Default node visibility mask
            /// </summary>
            /// <exclude/>
            protected const uint nodeVisible = (uint)(HudElementStates.IsVisible | HudElementStates.WasParentVisible | HudElementStates.IsRegistered);

            /// <summary>
            /// Default node input enabled mask
            /// </summary>
            /// <exclude/>
            protected const uint nodeInputEnabled = (uint)(HudElementStates.IsInputEnabled | HudElementStates.WasParentInputEnabled);

            /// <summary>
            /// Read-only reference to the node's parent
            /// </summary>
            IReadOnlyHudParent IReadOnlyHudNode.Parent => Parent;

            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public HudParentBase Parent { get; private set; }

            /// <summary>
            /// Returns true if the node has been registered to a parent. Does not necessarilly indicate that 
            /// the parent is registered or that the node is active.
            /// </summary>
            public bool Registered => (Config[StateID] & (uint)HudElementStates.IsRegistered) > 0;

            /// <summary>
            /// Specialized ZOffset range used for creating windows.
            /// </summary>
            protected byte OverlayOffset
            {
                get { return (byte)Config[ZOffsetInnerID]; }
                set
                {
                    _config[ZOffsetInnerID] = value;

                    // Update combined ZOffset for layer sorting
                    {
                        byte outerOffset = (byte)((sbyte)Config[ZOffsetID] - sbyte.MinValue);
                        ushort innerOffset = (ushort)(Config[ZOffsetInnerID] << 8);

                        // Combine local node inner and outer offsets with parent and pack into
                        // full ZOffset
                        if (Parent != null)
                        {
                            ushort parentFull = (ushort)Parent.Config[FullZOffsetID];
                            byte parentOuter = (byte)((parentFull & 0x00FF) + sbyte.MinValue);
                            ushort parentInner = (ushort)(parentFull & 0xFF00);

                            outerOffset = (byte)Math.Min((outerOffset + parentOuter), byte.MaxValue);
                            innerOffset = (ushort)Math.Min(innerOffset + parentInner, 0xFF00);
                        }

                        _config[FullZOffsetID] = (ushort)(innerOffset | outerOffset);
                    }
                }
            }

            public HudNodeBase(HudParentBase parent)
            {
                _config[VisMaskID] = nodeVisible;
                _config[InputMaskID] = nodeInputEnabled;
                _config[StateID] &= ~(uint)(HudElementStates.IsRegistered);

                Register(parent);
            }

            /// <summary>
            /// Updates internal state. Override Layout() for customization.
            /// </summary>
            /// <exclude/>
            protected override void BeginLayout(bool _)
            {
                if ((Config[StateID] & (uint)HudElementStates.IsSpaceNode) == 0)
                    HudSpace = Parent?.HudSpace;

                if (HudSpace != null)
                    _config[StateID] |= (uint)HudElementStates.IsSpaceNodeReady;
                else
                    _config[StateID] &= ~(uint)HudElementStates.IsSpaceNodeReady;

                if ((Config[StateID] & (uint)HudElementStates.IsLayoutCustom) > 0)
                    Layout();
            }

            /// <summary>
            /// Registers the element to the given parent object.
            /// </summary>
            public virtual bool Register(HudParentBase newParent)
            {
                if (newParent == this)
                    throw new Exception("Types of HudNodeBase cannot be parented to themselves!");

                if (newParent != null)
                {
                    Parent = newParent;

                    if (Parent.RegisterChild(this))
                        _config[StateID] |= (uint)HudElementStates.IsRegistered;
                    else
                        _config[StateID] &= ~(uint)HudElementStates.IsRegistered;
                }

                if ((Config[StateID] & (uint)HudElementStates.IsRegistered) > 0)
                {
                    _config[StateID] &= ~(uint)HudElementStates.WasParentVisible;
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            public virtual bool Unregister()
            {
                if (Parent != null)
                {
                    HudParentBase lastParent = Parent;
                    Parent = null;

                    lastParent.RemoveChild(this);
                    _config[StateID] &= (uint)~(HudElementStates.IsRegistered | HudElementStates.WasParentVisible);
                }

                return !((Config[StateID] & (uint)HudElementStates.IsRegistered) > 0);
            }
        }
    }
}