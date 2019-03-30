using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using System.Xml.Serialization;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.BuildVision2
{
    public struct PropMenuConfig
    {
        [XmlIgnore]
        public static readonly PropMenuConfig defaults = new PropMenuConfig
        {
            hideIfNotVis = false,
            forceToCenter = false,
            clampHudPos = true,
            apiMaxVisible = 11,
            fallbkMaxVisible = 8,
            apiHudScale = 1.25f,

            blockIncTextColor = "180,0,0",
            defaultTextColor = "200,190,160",
            highlightTextColor = "200,170,0",
            selectedTextColor = "30,200,30",
        };

        [XmlElement(ElementName = "HideHudIfOutOfView")]
        public bool hideIfNotVis;

        [XmlElement (ElementName ="ClampHudToScreenEdges")]
        public bool clampHudPos;

        [XmlElement(ElementName = "LockHudToScreenCenter")]
        public bool forceToCenter;

        [XmlElement(ElementName = "MaxNumberOfItemsOnApiHud")]
        public int apiMaxVisible;

        [XmlElement(ElementName = "MaxNumberOfItemsOnFallbackHud")]
        public int fallbkMaxVisible;

        [XmlElement(ElementName = "ApiHudSize")]
        public float apiHudScale;

        [XmlElement(ElementName = "BlockIncompleteTextColorRGB")]
        public string blockIncTextColor;

        [XmlElement(ElementName = "DefaultTextColorRGB")]
        public string defaultTextColor;

        [XmlElement(ElementName = "HighlightTextColorRGB")]
        public string highlightTextColor;

        [XmlElement(ElementName = "SelectedTextColorRGB")]
        public string selectedTextColor;

        /// <summary>
        /// Checks any fields have invalid values and resets them to the default if necessary.
        /// </summary>
        public void Validate()
        {
            if (apiMaxVisible == default(int))
                apiMaxVisible = defaults.apiMaxVisible;

            if (fallbkMaxVisible == default(int))
                fallbkMaxVisible = defaults.fallbkMaxVisible;

            if (apiHudScale == default(float))
                apiHudScale = defaults.apiHudScale;

            if (blockIncTextColor == default(string))
                blockIncTextColor = defaults.blockIncTextColor;

            if (defaultTextColor == default(string))
                defaultTextColor = defaults.defaultTextColor;

            if (highlightTextColor == default(string))
                highlightTextColor = defaults.highlightTextColor;

            if (selectedTextColor == default(string))
                selectedTextColor = defaults.selectedTextColor;
        }
    }

    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed class PropertiesMenu
    {
        public static PropertiesMenu Instance { get; private set; }

        private static Binds Binds { get { return Binds.Instance; } }
        private int apiMaxVisible, fallbkMaxVisible;
        private float apiHudScale;

        private IMyHudNotification fallbkHeader;
        private IMyHudNotification[] fallbkList;
        private HudAPIv2 hudApi;
        private HudAPIv2.HUDMessage hudApiMessageBg, hudApiMessageFore;
        private PropertyBlock target;

        private bool hideIfNotVis, clampHudPos, forceToCenter;
        private int index, visStart, visEnd, selection;
        private bool apiHudOpen, fallbackHudOpen;
        private string headerText, blockIncTextColor, defaultTextColor, 
            highlightTextColor, selectedTextColor;

        private PropertiesMenu(PropMenuConfig cfg)
        {
            fallbkList = new IMyHudNotification[fallbkMaxVisible];
            hudApi = new HudAPIv2();
            fallbkHeader = MyAPIGateway.Utilities.CreateNotification("");
            target = null;

            index = 0;
            visStart = 0;
            visEnd = 0;

            apiHudOpen = false;
            fallbackHudOpen = false;
            selection = -1;
            UpdateConfig(cfg);
        }

        /// <summary>
        /// Returns the current instance or creates one if necessary.
        /// </summary>
        public static PropertiesMenu GetInstance(PropMenuConfig cfg)
        {
            if (Instance == null)
                Instance = new PropertiesMenu(cfg);

            return Instance;
        }

        /// <summary>
        /// Sets the menu's instance to null.
        /// </summary>
        public void Close()
        {
            hudApi.Close();
            Instance = null;
        }

        /// <summary>
        /// Updates the configuration of the properties menu.
        /// </summary>
        public void UpdateConfig(PropMenuConfig cfg)
        {
            hideIfNotVis = cfg.hideIfNotVis;
            forceToCenter = cfg.forceToCenter;
            clampHudPos = cfg.clampHudPos;
            apiMaxVisible = cfg.apiMaxVisible;
            fallbkMaxVisible = cfg.fallbkMaxVisible;
            apiHudScale = cfg.apiHudScale;

            blockIncTextColor = cfg.blockIncTextColor;
            defaultTextColor = cfg.defaultTextColor;
            highlightTextColor = cfg.highlightTextColor;
            selectedTextColor = cfg.selectedTextColor;

            fallbkList = new IMyHudNotification[fallbkMaxVisible];
        }

        public PropMenuConfig GetConfig()
        {
            return new PropMenuConfig
            {
                hideIfNotVis = hideIfNotVis,
                forceToCenter = forceToCenter,
                clampHudPos = clampHudPos,
                apiMaxVisible = apiMaxVisible,
                fallbkMaxVisible = fallbkMaxVisible,
                apiHudScale = apiHudScale,

                blockIncTextColor = blockIncTextColor,
                defaultTextColor = defaultTextColor,
                highlightTextColor = highlightTextColor,
                selectedTextColor = selectedTextColor
            };
        }

        /// <summary>
        /// Sets target block and acquires its properties.
        /// </summary>
        public void SetTarget(PropertyBlock block)
        {
            target = block;
            index = 0;
            visStart = 0;
            visEnd = 0;

            headerText = $"Build Vision 2: {target.TBlock.CustomName}";
            selection = -1;
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public void Update(bool forceFallbackHud)
        {
            if (target != null)
            {
                UpdateSelection(forceFallbackHud);

                if (hudApi.Heartbeat && !forceFallbackHud)
                {
                    if (fallbackHudOpen) HideFallbackHud();
                    UpdateApiMenu();
                    apiHudOpen = true;
                }
                else
                {
                    if (apiHudOpen)
                        HideApiHud();

                    if (fallbkList == null || fallbkList.Length != fallbkMaxVisible)
                        fallbkList = new IMyHudNotification[fallbkMaxVisible];

                    UpdateFallbackMenu();
                    fallbackHudOpen = true;
                }
            }
        }

        /// <summary>
        /// Updates scrollable index, range of visible scrollables and input for selected property.
        /// </summary>
        private void UpdateSelection(bool fallback)
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
                index = Clamp(index, 0, target.ScrollableCount - 1);
            }

            GetVisibleProperties(fallback);
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

        /// <summary>
        /// Determines what range of the block's properties are visible based on index and total number of properties.
        /// </summary>
        private void GetVisibleProperties(bool fallback)
        {
            int maxVisible = fallback ? fallbkMaxVisible : apiMaxVisible;

            if (target.ScrollableCount <= maxVisible)
            {
                fallbkHeader.Text = headerText;
                visEnd = target.ScrollableCount;
            }
            else
            {
                if (index >= (visStart + maxVisible))
                    visStart++;
                else if (index < visStart)
                    visStart = index;

                visEnd = Clamp((visStart + maxVisible), 0, target.ScrollableCount);
                fallbkHeader.Text = $"{headerText} ({visStart + 1} - {visEnd} of {target.ScrollableCount})";
            }
        }

        private void UpdateApiMenu()
        {
            if (hudApiMessageBg == null)
            {
                hudApiMessageBg = new HudAPIv2.HUDMessage();
                hudApiMessageBg.Blend = BlendTypeEnum.LDR;
                hudApiMessageBg.Options |= HudAPIv2.Options.Shadowing;
                hudApiMessageBg.ShadowColor = new Color(0,0,0);
                hudApiMessageBg.Scale = apiHudScale;

                hudApiMessageFore = new HudAPIv2.HUDMessage();
                hudApiMessageFore.Blend = BlendTypeEnum.LDR;
                hudApiMessageFore.Scale = apiHudScale;
            }

            UpdateApiMenuPos();
            hudApiMessageBg.Visible = true;
            hudApiMessageBg.Message = GetApiTextBg();

            hudApiMessageFore.Visible = hudApiMessageBg.Visible;
            hudApiMessageFore.Message = GetApiTextFore();
            hudApiMessageFore.Origin = hudApiMessageBg.Origin;
        }

        /// <summary>
        /// Updates position of menu on screen.
        /// </summary>
        private void UpdateApiMenuPos()
        {
            Vector3D targetPos, worldPos;
            Vector2D screenPos, offset = hudApiMessageBg.GetTextLength() * .5f;

            if (!forceToCenter)
            {
                if (LocalPlayer.IsLookingInBlockDir(target.TBlock))
                {
                    targetPos = target.GetPosition();
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                    screenPos = new Vector2D(worldPos.X, worldPos.Y);

                    if (clampHudPos)
                    {
                        screenPos.X = Clamp(screenPos.X, -.95 + offset.X, .95 - offset.X);
                        screenPos.Y = Clamp(screenPos.Y, -.95 - offset.Y, .95 + offset.Y);
                    }

                    screenPos -= offset;
                    hudApiMessageBg.Origin = screenPos;
                }
                else if (hideIfNotVis)
                    hudApiMessageBg.Origin = new Vector2D(-double.MinValue, -double.MinValue);
                else
                    hudApiMessageBg.Origin = new Vector2D(-offset.X, -offset.Y);              
            }
            else
                hudApiMessageBg.Origin = new Vector2D(-offset.X, -offset.Y);
        }

        /// <summary>
        /// Updates text colors and resets alive time for fallback hud.
        /// </summary>
        private void UpdateFallbackMenu()
        {
            int elements = Clamp(visEnd - visStart, 0, fallbkMaxVisible), i, action;

            fallbkHeader.Show();
            fallbkHeader.ResetAliveTime();

            for (int notif = 0; notif < elements; notif++)
            {
                if (fallbkList[notif] == null)
                    fallbkList[notif] = MyAPIGateway.Utilities.CreateNotification("");

                i = notif + visStart;
                action = i - target.Properties.Count;

                fallbkList[notif].Show();
                fallbkList[notif].ResetAliveTime();

                if (i >= target.Properties.Count)
                    fallbkList[notif].Text = target.Actions[action].GetName();
                else
                    fallbkList[notif].Text = target.Properties[i].GetName();

                if (i == selection)
                    fallbkList[notif].Font = MyFontEnum.Green;
                else if (i == index)
                    fallbkList[notif].Font = MyFontEnum.Red;
                else
                    fallbkList[notif].Font = MyFontEnum.White;
            }

            for (int n = elements; n < fallbkList.Length; n++)
            {
                if (fallbkList[n] == null)
                    fallbkList[n] = MyAPIGateway.Utilities.CreateNotification("");

                fallbkList[n].Text = "";
                fallbkList[n].Hide();
            }
        }

        /// <summary>
        /// Gets finished string for the Text HUD API to display.
        /// </summary>
        private StringBuilder GetApiTextFore()
        {
            StringBuilder sb = new StringBuilder();
            int elements = Clamp(visEnd - visStart, 0, apiMaxVisible), i, action;
            string colorCode;

            if (target.IsFunctional)
                sb.AppendLine($"<color={defaultTextColor}>[{headerText}]");
            else
                sb.AppendLine($"<color={blockIncTextColor}>[{headerText} (incomplete)]");

            for (int n = 0; n < elements; n++)
            {
                i = n + visStart;
                action = i - target.Properties.Count;

                if (i == selection)
                    colorCode = $"<color={selectedTextColor}>";
                else if (i == index)
                    colorCode = $"<color={highlightTextColor}>";
                else
                    colorCode = $"<color={defaultTextColor}>";

                if (i >= target.Properties.Count)
                    sb.AppendLine(colorCode + target.Actions[action].GetName());
                else
                    sb.AppendLine(colorCode + target.Properties[i].GetName());
            }

            sb.AppendLine($"<color={defaultTextColor}>[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");
            return sb;
        }

        /// <summary>
        /// Gets finished string for the Text HUD API to render the text shadows.
        /// </summary>
        private StringBuilder GetApiTextBg()
        {
            StringBuilder sb = new StringBuilder();
            int elements = Clamp(visEnd - visStart, 0, apiMaxVisible), i, action;
            string backgroundColor = "<color=0,0,0>";

            if (target.IsFunctional)
                sb.AppendLine($"{backgroundColor}[{headerText}]");
            else
                sb.AppendLine($"{backgroundColor}[{headerText} (incomplete)]");

            for (int n = 0; n < elements; n++)
            {
                i = n + visStart;
                action = i - target.Properties.Count;

                if (i >= target.Properties.Count)
                    sb.AppendLine(backgroundColor + target.Actions[action].GetName());
                else
                    sb.AppendLine(backgroundColor + target.Properties[i].GetName());
            }

            sb.AppendLine($"{backgroundColor}[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");
            return sb;
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        public void Hide()
        {
            HideApiHud();
            HideFallbackHud();

            index = 0;
            visStart = 0;
            visEnd = 0;
            selection = -1;
        }

        /// <summary>
        /// Hides elements from the Text Hud API.
        /// </summary>
        private void HideApiHud()
        {
            if (hudApiMessageBg != null)
            {
                hudApiMessageBg.Visible = false;
                hudApiMessageFore.Visible = false;
            }

            apiHudOpen = false;
        }

        /// <summary>
        /// Hides elements from the IMyHudnotification based fallback hud.
        /// </summary>
        private void HideFallbackHud()
        {
            fallbkHeader.Hide();

            foreach (IMyHudNotification prop in fallbkList)
            {
                if (prop != null)
                    prop.Hide();
            }

            fallbackHudOpen = false;
        }

        /// <summary>
        /// Clamps an int between two values.
        /// </summary>
        private double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        /// <summary>
        /// Clamps an int between two values.
        /// </summary>
        private int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }
    }
}
 