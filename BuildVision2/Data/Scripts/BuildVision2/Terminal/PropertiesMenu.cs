using System.Xml.Serialization;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed partial class PropertiesMenu : ModBase.Component<PropertiesMenu>
    {
        public static PropMenuConfig Cfg { get; set; } = PropMenuConfig.Defaults;
        public static ApiHudConfig ApiHudCfg { get { return Cfg.apiHudConfig; } set { Cfg.apiHudConfig = value; } }
        public static NotifHudConfig NotifHudCfg { get { return Cfg.fallbackHudConfig; } set { Cfg.fallbackHudConfig = value; } }
        public static bool menuOpen;

        private static HudUtilities HudUtilities { get { return HudUtilities.Instance; } }
        private PropertyBlock target;

        private ApiHud apiHud;
        private NotifHud fallbackHud;
        private int index, selection;

        static PropertiesMenu()
        {
            UpdateAfterSimActions.Add(() => Instance.Update());
        }

        public PropertiesMenu()
        {
            apiHud = new ApiHud();
            fallbackHud = new NotifHud();
            target = null;

            index = 0;
            selection = -1;
            menuOpen = false;
        }

        /// <summary>
        /// Sets target block and acquires its properties.
        /// </summary>
        public void SetTarget(PropertyBlock block)
        {
            target = block;
            index = 0;
            selection = -1;
            apiHud.SetTarget(block);
            fallbackHud.SetTarget(block);
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        private void Update()
        {
            if (menuOpen && target != null)
            {
                UpdateSelection();

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
            {
                Hide();
            }
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        public void Hide()
        {
            if (menuOpen)
            {
                apiHud.Hide();
                fallbackHud.Hide();

                index = 0;
                selection = -1;
                menuOpen = false;
            }
        }

        /// <summary>
        /// Updates scrollable index, range of visible scrollables and input for selected property.
        /// </summary>
        private void UpdateSelection()
        {
            int scrolllDir = GetScrollDir(), action = index - target.Properties.Count;

            if (KeyBinds.Select.IsNewPressed)
            {
                if (index >= target.Properties.Count)
                    target.Actions[action].Action();
                else
                    selection = (selection == -1) ? index : -1;
            }

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
                index= Utilities.Clamp(index, 0, target.ScrollableCount - 1);
            }
        }

        private int GetScrollDir()
        {
            if ((KeyBinds.ScrollUp.Analog && KeyBinds.ScrollUp.IsPressed) || KeyBinds.ScrollUp.IsNewPressed)
                return 1;
            else if ((KeyBinds.ScrollDown.Analog && KeyBinds.ScrollDown.IsPressed) || KeyBinds.ScrollDown.IsNewPressed)
                return -1;
            else
                return 0;
        }
    }
}
 