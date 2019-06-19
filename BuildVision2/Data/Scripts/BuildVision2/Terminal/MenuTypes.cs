using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class PropertiesMenu
    {
        private abstract class PropertyList
        {
            public bool Open { get; protected set; }

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
            public virtual void Show()
            {
                visStart = 0;
                visEnd = 0;
                Open = true;
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
                if (Target.ElementCount <= maxVisible)
                {
                    visEnd = Target.ElementCount;
                }
                else
                {
                    if (index >= (visStart + maxVisible))
                        visStart++;
                    else if (index < visStart)
                        visStart = index;

                    visEnd = Utils.Math.Clamp((visStart + maxVisible), 0, Target.ElementCount);
                    visStart = Utils.Math.Clamp(visEnd - maxVisible, 0, visEnd);
                }
            }
        }

        /// <summary>
        /// List GUI using HudUtilities elements
        /// </summary>
        private class ApiHud : PropertyList
        {
            public readonly HudUtilities.ScrollMenu menu;

            public ApiHud()
            {
                menu = new HudUtilities.ScrollMenu(20);
            }

            /// <summary>
            /// Updates current list position and ensures the menu is visible
            /// </summary>
            public override void Update(int index, int selection)
            {
                if (Open)
                {
                    this.index = index;
                    this.selection = selection;
                    headerText = ModBase.ModName;

                    maxVisible = ApiHudCfg.maxVisible;
                    menu.BodyColor = ApiHudCfg.colors.listBgColor;
                    menu.HeaderColor = ApiHudCfg.colors.headerColor;
                    menu.SelectionBoxColor = ApiHudCfg.colors.selectionBoxColor;
                    menu.TextScale = .85;
                    menu.Visible = true;

                    if (ApiHudCfg.resolutionScaling)
                        menu.Scale = ApiHudCfg.hudScale * HudUtilities.ResScale;
                    else
                        menu.Scale = ApiHudCfg.hudScale;

                    GetVisibleProperties();
                    UpdateText();
                    UpdatePos();
                }
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

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock))
                {
                    if (!ApiHudCfg.useCustomPos)
                    {
                        targetPos = Target.GetPosition();
                        worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                        screenPos = new Vector2D(worldPos.X, worldPos.Y);
                    }
                    else
                        screenPos = ApiHudCfg.hudPos;

                    screenBounds = new Vector2D(1d, 1d) - menu.Size / 2;

                    if (ApiHudCfg.clampHudPos)
                    {
                        screenPos.X = Utils.Math.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                        screenPos.Y = Utils.Math.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                    }
                }
                else
                    screenPos = Vector2D.Zero;

                menu.ScaledPos = screenPos;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private void UpdateText()
            {
                int elements = Utils.Math.Clamp(visEnd - visStart, 0, ApiHudCfg.maxVisible), i, action;
                string[] list = new string[elements];
                string colorCode;

                menu.HeaderText = $"<color={ApiHudCfg.colors.headerText}>{headerText}";
                menu.SelectionIndex = index - visStart;

                for (int n = 0; n < elements; n++)
                {
                    i = n + visStart;
                    action = i - Target.Properties.Count;

                    if (i == selection)
                        colorCode = $"<color={ApiHudCfg.colors.selectedText}>";
                    else if (i == index)
                        colorCode = $"<color={ApiHudCfg.colors.highlightText}>";
                    else
                        colorCode = $"<color={ApiHudCfg.colors.bodyText}>";

                    if (i >= Target.Properties.Count)
                        list[n] = colorCode + Target.Actions[action].Value;
                    else
                        list[n] = $"<color={ApiHudCfg.colors.bodyText}>{Target.Properties[i].Name}: {colorCode}{Target.Properties[i].Value}";
                }

                menu.ListText = list;
                menu.FooterLeftText = $"<color={ApiHudCfg.colors.headerText}>[{visStart + 1} - {visEnd} of {Target.ElementCount}]";

                if (Target.IsWorking)
                    menu.FooterRightText = $"<color={ApiHudCfg.colors.headerText}>[Working]";
                else if (Target.IsFunctional)
                    menu.FooterRightText = $"<color={ApiHudCfg.colors.headerText}>[Functional]";
                else
                    menu.FooterRightText = $"<color={ApiHudCfg.colors.blockIncText}>[Incomplete]";
            }
        }

        /// <summary>
        /// List GUI based on IMyHudNotification objects
        /// </summary>
        private class NotifHud : PropertyList
        {
            private readonly IMyHudNotification header;
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
                if (Open)
                {
                    this.index = index;
                    this.selection = selection;
                    headerText = ModBase.ModName;
                    maxVisible = NotifHudCfg.maxVisible;

                    if (list == null || list.Length < ApiHudCfg.maxVisible)
                        list = new IMyHudNotification[ApiHudCfg.maxVisible];

                    GetVisibleProperties();
                    UpdateText();
                }
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
                int elements = Utils.Math.Clamp(visEnd - visStart, 0, ApiHudCfg.maxVisible), i, action;

                header.Show();
                header.AliveTime = int.MaxValue;
                header.Text = $"{headerText} ({visStart + 1} - {visEnd} of {Target.ElementCount})";

                for (int notif = 0; notif < elements; notif++)
                {
                    if (list[notif] == null)
                        list[notif] = MyAPIGateway.Utilities.CreateNotification("");

                    i = notif + visStart;
                    action = i - Target.Properties.Count;

                    // make sure its still being shown
                    list[notif].Show();
                    list[notif].AliveTime = int.MaxValue;

                    // get name
                    if (i >= Target.Properties.Count)
                        list[notif].Text = Target.Actions[action].Value;
                    else
                        list[notif].Text = $"{Target.Properties[i].Name}: {Target.Properties[i].Value}";

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