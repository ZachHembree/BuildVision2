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

        public static IBind PageUp { get { return BindGroup[16]; } }
        public static IBind PageDown { get { return BindGroup[17]; } }
        public static IBind Shift { get { return BindGroup[18]; } }
        public static IBind Space { get { return BindGroup[19]; } }

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