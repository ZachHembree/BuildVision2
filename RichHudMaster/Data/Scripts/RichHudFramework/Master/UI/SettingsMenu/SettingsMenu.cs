using RichHudFramework.Game;
using RichHudFramework.UI.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using Sandbox.ModAPI;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ControlMembers = VRage.MyTuple<
    System.Func<object, int, object>, // GetOrSetMember
    object // ID
>;

namespace RichHudFramework
{
    using UI.Server;
    using ControlContainerMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember,
        MyTuple<object, Func<int>>, // Member List
        object // ID
    >;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI.Server
    {
        using SettingsMenuMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMembers
            ControlContainerMembers, // MenuRoot
            Func<int, ControlMembers>, // GetNewControl
            Func<int, ControlContainerMembers> // GetNewContainer
        >;

        public sealed partial class ModMenu : ModBase.ComponentBase
        {
            public static readonly GlyphFormat HeaderText = new GlyphFormat(Color.White, TextAlignment.Center, 1.15f);
            public static readonly GlyphFormat ControlText = GlyphFormat.Blueish.WithSize(1.08f);

            private static ModMenu Instance
            {
                get { Init(); return instance; }
                set { instance = value; }
            }
            private static ModMenu instance;
            private readonly SettingsMenu settingsMenu;

            private ModMenu() : base(false, true)
            {
                settingsMenu = new SettingsMenu(HudMain.Root);
            }

            public static void Init()
            {
                if (instance == null)
                {
                    instance = new ModMenu();
                }
            }

            public static MyTuple<object, IHudElement> GetClientData(string clientName)
            {
                ModControlRoot newRoot = Instance.settingsMenu.AddModRoot(clientName);

                var data = new SettingsMenuMembers()
                {
                    Item1 = GetOrSetMembers,
                    Item2 = newRoot.GetApiData(),
                    Item3 = GetNewTerminalControl,
                    Item4 = GetNewControlContainer
                };

                return new MyTuple<object, IHudElement>(data, newRoot);
            }

            private static object GetOrSetMembers(object data, int memberEnum)
            {
                return null;
            }

            private static ControlMembers GetNewTerminalControl(int controlEnum)
            {
                var control = (MenuControls)controlEnum;

                switch (control)
                {
                    case MenuControls.Checkbox:
                        return new Checkbox().GetApiData();
                    case MenuControls.ColorPicker:
                        return new ColorPicker().GetApiData();
                    case MenuControls.DropdownControl:
                        return new DropdownControl<object>().GetApiData();
                    case MenuControls.ListControl:
                        return new ListControl<object>().GetApiData();
                    case MenuControls.OnOffButton:
                        return new OnOffButton().GetApiData();
                    case MenuControls.SliderSetting:
                        return new SliderSetting().GetApiData();
                    case MenuControls.TerminalButton:
                        return new TerminalButton().GetApiData();
                    case MenuControls.TextField:
                        return new TextField().GetApiData();
                }

                return default(ControlMembers);
            }

            private static ControlContainerMembers GetNewControlContainer(int containerEnum)
            {
                var container = (ControlContainers)containerEnum;

                switch (container)
                {
                    case ControlContainers.Tile:
                        return new ControlTile().GetApiData();
                    case ControlContainers.Category:
                        return new ControlCategory().GetApiData();
                    case ControlContainers.Page:
                        return new ControlPage().GetApiData();
                }

                return default(ControlContainerMembers);
            }

            private class SettingsMenu : WindowBase
            {
                public event Action OnSelectionChanged;

                public override Color BorderColor
                {
                    set
                    {
                        base.BorderColor = value;
                        topDivider.Color = value;
                        bottomDivider.Color = value;
                        middleDivider.Color = value;
                    }
                }

                public ModControlRoot Selection { get; private set; }

                public ControlPage CurrentPage => Selection?.SelectedElement;

                public override bool Visible => base.Visible && MyAPIGateway.Gui.ChatEntryVisible;

                public override float Scale => HudMain.ResScale;

                private readonly ModList modList;
                private readonly HudChain<HudElementBase> chain;
                private readonly TexturedBox topDivider, middleDivider, bottomDivider;
                private readonly Button closeButton;
                private readonly List<ControlPage> pages;
                private static readonly Material closeButtonMat = new Material("RichHudCloseButton", new Vector2(32f));

                public SettingsMenu(IHudParent parent = null) : base(parent)
                {
                    pages = new List<ControlPage>();

                    Title.Format = HeaderText;
                    Title.SetText("Rich HUD Terminal");

                    header.Height = 60f;
                    header.Background.Visible = false;

                    topDivider = new TexturedBox(header)
                    {
                        ParentAlignment = ParentAlignments.Bottom,
                        DimAlignment = DimAlignments.Width,
                        Padding = new Vector2(80f, 0f),
                        Height = 1f,
                    };

                    modList = new ModList();

                    middleDivider = new TexturedBox()
                    {
                        Padding = new Vector2(24f, 0f),
                        Width = 26f,
                    };

                    chain = new HudChain<HudElementBase>(topDivider)
                    {
                        AutoResize = true,
                        AlignVertical = false,
                        Spacing = 12f,
                        Padding = new Vector2(80f, 40f),
                        ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Left | ParentAlignments.InnerH,
                        ChildContainer = { modList, middleDivider },
                    };

                    bottomDivider = new TexturedBox(this)
                    {
                        ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                        DimAlignment = DimAlignments.Width,
                        Offset = new Vector2(0f, 40f),
                        Padding = new Vector2(80f, 0f),
                        Height = 1f,
                    };

                    closeButton = new Button(header)
                    {
                        Material = closeButtonMat,
                        highlightEnabled = false,
                        Size = new Vector2(30f),
                        Offset = new Vector2(-18f, -14f),
                        Color = new Color(173, 182, 189),
                        ParentAlignment = ParentAlignments.Top | ParentAlignments.Right | ParentAlignments.Inner
                    };

                    closeButton.MouseInput.OnLeftClick += CloseMenu;
                    SharedBinds.Escape.OnNewPress += CloseMenu;
                    SharedBinds.Tilde.OnNewPress += ToggleMenu;

                    BodyColor = new Color(37, 46, 53);
                    BorderColor = new Color(84, 98, 107);

                    Padding = new Vector2(80f, 40f);
                    MinimumSize = new Vector2(700f, 500f);

                    modList.Width = 200f;
                    Size = new Vector2(1320, 850f);
                    Offset = new Vector2(252f, 103f);
                }

                public ModControlRoot AddModRoot(string clientName)
                {
                    ModControlRoot modSettings = new ModControlRoot(this) { Name = clientName };

                    modList.AddToList(modSettings);
                    modSettings.OnModUpdate += UpdateSelection;

                    return modSettings;
                }

                public void AddPage(ControlPage page)
                {
                    pages.Add(page);
                    chain.Add(page);
                }

                public void ToggleMenu()
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                        Visible = !Visible;
                }

                public void CloseMenu()
                {
                    Visible = false;
                }

                protected override void Draw()
                {
                    base.Draw();

                    chain.Height = Height - header.Height - topDivider.Height - Padding.Y - bottomDivider.Height;
                    modList.Width = 200f; // temporary

                    if (CurrentPage != null)
                        CurrentPage.Width = Width - Padding.X - modList.Width - chain.Spacing;

                    BodyColor = BodyColor.SetAlpha((byte)(HudMain.UiBkOpacity * 255f));
                }

                private void UpdateSelection(ModControlRoot selection)
                {
                    Selection = selection;
                    UpdateSelectionVisibilty();
                    OnSelectionChanged?.Invoke();
                }

                private void UpdateSelectionVisibilty()
                {
                    for (int n = 0; n < pages.Count; n++)
                        pages[n].Visible = false;

                    if (CurrentPage != null)
                        CurrentPage.Visible = true;
                }

                private class ModList : HudElementBase
                {
                    public override float Width
                    {
                        get { return list.Width; }
                        set
                        {
                            header.Width = value;

                            for (int n = 0; n < list.List.Count; n++)
                                list.List[n].Width = value;
                        }
                    }

                    public override float Height
                    {
                        get { return list.Height + header.Height; }
                        set { list.Height = value - header.Height; }
                    }

                    private readonly LabelBox header;
                    private readonly ScrollBox<ModControlRoot> list;

                    public ModList(IHudParent parent = null) : base(parent)
                    {
                        list = new ScrollBox<ModControlRoot>(this)
                        {
                            AlignVertical = true,
                            FitToChain = false,
                            Color = new Color(41, 54, 62, 230),
                            ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                        };

                        header = new LabelBox(list)
                        {
                            AutoResize = false,
                            Format = ControlText,
                            Text = "Mod List:",
                            Color = new Color(32, 39, 45, 230),
                            TextPadding = new Vector2(30f, 0f),
                            Size = new Vector2(200f, 36f),
                            ParentAlignment = ParentAlignments.Top,
                        };

                        var listBorder = new BorderBox(this)
                        {
                            Color = new Color(53, 66, 75),
                            Thickness = 2f,
                            DimAlignment = DimAlignments.Both,
                        };
                    }

                    public void AddToList(ModControlRoot modSettings)
                    {
                        list.AddToList(modSettings);
                    }

                    protected override void Draw()
                    {
                        header.Width = list.Width;
                    }
                }
            }
        }
    }
}