using System.Collections.Generic;
using System;
using VRageMath;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Generates a settings menu using elements based on <see cref="TextHudApi.HudAPIv2.MenuItemBase"/>
    /// </summary>
    internal sealed class SettingsMenu : ModBase.Component<SettingsMenu>
    {
        private static BuildVision2 Main { get { return BuildVision2.Instance as BuildVision2; } }
        private static HudUtilities.MenuRoot SettingsMenuRoot { get { return HudUtilities.MenuRoot.Instance; } }

        /// <summary>
        /// Generates a collection containing all settings menu elements.
        /// </summary>
        public SettingsMenu()
        {
            HudUtilities.MenuRoot.Init("Build Vision", "Build Vision Settings");

            SettingsMenuRoot.AddChildren(new List<HudUtilities.IMenuElement>()
            {
                // General Settings
                new HudUtilities.MenuButton(
                    () => $"Force Fallback Hud: {PropertiesMenu.Cfg.forceFallbackHud}",
                    () => PropertiesMenu.Cfg.forceFallbackHud = !PropertiesMenu.Cfg.forceFallbackHud),
                new HudUtilities.MenuButton(
                    () => $"Close If Not In View: {Main.Cfg.general.closeIfNotInView}",
                    () => Main.Cfg.general.closeIfNotInView = !Main.Cfg.general.closeIfNotInView),
                new HudUtilities.MenuButton(
                    () => $"Can Open While Holding Tools: {Main.Cfg.general.canOpenIfHolding}",
                    () => Main.Cfg.general.canOpenIfHolding = !Main.Cfg.general.canOpenIfHolding),

                // GUI Settings
                new HudUtilities.MenuCategory("GUI Settings", "GUI Settings", new List<HudUtilities.IMenuElement>()
                {
                    new HudUtilities.MenuSliderInput(
                        () => $"Hud Scale: {Math.Round(PropertiesMenu.ApiHudCfg.hudScale, 2)}",
                        "Hud Scale", 0.5f, 2f,
                        () => PropertiesMenu.ApiHudCfg.hudScale,
                        (float scale) => PropertiesMenu.ApiHudCfg.hudScale = scale),
                    new HudUtilities.MenuSliderInput(
                        () => $"Max Visible Properties: {PropertiesMenu.ApiHudCfg.maxVisible}",
                        "Max Visible Properties", 6, 20,
                        () => PropertiesMenu.ApiHudCfg.maxVisible,
                        (float maxVisible) => PropertiesMenu.ApiHudCfg.maxVisible = (int)maxVisible),
                    new HudUtilities.MenuButton(
                        () => $"Clamp To Screen Edges: {PropertiesMenu.ApiHudCfg.clampHudPos}",
                        () => PropertiesMenu.ApiHudCfg.clampHudPos = !PropertiesMenu.ApiHudCfg.clampHudPos),
                    new HudUtilities.MenuButton(
                        () => $"Lock To Screen Center: {PropertiesMenu.ApiHudCfg.forceToCenter}",
                        () => PropertiesMenu.ApiHudCfg.forceToCenter = !PropertiesMenu.ApiHudCfg.forceToCenter),
                
                    //Text Colors
                    new HudUtilities.MenuCategory("Text Colors", "Text Colors", new List<HudUtilities.IMenuElement>()
                    {
                        new HudUtilities.MenuColorInput("Body Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.bodyText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.bodyText = Utilities.GetColorString(color, false), false),
                        new HudUtilities.MenuColorInput("Header Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.headerText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.headerText = Utilities.GetColorString(color, false), false),
                        new HudUtilities.MenuColorInput("Block Inc Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.blockIncText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.blockIncText = Utilities.GetColorString(color, false), false),
                        new HudUtilities.MenuColorInput("Highlight Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.highlightText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.highlightText = Utilities.GetColorString(color, false), false),
                        new HudUtilities.MenuColorInput("Selection Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.selectedText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.selectedText = Utilities.GetColorString(color, false), false),
                    }),

                    //Background Colors
                    new HudUtilities.MenuCategory("Background Colors", "Background Colors", new List<HudUtilities.IMenuElement>()
                    {
                        new HudUtilities.MenuColorInput("List Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.backgroundColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.backgroundColor = color),
                        new HudUtilities.MenuColorInput("Selection Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.selectionBoxColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.selectionBoxColor = color),
                        new HudUtilities.MenuColorInput("Header Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.headerColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.headerColor = color),
                    }),

                    new HudUtilities.MenuButton(
                        "Reset Settings",
                        () => PropertiesMenu.ApiHudCfg = ApiHudConfig.Defaults
                    ),
                }),

                // Bind Settings
                new HudUtilities.MenuCategory("Bind Settings", "Key Binds", GetBindSettings()),

                // Property Settings
                new HudUtilities.MenuCategory("Property Settings", "Property Settings", new List<HudUtilities.IMenuElement>()
                {
                    // Float Properties
                    new HudUtilities.MenuTextInput(
                        () => $"Float Div: {PropertyBlock.Cfg.floatDiv}", 
                        "Float Property Base Divisor",
                        (string input) =>
                        {
                            double.TryParse(input, out PropertyBlock.Cfg.floatDiv);
                            PropertyBlock.Cfg.Validate();
                        }),

                    new HudUtilities.MenuCategory("Float Multipliers", "Float Multipliers", new List<HudUtilities.IMenuElement>()
                    {
                        new HudUtilities.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.floatMult.X}",
                            "Float Multiplier X",
                            (string input) => 
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new HudUtilities.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.floatMult.Y}",
                            "Float Multiplier Y",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new HudUtilities.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.floatMult.Z}",
                            "Float Multiplier Z",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Z);
                                PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    // Color Properties
                    new HudUtilities.MenuCategory("Color Multipliers", "Color Multipliers", new List<HudUtilities.IMenuElement>()
                    {
                        new HudUtilities.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.colorMult.X}", 
                            "Color Multiplier X",
                            (string input) => 
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new HudUtilities.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.colorMult.Y}",
                            "Color Multiplier Y",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new HudUtilities.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.colorMult.Z}",
                            "Color Multiplier Z",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.Z);
                                PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    new HudUtilities.MenuButton(
                        "Reset Settings", 
                        () => PropertyBlock.Cfg = PropBlockConfig.Defaults
                    ),
                }),

                // General Settings Continued
                new HudUtilities.MenuButton(
                    () => "Open Help Menu",
                    () => ModBase.ShowMessageScreen("Help", ChatCommands.GetHelpMessage())),
                new HudUtilities.MenuButton(
                    () => "Reset Config",
                    () => Main.ResetConfig()),
                new HudUtilities.MenuButton(
                    () => "Save Config",
                    () => Main.SaveConfig()),
                new HudUtilities.MenuButton(
                    () => "Load Config",
                    () => Main.LoadConfig()),
            });
        }

        private List<HudUtilities.IMenuElement> GetBindSettings()
        {
            List<HudUtilities.IMenuElement> bindSettings = new List<HudUtilities.IMenuElement>(KeyBinds.BindManager.Count + 2);

            for (int n = 0; n < KeyBinds.BindManager.Count; n++)
            {
                bindSettings.Add(new HudUtilities.MenuTextInput(
                    KeyBinds.BindManager[n].Name, 
                    "Enter Control Names", 
                    (string input) =>
                    {
                        string[] args;
                        input = input.ToLower();

                        if (ChatCommands.CmdManager.TryParseCommand($"{KeyBinds.BindManager[n].Name} {input}", out args))
                            KeyBinds.BindManager.TryUpdateBind(KeyBinds.BindManager[n].Name, args);
                    }));
            }

            bindSettings.Add(new HudUtilities.MenuButton(
                () => "Open Bind Help Menu",
                () => ModBase.ShowMessageScreen("Bind Help", ChatCommands.GetBindHelpMessage())));

            bindSettings.Add(new HudUtilities.MenuButton(
                () => "Reset Binds",
                () => KeyBinds.Cfg = BindsConfig.Defaults));

            return bindSettings;
        }
    }
}