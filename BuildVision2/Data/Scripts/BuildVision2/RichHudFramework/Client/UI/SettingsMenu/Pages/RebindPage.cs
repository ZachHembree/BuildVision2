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
        public class RebindPage : TerminalPageBase, IRebindPage
        {
            public IReadOnlyCollection<IBindGroup> BindGroups { get; }

            public RebindPage GroupContainer => this;

            private readonly List<IBindGroup> bindGroups;

            public RebindPage() : base(ModPages.RebindPage)
            {
                bindGroups = new List<IBindGroup>();
                BindGroups = new ReadOnlyCollection<IBindGroup>(bindGroups);
            }

            public void Add(IBindGroup bindGroup)
            {
                GetOrSetMemberFunc(bindGroup.ID, (int)RebindPageAccessors.Add);
                bindGroups.Add(bindGroup);
            }

            public IEnumerator<IBindGroup> GetEnumerator() =>
                bindGroups.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                bindGroups.GetEnumerator();
        }
    }
}