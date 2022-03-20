using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;
using Sandbox.ModAPI;

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
        public IPropertyBlock Target { get; private set; }

        /// <summary>
        /// Enables/disables debug text
        /// </summary>
        public static bool DrawDebug { get; set; }

        private readonly PropertyWheelMenu propertyWheel;
        private readonly PropertyListMenu propertyList;
        private readonly Label debugText;
        private readonly StringBuilder notifText;
        private int textUpdateTick;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            propertyList = new PropertyListMenu(this) { Visible = false };
            propertyWheel = new PropertyWheelMenu(this) { Visible = false };

            propertyWheel.RegisterShortcut(new PropertyWheelShortcutEntry()
            {
                Text = "Open List Menu",
                ShortcutAction = () => OpenPropertyList(true)
            });

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };

            notifText = new StringBuilder();
        }

        /// <summary>
        /// Opens the menu using the given target
        /// </summary>
        public void OpenMenu(IPropertyBlock target, QuickActionMenuState initialState = default(QuickActionMenuState))
        {
            if ((MenuState & QuickActionMenuState.Peek) == 0)
            {
                CloseMenu();

                if (initialState != default(QuickActionMenuState))
                {
                    MenuState = initialState;
                }

                Target = target;
                UpdateStateMain();
            }
        }

        /// <summary>
        /// Closes and resets the menu
        /// </summary>
        public void CloseMenu()
        {
            propertyWheel.CloseMenu();
            propertyList.CloseMenu();

            MenuState = QuickActionMenuState.Closed;
        }

        /// <summary>
        /// Shows a temporary notification. Set continuous == true, if continuously
        /// updating text.
        /// </summary>
        public void ShowNotification(StringBuilder text, bool continuous = false)
        {
            if (propertyWheel.Visible)
            {
                propertyWheel.ShowNotification(text, continuous);   
            }
            else if (propertyList.Visible)
            {
                propertyList.ShowNotification(text, continuous);
            }
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
                debugBuilder.Append($"ConObj: {MyAPIGateway.Session.ControlledObject.GetType().Name}\n");
                debugBuilder.Append($"State: {MenuState}\n");
                debugBuilder.Append($"Wheel Menu Open: {propertyWheel.IsOpen}\n");
                debugBuilder.Append($"IsWidgetOpen: {propertyWheel.IsWidgetOpen}\n");
                debugBuilder.Append($"List Menu Open: {propertyList.IsOpen}\n");
                debugBuilder.Append($"Cursor Mode: {HudMain.InputMode}\n");
                debugBuilder.Append($"Blacklist Mode: {BindManager.BlacklistMode}\n");
                debugBuilder.Append($"Enable Cursor Pressed: {BvBinds.MultXOrMouse.IsPressed}\n");
            }

            debugText.Visible = DrawDebug;

            textUpdateTick++;
            textUpdateTick %= textTickDivider;
        }
    }
}