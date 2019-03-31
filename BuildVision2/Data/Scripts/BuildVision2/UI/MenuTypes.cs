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
                    hudScale = 1.25f,
                    maxVisible = 11,
                    hideIfNotVis = true,
                    clampHudPos = true,
                    forceToCenter = false,
                    textColors = TextColors.Defaults
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

        [XmlElement(ElementName = "TextColorsRGB")]
        public TextColors textColors;

        public struct TextColors
        {
            [XmlIgnore]
            public static readonly TextColors Defaults = new TextColors
            {
                deflt = "200,190,160",
                blockInc = "180,0,0",
                highlight = "200,170,0",
                selected = "30,200,30"
            };

            [XmlElement(ElementName = "Default")]
            public string deflt;

            [XmlElement(ElementName = "BlockIncomplete")]
            public string blockInc;

            [XmlElement(ElementName = "Highlight")]
            public string highlight;

            [XmlElement(ElementName = "Selected")]
            public string selected;

            /// <summary>
            /// Checks any if fields have invalid values and resets them to the default if necessary.
            /// </summary>
            public void Validate()
            {
                if (deflt == default(string))
                    deflt = Defaults.deflt;

                if (blockInc == default(string))
                    blockInc = Defaults.blockInc;

                if (highlight == default(string))
                    highlight = Defaults.highlight;

                if (selected == default(string))
                    selected = Defaults.selected;
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

            textColors.Validate();
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
        /// List GUI using Draygo's Text HUD API
        /// </summary>
        private class ApiHud
        {
            public bool Open { get; private set; }
            public ApiHudConfig cfg;

            private HudAPIv2.HUDMessage messageBg, messageFore;
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
            /// Updates current list position and draws the menu
            /// </summary>
            public void Update(int index, int selection)
            {
                this.index = index;
                this.selection = selection;
                headerText = $"Build Vision 2: {target.TBlock.CustomName}";
                Open = true;

                GetVisibleProperties();
                Draw();
            }

            /// <summary>
            /// Hides API HUD
            /// </summary>
            public void Hide()
            {
                if (messageBg != null)
                {
                    messageBg.Visible = false;
                    messageFore.Visible = false;
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

            private void Draw()
            {
                if (messageBg == null)
                {
                    messageBg = new HudAPIv2.HUDMessage();
                    messageBg.Blend = BlendTypeEnum.LDR;
                    messageBg.Options |= HudAPIv2.Options.Shadowing;
                    messageBg.ShadowColor = Color.Black;
                    messageBg.Scale = cfg.hudScale;

                    messageFore = new HudAPIv2.HUDMessage();
                    messageFore.Blend = BlendTypeEnum.LDR;
                    messageFore.Scale = cfg.hudScale;
                }

                UpdatePos();
                messageBg.Visible = true;
                messageBg.Message = GetTextBg();

                messageFore.Visible = messageBg.Visible;
                messageFore.Message = GetTextFore();
                messageFore.Origin = messageBg.Origin;
            }

            /// <summary>
            /// Updates position of menu on screen.
            /// </summary>
            private void UpdatePos()
            {
                Vector3D targetPos, worldPos;
                Vector2D screenPos, offset = messageBg.GetTextLength() * .5f;

                if (!cfg.forceToCenter)
                {
                    if (LocalPlayer.IsLookingInBlockDir(target.TBlock))
                    {
                        targetPos = target.GetPosition();
                        worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                        screenPos = new Vector2D(worldPos.X, worldPos.Y);

                        if (cfg.clampHudPos)
                        {
                            screenPos.X = Utilities.Clamp(screenPos.X, -.95 + offset.X, .95 - offset.X);
                            screenPos.Y = Utilities.Clamp(screenPos.Y, -.95 - offset.Y, .95 + offset.Y);
                        }

                        screenPos -= offset;
                        messageBg.Origin = screenPos;
                    }
                    else if (cfg.hideIfNotVis)
                        messageBg.Origin = new Vector2D(-double.MinValue, -double.MinValue);
                    else
                        messageBg.Origin = new Vector2D(-offset.X, -offset.Y);
                }
                else
                    messageBg.Origin = new Vector2D(-offset.X, -offset.Y);
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private StringBuilder GetTextFore()
            {
                StringBuilder sb = new StringBuilder();
                int elements = Utilities.Clamp(visEnd - visStart, 0, cfg.maxVisible), i, action;
                string colorCode;

                if (target.IsFunctional)
                    sb.AppendLine($"<color={cfg.textColors.deflt}>[{headerText}]");
                else
                    sb.AppendLine($"<color={cfg.textColors.blockInc}>[{headerText} (incomplete)]");

                for (int n = 0; n < elements; n++)
                {
                    i = n + visStart;
                    action = i - target.Properties.Count;

                    if (i == selection)
                        colorCode = $"<color={cfg.textColors.selected}>";
                    else if (i == index)
                        colorCode = $"<color={cfg.textColors.highlight}>";
                    else
                        colorCode = $"<color={cfg.textColors.deflt}>";

                    if (i >= target.Properties.Count)
                        sb.AppendLine(colorCode + target.Actions[action].GetName());
                    else
                        sb.AppendLine(colorCode + target.Properties[i].GetName());
                }

                sb.AppendLine($"<color={cfg.textColors.deflt}>[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");
                return sb;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to render the text shadows.
            /// </summary>
            private StringBuilder GetTextBg()
            {
                StringBuilder sb = new StringBuilder();
                int elements = Utilities.Clamp(visEnd - visStart, 0, cfg.maxVisible), i, action;
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
                Draw();                
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
            private void Draw()
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
                        list[notif].Text = target.Actions[action].GetName();
                    else
                        list[notif].Text = target.Properties[i].GetName();

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
