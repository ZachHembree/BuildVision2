using System;
using System.Collections.Generic;
using VRage;
using ControlMembers = VRage.MyTuple<string, int, System.Func<bool>, bool>;

namespace RichHudFramework
{
    namespace UI.Client
    {
        public sealed partial class BindManager
        {
            private class Control : IControl
            {
                public string Name { get; }
                public int Index { get; }
                public bool IsPressed => IsPressedFunc();
                public bool Analog { get; }

                private readonly Func<bool> IsPressedFunc;

                public Control(ControlMembers data)
                {
                    Name = data.Item1;
                    Index = data.Item2;
                    IsPressedFunc = data.Item3;
                    Analog = data.Item4;
                }

                public ControlMembers GetApiData()
                {
                    return new ControlMembers()
                    {
                        Item1 = Name,
                        Item2 = Index,
                        Item3 = IsPressedFunc,
                        Item4 = Analog
                    };
                }
            }
        }
    }
}