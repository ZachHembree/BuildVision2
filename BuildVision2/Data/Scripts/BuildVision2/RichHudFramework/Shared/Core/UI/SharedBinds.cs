using RichHudFramework.Game;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Wrapper used to provide easy access to library key binds.
    /// </summary>
    internal sealed class SharedBinds : ModBase.ComponentBase
    {
        public static IBind LeftButton { get { return BindGroup[0]; } }
        public static IBind RightButton { get { return BindGroup[1]; } }
        public static IBind MousewheelUp { get { return BindGroup[2]; } }
        public static IBind MousewheelDown { get { return BindGroup[3]; } }

        public static IBind Enter { get { return BindGroup[4]; } }
        public static IBind Back { get { return BindGroup[5]; } }
        public static IBind Delete { get { return BindGroup[6]; } }
        public static IBind Escape { get { return BindGroup[7]; } }

        public static IBind SelectAll { get { return BindGroup[8]; } }
        public static IBind Copy { get { return BindGroup[9]; } }
        public static IBind Cut { get { return BindGroup[10]; } }
        public static IBind Paste { get { return BindGroup[11]; } }

        public static IBind UpArrow { get { return BindGroup[12]; } }
        public static IBind DownArrow { get { return BindGroup[13]; } }
        public static IBind LeftArrow { get { return BindGroup[14]; } }
        public static IBind RightArrow { get { return BindGroup[15]; } }

        public static IBind Shift { get { return BindGroup[16]; } }
        public static IBind Tilde { get { return BindGroup[17]; } }

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
                new BindDefinition("mousewheelup", new string[] { "mousewheelup" }),
                new BindDefinition("mousewheeldown", new string[] { "mousewheeldown" }),

                new BindDefinition("enter", new string[] { "enter" }),
                new BindDefinition("back", new string[] { "back" }),
                new BindDefinition("delete", new string[] { "delete" }),
                new BindDefinition("escape", new string[] { "escape" }),

                new BindDefinition("selectall", new string[] { "control", "a" }),
                new BindDefinition("copy", new string[] { "control", "c" }),
                new BindDefinition("cut", new string[] { "control", "x" }),
                new BindDefinition("paste", new string[] { "control", "v" }),
                    
                new BindDefinition("uparrow", new string[] { "up" }),
                new BindDefinition("downarrow", new string[] { "down" }),
                new BindDefinition("leftarrow", new string[] { "left" }),
                new BindDefinition("rightarrow", new string[] { "right" }),

                new BindDefinition("shift", new string[] { "shift" }),
                new BindDefinition("tilde", new string[] { "oemtilde" }),
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