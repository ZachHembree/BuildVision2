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
        /// Wrapper used to access types of <see cref="IHudNode"/> via the API.
        /// </summary>
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