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
                    secondaryGroup = SecondaryGroup.GetBindDefinitions()
                };
            }
            set
            {
                Instance.modifierGroup.TryLoadBindData(value.modifierGroup);
                Instance.mainGroup.TryLoadBindData(value.mainGroup);
                Instance.secondaryGroup.TryLoadBindData(value.secondaryGroup);
            }
        }

        public static IBind EnableMouse { get; private set; }
        public static IBind OpenWheel { get; private set; }
        public static IBind OpenList { get; private set; }

        public static IBind StartDupe { get; private set; }
        public static IBind StopDupe { get; private set; }

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
        private readonly IBindGroup modifierGroup, mainGroup, secondaryGroup;

        private BvBinds() : base(false, true)
        {
            modifierGroup = BindManager.GetOrCreateGroup("Modifiers");
            modifierGroup.RegisterBinds(BindsConfig.DefaultModifiers);

            mainGroup = BindManager.GetOrCreateGroup("Main");
            mainGroup.RegisterBinds(BindsConfig.DefaultMain);

            secondaryGroup = BindManager.GetOrCreateGroup("Secondary");
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
            MultX = modifierGroup["MultX/Mouse"];
            MultY = modifierGroup["MultY"];
            MultZ = modifierGroup["MultZ"];
            EnableMouse = MultX;

            OpenWheel = mainGroup["OpenWheel"];
            OpenList = mainGroup["OpenList"];
            StartDupe = mainGroup["StartDupe"];
            StopDupe = mainGroup["StopDupe"];

            Select = secondaryGroup["Select/Confirm"];
            Cancel = secondaryGroup["Cancel/Back"];
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