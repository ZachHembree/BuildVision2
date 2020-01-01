using RichHudFramework.Game;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class BvMain
    {
        private void InitSettingsMenu()
        {
            ModMenu.Root.Enabled = true;

            ModMenu.Root.Add(new ControlPage()
            { 
                Name = "Settings",
                CategoryContainer =
                {
                    GetGeneralSettings(),
                    GetGuiSettings()
                },
            });
        }

        private ControlCategory GetGeneralSettings()
        {
            // Close if not in view
            var autoCloseBox = new Checkbox()
            {
                Name = "Close if target not in sight",
                Value = Cfg.general.closeIfNotInView,
            };

            autoCloseBox.OnControlChanged += () => 
            {
                Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView;
            };

            // Can open while holding tools
            var toolOpenBox = new Checkbox()
            {
                Name = "Can open while holding tools",
                Value = Cfg.general.canOpenIfHolding,
            };

            toolOpenBox.OnControlChanged += () => 
            {
                Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding;
            };

            // Open range slider
            var openRangeSlider = new SliderSetting()
            {
                Name = "Max open range",
                Min = 2.5f,
                Max = 20f,
                ValueText = $"{Cfg.general.maxOpenRange.Round(1)}m",
                Value = (float)Cfg.general.maxOpenRange,
            };

            openRangeSlider.OnControlChanged += () => 
            {
                openRangeSlider.ValueText = $"{openRangeSlider.Value.Round(1)}m";
                Cfg.general.maxOpenRange = openRangeSlider.Value;
            };

            // Control range slider
            var controlRangeSlider = new SliderSetting()
            {
                Name = "Max control range",
                Min = 2.5f,
                Max = 60f,
                Value = (float)Cfg.general.maxControlRange.Round(1),
            };

            openRangeSlider.OnControlChanged += () =>
            {
                openRangeSlider.ValueText = $"{openRangeSlider.Value.Round(1)}m";
                Cfg.general.maxControlRange = openRangeSlider.Value;
            };

            return new ControlCategory()
            {
                HeaderText = "General Settings",
                SubheaderText = "",
                TileContainer =
                {
                    new ControlTile() { autoCloseBox, toolOpenBox, },
                    new ControlTile() { openRangeSlider, controlRangeSlider, },
                },
            };
        }

        private ControlCategory GetGuiSettings()
        {
            // Resolution scale
            var resScaling = new Checkbox()
            {
                Name = "Resolution scaling",
                Value = PropertiesMenu.Cfg.resolutionScaling,
            };

            resScaling.OnControlChanged += () =>
            {
                PropertiesMenu.Cfg.resolutionScaling = !PropertiesMenu.Cfg.resolutionScaling;
            };

            // Menu size
            var menuScale = new SliderSetting()
            {
                Name = "Menu scale",
                Min = .75f,
                Max = 2f,
                Value = PropertiesMenu.Cfg.hudScale,
                ValueText = $"{(PropertiesMenu.Cfg.hudScale * 100f).Round()}%",
            };

            menuScale.OnControlChanged += () =>
            {
                menuScale.ValueText = $"{(menuScale.Value * 100f).Round()}%";
                PropertiesMenu.Cfg.hudScale = menuScale.Value;
            };

            // Menu opacity
            var opacity = new SliderSetting()
            {
                Name = "Menu opacity",
                Min = 0f,
                Max = 1f,
                Value = PropertiesMenu.Cfg.hudOpacity,
                ValueText = $"{(PropertiesMenu.Cfg.hudOpacity * 100f).Round()}%",
            };

            opacity.OnControlChanged += () =>
            {
                opacity.ValueText = $"{(opacity.Value * 100f).Round()}%";
                PropertiesMenu.Cfg.hudOpacity = opacity.Value;
            };

            var tile1 = new ControlTile()
            {
                resScaling,
                menuScale,
                opacity,
            };

            // Max visible properties
            var maxVisible = new SliderSetting()
            {
                Name = "Max visible properties",
                Min = 6,
                Max = 40,
                Value = PropertiesMenu.Cfg.maxVisible,
                ValueText = $"{PropertiesMenu.Cfg.maxVisible}",
            };

            maxVisible.OnControlChanged += () =>
            {
                maxVisible.ValueText = $"{(int)maxVisible.Value}";
                PropertiesMenu.Cfg.maxVisible = (int)maxVisible.Value;
            };

            // Clamp to screen edges
            var clampToEdges = new Checkbox()
            {
                Name = "Clamp to screen edges",
                Value = PropertiesMenu.Cfg.clampHudPos,
            };

            clampToEdges.OnControlChanged += () =>
            {
                PropertiesMenu.Cfg.clampHudPos = !PropertiesMenu.Cfg.clampHudPos;
            };

            // Use custom position
            var customPos = new Checkbox()
            {
                Name = "Use custom position",
                Value = PropertiesMenu.Cfg.useCustomPos,
            };

            customPos.OnControlChanged += () =>
            {
                PropertiesMenu.Cfg.useCustomPos = !PropertiesMenu.Cfg.useCustomPos;
            };

            // Set custom position
            var setPosition = new TerminalButton()
            {
                Name = "Set custom position",
            };

            var tile2 = new ControlTile()
            {
                clampToEdges,
                customPos,
                setPosition,
            };

            var resetGuiSettings = new TerminalButton()
            {
                Name = "Reset GUI settings",
            };

            var tile3 = new ControlTile()
            {
                resetGuiSettings,
            };

            return new ControlCategory()
            {
                HeaderText = "GUI Settings",
                SubheaderText = "",
                TileContainer = { tile1, tile2, tile3 }
            };
        }
    }
}