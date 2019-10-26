using System;
using System.Collections.Generic;
using VRage;

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
        using HudNodeMembers = MyTuple<
            HudParentMembers, // Base members
            Func<object>, // GetParentID
            object, // GetParentData (Func<HudParentMembers>)
            Action, // GetFocus
            Action<object>, // Register
            Action // Unregister
        >;

        public class HudParentData : IHudParent
        {
            protected static HudParentData Default = new HudParentData();

            public bool Visible => VisFunc();
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

            public void Add(IHudNode child)
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

        public sealed class HudNodeData : HudParentData, IHudNode
        {
            public IHudParent Parent
            {
                get
                {
                    if (parent.ID != GetParentID())
                    {
                        if (GetParentID() != null)
                            parent = new HudParentData(GetParentFunc());
                        else
                            parent = HudParentData.Default;
                    }

                    return parent;
                }
            }

            private IHudParent parent;
            private readonly Func<object> GetParentID;
            private readonly Func<HudParentMembers> GetParentFunc;
            private readonly Action GetFocusAction;
            private readonly Action<object> RegisterAction;
            private readonly Action UnregisterAction;

            public HudNodeData(HudNodeMembers apiData) : base(apiData.Item1)
            {
                GetParentID = apiData.Item2;
                GetParentFunc = apiData.Item3 as Func<HudParentMembers>;
                GetFocusAction = apiData.Item4;
                RegisterAction = apiData.Item5;
                UnregisterAction = apiData.Item6;

                parent = Default;
            }

            public void Register(IHudParent parent) =>
                RegisterAction(parent.GetApiData());

            public void GetFocus() =>
                GetFocusAction();

            public void Unregister() =>
                UnregisterAction();

            public new HudNodeMembers GetApiData()
            {
                return new HudNodeMembers
                {
                    Item1 = base.GetApiData(),
                    Item2 = GetParentID,
                    Item3 = GetParentFunc,
                    Item4 = GetFocusAction,
                    Item5 = RegisterAction,
                    Item6 = UnregisterAction
                };
            }
        }
    }
}