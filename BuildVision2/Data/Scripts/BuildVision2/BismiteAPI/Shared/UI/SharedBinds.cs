using DarkHelmet.Game;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Wrapper used to provide easy access to library key binds.
    /// </summary>
    internal sealed class SharedBinds : ModBase.ComponentBase
    {
        public static IBind LeftButton { get { return BindGroup[0]; } }
        public static IBind RightButton { get { return BindGroup[1]; } }
        public static IBind Enter { get { return BindGroup[2]; } }
        public static IBind Back { get { return BindGroup[3]; } }
        public static IBind Delete { get { return BindGroup[4]; } }
        public static IBind Escape { get { return BindGroup[5]; } }

        public static IBindGroup BindGroup { get { return Instance.sharedBinds; } }
        private static SharedBinds Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static SharedBinds instance;
        private readonly IBindGroup sharedBinds;

        private SharedBinds() : base(false, true)
        {
            sharedBinds = BindManager.GetOrCreateGroup("SharedBinds");
            sharedBinds.RegisterBinds(new BindDefinition[]
            {
                new BindDefinition("leftbutton", new string[] { "leftbutton" }),
                new BindDefinition("rightbutton", new string[] { "rightbutton" }),
                new BindDefinition("enter", new string[] { "enter" }),
                new BindDefinition("back", new string[] { "back" }),
                new BindDefinition("delete", new string[] { "delete" }),
                new BindDefinition("escape", new string[] { "escape" }),
            });
        }

        private static void Init()
        {
            if (instance == null)
                instance = new SharedBinds();
        }

        public override void Close()
        {
            Instance = null;
        }
    }
}