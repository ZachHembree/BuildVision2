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

            /// <summary>
            /// Determines whether or not the terminal is currently open.
            /// </summary>
            public static bool Open => (bool)Instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.GetMenuOpen);

            private static RichHudTerminal Instance
            {
                get { Init(); return _instance; }
                set { _instance = value; }
            }
            private static RichHudTerminal _instance;

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
                if (_instance == null)
                {
                    _instance = new RichHudTerminal();
                }
            }

            /// <summary>
            /// Toggles the menu between open and closed
            /// </summary>
            public static void ToggleMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.ToggleMenu);
            }

            /// <summary>
            /// Open the menu if chat is visible
            /// </summary>
            public static void OpenMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.OpenMenu);
            }

            /// <summary>
            /// Close the menu
            /// </summary>
            public static void CloseMenu()
            {
                if (_instance == null)
                    Init();

                _instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.CloseMenu);
            }

            /// <summary>
            /// Sets the current page to the one given
            /// </summary>
            public static void OpenToPage(TerminalPageBase newPage)
            {
                _instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.OpenToPage);
            }

            /// <summary>
            /// Sets the current page to the one given
            /// </summary>
            public static void SetPage(TerminalPageBase newPage)
            {
                _instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.SetPage);
            }

            public override void Close()
            {
                _instance = null;
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
                    get 
                    {
                        object id = GetOrSetMemberFunc(null, (int)ModControlRootAccessors.Selection);

                        if (id != null)
                        {
                            for (int n = 0; n < Pages.Count; n++)
                            {
                                if (id == Pages[n].ID)
                                    return Pages[n];
                            }
                        }

                        return null;
                    }
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
                /// Adds the given ranges of pages to the control root.
                /// </summary>
                public void AddRange(IReadOnlyList<TerminalPageBase> pages)
                {
                    var idList = new object[pages.Count];

                    for (int n = 0; n < pages.Count; n++)
                        idList[n] = pages[n].ID;

                    GetOrSetMemberFunc(idList, (int)ModControlRootAccessors.AddRange);
                }

                /// <summary>
                /// Retrieves data used by the Framework API
                /// </summary>
                public ControlContainerMembers GetApiData() =>
                    data;

                protected void ModRootCallback()
                {
                    OnSelectionChanged?.Invoke(this, EventArgs.Empty);
                }

                IEnumerator<ITerminalPage> IEnumerable<ITerminalPage>.GetEnumerator() =>
                    Pages.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    Pages.GetEnumerator();

                private class TerminalPage : TerminalPageBase
                {
                    public TerminalPage(ControlMembers data) : base(data)
                    { }
                }
            }
        }
    }
}