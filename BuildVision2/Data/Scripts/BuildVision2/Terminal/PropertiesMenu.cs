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
                Instance.target = value;
                Instance.Deselect();
                Instance.ResetIndex();
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
        private int index, selection;

        private PropertiesMenu()
        {
            apiHud = new ApiHud();
            fallbackHud = new NotifHud();
            target = null;

            index = 0;
            selection = -1;
            open = false;

            MyAPIGateway.Utilities.MessageEntered += MessageHandler;

            KeyBinds.Select.OnNewPress += Select;
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

        private static bool IsElementEnabled(int index)
        {
            int a = index - Target.Properties.Count;
            return (index < Target.Properties.Count && Target.Properties[index].Enabled) || (a >= 0 && Target.Actions[a].Enabled);
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
                int newIndex = index;
                bool scrollDown = scrolllDir > 0;

                while ((scrollDown && newIndex < target.ElementCount - 1) || (!scrollDown && newIndex > 0))
                {
                    newIndex += scrolllDir;

                    if (IsElementEnabled(newIndex))
                    {
                        index = newIndex;
                        break;
                    }
                }
            }
        }

        private void Select()
        {
            if (open)
            {
                int action = index - target.Properties.Count;

                if (index >= target.Properties.Count)
                    target.Actions[action].Action();
                else
                {
                    if (selection == -1)
                    {
                        target.Properties[index].OnSelect();
                        selection = index;
                    }
                    else
                        Deselect();
                }
            }
        }

        private void Deselect()
        {
            if (index < target.Properties.Count && selection != -1)
            {
                target.Properties[selection].OnDeselect();
                selection = -1;
            }
        }

        /// <summary>
        /// Resets index to the first visible element.
        /// </summary>
        private void ResetIndex()
        {
            index = 0;

            for (int n = 0; n < target.ElementCount; n++)
            {
                if (IsElementEnabled(n))
                {
                    index = n;
                    break;
                }
            }
        }

        public override void HandleInput()
        {
            if (selection != -1)
                target.Properties[selection].HandleInput();
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
                    if (HudUtilities.Heartbeat && !Cfg.forceFallbackHud)
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