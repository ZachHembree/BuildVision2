using System;
using System.Collections.Generic;
using VRage;
using DarkHelmet.Game;

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
        using System.Collections;
        using HudNodeMembers = MyTuple<
            HudParentMembers, // Base members
            Func<object>, // GetParentID
            object, // GetParentData (Func<HudParentMembers>)
            Action, // GetFocus
            Action<object>, // Register
            Action // Unregister
        >;

        /// <summary>
        /// Base for all hud elements that serve as parents of other HUD elements. Types deriving from this class cannot be
        /// parented to other elements; only types of <see cref="IHudNode"/> can be parented.
        /// </summary>
        public abstract class HudParentBase : IHudParent, IEnumerable<IHudNode>
        {
            /// <summary>
            /// Allows collection initalizer syntax to be used in conjunction with normal initializers.
            /// </summary>
            public HudParentBase ChildContainer => this;

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

            public IEnumerator<IHudNode> GetEnumerator() =>
                children.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// If visible == true, it will update the input of the element before updating 
            /// the input of its child elements.
            /// </summary>
            public virtual void BeforeInput()
            {
                if (Visible)
                {
                    HandleInput();

                    for (int n = children.Count - 1; n >= 0; n--)
                    {
                        if (children[n].Visible)
                            children[n].BeforeInput();
                    }
                }
            }

            /// <summary>
            /// Used to internally update the input of any deriving types. Will not be called
            /// if visible != true.
            /// </summary>
            protected virtual void HandleInput() { }

            /// <summary>
            /// If visible == true, the element will draw itself before updating its child
            /// elements.
            /// </summary>
            public virtual void BeforeDraw()
            {
                if (Visible)
                {
                    Draw();

                    foreach (IHudNode child in children)
                    {
                        if (child.Visible)
                            child.BeforeDraw();
                    }
                }
            }

            /// <summary>
            /// Used to internally draw any deriving types. Will not be called
            /// if visible != true.
            /// </summary>
            protected virtual void Draw() { }

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
            public void Add(IHudNode child)
            {
                if (child.Parent == null)
                    child.Register(this);
                else if (child.Parent.ID == ID)
                    children.Add(child);
            }

            private void Add(HudNodeMembers childData) =>
                Add(new HudNodeData(childData));

            /// <summary>
            /// Registers a collection of child nodes to the object.
            /// </summary>
            /// <param name="newChildren"></param>
            public void RegisterChildren(IEnumerable<IHudNode> newChildren)
            {
                foreach (IHudNode child in newChildren)
                    child.Register(this);
            }

            /// <summary>
            /// Unregisters the specified node from the parent.
            /// </summary>
            /// <param name="child"></param>
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

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudParent"/> through the API.
            /// </summary>
            public HudParentMembers GetApiData()
            {
                return new HudParentMembers()
                {
                    Item1 = () => Visible,
                    Item2 = this,
                    Item3 = (Action<HudNodeMembers>)Add,
                    Item4 = () => ModBase.RunSafeAction(BeforeDraw),
                    Item5 = BeforeInput,
                    Item6 = new MyTuple<Action<object>, Action<object>>()
                    {
                        Item1 = RemoveChild,
                        Item2 = SetFocus
                    }
                };
            }           
        }
    }
}