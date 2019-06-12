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
        private List<MenuUtilities.IMenuElement> GetSettingsMenuElements()
        {
            return new List<MenuUtilities.IMenuElement>()
            {
                // General Settings Pt.1
                new MenuUtilities.MenuButton(
                        () => $"Force Fallback Hud: {PropertiesMenu.Cfg.forceFallbackHud}",
                        () => PropertiesMenu.Cfg.forceFallbackHud = !PropertiesMenu.Cfg.forceFallbackHud),
                new MenuUtilities.MenuButton(
                        () => $"Close If Not In View: {Cfg.general.closeIfNotInView}",
                        () => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView),
                new MenuUtilities.MenuButton(
                        () => $"Can Open While Holding Tools: {Cfg.general.canOpenIfHolding}",
                        () => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding),
                
                // GUI Settings
                new MenuUtilities.MenuCategory("GUI Settings", "GUI Settings", new List<MenuUtilities.IMenuElement>()
                {
                    new MenuUtilities.MenuButton(
                        () => $"Enable Resolution Scaling: {PropertiesMenu.ApiHudCfg.resolutionScaling}",
                        () => PropertiesMenu.ApiHudCfg.resolutionScaling = !PropertiesMenu.ApiHudCfg.resolutionScaling),
                    new MenuUtilities.MenuSliderInput(
                        () => $"Hud Scale: {Math.Round(PropertiesMenu.ApiHudCfg.hudScale, 2)}",
                        "Hud Scale", 0.5f, 2f,
                        () => PropertiesMenu.ApiHudCfg.hudScale,
                        (float scale) => PropertiesMenu.ApiHudCfg.hudScale = scale),
                    new MenuUtilities.MenuSliderInput(
                        () => $"Header Bg Opacity: {Math.Round(PropertiesMenu.ApiHudCfg.colors.headerColor.A / 255d, 2)}",
                        "Header Bg Opacity", .01f, 1f,
                        () => (float)Math.Round(PropertiesMenu.ApiHudCfg.colors.headerColor.A / 255d, 2),
                        (float value) => PropertiesMenu.ApiHudCfg.colors.headerColor.A = (byte)(value * 255f)),
                    new MenuUtilities.MenuSliderInput(
                        () => $"List Bg Opacity: {Math.Round(PropertiesMenu.ApiHudCfg.colors.listBgColor.A / 255d, 2)}",
                        "List Bg Opacity", .01f, 1f,
                        () => (float)Math.Round(PropertiesMenu.ApiHudCfg.colors.listBgColor.A / 255d, 2),
                        (float value) => PropertiesMenu.ApiHudCfg.colors.listBgColor.A = (byte)(value * 255f)),
                    new MenuUtilities.MenuSliderInput(
                        () => $"Max Visible Properties: {PropertiesMenu.ApiHudCfg.maxVisible}",
                        "Max Visible Properties", 6, 20,
                        () => PropertiesMenu.ApiHudCfg.maxVisible,
                        (float maxVisible) => PropertiesMenu.ApiHudCfg.maxVisible = (int)maxVisible),
                    new MenuUtilities.MenuButton(
                        () => $"Clamp To Screen Edges: {PropertiesMenu.ApiHudCfg.clampHudPos}",
                        () => PropertiesMenu.ApiHudCfg.clampHudPos = !PropertiesMenu.ApiHudCfg.clampHudPos),
                    new MenuUtilities.MenuButton(
                        () => $"Lock To Screen Center: {PropertiesMenu.ApiHudCfg.forceToCenter}",
                        () => PropertiesMenu.ApiHudCfg.forceToCenter = !PropertiesMenu.ApiHudCfg.forceToCenter),
                    new MenuUtilities.MenuButton(
                        "Reset Settings",
                        () => PropertiesMenu.ApiHudCfg = ApiHudConfig.Defaults
                    ),
                }),
                
                // Bind Settings
                new MenuUtilities.MenuCategory("Bind Settings", "Key Binds", GetBindSettings()),

                // Property Settings
                new MenuUtilities.MenuCategory("Property Settings", "Property Settings", new List<MenuUtilities.IMenuElement>()
                {
                    // Float Properties
                    new MenuUtilities.MenuTextInput(
                        () => $"Float Div: {PropertyBlock.Cfg.floatDiv}",
                        "Float Property Base Divisor",
                        (string input) =>
                        {
                            double.TryParse(input, out PropertyBlock.Cfg.floatDiv);
                            PropertyBlock.Cfg.Validate();
                        }),

                    new MenuUtilities.MenuCategory("Float Multipliers", "Float Multipliers", new List<MenuUtilities.IMenuElement>()
                    {
                        new MenuUtilities.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.floatMult.X}",
                            "Float Multiplier X",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new MenuUtilities.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.floatMult.Y}",
                            "Float Multiplier Y",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new MenuUtilities.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.floatMult.Z}",
                            "Float Multiplier Z",
                            (string input) =>
                            {
                                float.TryParse(input, out PropertyBlock.Cfg.floatMult.Z);
                                PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    // Color Properties
                    new MenuUtilities.MenuCategory("Color Multipliers", "Color Multipliers", new List<MenuUtilities.IMenuElement>()
                    {
                        new MenuUtilities.MenuTextInput(
                            () => $"X: {PropertyBlock.Cfg.colorMult.X}",
                            "Color Multiplier X",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.X);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new MenuUtilities.MenuTextInput(
                            () => $"Y: {PropertyBlock.Cfg.colorMult.Y}",
                            "Color Multiplier Y",
                            (string input) =>
                            {
                                int.TryParse(input, out PropertyBlock.Cfg.colorMult.Y);
                                PropertyBlock.Cfg.Validate();
                            }),
                        new MenuUtilities.MenuTextInput(
                            () => $"Z: {PropertyBlock.Cfg.colorMult.Z}",
                            "Color Multiplier Z",
                            (string input) =>
                            {
                            int.TryParse(input, out PropertyBlock.Cfg.colorMult.Z);
                            PropertyBlock.Cfg.Validate();
                            }),
                    }),

                    new MenuUtilities.MenuButton(
                        "Reset Settings",
                        () => PropertyBlock.Cfg = PropBlockConfig.Defaults
                    ),
                }),

                // General Settings Pt.2
                new MenuUtilities.MenuButton(
                        () => "Open Help Menu",
                        () => ModBase.ShowMessageScreen("Help", GetHelpMessage())),
                new MenuUtilities.MenuButton(
                        () => "Reset Config",
                        () => BvConfig.ResetConfig()),
                new MenuUtilities.MenuButton(
                        () => "Save Config",
                        () => BvConfig.SaveStart()),
                new MenuUtilities.MenuButton(
                        () => "Load Config",
                        () => BvConfig.LoadStart()),
            };
        }

        /// <summary>
        /// Creates list of settings for configuring keybinds via the Text HUD API's Mod Menu
        /// </summary>
        private static List<MenuUtilities.IMenuElement> GetBindSettings()
        {
            List<MenuUtilities.IMenuElement> bindSettings = new List<MenuUtilities.IMenuElement>(BindManager.Count + 2);

            for (int n = 0; n < BindManager.Count; n++)
            {
                bindSettings.Add(new MenuUtilities.MenuTextInput(
                    BindManager.GetBindByIndex(n).Name, 
                    "Enter Control Names", 
                    (string input) =>
                    {
                        string[] args;
                        input = input.ToLower();

                        if (CmdManager.TryParseCommand($"{BindManager.GetBindByIndex(n).Name} {input}", out args))
                            BindManager.TryUpdateBind(BindManager.GetBindByIndex(n).Name, args);
                    }));
            }

            bindSettings.Add(new MenuUtilities.MenuButton(
                () => "Open Bind Help Menu",
                () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())));

            bindSettings.Add(new MenuUtilities.MenuButton(
                () => "Reset Binds",
                () => KeyBinds.Cfg = BindsConfig.Defaults));

            return bindSettings;
        }
    }
}