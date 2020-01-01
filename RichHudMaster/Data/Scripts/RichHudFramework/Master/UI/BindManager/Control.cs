using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Input;
using BindDefinitionData = VRage.MyTuple<string, string[]>;
using BindMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    System.Func<bool>, // IsPressed
    System.Func<bool>, // IsPressedAndHeld
    System.Func<bool>, // IsNewPressed
    System.Func<bool> // IsReleased
>;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;

namespace RichHudFramework
{
    using BindGroupMembers = MyTuple<
        string, // Name                
        BindMembers[],// Binds
        Func<IList<int>, int, bool>, // DoesComboConflict
        Func<string, int[], bool, BindMembers?>, // TryRegisterBind
        Func<IList<BindDefinitionData>, BindMembers[]>, // TryLoadBindData
        MyTuple<
            Func<string, string[], bool, BindMembers?>, // TryRegisterBind2
            Func<BindDefinitionData[]>, // GetBindData
            Action, // HandleInput
            Action // ClearSubscribers
        >
    >;

    namespace UI.Server
    {
        public sealed partial class BindManager
        {
            /// <summary>
            /// General purpose button wrapper for MyKeys and anything else associated with a name and an IsPressed method.
            /// </summary>
            private class Control : IControl
            {
                public string Name { get; }
                public bool IsPressed { get { return isPressedFunc(); } }
                public bool Analog { get; }
                public int Index { get; }

                private readonly Func<bool> isPressedFunc;

                public Control(MyKeys seKey, int index, bool Analog = false)
                    : this(seKey.ToString(), index, () => MyAPIGateway.Input.IsKeyPress(seKey), Analog)
                { }

                public Control(string name, int index, Func<bool> IsPressed, bool Analog = false)
                {
                    Name = name;
                    Index = index;
                    isPressedFunc = IsPressed;
                    this.Analog = Analog;
                }

                public ControlMembers GetApiData()
                {
                    return new ControlMembers()
                    {
                        Item1 = Name,
                        Item2 = Index,
                        Item3 = isPressedFunc,
                        Item4 = Analog
                    };
                }
            }
        }
    }
}