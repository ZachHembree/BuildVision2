using System;
using System.Collections.Generic;
using VRage;
using RichHudFramework.Game;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
{
    using HudElementMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        Action, // Draw
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMembers
    >;

    namespace UI
    {
        /// <summary>
        /// Base for all hud elements that serve as parents of other HUD elements. Types deriving from this class cannot be
        /// parented to other elements; only types of <see cref="IHudNode"/> can be parented.
        /// </summary>
        public abstract class HudParentBase : IHudParent
        {
            /// <summary>
            /// Determines whether or not an element will be drawn or process input. Visible by default.
            /// </summary>
            public virtual bool Visible { get; set; }

            /// <summary>
            /// Unique identifer.
            /// </summary>
            public object ID => this;

            protected readonly List<IHudNode> children;

            /// <summary>
            /// Initializes a new <see cref="HudParentBase"/> with no child elements.
            /// </summary>
            public HudParentBase()
            {
                Visible = true;
                children = new List<IHudNode>();
            }

            public virtual void HandleInput()
            {
                for (int n = children.Count - 1; n >= 0; n--)
                {
                    if (children[n].Visible)
                        children[n].HandleInput();
                }
            }

            public virtual void Draw()
            {
                foreach (IHudNode child in children)
                {
                    if (child.Visible)
                        child.Draw();
                }
            }

            /// <summary>
            /// Moves the specified child element to the end of the update list in
            /// order to ensure that it's drawn on top/updated last.
            /// </summary>
            public void SetFocus(IHudNode child) =>
                SetFocus(child.ID);

            private void SetFocus(object childID)
            {
                int last = children.Count - 1,
                    childIndex = children.FindIndex(x => x.ID == childID);

                children.Swap(last, childIndex);
            }

            /// <summary>
            /// Registers a child node to the object.
            /// </summary>
            public void RegisterChild(IHudNode child)
            {
                if (child.Parent == null)
                    child.Register(this);
                else if (child.Parent.ID == ID)
                    children.Add(child);
            }

            /// <summary>
            /// Registers a collection of child nodes to the object.
            /// </summary>
            public void RegisterChildren(IEnumerable<IHudNode> newChildren)
            {
                foreach (IHudNode child in newChildren)
                    child.Register(this);
            }

            /// <summary>
            /// Unregisters the specified node from the parent.
            /// </summary>
            public void RemoveChild(IHudNode child)
            {
                if (child.Parent != null && child.Parent.ID == ID)
                    child.Unregister();
                else
                {
                    int childIndex = children.FindIndex(x => x.ID == child.ID);
                    children.RemoveAt(childIndex);
                }
            }

            private void RemoveChild(object childID) =>
                RemoveChild(children.Find(x => x.ID == childID));

            protected virtual object GetOrSetMember(object data, int memberEnum)
            {
                switch ((HudParentAccessors)memberEnum)
                {
                    case HudParentAccessors.Add:
                        RegisterChild(new HudNodeData((HudElementMembers)data));
                        break;
                    case HudParentAccessors.RemoveChild:
                        RemoveChild(data);
                        break;
                    case HudParentAccessors.SetFocus:
                        SetFocus(data);
                        break;
                }

                return null;
            }

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudParent"/> through the API.
            /// </summary>
            public HudElementMembers GetApiData()
            {
                return new HudElementMembers()
                {
                    Item1 = () => Visible,
                    Item2 = this,
                    Item3 = Draw,
                    Item4 = HandleInput,
                    Item5 = GetOrSetMember
                };
            }           
        }
    }
}