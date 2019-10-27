using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed partial class PropertiesMenu : ModBase.ComponentBase
    {
        public static PropMenuConfig Cfg { get { return BvConfig.Current.menu; } set { BvConfig.Current.menu = value; } }
        public static ApiHudConfig ApiHudCfg { get { return Cfg.apiHudConfig; } set { Cfg.apiHudConfig = value; } }
        public static NotifHudConfig NotifHudCfg { get { return Cfg.fallbackHudConfig; } set { Cfg.fallbackHudConfig = value; } }
        public static PropertyBlock Target
        {
            get { return Instance.target; }
            set
            {
                if (Instance.target != null)
                {
                    Instance.Deselect();
                    Instance.ResetIndex();
                }

                Instance.target = value;
            }
        }
        private PropertyBlock target;

        public static bool Open { get { return Instance.open; } set { Instance.open = value; } }
        private bool open;

        private static PropertiesMenu Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static PropertiesMenu instance;

        private readonly ApiHud apiHud;
        private readonly NotifHud fallbackHud;
        private readonly Utils.Stopwatch listWrapTimer;
        private int index, selection;

        private PropertiesMenu() : base(false, true)
        {
            apiHud = new ApiHud();
            fallbackHud = new NotifHud();
            target = null;

            index = 0;
            selection = -1;
            open = false;
            listWrapTimer = new Utils.Stopwatch();

            MyAPIGateway.Utilities.MessageEntered += MessageHandler;

            KeyBinds.Select.OnNewPress += Select;
            SharedBinds.Enter.OnNewPress += ToggleTextBox;

            KeyBinds.ScrollUp.OnPressAndHold += ScrollUp;
            KeyBinds.ScrollDown.OnPressAndHold += ScrollDown;
        }

        private static void Init()
        {
            if (instance == null)
                instance = new PropertiesMenu();
        }

        public override void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

        private void MessageHandler(string message, ref bool sendToOthers)
        {
            if (open)
                sendToOthers = false;
        }

        private void ScrollDown()
        {
            if (open)
                UpdateIndex(1);
        }

        private void ScrollUp()
        {
            if (open)
                UpdateIndex(-1);
        }

        /// <summary>
        /// Updates scrollable index, range of visible scrollables and input for selected property.
        /// </summary>
        private void UpdateIndex(int scrolllDir)
        {
            if (selection == -1)
            {
                int newIndex = index, max = target.BlockMembers.Count - 1;
                bool scrollDown = scrolllDir > 0;

                for (int n = 0; n < target.BlockMembers.Count; n++)
                {
                    newIndex += scrolllDir;

                    if (listWrapTimer.ElapsedMilliseconds > 300)
                    {
                        if (newIndex < 0)
                            newIndex = max;
                        else if (newIndex >= target.BlockMembers.Count)
                            newIndex = 0;
                    }
                    else
                        newIndex = Utils.Math.Clamp(newIndex, 0, max);

                    if (target.BlockMembers[newIndex].Enabled)
                    {
                        index = newIndex;
                        break;
                    }
                }

                listWrapTimer.Start();
            }
        }

        private void ToggleTextBox()
        {
            if (open && target.BlockMembers[index].InputType.HasFlag(BlockInputType.Text))
            {
                if (selection == -1 && !MyAPIGateway.Gui.ChatEntryVisible)
                    Select();
                else if (MyAPIGateway.Gui.ChatEntryVisible)
                    Deselect();
            }
        }

        private void Select()
        {
            if (open)
            {
                if (selection == -1)
                {
                    target.BlockMembers[index].OnSelect();
                    selection = index;

                    if (target.BlockMembers[index].InputType == BlockInputType.None)
                        Deselect();
                }
                else
                    Deselect();
            }
        }

        private void Deselect()
        {
            if (selection != -1)
            {
                target.BlockMembers[selection].OnDeselect();
                selection = -1;
            }
        }

        /// <summary>
        /// Resets index to the first visible element.
        /// </summary>
        private void ResetIndex()
        {
            index = 0;

            for (int n = 0; n < target.BlockMembers.Count; n++)
            {
                if (target.BlockMembers[n].Enabled)
                {
                    index = n;
                    break;
                }
            }
        }

        public override void HandleInput()
        {
            if (selection != -1)
                target.BlockMembers[selection].HandleInput();
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public override void Update()
        {
            if (target != null)
            {
                if (open)
                {
                    if (!Cfg.forceFallbackHud)
                    {
                        if (fallbackHud.Open)
                            fallbackHud.Hide();

                        if (!apiHud.Open)
                            apiHud.Show();

                        apiHud.Update(index, selection);
                    }
                    else
                    {
                        if (apiHud.Open)
                            apiHud.Hide();

                        if (!fallbackHud.Open)
                            fallbackHud.Show();

                        fallbackHud.Update(index, selection);
                    }
                }
                else
                {
                    apiHud.Hide();
                    fallbackHud.Hide();
                    Deselect();
                    ResetIndex();
                }
            }
        }

        public override void Draw()
        {
            if (target != null && open && apiHud.Open)
                apiHud.Draw();
        }

        /// <summary>
        /// Shows the menu if its hidden.
        /// </summary>
        public static void Show()
        {
            Instance.open = true;
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        public static void Hide()
        {
            Instance.open = false;
        }
    }
}