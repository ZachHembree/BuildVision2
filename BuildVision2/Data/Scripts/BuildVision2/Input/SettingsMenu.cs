using DarkHelmet.Game;
using DarkHelmet.UI;
using System;
using System.Collections.Generic;

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
                new MenuUtilities.MenuSliderInput(
                        () => $"Max Open Range: {Math.Round(Cfg.general.maxOpenRange, 1)}",
                        "Max Open Range", 2.5f, 20f,
                        () => (float)Cfg.general.maxOpenRange,
                        x => Cfg.general.maxOpenRange = x),
                new MenuUtilities.MenuSliderInput(
                        () => $"Max Control Range: {Math.Round(Cfg.general.maxControlRange, 1)}",
                        "Max Control Range", 2.5f, 60f,
                        () => (float)Cfg.general.maxControlRange,
                        x => Cfg.general.maxControlRange = x),

                // GUI Settings
                new MenuUtilities.MenuCategory("GUI Settings", "GUI Settings", new List<MenuUtilities.IMenuElement>()
                {
                    // Scaling
                    new MenuUtilities.MenuButton(
                        () => $"Enable Resolution Scaling: {PropertiesMenu.ApiHudCfg.resolutionScaling}",
                        () => PropertiesMenu.ApiHudCfg.resolutionScaling = !PropertiesMenu.ApiHudCfg.resolutionScaling),
                    new MenuUtilities.MenuSliderInput(
                        () => $"Hud Scale: {Math.Round(PropertiesMenu.ApiHudCfg.hudScale, 2)}",
                        "Hud Scale", 0.75f, 2f,
                        () => PropertiesMenu.ApiHudCfg.hudScale,
                        x => PropertiesMenu.ApiHudCfg.hudScale = x),
                    // Opacity
                    new MenuUtilities.MenuSliderInput(
                        () => $"Menu Opacity: {FloatToPercent(PropertiesMenu.ApiHudCfg.hudOpacity)}",
                        "Header Bg Opacity", 0, 100,
                        () => FloatToPercent(PropertiesMenu.ApiHudCfg.hudOpacity),
                        x => PropertiesMenu.ApiHudCfg.hudOpacity = x / 100f),
                    // Misc
                    new MenuUtilities.MenuSliderInput(
                        () => $"Max Visible Properties: {PropertiesMenu.ApiHudCfg.maxVisible}",
                        "Max Visible Properties", 6, 20,
                        () => PropertiesMenu.ApiHudCfg.maxVisible,
                        x => PropertiesMenu.ApiHudCfg.maxVisible = x),
                    new MenuUtilities.MenuButton(
                        () => $"Clamp To Screen Edges: {PropertiesMenu.ApiHudCfg.clampHudPos}",
                        () => PropertiesMenu.ApiHudCfg.clampHudPos = !PropertiesMenu.ApiHudCfg.clampHudPos),
                    new MenuUtilities.MenuButton(
                        () => $"Use Custom Position: {PropertiesMenu.ApiHudCfg.useCustomPos}",
                        () => PropertiesMenu.ApiHudCfg.useCustomPos = !PropertiesMenu.ApiHudCfg.useCustomPos),
                    new MenuUtilities.MenuPositionInput(
                        "Set Hud Position", "",
                        () => PropertiesMenu.ApiHudCfg.hudPos.ToDouble(),
                        x => PropertiesMenu.ApiHudCfg.hudPos = x.ToSingle()),
                    new MenuUtilities.MenuButton(
                        "Reset Settings",
                        () => PropertiesMenu.ApiHudCfg = ApiHudConfig.Defaults),
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
                        "Open Help Menu",
                        () => ModBase.ShowMessageScreen("Help", GetHelpMessage())),
                new MenuUtilities.MenuButton(
                        "Reset Config",
                        () => BvConfig.ResetConfig()),
                new MenuUtilities.MenuButton(
                        "Save Config",
                        () => BvConfig.SaveStart()),
                new MenuUtilities.MenuButton(
                        "Load Config",
                        () => BvConfig.LoadStart()),
            };
        }

        private static int FloatToPercent(float value) =>
            (int)Math.Round(value * 100d);

        /// <summary>
        /// Creates list of settings for configuring keybinds via the Text HUD API's Mod Menu
        /// </summary>
        private static List<MenuUtilities.IMenuElement> GetBindSettings()
        {
            List<MenuUtilities.IMenuElement> bindSettings = new List<MenuUtilities.IMenuElement>(KeyBinds.BindGroup.Count + 2);

            for (int n = 0; n < KeyBinds.BindGroup.Count; n++)
            {
                string bindName = KeyBinds.BindGroup[n].Name;

                bindSettings.Add(new MenuUtilities.MenuButton(
                    bindName,
                    () => RebindMenu.UpdateBind(KeyBinds.BindGroup, bindName)));
            }

            bindSettings.Add(new MenuUtilities.MenuButton(
                "Open Bind Help Menu",
                () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())));

            bindSettings.Add(new MenuUtilities.MenuButton(
                "Reset Binds",
                () => KeyBinds.Cfg = BindsConfig.Defaults));

            return bindSettings;
        }
    }
}