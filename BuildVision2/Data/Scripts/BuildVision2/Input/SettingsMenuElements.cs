using System.Collections.Generic;
using System;
using VRageMath;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class BvMain
    {
        /// <summary>
        /// Generates list of settings for configuring Build Vision via the Text HUD API's Mod Menu
        /// </summary>
        private List<SettingsMenu.IMenuElement> GetSettingsMenuElements()
        {
            return new List<SettingsMenu.IMenuElement>()
            {
                // General Settings Pt.1
                new SettingsMenu.MenuButton(
                        () => $"Force Fallback Hud: {PropertiesMenu.Cfg.forceFallbackHud}",
                        () => PropertiesMenu.Cfg.forceFallbackHud = !PropertiesMenu.Cfg.forceFallbackHud),
                new SettingsMenu.MenuButton(
                        () => $"Close If Not In View: {Cfg.general.closeIfNotInView}",
                        () => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView),
                new SettingsMenu.MenuButton(
                        () => $"Can Open While Holding Tools: {Cfg.general.canOpenIfHolding}",
                        () => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding),

                // GUI Settings
                new SettingsMenu.MenuCategory("GUI Settings", "GUI Settings", new List<SettingsMenu.IMenuElement>()
                {
                    new SettingsMenu.MenuSliderInput(
                        () => $"Hud Scale: {Math.Round(PropertiesMenu.ApiHudCfg.hudScale, 2)}",
                        "Hud Scale", 0.5f, 2f,
                        () => PropertiesMenu.ApiHudCfg.hudScale,
                        (float scale) => PropertiesMenu.ApiHudCfg.hudScale = scale),
                    new SettingsMenu.MenuSliderInput(
                        () => $"Max Visible Properties: {PropertiesMenu.ApiHudCfg.maxVisible}",
                        "Max Visible Properties", 6, 20,
                        () => PropertiesMenu.ApiHudCfg.maxVisible,
                        (float maxVisible) => PropertiesMenu.ApiHudCfg.maxVisible = (int)maxVisible),
                    new SettingsMenu.MenuButton(
                        () => $"Clamp To Screen Edges: {PropertiesMenu.ApiHudCfg.clampHudPos}",
                        () => PropertiesMenu.ApiHudCfg.clampHudPos = !PropertiesMenu.ApiHudCfg.clampHudPos),
                    new SettingsMenu.MenuButton(
                        () => $"Lock To Screen Center: {PropertiesMenu.ApiHudCfg.forceToCenter}",
                        () => PropertiesMenu.ApiHudCfg.forceToCenter = !PropertiesMenu.ApiHudCfg.forceToCenter),
                
                    //Text Colors
                    new SettingsMenu.MenuCategory("Text Colors", "Text Colors", new List<SettingsMenu.IMenuElement>()
                    {
                        new SettingsMenu.MenuColorInput("Body Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.bodyText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.bodyText = Utilities.GetColorString(color, false), false),
                        new SettingsMenu.MenuColorInput("Header Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.headerText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.headerText = Utilities.GetColorString(color, false), false),
                        new SettingsMenu.MenuColorInput("Block Inc Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.blockIncText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.blockIncText = Utilities.GetColorString(color, false), false),
                        new SettingsMenu.MenuColorInput("Highlight Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.highlightText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.highlightText = Utilities.GetColorString(color, false), false),
                        new SettingsMenu.MenuColorInput("Selection Text Color",
                            () => Utilities.ParseColor(PropertiesMenu.ApiHudCfg.colors.selectedText),
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.selectedText = Utilities.GetColorString(color, false), false),
                    }),

                    //Background Colors
                    new SettingsMenu.MenuCategory("Background Colors", "Background Colors", new List<SettingsMenu.IMenuElement>()
                    {
                        new SettingsMenu.MenuColorInput("List Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.backgroundColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.backgroundColor = color),
                        new SettingsMenu.MenuColorInput("Selection Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.selectionBoxColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.selectionBoxColor = color),
                        new SettingsMenu.MenuColorInput("Header Bg Color",
                            () => PropertiesMenu.ApiHudCfg.colors.headerColor,
                            (Color color) => PropertiesMenu.ApiHudCfg.colors.headerColor = color),
                    }),

                    new SettingsMenu.MenuButton(
                        "Reset Settings",
                        () => PropertiesMenu.ApiHudCfg = ApiHudConfig.Defaults
                    ),
                }),

                // Bind Settings
                new SettingsMenu.MenuCategory("Bind Settings", "Key Binds", GetBindSettings()),

                // Property Settings
                new SettingsMenu.MenuCategory("Property Settings", "Property Settings", new List<SettingsMenu.IMenuElement>()
                {
                    // Float Properties
                    new SettingsMenu.MenuTextInput(
                        () => $"Float Div: {PropertyBlock.Cfg.floatDiv}",
                        "Float Property Base Divisor",
                        (string input) =>
                        {
                            double.TryParse(input, out PropertyBlock.Cfg.floatDiv);
                            PropertyBlock.Cfg.Validate();
                        }),

                    new SettingsMenu.MenuCategory("Float Multipliers", "Float Multipliers", new List<SettingsMenu.IMenuElement>()
                    {
                        new SettingsMenu.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.floatMult.X}",
                            "Float Multiplier X",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new SettingsMenu.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.floatMult.Y}",
                            "Float Multiplier Y",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new SettingsMenu.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.floatMult.Z}",
                            "Float Multiplier Z",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Z);
                                PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    // Color Properties
                    new SettingsMenu.MenuCategory("Color Multipliers", "Color Multipliers", new List<SettingsMenu.IMenuElement>()
                    {
                        new SettingsMenu.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.colorMult.X}",
                            "Color Multiplier X",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new SettingsMenu.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.colorMult.Y}",
                            "Color Multiplier Y",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new SettingsMenu.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.colorMult.Z}",
                            "Color Multiplier Z",
                            (string input) =>
                            {
                            int.TryParse(input, out PropertyBlock.Cfg.colorMult.Z);
                            PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    new SettingsMenu.MenuButton(
                        "Reset Settings",
                        () => PropertyBlock.Cfg = PropBlockConfig.Defaults
                    ),
                }),

                // General Settings Pt.2
                new SettingsMenu.MenuButton(
                        () => "Open Help Menu",
                        () => ModBase.ShowMessageScreen("Help", GetHelpMessage())),
                new SettingsMenu.MenuButton(
                        () => "Reset Config",
                        () => ResetConfig()),
                new SettingsMenu.MenuButton(
                        () => "Save Config",
                        () => SaveConfig()),
                new SettingsMenu.MenuButton(
                        () => "Load Config",
                        () => LoadConfig()),
            };
        }

        /// <summary>
        /// Creates list of settings for configuring keybinds via the Text HUD API's Mod Menu
        /// </summary>
        private static List<SettingsMenu.IMenuElement> GetBindSettings()
        {
            List<SettingsMenu.IMenuElement> bindSettings = new List<SettingsMenu.IMenuElement>(KeyBinds.BindManager.Count + 2);

            for (int n = 0; n < KeyBinds.BindManager.Count; n++)
            {
                bindSettings.Add(new SettingsMenu.MenuTextInput(
                    KeyBinds.BindManager[n].Name, 
                    "Enter Control Names", 
                    (string input) =>
                    {
                        string[] args;
                        input = input.ToLower();

                        if (CmdManager.TryParseCommand($"{KeyBinds.BindManager[n].Name} {input}", out args))
                            KeyBinds.BindManager.TryUpdateBind(KeyBinds.BindManager[n].Name, args);
                    }));
            }

            bindSettings.Add(new SettingsMenu.MenuButton(
                () => "Open Bind Help Menu",
                () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())));

            bindSettings.Add(new SettingsMenu.MenuButton(
                () => "Reset Binds",
                () => KeyBinds.Cfg = BindsConfig.Defaults));

            return bindSettings;
        }
    }
}