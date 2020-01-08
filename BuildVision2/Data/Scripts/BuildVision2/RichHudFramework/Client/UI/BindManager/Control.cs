using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using ControlMembers = MyTuple<string, string, int, Func<bool>, bool, ApiMemberAccessor>;

    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private class Control : IControl
            {
                public string Name { get; }
                public string DisplayName { get; }
                public int Index { get; }
                public bool IsPressed => IsPressedFunc();
                public bool Analog { get; }

                private readonly Func<bool> IsPressedFunc;

                public Control(ControlMembers data)
                {
                    Name = data.Item1;
                    DisplayName = data.Item2;
                    Index = data.Item3;
                    IsPressedFunc = data.Item4;
                    Analog = data.Item5;
                }

                public ControlMembers GetApiData()
                {
                    return new ControlMembers()
                    {
                        Item1 = Name,
                        Item3 = Index,
                        Item4 = IsPressedFunc,
                        Item5 = Analog
                    };
                }
            }
        }
    }
}