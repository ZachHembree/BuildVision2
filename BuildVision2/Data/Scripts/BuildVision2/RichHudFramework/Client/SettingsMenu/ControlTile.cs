using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
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

    public class ControlTile : IControlTile
    {
        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)ControlTileAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)ControlTileAccessors.Enabled); }
        }

        public IReadOnlyCollection<ITerminalControl> Controls { get; }

        public IControlTile ControlContainer => this;

        public object ID => tileMembers.Item3;

        private ApiMemberAccessor GetOrSetMemberFunc => tileMembers.Item1;
        private readonly ControlContainerMembers tileMembers;

        public ControlTile() : this(ModMenu.GetNewMenuTile())
        { }

        internal ControlTile(ControlContainerMembers data)
        {
            tileMembers = data;

            var GetControlDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
            Func<int, TerminalControlBase> GetControlFunc = (x => new TerminalControl(GetControlDataFunc(x)));

            Controls = new ReadOnlyCollectionData<ITerminalControl>(GetControlFunc, data.Item2.Item2);
        }

        IEnumerator<ITerminalControl> IEnumerable<ITerminalControl>.GetEnumerator() =>
            Controls.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            Controls.GetEnumerator();

        public void Add(TerminalControlBase control) =>
            GetOrSetMemberFunc(control.ID, (int)ControlTileAccessors.AddControl);

        public ControlContainerMembers GetApiData() =>
            tileMembers;

        private class TerminalControl : TerminalControlBase
        {
            public TerminalControl(ControlMembers data) : base(data)
            { }
        }
    }
}