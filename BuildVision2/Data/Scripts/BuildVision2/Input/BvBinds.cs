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
                    modifierGroup = MainGroup.GetBindDefinitions(),
                    mainGroup = SecondaryGroup.GetBindDefinitions(),
                };
            }
            set
            {
                Instance.mainGroup.TryLoadBindData(value.modifierGroup);
                Instance.secondaryGroup.TryLoadBindData(value.mainGroup);
            }
        }

        public static IBind EnableMouse { get; private set; }
        public static IBind OpenWheel { get; private set; }
        public static IBind StartDupe { get; private set; }
        public static IBind OpenList { get; private set; }

        public static IBind Select { get; private set; }
        public static IBind Cancel { get; private set; }

        public static IBind ScrollUp { get; private set; }
        public static IBind ScrollDown { get; private set; }

        public static IBind MultX { get; private set; }
        public static IBind MultY { get; private set; }
        public static IBind MultZ { get; private set; }

        public static IBindGroup MainGroup { get { return Instance.mainGroup; } }
        public static IBindGroup SecondaryGroup { get { return Instance.secondaryGroup; } }

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
        private readonly IBindGroup mainGroup, secondaryGroup;

        private BvBinds() : base(false, true)
        {
            mainGroup = BindManager.GetOrCreateGroup("BvMain");
            mainGroup.RegisterBinds(BindsConfig.DefaultMain);

            secondaryGroup = BindManager.GetOrCreateGroup("BvSecondary");
            secondaryGroup.RegisterBinds(BindsConfig.DefaultSecondary);
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
            MultX = mainGroup["MultX"];
            MultY = mainGroup["MultY"];
            MultZ = mainGroup["MultZ"];
            OpenWheel = mainGroup["OpenWheel"];
            StartDupe = mainGroup["StartDupe"];
            OpenList = mainGroup["OpenList"];

            EnableMouse = secondaryGroup["EnableMouse"];
            Select = secondaryGroup["Select"];
            Cancel = secondaryGroup["Cancel"];
            ScrollUp = secondaryGroup["ScrollUp"];
            ScrollDown = secondaryGroup["ScrollDown"];
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