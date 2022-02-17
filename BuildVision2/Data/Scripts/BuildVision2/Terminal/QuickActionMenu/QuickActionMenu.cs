using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        /// <summary>
        /// Returns the current state of the menu
        /// </summary>
        public QuickActionMenuState MenuState { get; private set; }

        /// <summary>
        /// Currently assigned block target
        /// </summary>
        public PropertyBlock Target { get; private set; }

        /// <summary>
        /// Enables/disables debug text
        /// </summary>
        public static bool DrawDebug { get; set; }

        private readonly PropertyWheelMenu propertyWheel;
        private readonly PropertyListMenu propertyList;
        private readonly Label debugText;
        private int textUpdateTick;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            propertyWheel = new PropertyWheelMenu(this) { Visible = false };
            propertyList = new PropertyListMenu(this) { Visible = false };

            propertyWheel.RegisterShortcut(new PropertyWheelShortcutEntry()
            {
                Text = "Open List Menu",
                ShortcutAction = OpenPropertyList
            });

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };
        }

        /// <summary>
        /// Opens the menu using the given target
        /// </summary>
        public void OpenMenu(PropertyBlock target)
        {
            if (MenuState == QuickActionMenuState.Closed || Target != target)
            {
                CloseMenu();
                Target = target;
                Visible = true;
            }
        }

        /// <summary>
        /// Closes and resets the menu
        /// </summary>
        public void CloseMenu()
        {
            propertyWheel.CloseMenu();
            propertyList.CloseMenu();

            Target = null;
            Visible = false;
            MenuState = QuickActionMenuState.Closed;
        }

        protected override void Layout()
        {
            if ((MenuState & QuickActionMenuState.ListMenuOpen) > 0)
                Size = propertyList.Size;
            else
                Size = propertyWheel.Size;

            if (DrawDebug && textUpdateTick == 0)
            {
                ITextBuilder debugBuilder = debugText.TextBoard;
                debugBuilder.Clear();
                debugBuilder.Append($"State: {MenuState}\n");
                debugBuilder.Append($"Wheel Menu Open: {propertyWheel.IsOpen}\n");
                debugBuilder.Append($"IsWidgetOpen: {propertyWheel.IsWidgetOpen}\n");
                debugBuilder.Append($"List Menu Open: {propertyList.IsOpen}\n");
                debugBuilder.Append($"Cursor Mode: {HudMain.InputMode}\n");
                debugBuilder.Append($"Blacklist Mode: {BindManager.BlacklistMode}\n");
                debugBuilder.Append($"Enable Cursor Pressed: {BvBinds.EnableMouse.IsPressed}\n");
            }

            debugText.Visible = DrawDebug;

            textUpdateTick++;
            textUpdateTick %= textTickDivider;
        }
    }
}