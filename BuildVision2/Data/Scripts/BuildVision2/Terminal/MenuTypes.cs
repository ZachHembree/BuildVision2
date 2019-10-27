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

            public virtual void Draw() { }

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
                int totalEnabled = Target.EnabledMembers, visIndex = GetVisIndex();
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

                        if (Target.BlockMembers[start].Enabled)
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
                    if (Target.BlockMembers[n].Enabled)
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
            private static readonly Color
                bodyTextColor = new Color(210, 235, 245),
                headerTextColor = new Color(210, 235, 245),
                blockIncTextColor = new Color(200, 35, 35),
                highlightTextColor = new Color(220, 190, 20),
                selectedTextColor = new Color(50, 200, 50),
                headerColor = new Color(41, 54, 62),
                listBgColor = new Color(70, 78, 86),
                selectionBoxColor = new Color(41, 54, 62);

            public ApiHud()
            {
                menu = new BvScrollMenu(20) { Visible = false, TextScale = .885f };

                menu.list.BgColor = listBgColor;
                menu.header.BgColor = headerColor;
                menu.footer.BgColor = headerColor;
                menu.selectionBox.Color = selectionBoxColor;
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
                    menu.BgOpacity = ApiHudCfg.hudOpacity;
                    menu.Visible = true;

                    if (ApiHudCfg.resolutionScaling)
                        menu.Scale = ApiHudCfg.hudScale * HudMain.ResScale;
                    else
                        menu.Scale = ApiHudCfg.hudScale;

                    GetVisibleProperties();
                    UpdateText();
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
            public override void Draw()
            {
                Vector3D targetPos, worldPos;
                Vector2 screenPos, screenBounds = Vector2.One;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !ApiHudCfg.useCustomPos)
                {
                    menu.OriginAlignment = OriginAlignment.Center;
                    targetPos = Target.GetPosition() + Target.modelOffset * .75;
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                    screenPos = new Vector2((float)worldPos.X, (float)worldPos.Y);
                    screenBounds -= menu.NativeSize * menu.Scale / 2f;
                }
                else
                {
                    menu.OriginAlignment = OriginAlignment.Auto;
                    screenPos = ApiHudCfg.hudPos;
                }

                if (ApiHudCfg.clampHudPos)
                {
                    screenPos.X = Utils.Math.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                    screenPos.Y = Utils.Math.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                menu.NativeOrigin = screenPos;
            }

            /// <summary>
            /// Gets finished string for the Text HUD API to display.
            /// </summary>
            private void UpdateText()
            {
                int i = start;
                RichText currentText;
                GlyphFormat currentFormat,
                    headerFormat = new GlyphFormat(headerTextColor),
                    bodyFormat = new GlyphFormat(bodyTextColor),
                    selectionFormat = new GlyphFormat(selectedTextColor),
                    highlightFormat = new GlyphFormat(highlightTextColor);

                menu.list.Clear();
                menu.header.Text.Append(new RichText(headerText, headerFormat));

                for (int n = 0; (n < visCount && i < Target.BlockMembers.Count); n++)
                {
                    currentText = new RichText();

                    if (i == selection)
                    {
                        currentFormat = selectionFormat;
                        menu.SelectionIndex = menu.list.Count;
                    }
                    else if (i == index)
                    {
                        currentFormat = highlightFormat;
                        menu.SelectionIndex = menu.list.Count;
                    }
                    else
                        currentFormat = bodyFormat;

                    if (Target.BlockMembers[i].Name.Length > 0)
                        currentText += new RichText($"{Target.BlockMembers[i].Name}: ", bodyFormat);

                    currentText += new RichText(Target.BlockMembers[i].Value, currentFormat);

                    menu.list.Add(currentText);
                    i++;

                    while (i < Target.BlockMembers.Count && !Target.BlockMembers[i].Enabled)
                        i++;
                }

                menu.footer.LeftText.Append(new RichText($"[{visStart + 1} - {visStart + visCount} of {Target.EnabledMembers}]", headerFormat));

                if (Target.IsWorking)
                    menu.footer.RightText.Append(new RichText("[Working]", headerFormat));
                else if (Target.IsFunctional)
                    menu.footer.RightText.Append(new RichText("[Functional]", headerFormat));
                else
                    menu.footer.RightText.Append(new RichText("[Incomplete]", new GlyphFormat(blockIncTextColor)));
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
                int i = start;

                header.Show();
                header.AliveTime = int.MaxValue;
                header.Text = $"{headerText} ({visStart + 1} - {visStart + visCount} of {Target.EnabledMembers})";

                for (int n = 0; (i < Target.BlockMembers.Count && n < visCount); n++)
                {
                    if (list[n] == null)
                        list[n] = MyAPIGateway.Utilities.CreateNotification("");

                    // make sure its still being shown
                    list[n].Show();
                    list[n].AliveTime = int.MaxValue;

                    // get name
                    if (Target.BlockMembers[i].Name.Length == 0)
                        list[n].Text = Target.BlockMembers[i].Value;
                    else
                        list[n].Text = $"{Target.BlockMembers[i].Name}: {Target.BlockMembers[i].Value}";

                    // get color
                    if (i == selection)
                        list[n].Font = MyFontEnum.Green;
                    else if (i == index)
                        list[n].Font = MyFontEnum.Red;
                    else
                        list[n].Font = MyFontEnum.White;

                    i++;

                    while (i < Target.BlockMembers.Count && !Target.BlockMembers[i].Enabled)
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