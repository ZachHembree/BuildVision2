using System.Xml.Serialization;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed partial class PropertiesMenu : Singleton<PropertiesMenu>
    {
        public static PropMenuConfig Cfg { get; set; } = PropMenuConfig.Defaults;
        public static ApiHudConfig ApiHudCfg { get { return Cfg.apiHudConfig; } set { Cfg.apiHudConfig = value; } }
        public static NotifHudConfig NotifHudCfg { get { return Cfg.fallbackHudConfig; } set { Cfg.fallbackHudConfig = value; } }
        public static bool MenuOpen { get { return menuOpen; } set { menuOpen = value; Instance?.Update(); } }

        private static HudUtilities HudUtilities { get { return HudUtilities.Instance; } }
        private PropertyBlock target;
        private static bool menuOpen;

        private ApiHud apiHud;
        private NotifHud fallbackHud;
        private int index, selection;

        public PropertiesMenu()
        {
            apiHud = new ApiHud();
            fallbackHud = new NotifHud();
            target = null;

            index = 0;
            selection = -1;
            menuOpen = false;
            KeyBinds.Select.OnNewPress += SelectProperty;
            KeyBinds.ScrollUp.OnNewPress += ScrollUp;
            KeyBinds.ScrollDown.OnNewPress += ScrollDown;
        }

        /// <summary>
        /// Sets target block and acquires its properties.
        /// </summary>
        public void SetTarget(PropertyBlock newTarget)
        {
            target = newTarget;
            index = 0;
            selection = -1;
            apiHud.SetTarget(newTarget);
            fallbackHud.SetTarget(newTarget);
        }

        private void SelectProperty()
        {
            if (MenuOpen)
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
            if (MenuOpen)
                UpdateSelection(-1);
        }

        private void ScrollUp()
        {
            if (MenuOpen)
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
                index = Utilities.Clamp(index, 0, target.ScrollableCount - 1);
            }
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public void Update()
        {
            if (MenuOpen && target != null)
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
        /// Hides all menu elements.
        /// </summary>
        private void Hide()
        {
            apiHud.Hide();
            fallbackHud.Hide();

            index = 0;
            selection = -1;
        }
    }
}
 