using RichHudFramework.Game;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework;
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
                        () => $"Enable Resolution Scaling: {PropertiesMenu.Cfg.resolutionScaling}",
                        () => PropertiesMenu.Cfg.resolutionScaling = !PropertiesMenu.Cfg.resolutionScaling),
                    new MenuUtilities.MenuSliderInput(
                        () => $"Hud Scale: {Math.Round(PropertiesMenu.Cfg.hudScale, 2)}",
                        "Hud Scale", 0.75f, 2f,
                        () => PropertiesMenu.Cfg.hudScale,
                        x => PropertiesMenu.Cfg.hudScale = x),
                    // Opacity
                    new MenuUtilities.MenuSliderInput(
                        () => $"Menu Opacity: {FloatToPercent(PropertiesMenu.Cfg.hudOpacity)}",
                        "Header Bg Opacity", 0, 100,
                        () => FloatToPercent(PropertiesMenu.Cfg.hudOpacity),
                        x => PropertiesMenu.Cfg.hudOpacity = x / 100f),
                    // Misc
                    new MenuUtilities.MenuSliderInput(
                        () => $"Max Visible Properties: {PropertiesMenu.Cfg.maxVisible}",
                        "Max Visible Properties", 6, 20,
                        () => PropertiesMenu.Cfg.maxVisible,
                        x => PropertiesMenu.Cfg.maxVisible = x),
                    new MenuUtilities.MenuButton(
                        () => $"Clamp To Screen Edges: {PropertiesMenu.Cfg.clampHudPos}",
                        () => PropertiesMenu.Cfg.clampHudPos = !PropertiesMenu.Cfg.clampHudPos),
                    new MenuUtilities.MenuButton(
                        () => $"Use Custom Position: {PropertiesMenu.Cfg.useCustomPos}",
                        () => PropertiesMenu.Cfg.useCustomPos = !PropertiesMenu.Cfg.useCustomPos),
                    new MenuUtilities.MenuPositionInput(
                        "Set Hud Position", "",
                        () => PropertiesMenu.Cfg.hudPos.ToDouble(),
                        x => PropertiesMenu.Cfg.hudPos = x.ToSingle()),
                    new MenuUtilities.MenuButton(
                        "Reset Settings",
                        () => PropertiesMenu.Cfg = HudConfig.Defaults),
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
            List<MenuUtilities.IMenuElement> bindSettings = new List<MenuUtilities.IMenuElement>(BvBinds.BindGroup.Count + 2);

            bindSettings.Add(new MenuUtilities.MenuButton(
                "Open Bind Help Menu",
                () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())));

            bindSettings.Add(new MenuUtilities.MenuButton(
                "Reset Binds",
                () => BvBinds.Cfg = BindsConfig.Defaults));

            return bindSettings;
        }
    }
}