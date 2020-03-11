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
        public enum HudNodeAccessors : int
        {
            GetParentID = 10,
            GetParentData = 11,
            GetFocus = 12,
            Register = 13,
            Unregister = 14,
            Registered = 15,
        }

        /// <summary>
        /// Base class for hud elements that can be parented to other elements.
        /// </summary>
        public abstract class HudNodeBase : HudParentBase, IHudNode
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public virtual IHudParent Parent { get; protected set; }

            /// <summary>
            /// Indicates whether or not the element has been registered to a parent.
            /// </summary>
            public bool Registered { get; private set; }

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
            public virtual void Register(IHudParent parent)
            {
                if (parent != null && Parent == null)
                {
                    Parent = parent;
                    Parent.RegisterChild(this);
                    Registered = true;
                }
            }

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            public virtual void Unregister()
            {
                if (Parent != null)
                {
                    IHudParent lastParent = Parent;

                    Parent = null;
                    lastParent.RemoveChild(this);
                    Registered = false;
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
                        case HudNodeAccessors.Registered:
                            return Registered;
                    }
                }

                return null;
            }
        }
    }
}