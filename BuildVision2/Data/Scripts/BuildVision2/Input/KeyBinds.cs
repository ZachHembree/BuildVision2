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
                };
            }
            set
            {
                Instance.modifierGroup.TryLoadBindData(value.modifierGroup);
                Instance.mainGroup.TryLoadBindData(value.mainGroup);
            }
        }

        public static IBind EnableMouse { get; private set; }
        public static IBind Open { get; private set; }

        public static IBind Hide { get; private set; }
        public static IBind Select { get; private set; }
        public static IBind Cancel { get; private set; }

        public static IBind ScrollUp { get; private set; }
        public static IBind ScrollDown { get; private set; }

        public static IBind MultX { get; private set; }
        public static IBind MultY { get; private set; }
        public static IBind MultZ { get; private set; }

        // REMOVE THESE
        public static IBind ToggleSelectMode { get; private set; }
        public static IBind SelectAll { get; private set; }
        public static IBind CopySelection { get; private set; }
        public static IBind PasteProperties { get; private set; }
        public static IBind UndoPaste { get; private set; }

        public static IBindGroup ModifierGroup { get { return Instance.modifierGroup; } }
        public static IBindGroup MainGroup { get { return Instance.mainGroup; } }

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
        private readonly IBindGroup modifierGroup, mainGroup;

        private BvBinds() : base(false, true)
        {
            modifierGroup = BindManager.GetOrCreateGroup("BvModifiers");
            modifierGroup.RegisterBinds(BindsConfig.DefaultModifiers);

            mainGroup = BindManager.GetOrCreateGroup("BvMain");
            mainGroup.RegisterBinds(BindsConfig.DefaultMain);
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
        }

        private void UpdateBindProperties()
        {
            EnableMouse = modifierGroup["EnableMouse"];
            MultX = modifierGroup["MultX"];
            MultY = modifierGroup["MultY"];
            MultZ = modifierGroup["MultZ"];

            Open = mainGroup["Open"];
            Hide = mainGroup["Hide"];
            Select = mainGroup["Select"];
            Cancel = mainGroup["Cancel"];
            ScrollUp = mainGroup["ScrollUp"];
            ScrollDown = mainGroup["ScrollDown"];
        }

        private void UpdateConfig() =>
            BvConfig.Current.binds = Cfg;

        private void UpdateBinds()
        {
            Cfg = BvConfig.Current.binds;
            _instance.UpdateBindProperties();
        }
    }
}