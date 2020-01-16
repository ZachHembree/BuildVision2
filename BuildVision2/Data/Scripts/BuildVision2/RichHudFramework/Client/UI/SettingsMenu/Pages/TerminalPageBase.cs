using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;

    namespace UI.Client
    {
        public abstract class TerminalPageBase : ITerminalPage
        {
            public RichText Name
            {
                get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Name)); }
                set { GetOrSetMemberFunc(value.GetApiData(), (int)TerminalPageAccessors.Name); }
            }

            public object ID => data.Item2;

            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)TerminalPageAccessors.Enabled); }
            }

            protected ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            protected readonly ControlMembers data;

            internal TerminalPageBase(ModPages pageEnum)
            {
                data = RichHudTerminal.GetNewMenuPage(pageEnum);
            }

            internal TerminalPageBase(ControlMembers data)
            {
                this.data = data;
            }

            public ControlMembers GetApiData() =>
                data;
        }
    }
}