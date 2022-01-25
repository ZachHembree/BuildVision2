using RichHudFramework.Internal;
using VRage.Input;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Wrapper used to provide easy access to library key binds.
    /// </summary>
    public sealed class SharedBinds : RichHudComponentBase
    {
        public static IBind LeftButton { get { return Instance.sharedBinds[0]; } }
        public static IBind RightButton { get { return Instance.sharedBinds[1]; } }
        public static IBind MousewheelUp { get { return Instance.sharedBinds[2]; } }
        public static IBind MousewheelDown { get { return Instance.sharedBinds[3]; } }

        public static IBind Enter { get { return Instance.sharedBinds[4]; } }
        public static IBind Back { get { return Instance.sharedBinds[5]; } }
        public static IBind Delete { get { return Instance.sharedBinds[6]; } }
        public static IBind Escape { get { return Instance.sharedBinds[7]; } }

        public static IBind SelectAll { get { return Instance.sharedBinds[8]; } }
        public static IBind Copy { get { return Instance.sharedBinds[9]; } }
        public static IBind Cut { get { return Instance.sharedBinds[10]; } }
        public static IBind Paste { get { return Instance.sharedBinds[11]; } }

        public static IBind UpArrow { get { return Instance.sharedBinds[12]; } }
        public static IBind DownArrow { get { return Instance.sharedBinds[13]; } }
        public static IBind LeftArrow { get { return Instance.sharedBinds[14]; } }
        public static IBind RightArrow { get { return Instance.sharedBinds[15]; } }

        public static IBind PageUp { get { return Instance.sharedBinds[16]; } }
        public static IBind PageDown { get { return Instance.sharedBinds[17]; } }
        public static IBind Shift { get { return Instance.sharedBinds[18]; } }
        public static IBind Space { get { return Instance.sharedBinds[19]; } }
        public static IBind Control { get { return Instance.sharedBinds[20]; } }

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
            sharedBinds.RegisterBinds(new BindGroupInitializer
            {
                { "leftbutton", MyKeys.LeftButton },
                { "rightbutton", MyKeys.RightButton },
                { "mousewheelup", RichHudControls.MousewheelUp },
                { "mousewheeldown", RichHudControls.MousewheelDown },

                { "enter", MyKeys.Enter },
                { "back", MyKeys.Back },
                { "delete", MyKeys.Delete },
                { "escape", MyKeys.Escape },

                { "selectall", MyKeys.Control, MyKeys.A },
                { "copy", MyKeys.Control, MyKeys.C },
                { "cut", MyKeys.Control, MyKeys.X },
                { "paste", MyKeys.Control, MyKeys.V },

                { "uparrow", MyKeys.Up },
                { "downarrow", MyKeys.Down },
                { "leftarrow", MyKeys.Left },
                { "rightarrow", MyKeys.Right },
                
                { "pageup", MyKeys.PageUp },
                { "pagedown", MyKeys.PageDown },
                { "shift", MyKeys.Shift },
                { "space", MyKeys.Space },
                { "control", MyKeys.Control },
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