using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Wrapper used to access types of <see cref="IHudParent"/> via the API.
        /// </summary>
        public class HudParentData : IHudParent
        {
            public bool Visible { get { return VisFunc(); } set { } }
            public object ID { get; }

            private readonly List<object> localChildren;

            private readonly Func<bool> VisFunc;
            private readonly Action BeforeDrawAction, DrawAction;
            private readonly Action InputAction;
            protected readonly ApiMemberAccessor GetOrSetMemberFunc;

            public HudParentData(HudElementMembers members)
            {
                localChildren = new List<object>();

                VisFunc = members.Item1;
                ID = members.Item2;
                BeforeDrawAction = members.Item3;
                DrawAction = members.Item4;
                InputAction = members.Item5;
                GetOrSetMemberFunc = members.Item6;
            }

            public void RegisterChild(IHudNode child)
            {
                localChildren.Add(child.ID);
                GetOrSetMemberFunc(child.GetApiData(), (int)HudParentAccessors.Add);
            }

            public void BeforeDrawStart() =>
                BeforeDrawAction();

            public void DrawStart() =>
                DrawAction();

            public void HandleInputStart() =>
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
                for (int n = 0; n < localChildren.Count; n++)
                    GetOrSetMemberFunc(localChildren[n], (int)HudParentAccessors.RemoveChild);

                localChildren.Clear();
            }

            public HudElementMembers GetApiData()
            {
                return new HudElementMembers()
                {
                    Item1 = VisFunc,
                    Item2 = ID,
                    Item3 = BeforeDrawAction,
                    Item4 = DrawAction,
                    Item5 = InputAction,
                    Item6 = GetOrSetMemberFunc
                };
            }
        }
    }
}