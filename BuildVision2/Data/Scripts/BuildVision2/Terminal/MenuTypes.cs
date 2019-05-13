using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using VRage.Utils;
using System.Xml.Serialization;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class PropertiesMenu
    {
        private abstract class PropertyList
        {
            public bool Open { get; protected set; }

            protected PropertyBlock target;
            protected int visStart, visEnd, index, selection, maxVisible;
            protected string headerText;

            public PropertyList()
            {
                Open = false;
                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;
            }

            /// <summary>
            /// Updates the menu's current target
            /// </summary>
            public virtual void SetTarget(PropertyBlock target)
            {
                this.target = target;
                visStart = 0;
                visEnd = 0;
            }

            public abstract void Update(int index, int selection);

            public virtual void Hide()
            {
                Open = false;
                index = 0;
                visStart = 0;
                visEnd = 0;
                selection = -1;
            }

            /// <summary>
            /// Determines what range of the block's properties are visible based on index and total number of properties.
            /// </summary>
            protected virtual void GetVisibleProperties()
            {
                if (target.ScrollableCount <= maxVisible)
                {
                    visEnd = target.ScrollableCount;
                }
                else
                {
                    if (index >= (visStart + maxVisible))
                        visStart++;
                    else if (index < visStart)
                        visStart = index;

                    visEnd = Utilities.Clamp((visStart + maxVisible), 0, target.ScrollableCount);
                    visStart = Utilities.Clamp(visEnd - maxVisible, 0, visEnd); 
                }
            }
        }

        /// <summary>
        /// List GUI using HudUtilities elements
        /// </summary>
        private class ApiHud : PropertyList
        {
            private HudUtilities.ScrollMenu menu;

            public ApiHud()
            {
                menu = new HudUtilities.ScrollMenu(20);
            }

            /// <summary>
            /// Updates current list position and ensures the menu is visible
            /// </summary>
            public override void Update(int index, int selection)
            {
                this.index = index;
                this.selection = selection;
                headerText = target.TBlock.CustomName;

                maxVisible = ApiHudCfg.maxVisible;
                menu.BodyColor = ApiHudCfg.colors.backgroundColor;
                menu.HeaderColor = ApiHudCfg.colors.headerColor;
                menu.SelectionBoxColor = ApiHudCfg.colors.selectionBoxColor;
                menu.Scale = ApiHudCfg.hudScale;
                Open = true;
                menu.Visible = true;

                GetVisibleProperties();
                UpdateText();
                UpdatePos();
            }

            /// <summary>
            /// Hides API HUD
            /// </summary>
            public override void Hide()
            {
                if (menu != null)
                    menu.Visible = false;

                base.Hide();
            }
   
            /// <summary>
            /// Updates position of menu on screen.
            /// </summary>
            private void UpdatePos()
            {
                Vector3D targetPos, worldPos;
                Vector2D screenPos, screenBounds;
                
                if (!ApiHudCfg.forceToCenter)
                {
                    if (LocalPlayer.IsLookingInBlockDir(target.TBlock))
                    {
                        targetPos = target.GetPosition();
                        worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                        screenPos = new Vector2D(worldPos.X, worldPos.Y);
                        screenBounds = new Vector2D(1d, 1d) - menu.Size / 2;

                        if (ApiHudCfg.clampHudPos)
                        {
                            screenPos.X = Utilities.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                            screenPos.Y = Utilities.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                        }
                    }
                    else
                        screenPos = Vector2D.Zero;
                }
                else
                    screenPos = Vector2D.Zero;

                menu.ScaledPos = screenPos;
                menu.SelectionIndex = index - visStart;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private void UpdateText()
            {
                int elements = Utilities.Clamp(visEnd - visStart, 0, ApiHudCfg.maxVisible), i, action;
                StringBuilder[] list = new StringBuilder[elements];
                string colorCode;

                menu.HeaderText = new StringBuilder($"<color={ApiHudCfg.colors.headerText}>{headerText}");

                for (int n = 0; n < elements; n++)
                {
                    i = n + visStart;
                    action = i - target.Properties.Count;

                    if (i == selection)
                        colorCode = $"<color={ApiHudCfg.colors.selectedText}>";
                    else if (i == index)
                        colorCode = $"<color={ApiHudCfg.colors.highlightText}>";
                    else
                        colorCode = $"<color={ApiHudCfg.colors.bodyText}>";

                    if (i >= target.Properties.Count)
                        list[n] = new StringBuilder(colorCode + target.Actions[action].GetDisplay());
                    else
                        list[n] = new StringBuilder(
                            $"<color={ApiHudCfg.colors.bodyText}>{target.Properties[i].GetName()}: {colorCode}{target.Properties[i].GetValue()}");
                }

                menu.ListText = list;
                menu.FooterLeftText = new StringBuilder(
                        $"<color={ApiHudCfg.colors.headerText}>[{visStart + 1} - {visEnd} of {target.ScrollableCount}]");

                if (target.IsWorking)
                    menu.FooterRightText = new StringBuilder($"<color={ApiHudCfg.colors.headerText}>[Working]");
                else if (target.IsFunctional)
                    menu.FooterRightText = new StringBuilder($"<color={ApiHudCfg.colors.headerText}>[Functional]");
                else
                    menu.FooterRightText = new StringBuilder($"<color={ApiHudCfg.colors.blockIncText}>[Incomplete]");
            }
        }

        /// <summary>
        /// List GUI based on IMyHudNotification objects
        /// </summary>
        private class NotifHud : PropertyList
        {
            private IMyHudNotification header;
            private IMyHudNotification[] list;

            public NotifHud()
            {
                header = MyAPIGateway.Utilities.CreateNotification("");
                list = new IMyHudNotification[ApiHudCfg.maxVisible];
            }

            /// <summary>
            /// Updates the current list position and draws the menu.
            /// </summary>
            public override void Update(int index, int selection)
            {
                this.index = index;
                this.selection = selection;
                headerText = $"Build Vision: {target.TBlock.CustomName}";
                maxVisible = ApiHudCfg.maxVisible;
                Open = true;

                if (list == null || list.Length < ApiHudCfg.maxVisible)
                    list = new IMyHudNotification[ApiHudCfg.maxVisible];

                GetVisibleProperties();
                UpdateText();                
            }

            /// <summary>
            /// Hides notificatins
            /// </summary>
            public override void Hide()
            {
                header.Hide();

                foreach (IMyHudNotification prop in list)
                {
                    if (prop != null)
                        prop.Hide();
                }

                base.Hide();
            }

            /// <summary>
            /// Updates text colors and resets alive time for fallback hud.
            /// </summary>
            private void UpdateText()
            {
                int elements = Utilities.Clamp(visEnd - visStart, 0, ApiHudCfg.maxVisible), i, action;

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
