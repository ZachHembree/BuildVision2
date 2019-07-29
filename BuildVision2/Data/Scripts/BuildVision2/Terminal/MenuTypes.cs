using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using System.Collections.Generic;
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

            protected int start, visStart, index, selection, maxVisible, visCount;
            protected string headerText;

            public PropertyList()
            {
                Open = false;
                index = 0;
                start = 0;
                visStart = 0;
                selection = -1;
            }

            /// <summary>
            /// Updates the menu's current target
            /// </summary>
            public virtual void Show()
            {
                start = 0;
                visStart = 0;
                Open = true;
            }

            public abstract void Update(int index, int selection);

            public virtual void Hide()
            {
                Open = false;
                index = 0;
                start = 0;
                visStart = 0;
                selection = -1;
            }

            /// <summary>
            /// Determines what range of the block's properties are visible based on index and total number of enabled properties.
            /// </summary>
            protected virtual void GetVisibleProperties()
            {
                int totalEnabled = Target.EnabledElementCount, visIndex = GetVisIndex();
                visCount = (maxVisible > totalEnabled) ? totalEnabled : maxVisible;

                if (index < start)
                {
                    visStart = visIndex;
                    start = index;
                }

                if (visIndex >= visStart + visCount || (visStart + visCount > totalEnabled || visStart < 0))
                {
                    int n = 0;
                    visStart = visIndex - visCount + 1;
                    start = index;

                    while (n < visCount - 1 && start > 0)
                    {
                        start--;

                        if (IsElementEnabled(start))
                            n++;
                    }
                }
            }

            /// <summary>
            /// Returns the visible position of the property at the current index
            /// </summary>
            protected int GetVisIndex()
            {
                int visIndex = 0;

                for (int n = 0; n < index; n++)
                {
                    if (IsElementEnabled(n))
                        visIndex++;
                }

                return visIndex;
            }
        }

        /// <summary>
        /// List GUI using HudUtilities elements
        /// </summary>
        private class ApiHud : PropertyList
        {
            private readonly BvScrollMenu menu;
            private readonly List<string> propertyList;

            public ApiHud()
            {
                menu = new BvScrollMenu(20) { Visible = false, TextScale = .85 }; // .85, .92
                propertyList = new List<string>(20);
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
                    menu.list.BgColor = ApiHudCfg.colors.listBgColor;
                    menu.header.BgColor = ApiHudCfg.colors.headerColor;
                    menu.footer.BgColor = ApiHudCfg.colors.headerColor;
                    menu.selectionBox.color = ApiHudCfg.colors.selectionBoxColor;
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
                Vector2D screenPos, screenBounds = Vector2D.One;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !ApiHudCfg.useCustomPos)
                {
                    menu.originAlignment = OriginAlignment.Center;
                    targetPos = Target.GetPosition();
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                    screenPos = new Vector2D(worldPos.X, worldPos.Y);
                    screenBounds -= menu.RelativeSize * menu.Scale / 2d;
                }
                else
                {
                    menu.originAlignment = OriginAlignment.Auto;
                    screenPos = ApiHudCfg.hudPos;
                }

                if (ApiHudCfg.clampHudPos)
                {
                    screenPos.X = Utils.Math.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                    screenPos.Y = Utils.Math.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                menu.ScaledOrigin = screenPos;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private void UpdateText()
            {
                int i = start, action;
                string colorCode;

                propertyList.Clear();
                menu.header.Text = $"<color={ApiHudCfg.colors.headerText}>{headerText}";

                for (int n = 0; (n < visCount && i < Target.ElementCount); n++)
                {
                    action = i - Target.Properties.Count;

                    if (i == selection)
                    {
                        colorCode = $"<color={ApiHudCfg.colors.selectedText}>";
                        menu.SelectionIndex = propertyList.Count;
                    }
                    else if (i == index)
                    {
                        colorCode = $"<color={ApiHudCfg.colors.highlightText}>";
                        menu.SelectionIndex = propertyList.Count;
                    }
                    else
                        colorCode = $"<color={ApiHudCfg.colors.bodyText}>";

                    if (i >= Target.Properties.Count)
                        propertyList.Add(colorCode + Target.Actions[action].Value);
                    else
                        propertyList.Add($"<color={ApiHudCfg.colors.bodyText}>{Target.Properties[i].Name}: {colorCode}{Target.Properties[i].Value}");

                    i++;

                    while (i < Target.ElementCount && !IsElementEnabled(i))
                        i++;
                }

                menu.list.ListText = propertyList.ToArray();
                menu.footer.LeftText = $"<color={ApiHudCfg.colors.headerText}>[{visStart + 1} - {visStart + visCount} of {Target.EnabledElementCount}]";

                if (Target.IsWorking)
                    menu.footer.RightText = $"<color={ApiHudCfg.colors.headerText}>[Working]";
                else if (Target.IsFunctional)
                    menu.footer.RightText = $"<color={ApiHudCfg.colors.headerText}>[Functional]";
                else
                    menu.footer.RightText = $"<color={ApiHudCfg.colors.blockIncText}>[Incomplete]";
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
                int i = start, action;

                header.Show();
                header.AliveTime = int.MaxValue;
                header.Text = $"{headerText} ({visStart + 1} - {visStart + visCount} of {Target.EnabledElementCount})";

                for (int n = 0; (i < Target.ElementCount && n < visCount); n++)
                {
                    if (list[n] == null)
                        list[n] = MyAPIGateway.Utilities.CreateNotification("");

                    action = i - Target.Properties.Count;

                    // make sure its still being shown
                    list[n].Show();
                    list[n].AliveTime = int.MaxValue;

                    // get name
                    if (i >= Target.Properties.Count)
                        list[n].Text = Target.Actions[action].Value;
                    else
                        list[n].Text = $"{Target.Properties[i].Name}: {Target.Properties[i].Value}";

                    // get color
                    if (i == selection)
                        list[n].Font = MyFontEnum.Green;
                    else if (i == index)
                        list[n].Font = MyFontEnum.Red;
                    else
                        list[n].Font = MyFontEnum.White;

                    i++;

                    while (i < Target.ElementCount && !IsElementEnabled(i))
                        i++;
                }

                // hide everything else
                for (int n = visCount; n < list.Length; n++)
                {
                    if (list[n] != null)
                    {
                        list[n].Text = "";
                        list[n].Hide();
                    }
                }
            }
        }
    }
}