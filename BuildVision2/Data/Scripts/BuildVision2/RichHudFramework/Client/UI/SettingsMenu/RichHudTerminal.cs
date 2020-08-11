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
    using Client;
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
        using SettingsMenuMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMembers
            ControlContainerMembers, // MenuRoot
            Func<int, ControlMembers>, // GetNewControl
            Func<int, ControlContainerMembers>, // GetNewContainer
            Func<int, ControlMembers> // GetNewModPage
        >;

        /// <summary>
        /// Windowed settings menu shared by mods using the framework.
        /// </summary>
        public sealed class RichHudTerminal : RichHudClient.ApiModule<SettingsMenuMembers>
        {
            /// <summary>
            /// Mod control root for the client.
            /// </summary>
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

            public static ControlMembers GetNewMenuControl(MenuControls controlEnum) =>
                Instance.GetNewControlFunc((int)controlEnum);

            public static ControlContainerMembers GetNewMenuTile() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Tile);

            public static ControlContainerMembers GetNewMenuCategory() =>
                Instance.GetNewContainerFunc((int)ControlContainers.Category);

            public static ControlMembers GetNewMenuPage(ModPages pageEnum) =>
                Instance.GetNewPageFunc((int)pageEnum);

            /// <summary>
            /// Indented dropdown list of terminal pages. Root UI element for all terminal controls
            /// associated with a given mod.
            /// </summary>
            private class ModControlRoot : IModControlRoot
            {
                /// <summary>
                /// Invoked when a new page is selected
                /// </summary>
                public event EventHandler OnSelectionChanged;

                /// <summary>
                /// Name of the mod as it appears in the <see cref="RichHudTerminal"/> mod list
                /// </summary>
                public string Name
                {
                    get { return GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Name) as string; }
                    set { GetOrSetMemberFunc(value, (int)ModControlRootAccessors.Name); }
                }

                /// <summary>
                /// Read only collection of <see cref="ITerminalPage"/>s assigned to this object.
                /// </summary>
                public IReadOnlyList<ITerminalPage> Pages { get; }

                public IModControlRoot PageContainer => this;

                /// <summary>
                /// Unique identifer
                /// </summary>
                public object ID => data.Item3;

                /// <summary>
                /// Currently selected <see cref="ITerminalPage"/>.
                /// </summary>
                public ITerminalPage Selection
                {
                    get { return new TerminalPage((ControlMembers)GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Selection)); }
                }

                /// <summary>
                /// Determines whether or not the element will appear in the list.
                /// Disabled by default.
                /// </summary>
                public bool Enabled
                {
                    get { return (bool)GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Enabled); }
                    set { GetOrSetMemberFunc(value, (int)ModControlRootAccessors.Enabled); }
                }

                private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
                private readonly ControlContainerMembers data;

                public ModControlRoot(ControlContainerMembers data)
                {
                    this.data = data;

                    var GetPageDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
                    Func<int, ITerminalPage> GetPageFunc = (x => new TerminalPage(GetPageDataFunc(x)));
                    Pages = new ReadOnlyApiCollection<ITerminalPage>(GetPageFunc, data.Item2.Item2);

                    GetOrSetMemberFunc(new Action(ModRootCallback), (int)ModControlRootAccessors.GetOrSetCallback);
                }

                /// <summary>
                /// Adds the given <see cref="TerminalPageBase"/> to the object.
                /// </summary>
                public void Add(TerminalPageBase page) =>
                    GetOrSetMemberFunc(page.ID, (int)ModControlRootAccessors.AddPage);

                /// <summary>
                /// Retrieves data used by the Framework API
                /// </summary>
                public ControlContainerMembers GetApiData() =>
                    data;

                IEnumerator<ITerminalPage> IEnumerable<ITerminalPage>.GetEnumerator() =>
                    Pages.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    Pages.GetEnumerator();

                protected void ModRootCallback()
                {
                    OnSelectionChanged?.Invoke(this, EventArgs.Empty);
                }

                private class TerminalPage : TerminalPageBase
                {
                    public TerminalPage(ControlMembers data) : base(data)
                    { }
                }
            }
        }
    }
}