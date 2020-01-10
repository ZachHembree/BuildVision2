using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using HudElementMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        Action, // BeforeDrawStart
        Action, // DrawStart
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMembers
    >;

    namespace UI
    {
        internal enum HudNodeAccessors : int
        {
            GetParentID = 10,
            GetParentData = 11,
            GetFocus = 12,
            Register = 13,
            Unregister = 14
        }

        /// <summary>
        /// Base class for hud elements that can serve as child and/or parent nodes. Derives from <see cref="HudParentBase"/>
        /// and implements <see cref="IHudNode"/>.
        /// </summary>
        public abstract class HudNodeBase : HudParentBase, IHudNode
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public virtual IHudParent Parent { get; protected set; }

            /// <summary>
            /// Initializes the node without any child elements and with/without a parent node.
            /// </summary>
            public HudNodeBase(IHudParent parent)
            {
                if (parent != null)
                {
                    if (parent.ID == ID)
                        throw new Exception("Types of HudNodeBase cannot be parented to themselves!");
                    else
                        Register(parent);
                }
            }

            /// <summary>
            /// Moves the element to the end of its parent's update list in order to ensure
            /// that it's drawn/updated last.
            /// </summary>
            public void GetFocus() =>
                Parent?.SetFocus(this);

            /// <summary>
            /// Registers the element to the given parent object.
            /// </summary>
            public void Register(IHudParent parent)
            {
                if (parent != null && Parent == null)
                {
                    Parent = parent;
                    Parent.RegisterChild(this);
                }
            }

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            public void Unregister()
            {
                if (Parent != null)
                {
                    IHudParent lastParent = Parent;

                    Parent = null;
                    lastParent.RemoveChild(this);
                }
            }

            protected override object GetOrSetMember(object data, int memberEnum)
            {
                if (memberEnum < 10)
                {
                    base.GetOrSetMember(data, memberEnum);
                }
                else
                {
                    switch ((HudNodeAccessors)memberEnum)
                    {
                        case HudNodeAccessors.GetFocus:
                            GetFocus();
                            break;
                        case HudNodeAccessors.GetParentData:
                            return Parent.GetApiData();
                        case HudNodeAccessors.GetParentID:
                            return Parent?.ID;
                        case HudNodeAccessors.Register:
                            Register(new HudNodeData((HudElementMembers)data));
                            break;
                        case HudNodeAccessors.Unregister:
                            Unregister();
                            break;
                    }
                }

                return null;
            }
        }
    }
}