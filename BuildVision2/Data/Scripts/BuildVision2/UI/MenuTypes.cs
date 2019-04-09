using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.Utils;
using System.Xml.Serialization;
using System;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Stores config information for the Text HUD API based menu
    /// </summary>
    public class ApiHudConfig
    {
        [XmlIgnore]
        public static ApiHudConfig Defaults
        {
            get
            {
                return new ApiHudConfig
                {
                    hudScale = 1f,
                    maxVisible = 11,
                    hideIfNotVis = true,
                    clampHudPos = true,
                    forceToCenter = false,
                    colors = Colors.Defaults
                };
            }
        }

        [XmlElement(ElementName = "HudScale")]
        public float hudScale;

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        [XmlElement(ElementName = "HideHudIfOutOfView")]
        public bool hideIfNotVis;

        [XmlElement(ElementName = "ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "LockHudToScreenCenter")]
        public bool forceToCenter;

        [XmlElement(ElementName = "ColorsRGB")]
        public Colors colors;

        public struct Colors
        {
            [XmlIgnore]
            public static readonly Colors Defaults = new Colors
            {
                deflt = "210,235,245",
                headerText = "210,235,245",
                blockInc = "200,15,15",
                highlight = "200,170,0",
                selected = "30,200,30",
                backgroundColor = new Color(65, 75, 80, 200),
                selectionBoxColor = new Color(41, 54, 62, 255),
                headerColor = new Color(54, 66, 76, 255)
            };

            [XmlIgnore]
            public Color backgroundColor, selectionBoxColor, headerColor;

            [XmlElement(ElementName = "Default")]
            public string deflt;

            [XmlElement(ElementName = "HeaderText")]
            public string headerText;

            [XmlElement(ElementName = "BlockIncomplete")]
            public string blockInc;

            [XmlElement(ElementName = "TextHighlight")]
            public string highlight;

            [XmlElement(ElementName = "Selected")]
            public string selected;

            [XmlElement(ElementName = "ListBg")]
            public string backgroundColorData;

            [XmlElement(ElementName = "SelectionBg")]
            public string selectionBoxColorData;

            [XmlElement(ElementName = "HeaderBg")]
            public string headerColorData;

            /// <summary>
            /// Checks any if fields have invalid values and resets them to the default if necessary.
            /// </summary>
            public void Validate()
            {
                if (deflt == null)
                    deflt = Defaults.deflt;

                if (blockInc == null)
                    blockInc = Defaults.blockInc;

                if (highlight == null)
                    highlight = Defaults.highlight;

                if (selected == null)
                    selected = Defaults.selected;

                if (backgroundColorData == null || !Utilities.TryParseColor(backgroundColorData, out backgroundColor))
                {
                    backgroundColorData = Utilities.GetColorString(Defaults.backgroundColor);
                    backgroundColor = Defaults.backgroundColor;
                }

                if (selectionBoxColorData == null || !Utilities.TryParseColor(selectionBoxColorData, out selectionBoxColor))
                {
                    selectionBoxColorData = Utilities.GetColorString(Defaults.selectionBoxColor);
                    selectionBoxColor = Defaults.selectionBoxColor;
                }

                if (headerColorData == null || !Utilities.TryParseColor(headerColorData, out headerColor))
                {
                    headerColorData = Utilities.GetColorString(Defaults.headerColor);
                    headerColor = Defaults.headerColor;
                }
            }
        }

        public ApiHudConfig() { }

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public void Validate()
        {
            ApiHudConfig defaults = Defaults;

            if (maxVisible == default(int))
                maxVisible = defaults.maxVisible;

            if (hudScale == default(float))
                hudScale = defaults.hudScale;

            colors.Validate();
        }
    }

    /// <summary>
    /// Stores config information for the built in Notifications based menu
    /// </summary>
    public class NotifHudConfig
    {
        [XmlIgnore]
        public static NotifHudConfig Defaults
        {
            get
            {
                return new NotifHudConfig
                {
                    maxVisible = 8
                };
            }
        }

        [XmlElement(ElementName = "MaxVisibleItems")]
        public int maxVisible;

        /// <summary>
        /// Checks any if fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public void Validate()
        {
            if (maxVisible == default(int))
                maxVisible = Defaults.maxVisible;
        }
    }

    internal sealed partial class PropertiesMenu
    {
        /// <summary>
        /// List GUI using HudUtilities elements
        /// </summary>
        private class ApiHud
        {
            public bool Open { get; private set; }
            public ApiHudConfig cfg;

            private HudUtilities.ScrollMenu menu;
            private PropertyBlock target;
            private int visStart, visEnd, index, selection;
            private string headerText;

            public ApiHud(ApiHudConfig cfg)
            {
                this.cfg = cfg;
                Open = false;

                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;                
            }

            /// <summary>
            /// Updates the menu's current target
            /// </summary>
            public void SetTarget(PropertyBlock target)
            {
                this.target = target;
                visStart = 0;
                visEnd = 0;
            }

            /// <summary>
            /// Updates current list position and ensures the menu is visible
            /// </summary>
            public void Update(int index, int selection)
            {
                this.index = index;
                this.selection = selection;
                headerText = $"Build Vision 2: {target.TBlock.CustomName}";
                Open = true;

                GetVisibleProperties();

                if (menu == null)
                    menu = new HudUtilities.ScrollMenu(cfg.maxVisible);

                menu.BodyColor = cfg.colors.backgroundColor;
                menu.HeaderColor = cfg.colors.headerColor;
                menu.SelectionBoxColor = cfg.colors.selectionBoxColor;
                menu.Scale = cfg.hudScale;

                UpdateText();
                UpdatePos();
                menu.Visible = true;
            }

            /// <summary>
            /// Hides API HUD
            /// </summary>
            public void Hide()
            {
                if (menu != null)
                    menu.Visible = false;

                Open = false;
                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;
            }

            /// <summary>
            /// Determines what range of the block's properties are visible based on index and total number of properties.
            /// </summary>
            private void GetVisibleProperties()
            {
                if (target.ScrollableCount <= cfg.maxVisible)
                {
                    visEnd = target.ScrollableCount;
                }
                else
                {
                    if (index >= (visStart + cfg.maxVisible))
                        visStart++;
                    else if (index < visStart)
                        visStart = index;

                    visEnd = Utilities.Clamp((visStart + cfg.maxVisible), 0, target.ScrollableCount);
                }
            }
   
            /// <summary>
            /// Updates position of menu on screen.
            /// </summary>
            private void UpdatePos()
            {
                Vector3D targetPos, worldPos;
                Vector2D screenPos, screenBounds;
                
                if (!cfg.forceToCenter)
                {
                    if (LocalPlayer.IsLookingInBlockDir(target.TBlock))
                    {
                        targetPos = target.GetPosition();
                        worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                        screenPos = new Vector2D(worldPos.X, worldPos.Y);
                        screenBounds = new Vector2D(1d, 1d) - menu.Size / 2;

                        if (cfg.clampHudPos)
                        {
                            screenPos.X = Utilities.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                            screenPos.Y = Utilities.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                        }
                    }
                    else if (cfg.hideIfNotVis)
                    {
                        menu.Visible = false;
                        screenPos = Vector2D.Zero;
                    }
                    else
                        screenPos = Vector2D.Zero;
                }
                else
                    screenPos = Vector2D.Zero;

                menu.ScaledPos = screenPos;
                menu.selectionIndex = index - visStart;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private void UpdateText()
            {
                int elements = Utilities.Clamp(visEnd - visStart, 0, cfg.maxVisible), i, action;
                StringBuilder[] list = new StringBuilder[elements];
                string colorCode;

                menu.HeaderText = new StringBuilder($"<color={cfg.colors.headerText}>{headerText}");

                for (int n = 0; n < elements; n++)
                {
                    i = n + visStart;
                    action = i - target.Properties.Count;

                    if (i == selection)
                        colorCode = $"<color={cfg.colors.selected}>";
                    else if (i == index)
                        colorCode = $"<color={cfg.colors.highlight}>";
                    else
                        colorCode = $"<color={cfg.colors.deflt}>";

                    if (i >= target.Properties.Count)
                        list[n] = new StringBuilder(colorCode + target.Actions[action].GetDisplay());
                    else
                        list[n] = new StringBuilder(
                            $"<color={cfg.colors.deflt}>{target.Properties[i].GetName()}: {colorCode}{target.Properties[i].GetValue()}");
                }

                menu.ListText = list;
                menu.FooterLeftText = new StringBuilder(
                        $"<color={cfg.colors.headerText}>[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");

                if (target.IsFunctional)
                    menu.FooterRightText = new StringBuilder(
                        $"<color={cfg.colors.headerText}>[Functional]");
                else
                    menu.FooterRightText = new StringBuilder(
                        $"<color={cfg.colors.blockInc}>[Incomplete]");
            }
        }

        /// <summary>
        /// List GUI based on IMyHudNotification objects
        /// </summary>
        private class NotifHud
        {
            public bool Open { get; private set; }
            public NotifHudConfig cfg;

            private IMyHudNotification header;
            private IMyHudNotification[] list;
            private PropertyBlock target;
            private string headerText;
            private int visStart, visEnd, index, selection;

            public NotifHud(NotifHudConfig cfg)
            {
                Open = false;
                this.cfg = cfg;
                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;

                header = MyAPIGateway.Utilities.CreateNotification("");
                list = new IMyHudNotification[cfg.maxVisible];
            }

            /// <summary>
            /// Updates the current target block of the menu
            /// </summary>
            public void SetTarget(PropertyBlock target)
            {
                this.target = target;
                visStart = 0;
                visEnd = 0;
            }

            /// <summary>
            /// Updates the current list position and draws the menu.
            /// </summary>
            public void Update(int index, int selection)
            {
                this.index = index;
                this.selection = selection;
                headerText = $"Build Vision 2: {target.TBlock.CustomName}";
                Open = true;

                if (list == null || list.Length != cfg.maxVisible)
                    list = new IMyHudNotification[cfg.maxVisible];

                GetVisibleProperties();
                UpdateText();                
            }

            /// <summary>
            /// Hides notificatins
            /// </summary>
            public void Hide()
            {
                header.Hide();

                foreach (IMyHudNotification prop in list)
                {
                    if (prop != null)
                        prop.Hide();
                }

                Open = false;
                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;
            }

            /// <summary>
            /// Determines what range of the block's properties are visible based on index and total number of properties.
            /// </summary>
            private void GetVisibleProperties()
            {
                if (target.ScrollableCount <= cfg.maxVisible)
                {
                    header.Text = headerText;
                    visEnd = target.ScrollableCount;
                }
                else
                {
                    if (index >= (visStart + cfg.maxVisible))
                        visStart++;
                    else if (index < visStart)
                        visStart = index;

                    visEnd = Utilities.Clamp((visStart + cfg.maxVisible), 0, target.ScrollableCount);
                }
            }

            /// <summary>
            /// Updates text colors and resets alive time for fallback hud.
            /// </summary>
            private void UpdateText()
            {
                int elements = Utilities.Clamp(visEnd - visStart, 0, cfg.maxVisible), i, action;

                header.Show();
                header.ResetAliveTime();
                header.Text = $"{headerText} ({visStart + 1} - {visEnd} of {target.ScrollableCount})";

                for (int notif = 0; notif < elements; notif++)
                {
                    if (list[notif] == null)
                        list[notif] = MyAPIGateway.Utilities.CreateNotification("");

                    i = notif + visStart;
                    action = i - target.Properties.Count;

                    // make sure its still being shown
                    list[notif].Show();
                    list[notif].ResetAliveTime();

                    // get name
                    if (i >= target.Properties.Count)
                        list[notif].Text = target.Actions[action].GetDisplay();
                    else
                        list[notif].Text = target.Properties[i].GetDisplay();

                    // get color
                    if (i == selection)
                        list[notif].Font = MyFontEnum.Green;
                    else if (i == index)
                        list[notif].Font = MyFontEnum.Red;
                    else
                        list[notif].Font = MyFontEnum.White;
                }

                // hide everything else
                for (int n = elements; n < list.Length; n++)
                {
                    if (list[n] == null)
                        list[n] = MyAPIGateway.Utilities.CreateNotification("");

                    list[n].Text = "";
                    list[n].Hide();
                }
            }
        }
    }
}
