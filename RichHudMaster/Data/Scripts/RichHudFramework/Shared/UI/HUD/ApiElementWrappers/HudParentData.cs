using System;
using System.Collections.Generic;
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
        /// Wrapper used to access types of <see cref="IHudParent"/> via the API.
        /// </summary>
        public class HudParentData : IHudParent
        {
            protected static HudParentData Default = new HudParentData();

            public bool Visible { get { return VisFunc(); } set { } }
            public object ID { get; }

            private readonly List<object> localChildren;
            private readonly Func<bool> VisFunc;
            private readonly Action<HudNodeMembers> AddFunc;
            private readonly Action DrawAction;
            private readonly Action InputAction;
            private readonly Action<object> RemoveChildAction;
            private readonly Action<object> SetFocusAction;

            public HudParentData(HudParentMembers members)
            {
                localChildren = new List<object>();

                VisFunc = members.Item1;
                ID = members.Item2;
                AddFunc = members.Item3 as Action<HudNodeMembers>;
                DrawAction = members.Item4;
                InputAction = members.Item5;

                var data2 = members.Item6;

                RemoveChildAction = data2.Item1;
                SetFocusAction = data2.Item2;
            }

            private HudParentData()
            {
                ID = null;
            }

            public void RegisterChild(IHudNode child)
            {
                localChildren.Add(child.ID);
                AddFunc(child.GetApiData());
            }

            public void BeforeDraw() =>
                DrawAction();

            public void BeforeInput() =>
                InputAction();

            public void RegisterChildren(IEnumerable<IHudNode> newChildren)
            {
                foreach (IHudNode child in newChildren)
                    child.Register(this);
            }

            public void RemoveChild(IHudNode child)
            {
                localChildren.Remove(child.ID);
                RemoveChildAction(child.ID);
            }

            public void SetFocus(IHudNode child) =>
                SetFocusAction(child.ID);

            public void ClearLocalChildren()
            {
                for (int n = localChildren.Count - 1; n >= 0; n--)
                    RemoveChildAction(localChildren[n]);

                localChildren.Clear();
            }

            public HudParentMembers GetApiData()
            {
                return new HudParentMembers()
                {
                    Item1 = VisFunc,
                    Item2 = ID,
                    Item3 = AddFunc,
                    Item4 = DrawAction,
                    Item5 = InputAction,
                    Item6 = new MyTuple<Action<object>, Action<object>>()
                    {
                        Item1 = RemoveChildAction,
                        Item2 = SetFocusAction
                    }
                };
            }
        }
    }
}