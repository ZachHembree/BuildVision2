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
        public class ControlCategory : IControlCategory
        {
            public RichText HeaderText
            {
                get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)ControlCatAccessors.HeaderText)); }
                set { GetOrSetMemberFunc(value.GetApiData(), (int)ControlCatAccessors.HeaderText); }
            }

            public RichText SubheaderText
            {
                get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)ControlCatAccessors.SubheaderText)); }
                set { GetOrSetMemberFunc(value.GetApiData(), (int)ControlCatAccessors.SubheaderText); }
            }

            public IReadOnlyCollection<IControlTile> Tiles { get; }

            public IControlCategory TileContainer => this;

            public object ID => data.Item3;

            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)ControlCatAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.Enabled); }
            }

            private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            private readonly ControlContainerMembers data;

            public ControlCategory() : this(ModMenu.GetNewMenuCategory())
            { }

            internal ControlCategory(ControlContainerMembers data)
            {
                this.data = data;

                var GetTileDataFunc = data.Item2.Item1 as Func<int, ControlContainerMembers>;
                Func<int, ControlTile> GetTileFunc = x => new ControlTile(GetTileDataFunc(x));

                Tiles = new ReadOnlyCollectionData<IControlTile>(GetTileFunc, data.Item2.Item2);
            }

            IEnumerator<IControlTile> IEnumerable<IControlTile>.GetEnumerator() =>
                Tiles.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Tiles.GetEnumerator();

            public void Add(ControlTile tile) =>
                GetOrSetMemberFunc(tile.ID, (int)ControlCatAccessors.AddTile);

            public ControlContainerMembers GetApiData() =>
                data;
        }
    }
}