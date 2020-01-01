using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
{
    using UI;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI.Client
    {
        using RichHudClient;
        using SettingsMenuMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMembers
            ControlContainerMembers, // MenuRoot
            Func<int, ControlMembers>, // GetNewControl
            Func<int, ControlContainerMembers> // GetNewContainer
        >;

        public sealed class ModMenu : RichHudClient.ApiComponentBase
        {
            public static IModControlRoot Root => Instance.menuRoot;

            private static ModMenu Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static ModMenu instance;

            private readonly ModControlRoot menuRoot;
            private readonly ApiMemberAccessor GetOrSetMembersFunc;
            private readonly Func<int, ControlMembers> GetNewControlFunc;
            private readonly Func<int, ControlContainerMembers> GetNewContainerFunc;

            private ModMenu() : base(ApiComponentTypes.SettingsMenu, false, true)
            {
                var data = (SettingsMenuMembers)GetApiData();

                GetOrSetMembersFunc = data.Item1;
                GetNewControlFunc = data.Item3;
                GetNewContainerFunc = data.Item4;

                menuRoot = new ModControlRoot(data.Item2);
            }

            public static void Init()
            {
                if (instance == null)
                {
                    instance = new ModMenu();
                }
            }

            internal static ControlMembers GetNewMenuControl(MenuControls controlEnum) =>
                Instance.GetNewControlFunc((int)controlEnum);

            internal static ControlContainerMembers GetNewMenuTile() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Tile);

            internal static ControlContainerMembers GetNewMenuCategory() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Category);

            internal static ControlContainerMembers GetNewMenuPage() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Page);

            private class ModControlRoot : IModControlRoot
            {
                public RichText Name
                {
                    get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Name)); }
                    set { GetOrSetMemberFunc(value.GetApiData(), (int)ModControlRootAccessors.Name); }
                }

                public IReadOnlyCollection<IControlPage> Pages { get; }

                public IModControlRoot PageContainer => this;

                public object ID => data.Item3;

                public IControlPage Selection
                {
                    get { return new ControlPage((ControlContainerMembers)GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Selection)); }
                }

                public bool Enabled
                {
                    get { return (bool)GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Enabled); }
                    set { GetOrSetMemberFunc(value, (int)ModControlRootAccessors.Enabled); }
                }

                public event Action OnSelectionChanged
                {
                    add { GetOrSetMemberFunc(new EventAccessor(true, value), (int)ModControlRootAccessors.OnSelectionChanged); }
                    remove { GetOrSetMemberFunc(new EventAccessor(false, value), (int)ModControlRootAccessors.OnSelectionChanged); }
                }

                private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
                private readonly ControlContainerMembers data;

                public ModControlRoot(ControlContainerMembers data)
                {
                    this.data = data;

                    var GetPageDataFunc = data.Item2.Item1 as Func<int, ControlContainerMembers>;
                    Func<int, ControlPage> GetPageFunc = (x => new ControlPage(GetPageDataFunc(x)));

                    Pages = new ReadOnlyCollectionData<IControlPage>(GetPageFunc, data.Item2.Item2);
                }

                IEnumerator<IControlPage> IEnumerable<IControlPage>.GetEnumerator() =>
                    Pages.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    Pages.GetEnumerator();

                public void Add(ControlPage page) =>
                    GetOrSetMemberFunc(page.ID, (int)ModControlRootAccessors.AddPage);

                public ControlContainerMembers GetApiData() =>
                    data;
            }
        }
    }
}