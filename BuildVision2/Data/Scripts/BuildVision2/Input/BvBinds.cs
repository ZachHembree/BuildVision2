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

        public static IBind EnableMouse { get; private set; }
        public static IBind OpenWheel { get; private set; }
        public static IBind OpenList { get; private set; }
        public static IBind LegacyClose { get; private set; }

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

        public static IBind MultX { get; private set; }
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
                { "blueprint", MyKeys.Control, MyKeys.B }
            });

            Blueprint = staticGroup["blueprint"];

            modifierGroup = BindManager.GetOrCreateGroup("Modifiers");
            modifierGroup.RegisterBinds(BindsConfig.DefaultModifiers);

            mainGroup = BindManager.GetOrCreateGroup("Main");
            mainGroup.RegisterBinds(BindsConfig.DefaultMain);

            secondaryGroup = BindManager.GetOrCreateGroup("Secondary");
            secondaryGroup.RegisterBinds(BindsConfig.DefaultSecondary);

            dupeGroup = BindManager.GetOrCreateGroup("Dupe");
            dupeGroup.RegisterBinds(BindsConfig.DefaultDupe);
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
            MultX = null;
            MultY = null;
            MultZ = null;
            EnableMouse = null;

            OpenWheel = null;
            OpenList = null;
            LegacyClose = null;

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
            MultX = modifierGroup["MultX/Mouse"];
            MultY = modifierGroup["MultY"];
            MultZ = modifierGroup["MultZ"];
            EnableMouse = MultX;

            OpenWheel = mainGroup["OpenWheel"];

            if (BvConfig.Current.genUI.legacyModeEnabled)
            {
                OpenList = secondaryGroup["LegacyOpen"];
                LegacyClose = mainGroup["LegacyClose"];
            }
            else
            {
                OpenList = mainGroup["OpenList"];
                LegacyClose = null;
            }

            Select = secondaryGroup["Select/Confirm"];
            Cancel = secondaryGroup["Cancel/Back"];
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