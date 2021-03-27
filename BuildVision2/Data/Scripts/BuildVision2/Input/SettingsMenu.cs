using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvMain
    {
        private TextPage helpMain;

        private void InitSettingsMenu()
        {
            RichHudTerminal.Root.Enabled = true;

            helpMain = new TextPage()
            {
                Name = "Help",
                HeaderText = "Build Vision Help",
                SubHeaderText = "",
                Text = HelpText.GetHelpMessage(),
            };

            RichHudTerminal.Root.AddRange(new IModRootMember[] 
            { 
                new ControlPage()
                {
                    Name = "Settings",
                    CategoryContainer =
                    {
                        GetTargetingSettings(),
                        GetGuiSettings(),
                        GetPropertySettings(),
                        GetHelpSettings(),
                    },
                },
                new RebindPage()
                {
                    Name = "Binds",
                    GroupContainer =
                    {
                        { BvBinds.OpenGroup, BindsConfig.DefaultOpen },
                        { BvBinds.MainGroup, BindsConfig.DefaultMain },
                    }
                },
                helpMain,
            });
        }

        private ControlCategory GetTargetingSettings()
        {
            var peekToggleBox = new TerminalOnOffButton()
            {
                Name = "Peek",
                Value = Cfg.general.enablePeek,
                CustomValueGetter = () => Cfg.general.enablePeek,
                ControlChangedHandler = ((sender, args) => Cfg.general.enablePeek = (sender as TerminalOnOffButton).Value),
            };

            // Close if not in view
            var autoCloseBox = new TerminalCheckbox()
            {
                Name = "Close if target not in sight",
                Value = Cfg.general.closeIfNotInView,
                CustomValueGetter = () => Cfg.general.closeIfNotInView,
                ControlChangedHandler = ((sender, args) => Cfg.general.closeIfNotInView = (sender as TerminalCheckbox).Value),
            };

            // Can open while holding tools
            var toolOpenBox = new TerminalCheckbox()
            {
                Name = "Can open while holding tools",
                Value = Cfg.general.canOpenIfHolding,
                CustomValueGetter = () => Cfg.general.canOpenIfHolding,
                ControlChangedHandler = ((sender, args) => Cfg.general.canOpenIfHolding = (sender as TerminalCheckbox).Value),
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
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    Cfg.general.maxOpenRange = slider.Value;
                    slider.ValueText = $"{slider.Value.Round(1)}m";
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
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    Cfg.general.maxControlRange = slider.Value;
                    slider.ValueText = $"{slider.Value.Round(1)}m";
                }
            };

            var targetingResetButton = new TerminalButton()
            {
                Name = "Reset targeting settings",
                ControlChangedHandler = (sender, args) => BvConfig.Current.general = TargetingConfig.Defaults,
            };

            return new ControlCategory()
            {
                HeaderText = "Targeting",
                SubheaderText = "Configure block targeting behavior",
                TileContainer =
                {
                    new ControlTile() { peekToggleBox, autoCloseBox, toolOpenBox, },
                    new ControlTile() { openRangeSlider, controlRangeSlider, },
                    new ControlTile() { targetingResetButton }
                },
            };
        }

        private ControlCategory GetGuiSettings()
        {
            // Resolution scale
            var resScaling = new TerminalCheckbox()
            {
                Name = "Resolution scaling",
                Value = BvConfig.Current.hudConfig.resolutionScaling,
                CustomValueGetter = () => BvConfig.Current.hudConfig.resolutionScaling,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.hudConfig.resolutionScaling = (sender as TerminalCheckbox).Value),
            };

            // Menu size
            var menuScale = new TerminalSlider()
            {
                Name = "Menu scale",
                Min = .75f,
                Max = 2f,
                Value = BvConfig.Current.hudConfig.hudScale,
                ValueText = $"{(BvConfig.Current.hudConfig.hudScale * 100f).Round()}%",
                CustomValueGetter = () => BvConfig.Current.hudConfig.hudScale,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.hudConfig.hudScale = slider.Value;
                    slider.ValueText = $"{(slider.Value * 100f).Round()}%";
                }
            };

            // Menu opacity
            var opacity = new TerminalSlider()
            {
                Name = "Menu opacity",
                Min = 0f,
                Max = 1f,
                Value = BvConfig.Current.hudConfig.hudOpacity,
                ValueText = $"{(BvConfig.Current.hudConfig.hudOpacity * 100f).Round()}%",
                CustomValueGetter = () => BvConfig.Current.hudConfig.hudOpacity,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.hudConfig.hudOpacity = slider.Value;
                    slider.ValueText = $"{(slider.Value * 100f).Round()}%";
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
                Value = BvConfig.Current.hudConfig.maxVisible,
                ValueText = $"{BvConfig.Current.hudConfig.maxVisible}",
                CustomValueGetter = () => BvConfig.Current.hudConfig.maxVisible,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.hudConfig.maxVisible = (int)slider.Value;
                    slider.ValueText = $"{(int)slider.Value}";
                }
            };

            // Clamp to screen edges
            var clampToEdges = new TerminalCheckbox()
            {
                Name = "Clamp to screen edges",
                Value = BvConfig.Current.hudConfig.clampHudPos,
                CustomValueGetter = () => BvConfig.Current.hudConfig.clampHudPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.hudConfig.clampHudPos = (sender as TerminalCheckbox).Value),
            };

            // Use custom position
            var customPos = new TerminalCheckbox()
            {
                Name = "Use custom position",
                Value = BvConfig.Current.hudConfig.useCustomPos,
                CustomValueGetter = () => BvConfig.Current.hudConfig.useCustomPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.hudConfig.useCustomPos = (sender as TerminalCheckbox).Value),
            };

            // Set custom position
            var setPosition = new TerminalDragBox()
            {
                Name = "Set custom position",
                AlignToEdge = true,
                Value = BvConfig.Current.hudConfig.hudPos,
                CustomValueGetter = () => BvConfig.Current.hudConfig.hudPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.hudConfig.hudPos = (sender as TerminalDragBox).Value),
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
                Value = BvConfig.Current.hudConfig.maxVisible,
                ValueText = $"{BvConfig.Current.hudConfig.maxVisible}",
                CustomValueGetter = () => BvConfig.Current.hudConfig.maxVisible,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.hudConfig.maxVisible = (int)slider.Value.Round();
                    slider.ValueText = $"{slider.Value.Round()}";
                }
            };

            var resetGuiSettings = new TerminalButton()
            {
                Name = "Reset GUI settings",
                ControlChangedHandler = (sender, args) => BvConfig.Current.hudConfig = HudConfig.Defaults,
            };

            var tile3 = new ControlTile()
            {
                maxVisibleSlider,
                resetGuiSettings,
            };

            return new ControlCategory()
            {
                HeaderText = "GUI Settings",
                SubheaderText = "Customize appearance and menu positioning",
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
                ControlChangedHandler = (sender, args) =>
                {
                    double value;
                    var textField = sender as TerminalTextField;

                    if (double.TryParse(textField.Value, out value))
                    {
                        PropertyBlock.Cfg.floatDiv = value;
                        PropertyBlock.Cfg.Validate();
                    }
                }
            };

            var resetProps = new TerminalButton()
            {
                Name = "Reset property settings",
                ControlChangedHandler = (sender, args) => PropertyBlock.Cfg = PropBlockConfig.Defaults,
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    float.TryParse(textField.Value, out PropertyBlock.Cfg.floatMult.X);
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    float.TryParse(textField.Value, out PropertyBlock.Cfg.floatMult.Y);
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    float.TryParse(textField.Value, out PropertyBlock.Cfg.floatMult.Z);
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    int.TryParse(textField.Value, out PropertyBlock.Cfg.colorMult.X);
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    int.TryParse(textField.Value, out PropertyBlock.Cfg.colorMult.Y);
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
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    int.TryParse(textField.Value, out PropertyBlock.Cfg.colorMult.Z);
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
                Name = "Open help",
                ControlChangedHandler = (sender, args) => RichHudTerminal.OpenToPage(helpMain)
            };

            var tile1 = new ControlTile()
            {
                openHelp,
            };

            var loadCfg = new TerminalButton()
            {
                Name = "Load config",
                ControlChangedHandler = (sender, args) => BvConfig.LoadStart(),
            };

            var saveCfg = new TerminalButton()
            {
                Name = "Save config",
                ControlChangedHandler = (sender, args) => BvConfig.SaveStart()
            };

            var resetCfg = new TerminalButton()
            {
                Name = "Reset config",
                ControlChangedHandler = (sender, args) => BvConfig.ResetConfig(),
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