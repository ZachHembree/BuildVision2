using DarkHelmet.Game;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Wrapper used to provide easy access to library key binds.
    /// </summary>
    internal sealed class SharedBinds : ModBase.ComponentBase
    {
        public static IBind LeftButton { get { return Instance.sharedBinds[0]; } }
        public static IBind RightButton { get { return Instance.sharedBinds[1]; } }
        public static IBind Enter { get { return Instance.sharedBinds[2]; } }
        public static IBind Back { get { return Instance.sharedBinds[3]; } }
        public static IBind Delete { get { return Instance.sharedBinds[4]; } }
        public static IBind Escape { get { return Instance.sharedBinds[5]; } }

        public static BindManager.Group BindGroup { get { return Instance.sharedBinds; } }
        private static SharedBinds Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static SharedBinds instance;

        private readonly BindManager.Group sharedBinds;

        private SharedBinds()
        {
            sharedBinds = new BindManager.Group("HudMain");
            sharedBinds.RegisterBinds(new BindData[]
            {
                new BindData("leftbutton", new string[] { "leftbutton" }),
                new BindData("rightbutton", new string[] { "rightbutton" }),
                new BindData("enter", new string[] { "enter" }),
                new BindData("back", new string[] { "back" }),
                new BindData("delete", new string[] { "delete" }),
                new BindData("escape", new string[] { "escape" }),
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