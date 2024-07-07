using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using VRage.Input;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Wrapper used to provide easy access to Build Vision key binds.
    /// </summary>
    public sealed class BvBinds : BvComponentBase
    {
        public static BindsConfig Cfg
        {
            get
            {
                return new BindsConfig
                {
                    modifierGroup = ModifierGroup.GetBindDefinitions(),
                    mainGroup = MainGroup.GetBindDefinitions(),
                    secondaryGroup = SecondaryGroup.GetBindDefinitions(),
                    dupeGroup = DupeGroup.GetBindDefinitions()
                };
            }
            set
            {
                Instance.modifierGroup.TryLoadBindData(value.modifierGroup);
                Instance.mainGroup.TryLoadBindData(value.mainGroup);
                Instance.secondaryGroup.TryLoadBindData(value.secondaryGroup);
                Instance.dupeGroup.TryLoadBindData(value.dupeGroup);
                Instance.UpdateBindProperties();
            }
        }

        public static IBind Blueprint { get; private set; }
        public static IBind OpenBpList { get; private set; }

        public static IBind OpenWheel { get; private set; }
        public static IBind OpenList { get; private set; }

        public static IBind StartDupe { get; private set; }
        public static IBind StopDupe { get; private set; }
        public static IBind ToggleDupe { get; private set; }
        public static IBind SelectAll { get; private set; }
        public static IBind CopySelection { get; private set; }
        public static IBind PasteProperties { get; private set; }
        public static IBind UndoPaste { get; private set; }

        public static IBind Select { get; private set; }
        public static IBind Cancel { get; private set; }

        public static IBind ScrollUp { get; private set; }
        public static IBind ScrollDown { get; private set; }

        public static IBind MultXOrMouse { get; private set; }
        public static IBind MultY { get; private set; }
        public static IBind MultZ { get; private set; }

        public static IBindGroup ModifierGroup { get { return Instance.modifierGroup; } }
        public static IBindGroup MainGroup { get { return Instance.mainGroup; } }
        public static IBindGroup SecondaryGroup { get { return Instance.secondaryGroup; } }
        public static IBindGroup DupeGroup { get { return Instance.dupeGroup; } }

        private static BvBinds Instance
        {
            get
            {
                if (_instance == null)
                    Init();

                return _instance;
            }
            set { _instance = value; }
        }
        private static BvBinds _instance;
        private readonly IBindGroup staticGroup, modifierGroup, mainGroup, secondaryGroup, dupeGroup;

        private BvBinds() : base(false, true)
        {
            staticGroup = BindManager.GetOrCreateGroup("Static");
            staticGroup.RegisterBinds(new BindGroupInitializer() 
            {
                { "blueprint", MyKeys.Control, MyKeys.B },
                { "openBpList", MyKeys.F10 }
            });

            Blueprint = staticGroup["blueprint"];
            OpenBpList = staticGroup["openBpList"];

            modifierGroup = BindManager.GetOrCreateGroup("Modifiers");
            modifierGroup.RegisterBinds(new BindGroupInitializer
            {
                { "MultX/Mouse", MyKeys.Control },
                { "MultY", MyKeys.Shift },
                { "MultZ", MyKeys.Control, MyKeys.Shift },

            });

            mainGroup = BindManager.GetOrCreateGroup("Main");
            mainGroup.RegisterBinds(new BindGroupInitializer
            {
                { "OpenWheel", MyKeys.Control, RichHudControls.MousewheelUp },
                { "OpenList", MyKeys.Control, RichHudControls.MousewheelDown },
                { "LegacyClose" },

            });

            secondaryGroup = BindManager.GetOrCreateGroup("Secondary");
            secondaryGroup.RegisterBinds(new BindGroupInitializer
            {
                { "Select/Confirm", MyKeys.LeftButton },
                { "Cancel/Back", MyKeys.RightButton },
                { "ScrollUp", RichHudControls.MousewheelUp },
                { "ScrollDown", RichHudControls.MousewheelDown },
                { "LegacyOpen", MyKeys.Control, MyKeys.MiddleButton },

            });

            dupeGroup = BindManager.GetOrCreateGroup("Dupe");
            dupeGroup.RegisterBinds(new BindGroupInitializer
            {
                { "StartDupe", MyKeys.Control, MyKeys.Alt, RichHudControls.MousewheelUp },
                { "StopDupe", MyKeys.Control, MyKeys.Alt, RichHudControls.MousewheelDown },
                { "ToggleDupe", MyKeys.Home },
                { "SelectAll", MyKeys.Insert },
                { "CopySelection", MyKeys.PageUp },
                { "PasteProperties", MyKeys.PageDown },
                { "UndoPaste", MyKeys.Delete },
            });
        }

        public static void Init()
        {
            if (_instance == null)
            {
                _instance = new BvBinds();
                Cfg = BvConfig.Current.binds;

                BvConfig.OnConfigSave += _instance.UpdateConfig;
                BvConfig.OnConfigLoad += _instance.UpdateBinds;

                _instance.UpdateBindProperties();
            }
        }

        public override void Close()
        {
            BvConfig.OnConfigSave -= UpdateConfig;
            BvConfig.OnConfigLoad -= UpdateBinds;
            Instance = null;

            Blueprint = null;
            OpenBpList = null;

            MultXOrMouse = null;
            MultY = null;
            MultZ = null;

            OpenWheel = null;
            OpenList = null;

            StartDupe = null;
            StopDupe = null;
            ToggleDupe = null;
            SelectAll = null;
            CopySelection = null;
            PasteProperties = null;
            UndoPaste = null;

            Select = null;
            Cancel = null;
            ScrollUp = null;
            ScrollDown = null;
        }

        private void UpdateBindProperties()
        {
            MultXOrMouse = modifierGroup["MultX/Mouse"];
            MultY = modifierGroup["MultY"];
            MultZ = modifierGroup["MultZ"];

            OpenWheel = mainGroup["OpenWheel"];

            if (BvConfig.Current.genUI.legacyModeEnabled)
            {
                OpenList = secondaryGroup["LegacyOpen"];
                Cancel = mainGroup["LegacyClose"];
            }
            else
            {
                OpenList = mainGroup["OpenList"];
                Cancel = secondaryGroup["Cancel/Back"];
            }

            Select = secondaryGroup["Select/Confirm"];
            ScrollUp = secondaryGroup["ScrollUp"];
            ScrollDown = secondaryGroup["ScrollDown"];

            StartDupe = dupeGroup["StartDupe"];
            StopDupe = dupeGroup["StopDupe"];
            ToggleDupe = dupeGroup["ToggleDupe"];
            SelectAll = dupeGroup["SelectAll"];
            CopySelection = dupeGroup["CopySelection"];
            PasteProperties = dupeGroup["PasteProperties"];
            UndoPaste = dupeGroup["UndoPaste"];
        }

        private void UpdateConfig()
        {
            BvConfig.Current.binds = Cfg;
        }

        private void UpdateBinds()
        {
            Cfg = BvConfig.Current.binds;
        }
    }
}