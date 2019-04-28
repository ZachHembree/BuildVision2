using System.Xml.Serialization;
using DarkHelmet.UI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed partial class PropertiesMenu
    {
        public static PropertiesMenu Instance { get; private set; }
        public PropMenuConfig Cfg
        {
            get
            {
                return new PropMenuConfig
                {
                    apiHudConfig = apiHud.Cfg,
                    fallbackHudConfig = fallbackHud.Cfg
                };
            }
            set
            {
                apiHud.Cfg = value.apiHudConfig;
                fallbackHud.Cfg = value.fallbackHudConfig;
            }
        }
        public ApiHudConfig ApiHudConfig { get { return apiHud.Cfg; } set { apiHud.Cfg = value; } }
        public NotifHudConfig NotifHudConfig { get { return fallbackHud.Cfg; } set { fallbackHud.Cfg = value; } }

        private static Binds Binds { get { return Binds.Instance; } }
        private static HudUtilities HudElements { get { return HudUtilities.Instance; } }
        private PropertyBlock target;

        private ApiHud apiHud;
        private NotifHud fallbackHud;
        private int index, selection;
        private bool menuOpen;

        private PropertiesMenu(PropMenuConfig cfg)
        {
            apiHud = new ApiHud(cfg.apiHudConfig);
            fallbackHud = new NotifHud(cfg.fallbackHudConfig);
            target = null;

            index = 0;
            selection = -1;
            menuOpen = false;
            Cfg = cfg;
        }

        /// <summary>
        /// Returns the current instance or creates one if necessary.
        /// </summary>
        public static void Init(PropMenuConfig cfg)
        {
            if (Instance == null)
                Instance = new PropertiesMenu(cfg);
        }

        /// <summary>
        /// Sets the menu's instance to null.
        /// </summary>
        public void Close()
        {
            Instance = null;
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
        public void Update(bool forceFallbackHud)
        {
            if (target != null)
            {
                menuOpen = true;
                UpdateSelection();

                if (HudElements.Heartbeat && !forceFallbackHud)
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

            if (Binds.select.IsNewPressed)
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
            if ((Binds.scrollUp.Analog && Binds.scrollUp.IsPressed) || Binds.scrollUp.IsNewPressed)
                return 1;
            else if ((Binds.scrollDown.Analog && Binds.scrollDown.IsPressed) || Binds.scrollDown.IsNewPressed)
                return -1;
            else
                return 0;
        }
    }
}
 