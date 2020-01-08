using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

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
        /// Wrapper used to access types of <see cref="IHudParent"/> via the API.
        /// </summary>
        public class HudParentData : IHudParent
        {
            protected static HudParentData Default = new HudParentData();

            public bool Visible { get { return VisFunc(); } set { } }
            public object ID { get; }

            private readonly List<object> localChildren;

            private readonly Func<bool> VisFunc;
            private readonly Action DrawAction;
            private readonly Action InputAction;
            protected readonly ApiMemberAccessor GetOrSetMemberFunc;

            public HudParentData(HudElementMembers members)
            {
                localChildren = new List<object>();

                VisFunc = members.Item1;
                ID = members.Item2;
                DrawAction = members.Item3;
                InputAction = members.Item4;
                GetOrSetMemberFunc = members.Item5;
            }

            private HudParentData()
            {
                ID = null;
            }

            public void RegisterChild(IHudNode child)
            {
                localChildren.Add(child.ID);
                GetOrSetMemberFunc(child.GetApiData(), (int)HudParentAccessors.Add);
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
                GetOrSetMemberFunc(child.ID, (int)HudParentAccessors.RemoveChild);
            }

            public void SetFocus(IHudNode child) =>
                GetOrSetMemberFunc(child.ID, (int)HudParentAccessors.SetFocus);

            public void ClearLocalChildren()
            {
                for (int n = localChildren.Count - 1; n >= 0; n--)
                    GetOrSetMemberFunc(localChildren[n], (int)HudParentAccessors.RemoveChild);

                localChildren.Clear();
            }

            public HudElementMembers GetApiData()
            {
                return new HudElementMembers()
                {
                    Item1 = VisFunc,
                    Item2 = ID,
                    Item3 = DrawAction,
                    Item4 = InputAction,
                    Item5 = GetOrSetMemberFunc
                };
            }
        }
    }
}