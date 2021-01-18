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

        public static IBind Peek { get { return Instance.openGroup[0]; } }
        public static IBind Open { get { return Instance.openGroup[1]; } }

        public static IBind Hide { get { return Instance.mainGroup[0]; } }
        public static IBind Select { get { return Instance.mainGroup[1]; } }
        public static IBind ScrollUp { get { return Instance.mainGroup[2]; } }
        public static IBind ScrollDown { get { return Instance.mainGroup[3]; } }

        public static IBind MultX { get { return Instance.mainGroup[4]; } }
        public static IBind MultY { get { return Instance.mainGroup[5]; } }
        public static IBind MultZ { get { return Instance.mainGroup[6]; } }

        public static IBind ToggleSelectMode { get { return Instance.mainGroup[7]; } }
        public static IBind SelectAll { get { return Instance.mainGroup[8]; } }
        public static IBind CopySelection { get { return Instance.mainGroup[9]; } }
        public static IBind PasteProperties { get { return Instance.mainGroup[10]; } }
        public static IBind UndoPaste { get { return Instance.mainGroup[11]; } }

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

        private static void Init()
        {
            if (_instance == null)
            {
                _instance = new BvBinds();
                Cfg = BvConfig.Current.binds;

                BvConfig.OnConfigSave += _instance.UpdateConfig;
                BvConfig.OnConfigLoad += _instance.UpdateBinds;
            }
        }

        public override void Close()
        {
            BvConfig.OnConfigSave -= UpdateConfig;
            BvConfig.OnConfigLoad -= UpdateBinds;
            Instance = null;
        }

        private void UpdateConfig() =>
            BvConfig.Current.binds = Cfg;

        private void UpdateBinds() =>
            Cfg = BvConfig.Current.binds;
    }
}