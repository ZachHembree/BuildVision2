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
    public sealed partial class BvMain
    {
        private void InitSettingsMenu()
        {
            RichHudTerminal.Root.Enabled = true;

            RichHudTerminal.Root.Add(new ControlPage()
            { 
                Name = "Settings",
                CategoryContainer =
                {
                    GetGeneralSettings(),
                    GetGuiSettings(),
                    GetPropertySettings(),
                    GetHelpSettings(),
                },
            });

            RichHudTerminal.Root.Add(new RebindPage()
            {
                Name = "Binds",
                GroupContainer =
                {
                    { BvBinds.BindGroup, BindsConfig.DefaultBinds.GetBindDefinitions() },
                }
            });
        }

        private ControlCategory GetGeneralSettings()
        {
            // Close if not in view
            var autoCloseBox = new TerminalCheckbox()
            {
                Name = "Close if target not in sight",
                Value = Cfg.general.closeIfNotInView,
                CustomValueGetter = () => Cfg.general.closeIfNotInView,
                ControlChangedAction = (x => Cfg.general.closeIfNotInView = x.Value),
            };

            // Can open while holding tools
            var toolOpenBox = new TerminalCheckbox()
            {
                Name = "Can open while holding tools",
                Value = Cfg.general.canOpenIfHolding,
                CustomValueGetter = () => Cfg.general.canOpenIfHolding,
                ControlChangedAction = (x => Cfg.general.canOpenIfHolding = x.Value),
            };

            // Open range slider
            var openRangeSlider = new TerminalSlider()
            {
                Name = "Max open range",
                Min = 2.5f,
                Max = 20f,
                ValueText = $"{Cfg.general.maxOpenRange.Round(1)}m",
                Value = (float)Cfg.general.maxOpenRange,
                CustomValueGetter = () => (float)Cfg.general.maxOpenRange,
                ControlChangedAction = x =>
                {
                    Cfg.general.maxOpenRange = x.Value;
                    x.ValueText = $"{x.Value.Round(1)}m";
                }
            };

            // Control range slider
            var controlRangeSlider = new TerminalSlider()
            {
                Name = "Max control range",
                Min = 2.5f,
                Max = 60f,
                ValueText = $"{Cfg.general.maxControlRange.Round(1)}m",
                Value = (float)Cfg.general.maxControlRange.Round(1),
                CustomValueGetter = () => (float)Cfg.general.maxControlRange,
                ControlChangedAction = x =>
                {
                    Cfg.general.maxControlRange = x.Value;
                    x.ValueText = $"{x.Value.Round(1)}m";
                }
            };

            return new ControlCategory()
            {
                HeaderText = "Targeting",
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
            var resScaling = new TerminalCheckbox()
            {
                Name = "Resolution scaling",
                Value = PropertiesMenu.Cfg.resolutionScaling,
                CustomValueGetter = () => PropertiesMenu.Cfg.resolutionScaling,
                ControlChangedAction = (x => PropertiesMenu.Cfg.resolutionScaling = x.Value),
            };

            // Menu size
            var menuScale = new TerminalSlider()
            {
                Name = "Menu scale",
                Min = .75f,
                Max = 2f,
                Value = PropertiesMenu.Cfg.hudScale,
                ValueText = $"{(PropertiesMenu.Cfg.hudScale * 100f).Round()}%",
                CustomValueGetter = () => PropertiesMenu.Cfg.hudScale,
                ControlChangedAction = x =>
                {
                    PropertiesMenu.Cfg.hudScale = x.Value;
                    x.ValueText = $"{(x.Value * 100f).Round()}%";
                }
            };

            // Menu opacity
            var opacity = new TerminalSlider()
            {
                Name = "Menu opacity",
                Min = 0f,
                Max = 1f,
                Value = PropertiesMenu.Cfg.hudOpacity,
                ValueText = $"{(PropertiesMenu.Cfg.hudOpacity * 100f).Round()}%",
                CustomValueGetter = () => PropertiesMenu.Cfg.hudOpacity,
                ControlChangedAction = x =>
                {
                    PropertiesMenu.Cfg.hudOpacity = x.Value;
                    x.ValueText = $"{(x.Value * 100f).Round()}%";
                }
            };

            var tile1 = new ControlTile()
            {
                resScaling,
                menuScale,
                opacity,
            };

            // Max visible properties
            var maxVisible = new TerminalSlider()
            {
                Name = "Max visible properties",
                Min = 6,
                Max = 40,
                Value = PropertiesMenu.Cfg.maxVisible,
                ValueText = $"{PropertiesMenu.Cfg.maxVisible}",
                CustomValueGetter = () => PropertiesMenu.Cfg.maxVisible,
                ControlChangedAction = x =>
                {
                    PropertiesMenu.Cfg.maxVisible = (int)x.Value;
                    x.ValueText = $"{(int)x.Value}";
                }
            };

            // Clamp to screen edges
            var clampToEdges = new TerminalCheckbox()
            {
                Name = "Clamp to screen edges",
                Value = PropertiesMenu.Cfg.clampHudPos,
                CustomValueGetter = () => PropertiesMenu.Cfg.clampHudPos,
                ControlChangedAction = (x => PropertiesMenu.Cfg.clampHudPos = x.Value),
            };

            // Use custom position
            var customPos = new TerminalCheckbox()
            {
                Name = "Use custom position",
                Value = PropertiesMenu.Cfg.useCustomPos,
                CustomValueGetter = () => PropertiesMenu.Cfg.useCustomPos,
                ControlChangedAction = (x => PropertiesMenu.Cfg.useCustomPos = x.Value),
            };

            // Set custom position
            var setPosition = new TerminalDragBox()
            {
                Name = "Set custom position",
                AlignToEdge = true,
                Value = PropertiesMenu.Cfg.hudPos,
                CustomValueGetter = () => PropertiesMenu.Cfg.hudPos,
                ControlChangedAction = (x => PropertiesMenu.Cfg.hudPos = x.Value),
            };

            var tile2 = new ControlTile()
            {
                clampToEdges,
                customPos,
                setPosition,
            };

            var maxVisibleSlider = new TerminalSlider()
            {
                Name = "Max Visible Properties",
                Min = 8,
                Max = 40,
                Value = PropertiesMenu.Cfg.maxVisible,
                ValueText = $"{PropertiesMenu.Cfg.maxVisible}",
                CustomValueGetter = () => PropertiesMenu.Cfg.maxVisible,
                ControlChangedAction = x =>
                {
                    PropertiesMenu.Cfg.maxVisible = (int)x.Value.Round();
                    x.ValueText = $"{x.Value.Round()}";
                }
            };

            var resetGuiSettings = new TerminalButton()
            {
                Name = "Reset GUI settings",
                ControlChangedAction = x => PropertiesMenu.Cfg = HudConfig.Defaults,
            };

            var tile3 = new ControlTile()
            {
                maxVisibleSlider,
                resetGuiSettings,
            };

            return new ControlCategory()
            {
                HeaderText = "GUI Settings",
                SubheaderText = "",
                TileContainer = { tile1, tile2, tile3 }
            };
        }

        private ControlCategory GetPropertySettings()
        {
            Func<char, bool> NumFilterFunc = x => (x >= '0' && x <= '9') || x == '.';

            // Float divider
            var floatDiv = new TerminalTextField()
            {
                Name = "Float Divider",
                Value = PropertyBlock.Cfg.floatDiv.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.floatDiv.ToString(),
                ControlChangedAction = x =>
                {
                    double value;

                    if (double.TryParse(x.Value, out value))
                    {
                        PropertyBlock.Cfg.floatDiv = value;
                        PropertyBlock.Cfg.Validate();
                    }
                }
            };

            var resetProps = new TerminalButton()
            {
                Name = "Reset property settings",
                ControlChangedAction = x => PropertyBlock.Cfg = PropBlockConfig.Defaults,
            };

            var tile1 = new ControlTile()
            {
                floatDiv,
                resetProps
            };

            // X
            var floatMultX = new TerminalTextField()
            {
                Name = "Float Mult X",
                Value = PropertyBlock.Cfg.floatMult.X.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.floatMult.X.ToString(),
                ControlChangedAction = x =>
                {
                    float.TryParse(x.Value, out PropertyBlock.Cfg.floatMult.X);
                    PropertyBlock.Cfg.Validate();
                }
            };

            // Y
            var floatMultY = new TerminalTextField()
            {
                Name = "Float Mult Y",
                Value = PropertyBlock.Cfg.floatMult.Y.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.floatMult.Y.ToString(),
                ControlChangedAction = x =>
                {
                    float.TryParse(x.Value, out PropertyBlock.Cfg.floatMult.Y);
                    PropertyBlock.Cfg.Validate();
                },
            };

            // Z
            var floatMultZ = new TerminalTextField()
            {
                Name = "Float Mult Z",
                Value = PropertyBlock.Cfg.floatMult.Z.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.floatMult.Z.ToString(),
                ControlChangedAction = x =>
                {
                    float.TryParse(x.Value, out PropertyBlock.Cfg.floatMult.Z);
                    PropertyBlock.Cfg.Validate();
                },
            };
            
            var tile2 = new ControlTile()
            {
                floatMultX,
                floatMultY,
                floatMultZ
            };

            // Color - X
            var colorMultX = new TerminalTextField()
            {
                Name = "Color Mult X",
                Value = PropertyBlock.Cfg.colorMult.X.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.colorMult.X.ToString(),
                ControlChangedAction = x =>
                {
                    int.TryParse(x.Value, out PropertyBlock.Cfg.colorMult.X);
                    PropertyBlock.Cfg.Validate();
                },
            };

            // Y
            var colorMultY = new TerminalTextField()
            {
                Name = "Color Mult Y",
                Value = PropertyBlock.Cfg.colorMult.Y.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.colorMult.Y.ToString(),
                ControlChangedAction = x =>
                {
                    int.TryParse(x.Value, out PropertyBlock.Cfg.colorMult.Y);
                    PropertyBlock.Cfg.Validate();
                },
            };

            // Z
            var colorMultZ = new TerminalTextField()
            {
                Name = "Color Mult X",
                Value = PropertyBlock.Cfg.colorMult.X.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.colorMult.Z.ToString(),
                ControlChangedAction = x =>
                {
                    int.TryParse(x.Value, out PropertyBlock.Cfg.colorMult.Z);
                    PropertyBlock.Cfg.Validate();
                },
            };

            var tile3 = new ControlTile()
            {
                colorMultX,
                colorMultY,
                colorMultZ
            };

            return new ControlCategory()
            {
                HeaderText = "Properties",
                SubheaderText = "Controls the rate at which scrolling changes property values.",
                TileContainer = { tile1, tile2, tile3 }
            };
        }

        private ControlCategory GetHelpSettings()
        {
            var openHelp = new TerminalButton()
            {
                Name = "Open help menu",
                ControlChangedAction = x => ExceptionHandler.ShowMessageScreen("Help", GetHelpMessage())
            };

            var openBindHelp = new TerminalButton()
            {
                Name = "Open bind help",
                ControlChangedAction = x => ExceptionHandler.ShowMessageScreen("Bind Help", GetBindHelpMessage()),
            };

            var tile1 = new ControlTile()
            {
                openHelp,
                openBindHelp,
            };

            var loadCfg = new TerminalButton()
            {
                Name = "Load config",
                ControlChangedAction = x => BvConfig.LoadStart(),
            };

            var saveCfg = new TerminalButton()
            {
                Name = "Save config",
                ControlChangedAction = x => BvConfig.SaveStart()
            };

            var resetCfg = new TerminalButton()
            {
                Name = "Reset config",
                ControlChangedAction = x => BvConfig.ResetConfig(),
            };

            var tile2 = new ControlTile()
            {
                loadCfg,
                saveCfg,
                resetCfg
            };

            return new ControlCategory()
            {
                HeaderText = "Help",
                SubheaderText = "Help text and config controls",
                TileContainer = { tile1, tile2 }
            };
        }
    }
}