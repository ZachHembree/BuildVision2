using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

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

    namespace UI.Server
    {
        public class ControlTile : HudElementBase, IListBoxEntry, IControlTile
        {
            public IReadOnlyCollection<ITerminalControl> Controls { get; }
            public IControlTile ControlContainer => this;
            public override float Width { get { return controls.Width + Padding.X; } set { controls.Width = value - Padding.X; } }
            public bool Enabled { get; set; }

            private readonly HudChain<TerminalControlBase> controls;

            public ControlTile(IHudParent parent = null) : base(parent)
            {
                var background = new TexturedBox(this)
                {
                    Color = new Color(41, 54, 62, 230),
                    DimAlignment = DimAlignments.Both,
                };

                var border = new BorderBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = new Color(58, 68, 77),
                    Thickness = 2f,
                };

                controls = new HudChain<TerminalControlBase>(this)
                {
                    AutoResize = true,
                    AlignVertical = true,
                    Spacing = 30f,
                };

                Controls = new ReadOnlyCollectionData<ITerminalControl>(x => controls.List[x], () => controls.List.Count);

                Padding = new Vector2(16f);
                Size = new Vector2(300f, 250f);
                Enabled = true;
            }

            public void Add(TerminalControlBase newControl)
            {
                controls.Add(newControl);
            }

            IEnumerator<ITerminalControl> IEnumerable<ITerminalControl>.GetEnumerator() =>
                Controls.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Controls.GetEnumerator();

            public new ControlContainerMembers GetApiData()
            {
                return new ControlContainerMembers()
                {
                    Item1 = GetOrSetMember,
                    Item2 = new MyTuple<object, Func<int>>
                    {
                        Item1 = (Func<int, ControlMembers>)(x => controls.List[x].GetApiData()),
                        Item2 = () => controls.List.Count
                    },
                    Item3 = this
                };
            }

            private object GetOrSetMember(object data, int memberEnum)
            {
                var member = (ControlTileAccessors)memberEnum;

                switch (member)
                {
                    case ControlTileAccessors.AddControl:
                        {
                            Add(data as TerminalControlBase);
                            break;
                        }
                    case ControlTileAccessors.Enabled:
                        {
                            if (data == null)
                                return Enabled;
                            else
                                Enabled = (bool)data;

                            break;
                        }
                }

                return null;
            }
        }
    }
}