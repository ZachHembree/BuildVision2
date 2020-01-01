using RichHudFramework.Game;
using RichHudFramework.UI.Rendering;
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

    namespace UI.Server
    {
        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;
        public sealed partial class ModMenu : ModBase.ComponentBase
        {
            private class ModControlRoot : HudElementBase, IModControlRoot, IListBoxEntry
            {
                public event Action OnSelectionChanged;
                internal event Action<ModControlRoot> OnModUpdate;

                public override float Width { get { return pageControl.Width; } set { pageControl.Width = value; } }
                public override float Height { get { return pageControl.Height; } set { pageControl.Height = value; } }

                public RichText Name { get { return pageControl.Name; } set { pageControl.Name = value; } }
                public IReadOnlyCollection<IControlPage> Pages { get; }
                public IModControlRoot PageContainer => this;
                public IControlPage Selection => SelectedElement;
                public ControlPage SelectedElement => pageControl.Selection?.AssocMember;
                public bool Enabled { get; set; }

                private readonly TreeBox<ControlPage> pageControl;
                private readonly SettingsMenu menu;

                public ModControlRoot(SettingsMenu parent) : base(null)
                {
                    menu = parent;

                    pageControl = new TreeBox<ControlPage>(this);
                    Pages = new ReadOnlyCollectionData<IControlPage>(x => pageControl.Members[x].AssocMember, () => pageControl.Members.Count);

                    pageControl.MouseInput.OnLeftClick += UpdateSelection;
                    pageControl.OnSelectionChanged += () => UpdateSelection();

                    Enabled = true;
                    Visible = true;
                }

                private void UpdateSelection()
                {
                    OnSelectionChanged?.Invoke();
                    OnModUpdate?.Invoke(this);
                }

                public void Add(ControlPage page)
                {
                    ListBoxEntry<ControlPage> listMember = pageControl.Add(page.Name, page);
                    page.NameBuilder = listMember.TextBoard;
                    menu.AddPage(page);
                }

                IEnumerator<IControlPage> IEnumerable<IControlPage>.GetEnumerator() =>
                    Pages.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    Pages.GetEnumerator();

                public new ControlContainerMembers GetApiData()
                {
                    return new ControlContainerMembers()
                    {
                        Item1 = GetOrSetMember,
                        Item2 = new MyTuple<object, Func<int>>()
                        {
                            Item1 = (Func<int, ControlContainerMembers>)(x => pageControl.Members[x].AssocMember.GetApiData()),
                            Item2 = () => pageControl.Members.Count
                        },
                        Item3 = this
                    };
                }

                private object GetOrSetMember(object data, int memberEnum)
                {
                    var member = (ModControlRootAccessors)memberEnum;

                    switch (member)
                    {
                        case ModControlRootAccessors.OnSelectionChanged:
                            {
                                var eventData = (MyTuple<bool, Action>)data;

                                if (eventData.Item1)
                                    OnSelectionChanged += eventData.Item2;
                                else
                                    OnSelectionChanged -= eventData.Item2;

                                break;
                            }
                        case ModControlRootAccessors.Name:
                            {
                                if (data == null)
                                    return Name.GetApiData();
                                else
                                    Name = new RichText(data as RichStringMembers[]);

                                break;
                            }
                        case ModControlRootAccessors.Enabled:
                            {
                                if (data == null)
                                    return Enabled;
                                else
                                    Enabled = (bool)data;

                                break;
                            }
                        case ModControlRootAccessors.Selection:
                            {
                                return SelectedElement.GetApiData();
                            }
                        case ModControlRootAccessors.AddPage:
                            {
                                Add(data as ControlPage);
                                break;
                            }
                    }

                    return null;
                }
            }
        }
    }
}