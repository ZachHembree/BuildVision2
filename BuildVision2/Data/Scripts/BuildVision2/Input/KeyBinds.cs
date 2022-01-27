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
                    openGroup = OpenGroup.GetBindDefinitions(),
                    mainGroup = MainGroup.GetBindDefinitions(),
                };
            }
            set
            {
                Instance.openGroup.TryLoadBindData(value.openGroup);
                Instance.mainGroup.TryLoadBindData(value.mainGroup);
            }
        }

        public static IBind EnableMouse { get; private set; }
        public static IBind Open { get; private set; }

        public static IBind Hide { get; private set; }
        public static IBind Select { get; private set; }
        public static IBind Confirm { get; private set; }

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

        public static IBindGroup OpenGroup { get { return Instance.openGroup; } }
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
        private readonly IBindGroup openGroup, mainGroup;

        private BvBinds() : base(false, true)
        {
            openGroup = BindManager.GetOrCreateGroup("BvOpen");
            openGroup.RegisterBinds(BindsConfig.DefaultOpen);

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
            EnableMouse = openGroup["EnableMouse"];
            Open = openGroup["Open"];

            Hide = mainGroup["Hide"];
            Select = mainGroup["Select"];
            Confirm = mainGroup["Confirm"];
            ScrollUp = mainGroup["ScrollUp"];
            ScrollDown = mainGroup["ScrollDown"];
            MultX = mainGroup["MultX"];
            MultY = mainGroup["MultY"];
            MultZ = mainGroup["MultZ"];
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