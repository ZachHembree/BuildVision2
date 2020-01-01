using RichHudFramework.Game;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Wrapper used to provide easy access to Build Vision key binds.
    /// </summary>
    internal sealed class BvBinds : ModBase.ComponentBase
    {
        public static BindsConfig Cfg
        {
            get { return new BindsConfig { bindData = BindGroup.GetBindDefinitions() }; }
            set { Instance.bindGroup.TryLoadBindData(value.bindData); }
        }

        public static IBind Open { get { return BindGroup[0]; } }
        public static IBind Hide { get { return BindGroup[1]; } }
        public static IBind Select { get { return BindGroup[2]; } }
        public static IBind ScrollUp { get { return BindGroup[3]; } }
        public static IBind ScrollDown { get { return BindGroup[4]; } }
        public static IBind MultX { get { return BindGroup[5]; } }
        public static IBind MultY { get { return BindGroup[6]; } }
        public static IBind MultZ { get { return BindGroup[7]; } }
        public static IBindGroup BindGroup { get { return Instance.bindGroup; } }

        private static BvBinds Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static BvBinds instance;
        private static readonly string[] bindNames = new string[] { "Open", "Close", "Select", "ScrollUp", "ScrollDown", "MultX", "MultY", "MultZ" };

        private readonly IBindGroup bindGroup;

        private BvBinds() : base(false, true)
        {
            bindGroup = BindManager.GetOrCreateGroup("BuildVision");
            bindGroup.RegisterBinds(bindNames);
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new BvBinds();
                Cfg = BvConfig.Current.binds;

                BvConfig.OnConfigSave += instance.UpdateConfig;
                BvConfig.OnConfigLoad += instance.UpdateBinds;
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