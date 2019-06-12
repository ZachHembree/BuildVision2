using DarkHelmet.Game;
using DarkHelmet.UI;

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
                Instance.index = 0;
                Instance.selection = -1;
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

        private ApiHud apiHud;
        private NotifHud fallbackHud;
        private int index, selection;

        private PropertiesMenu()
        {
            apiHud = new ApiHud();
            fallbackHud = new NotifHud();
            target = null;

            index = 0;
            selection = -1;
            open = false;
        }

        private static void Init()
        {
            if (instance == null)
            {
                instance = new PropertiesMenu();

                KeyBinds.Select.OnNewPress += instance.SelectProperty;
                KeyBinds.ScrollUp.OnPressAndHold += instance.ScrollUp;
                KeyBinds.ScrollDown.OnPressAndHold += instance.ScrollDown;
            }
        }

        public override void Close()
        {
            Instance = null;
        }

        private void SelectProperty()
        {
            if (open)
            {
                int action = index - target.Properties.Count;

                if (index >= target.Properties.Count)
                    target.Actions[action].Action();
                else
                    selection = (selection == -1) ? index : -1;
            }
        }

        private void ScrollDown()
        {
            if (open)
                UpdateSelection(-1);
        }

        private void ScrollUp()
        {
            if (open)
                UpdateSelection(1);
        }

        /// <summary>
        /// Updates scrollable index, range of visible scrollables and input for selected property.
        /// </summary>
        private void UpdateSelection(int scrolllDir)
        {
            if (selection != -1 && index < target.Properties.Count)
            {
                if (scrolllDir > 0)
                    target.Properties[index].ScrollUp();
                else if (scrolllDir < 0)
                    target.Properties[index].ScrollDown();
            }
            else
            {
                index -= scrolllDir;
                index = Utils.Math.Clamp(index, 0, target.ScrollableCount - 1);
            }
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public override void Update()
        {
            if (open && target != null)
            {
                if (HudUtilities.Heartbeat && !Cfg.forceFallbackHud)
                {
                    if (fallbackHud.Open)
                        fallbackHud.Hide();

                    apiHud.Update(index, selection);
                }
                else
                {
                    if (apiHud.Open)
                        apiHud.Hide();

                    fallbackHud.Update(index, selection);
                }
            }
            else
                Hide();
        }

        /// <summary>
        /// Shows the menu if its hidden.
        /// </summary>
        public static void Show()
        {
            Instance.open = true;
            Instance.apiHud.Show();
            Instance.fallbackHud.Show();
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        public static void Hide()
        {
            Instance.open = false;
            Instance.apiHud.Hide();
            Instance.fallbackHud.Hide();

            Instance.index = 0;
            Instance.selection = -1;
        }
    }
}
