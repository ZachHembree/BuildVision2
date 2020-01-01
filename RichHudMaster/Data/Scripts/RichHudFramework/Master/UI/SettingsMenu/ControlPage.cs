using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using RichHudFramework.UI.Rendering;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI.Server
    {
        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;

        public class ControlPage : HudElementBase, IListBoxEntry, IControlPage
        {
            public override float Width
            {
                get { return catBox.Width; }
                set
                {
                    catBox.Width = value;
                }
            }
            public override float Height { get { return catBox.Height; } set { catBox.Height = value; } }
            public override Vector2 Padding { get { return catBox.Padding; } set { catBox.Padding = value; } }

            public RichText Name
            {
                get
                {
                    if (NameBuilder != null)
                        return NameBuilder.GetText();
                    else
                        return name;
                }

                set
                {
                    if (NameBuilder != null)
                        NameBuilder.SetText(value);
                    else
                        name = value;
                }
            }

            public ITextBoard NameBuilder { get; set; }
            public IReadOnlyCollection<IControlCategory> Categories { get; }
            public IControlPage CategoryContainer => this;
            public bool Enabled { get; set; }

            private readonly ScrollBox<ControlCategory> catBox;
            private RichText name;

            public ControlPage(IHudParent parent = null) : base(parent)
            {
                catBox = new ScrollBox<ControlCategory>(this)
                {
                    Spacing = 30f,
                    FitToChain = false,
                    AlignVertical = true,
                };

                catBox.background.Visible = false;
                Categories = new ReadOnlyCollectionData<IControlCategory>(x => catBox.Members.List[x], () => catBox.Members.List.Count);

                Enabled = true;
                Visible = false;
                Name = new RichText("NewPage", GlyphFormat.White);
            }

            protected override void Draw()
            {
                base.Draw();

                for (int n = 0; n < catBox.List.Count; n++)
                    catBox.List[n].Width = Width - catBox.scrollBar.Width;
            }

            public void Add(ControlCategory category)
            {
                catBox.AddToList(category);
            }

            IEnumerator<IControlCategory> IEnumerable<IControlCategory>.GetEnumerator() =>
                Categories.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                Categories.GetEnumerator();

            public new ControlContainerMembers GetApiData()
            {
                return new ControlContainerMembers()
                {
                    Item1 = GetOrSetMember,
                    Item2 = new MyTuple<object, Func<int>>()
                    {
                        Item1 = (Func<int, ControlContainerMembers>)(x => Categories[x].GetApiData()),
                        Item2 = () => Categories.Count
                    },
                    Item3 = this,
                };
            }

            private object GetOrSetMember(object data, int memberEnum)
            {
                var member = (ControlPageAccessors)memberEnum;

                switch (member)
                {
                    case ControlPageAccessors.Name:
                        {
                            if (data == null)
                                return Name.GetApiData();
                            else
                                Name = new RichText(data as RichStringMembers[]);

                            break;
                        }
                    case ControlPageAccessors.AddCategory:
                        {
                            Add(data as ControlCategory);
                            break;
                        }
                }

                return null;
            }
        }
    }
}