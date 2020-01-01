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
        public class ControlPage : IControlPage
        {
            public RichText Name
            {
                get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)ControlPageAccessors.Name)); }
                set { GetOrSetMemberFunc(value.GetApiData(), (int)ControlPageAccessors.Name); }
            }

            public IReadOnlyCollection<IControlCategory> Categories { get; }

            public IControlPage CategoryContainer => this;

            public object ID => data.Item3;

            public bool Enabled
            {
                get { return (bool)GetOrSetMemberFunc(null, (int)ControlPageAccessors.Enabled); }
                set { GetOrSetMemberFunc(value, (int)ControlPageAccessors.Enabled); }
            }

            private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
            private readonly ControlContainerMembers data;

            public ControlPage() : this(ModMenu.GetNewMenuPage())
            { }

            internal ControlPage(ControlContainerMembers data)
            {
                this.data = data;

                var GetCatDataFunc = data.Item2.Item1 as Func<int, ControlContainerMembers>;
                Func<int, ControlCategory> GetCatFunc = (x => new ControlCategory(GetCatDataFunc(x)));

                Categories = new ReadOnlyCollectionData<IControlCategory>(GetCatFunc, data.Item2.Item2);
            }

            IEnumerator<IControlCategory> IEnumerable<IControlCategory>.GetEnumerator() =>
                Categories.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Categories.GetEnumerator();

            public void Add(ControlCategory category) =>
                GetOrSetMemberFunc(category.ID, (int)ControlPageAccessors.AddCategory);

            public ControlContainerMembers GetApiData() =>
                data;
        }
    }
}