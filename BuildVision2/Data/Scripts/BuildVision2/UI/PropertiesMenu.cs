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
            forceToCenter = false,
            clampHudPos = true,
            apiMaxVisible = 11,
            fallbkMaxVisible = 8,
            apiHudScale = 1.25f
        };

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
        }
    }

    /// <summary>
    /// Renders menu of block terminal properties given a target block; singleton.
    /// </summary>
    internal sealed class PropertiesMenu
    {
        public static PropertiesMenu Instance { get; private set; }

        private Binds binds;
        private int apiMaxVisible, fallbkMaxVisible;
        private float apiHudScale;

        private IMyHudNotification fallbkHeader;
        private IMyHudNotification[] fallbkList;
        private HudAPIv2 hudApi;
        private HudAPIv2.HUDMessage hudApiMessage;
        private PropertyBlock target;

        private bool clampHudPos, forceToCenter;
        private int index, visStart, visEnd, selection;
        private bool apiHudOpen, fallbackHudOpen;
        private string headerText;

        private PropertiesMenu(PropMenuConfig cfg, Binds binds)
        {
            this.binds = binds;
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
        public static PropertiesMenu GetInstance(PropMenuConfig cfg, Binds binds)
        {
            if (Instance == null && binds != null)
                Instance = new PropertiesMenu(cfg, binds);

            return Instance;
        }

        /// <summary>
        /// Sets the menu's instance to null.
        /// </summary>
        public void Close()
        {
            Instance = null;
        }

        /// <summary>
        /// Updates the configuration of the properties menu.
        /// </summary>
        public void UpdateConfig(PropMenuConfig cfg)
        {
            forceToCenter = cfg.forceToCenter;
            clampHudPos = cfg.clampHudPos;
            apiMaxVisible = cfg.apiMaxVisible;
            fallbkMaxVisible = cfg.fallbkMaxVisible;
            apiHudScale = cfg.apiHudScale;
            fallbkList = new IMyHudNotification[fallbkMaxVisible];
        }

        public PropMenuConfig GetConfig()
        {
            return new PropMenuConfig
            {
                forceToCenter = forceToCenter,
                clampHudPos = clampHudPos,
                apiMaxVisible = apiMaxVisible,
                fallbkMaxVisible = fallbkMaxVisible,
                apiHudScale = apiHudScale
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

            if (binds.select.IsNewPressed)
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
            if ((binds.scrollUp.Analog && binds.scrollUp.IsPressed) || binds.scrollUp.IsNewPressed)
                return 1;
            else if ((binds.scrollDown.Analog && binds.scrollDown.IsPressed) || binds.scrollDown.IsNewPressed)
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
            if (hudApiMessage == null)
            {
                hudApiMessage = new HudAPIv2.HUDMessage();
                hudApiMessage.Blend = BlendTypeEnum.LDR;
                //hudApiMessage.Options |= HudAPIv2.Options.Shadowing;
                //hudApiMessage.ShadowColor = new Color(0,0,0,80);
                hudApiMessage.Scale = apiHudScale;
            }

            UpdateApiMenuPos();
            hudApiMessage.Visible = true;
            hudApiMessage.Message = GetApiText();
        }

        /// <summary>
        /// Updates position of menu on screen.
        /// </summary>
        private void UpdateApiMenuPos()
        {
            Vector3D targetPos, worldPos;
            Vector2D screenPos, offset = hudApiMessage.GetTextLength() * .5f;

            if (!forceToCenter && LocalPlayer.IsLookingInBlockDir(target.TBlock))
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
                hudApiMessage.Origin = screenPos;
            }
            else
                hudApiMessage.Origin = new Vector2D(-offset.X, -offset.Y);
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
        private StringBuilder GetApiText()
        {
            StringBuilder sb = new StringBuilder();
            int elements = Clamp(visEnd - visStart, 0, apiMaxVisible), i, action;
            string colorCode;

            if (target.IsFunctional)
                sb.AppendLine($"<color=200,200,200,255>[{headerText}]");
            else
                sb.AppendLine($"<color=200,0,0,255>[{headerText} (Incomplete)]");

            for (int n = 0; n < elements; n++)
            {
                i = n + visStart;
                action = i - target.Properties.Count;

                if (i == selection)
                    colorCode = "<color=30,200,30,255>";
                else if (i == index)
                    colorCode = "<color=200,170,0,255>";
                else
                    colorCode = "<color=200,200,200,255>";

                if (i >= target.Properties.Count)
                    sb.AppendLine(colorCode + target.Actions[action].GetName());
                else
                    sb.AppendLine(colorCode + target.Properties[i].GetName());
            }

            sb.AppendLine($"<color=200,200,200,255>[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");
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
            if (hudApiMessage != null)
                hudApiMessage.Visible = false;

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