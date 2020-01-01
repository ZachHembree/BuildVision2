using System;
using VRage;

namespace RichHudFramework
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
            object, // GetParentData (Func<HudParentMembers>)
            Action, // GetFocus
            Action<object>, // Register
            Action // Unregister
        >;

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
                if (Parent == null)
                {
                    Parent = parent;
                    Parent.RegisterChild(this);
                }
            }

            private void Register(object parentData) =>
                Register(new HudNodeData((HudNodeMembers)parentData));

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

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudNode"/> through the API.
            /// </summary>
            public new HudNodeMembers GetApiData()
            {
                var apiData = new HudNodeMembers
                {
                    Item1 = base.GetApiData(),
                    Item2 = () => Parent,
                    Item3 = (Func<HudParentMembers>)Parent.GetApiData,
                    Item4 = GetFocus,
                    Item5 = Register,
                    Item6 = Unregister
                };

                return apiData;
            }
        }
    }
}