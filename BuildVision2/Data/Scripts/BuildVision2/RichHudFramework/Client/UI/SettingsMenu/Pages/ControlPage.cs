using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
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
        public class ControlPage : TerminalPageBase, IControlPage
        {
            public IReadOnlyCollection<IControlCategory> Categories { get; }

            public IControlPage CategoryContainer => this;

            public ControlPage() : base(ModPages.ControlPage)
            {
                var catData = (MyTuple<object, Func<int>>)GetOrSetMemberFunc(null, (int)ControlPageAccessors.CategoryData);
                var GetCatDataFunc = catData.Item1 as Func<int, ControlContainerMembers>;

                Func<int, ControlCategory> GetCatFunc = (x => new ControlCategory(GetCatDataFunc(x)));
                Categories = new ReadOnlyCollectionData<IControlCategory>(GetCatFunc, catData.Item2);
            }

            IEnumerator<IControlCategory> IEnumerable<IControlCategory>.GetEnumerator() =>
                Categories.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Categories.GetEnumerator();

            public void Add(ControlCategory category) =>
                GetOrSetMemberFunc(category.ID, (int)ControlPageAccessors.AddCategory);
        }
    }
}