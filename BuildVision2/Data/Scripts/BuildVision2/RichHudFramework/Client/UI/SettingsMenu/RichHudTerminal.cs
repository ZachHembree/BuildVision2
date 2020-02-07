using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
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
        using RichHudFramework.Client;
        using SettingsMenuMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMembers
            ControlContainerMembers, // MenuRoot
            Func<int, ControlMembers>, // GetNewControl
            Func<int, ControlContainerMembers>, // GetNewContainer
            Func<int, ControlMembers> // GetNewModPage
        >;

        public sealed class RichHudTerminal : RichHudClient.ApiModule<SettingsMenuMembers>
        {
            public static IModControlRoot Root => Instance.menuRoot;

            private static RichHudTerminal Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static RichHudTerminal instance;

            private readonly ModControlRoot menuRoot;
            private readonly ApiMemberAccessor GetOrSetMembersFunc;
            private readonly Func<int, ControlMembers> GetNewControlFunc;
            private readonly Func<int, ControlContainerMembers> GetNewContainerFunc;
            private readonly Func<int, ControlMembers> GetNewPageFunc;

            private RichHudTerminal() : base(ApiModuleTypes.SettingsMenu, false, true)
            {
                var data = GetApiData();

                GetOrSetMembersFunc = data.Item1;
                GetNewControlFunc = data.Item3;
                GetNewContainerFunc = data.Item4;
                GetNewPageFunc = data.Item5;

                menuRoot = new ModControlRoot(data.Item2);
            }

            public static void Init()
            {
                if (instance == null)
                {
                    instance = new RichHudTerminal();
                }
            }

            public override void Close()
            {
                instance = null;
            }

            internal static ControlMembers GetNewMenuControl(MenuControls controlEnum) =>
                Instance.GetNewControlFunc((int)controlEnum);

            internal static ControlContainerMembers GetNewMenuTile() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Tile);

            internal static ControlContainerMembers GetNewMenuCategory() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Category);

            internal static ControlMembers GetNewMenuPage(ModPages pageEnum) =>
                Instance.GetNewPageFunc((int)pageEnum);

            private class ModControlRoot : IModControlRoot
            {
                public RichText Name
                {
                    get { return new RichText(GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Name) as IList<RichStringMembers>); }
                    set { GetOrSetMemberFunc(value.ApiData, (int)ModControlRootAccessors.Name); }
                }

                public IReadOnlyCollection<ITerminalPage> Pages { get; }

                public IModControlRoot PageContainer => this;

                public object ID => data.Item3;

                public ITerminalPage Selection
                {
                    get { return new TerminalPage((ControlMembers)GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Selection)); }
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

                    var GetPageDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
                    Func<int, ITerminalPage> GetPageFunc = (x => new TerminalPage(GetPageDataFunc(x)));

                    Pages = new ReadOnlyCollectionData<ITerminalPage>(GetPageFunc, data.Item2.Item2);
                }

                IEnumerator<ITerminalPage> IEnumerable<ITerminalPage>.GetEnumerator() =>
                    Pages.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    Pages.GetEnumerator();

                public void Add(TerminalPageBase page) =>
                    GetOrSetMemberFunc(page.ID, (int)ModControlRootAccessors.AddPage);

                public ControlContainerMembers GetApiData() =>
                    data;

                private class TerminalPage : TerminalPageBase
                {
                    public TerminalPage(ControlMembers data) : base(data)
                    { }
                }
            }
        }
    }
}