using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using RichHudFramework.UI.Rendering.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvMain
    {
        private TextPage helpMain;
        private RebindPage bindsPage, legacyBindsPage;

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
            bindsPage = new RebindPage()
            {
                Name = "Binds",
                GroupContainer =
                {
                    { BvBinds.ModifierGroup, BindsConfig.DefaultModifiers },
                    { BvBinds.MainGroup, BindsConfig.DefaultMain },
                    { BvBinds.SecondaryGroup, BindsConfig.DefaultSecondary },
                    { BvBinds.DupeGroup, BindsConfig.DefaultDupe },
                }
            };
            legacyBindsPage = new RebindPage()
            {
                Name = "Binds",
                Enabled = false,
                GroupContainer =
                {
                    { BvBinds.ModifierGroup, BindsConfig.DefaultModifiers },
                    { BvBinds.MainGroup, BindsConfig.DefaultLegacyMain },
                    { BvBinds.SecondaryGroup, BindsConfig.DefaultLegacySecondary },
                    { BvBinds.DupeGroup, BindsConfig.DefaultLegacyDupe },
                }
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
                bindsPage,
                legacyBindsPage,
                helpMain,
            });
        }

        private ControlCategory GetTargetingSettings()
        {
            var peekToggleBox = new TerminalOnOffButton()
            {
                Name = "Peek",
                Value = Cfg.targeting.enablePeek,
                CustomValueGetter = () => Cfg.targeting.enablePeek,
                ControlChangedHandler = ((sender, args) => Cfg.targeting.enablePeek = (sender as TerminalOnOffButton).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Enables/disables preview of block configuration"
                },
            };

            // Close if not in view
            var autoCloseBox = new TerminalCheckbox()
            {
                Name = "Close if target not in sight",
                Value = Cfg.targeting.closeIfNotInView,
                CustomValueGetter = () => Cfg.targeting.closeIfNotInView,
                ControlChangedHandler = ((sender, args) => Cfg.targeting.closeIfNotInView = (sender as TerminalCheckbox).Value),
            };

            // Can open while holding tools
            var toolOpenBox = new TerminalCheckbox()
            {
                Name = "Can open while placing",
                Value = Cfg.targeting.canOpenIfPlacing,
                CustomValueGetter = () => Cfg.targeting.canOpenIfPlacing,
                ControlChangedHandler = ((sender, args) => Cfg.targeting.canOpenIfPlacing = (sender as TerminalCheckbox).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "If unchecked, Build Vision will not open\n",
                    "while placing blocks. Only works if Legacy\n",
                    "Mode is off."
                },
            };

            // Open range slider
            var openRangeSliderMax = new TerminalSlider()
            {
                Name = "Max open range",
                Min = 2.5f,
                Max = 20f,
                ValueText = $"{Cfg.targeting.maxOpenRange.Round(1)}m",
                Value = (float)Cfg.targeting.maxOpenRange,
                CustomValueGetter = () => (float)Cfg.targeting.maxOpenRange,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    Cfg.targeting.maxOpenRange = slider.Value;
                    slider.ValueText = $"{slider.Value.Round(1)}m";
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Build Vision will not open for target\n",
                    "blocks beyond this distance"
                },
            };

            // Control range slider
            var controlRangeSlider = new TerminalSlider()
            {
                Name = "Max control range",
                Min = 2.5f,
                Max = 60f,
                ValueText = $"{Cfg.targeting.maxControlRange.Round(1)}m",
                Value = (float)Cfg.targeting.maxControlRange.Round(1),
                CustomValueGetter = () => (float)Cfg.targeting.maxControlRange,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    Cfg.targeting.maxControlRange = slider.Value;
                    slider.ValueText = $"{slider.Value.Round(1)}m";
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Auto-close distance after Build Vision\n",
                    "has been opened"
                },
            };

            // Close if not in view
            var specLimitBox = new TerminalCheckbox()
            {
                Name = "Limit spectator range",
                Value = Cfg.targeting.isSpecRangeLimited,
                CustomValueGetter = () => Cfg.targeting.isSpecRangeLimited,
                ControlChangedHandler = ((sender, args) => Cfg.targeting.isSpecRangeLimited = (sender as TerminalCheckbox).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "If enabled the auto-close distance will be\n",
                    "enforced for the spectator camera."
                },
            };

            var targetingResetButton = new TerminalButton()
            {
                Name = "Reset targeting settings",
                ControlChangedHandler = (sender, args) => BvConfig.Current.targeting = TargetingConfig.Defaults,
            };

            return new ControlCategory()
            {
                HeaderText = "Targeting",
                SubheaderText = "Configure block targeting behavior",
                TileContainer =
                {
                    new ControlTile() { peekToggleBox, autoCloseBox, toolOpenBox, },
                    new ControlTile() { openRangeSliderMax, controlRangeSlider },
                    new ControlTile() { specLimitBox, targetingResetButton }
                },
            };
        }

        private ControlCategory GetGuiSettings()
        {
            // Legacy mode toggle
            var legacyToggleBox = new TerminalOnOffButton()
            {
                Name = "Legacy Mode",
                Value = Cfg.genUI.legacyModeEnabled,
                CustomValueGetter = () => Cfg.genUI.legacyModeEnabled,
                ControlChangedHandler = (sender, args) => 
                {
                    bool value = (sender as TerminalOnOffButton).Value;

                    if (value != Cfg.genUI.legacyModeEnabled)
                    {
                        Cfg.genUI.legacyModeEnabled = value;
                        bindsPage.Enabled = !value;
                        legacyBindsPage.Enabled = value;

                        if (Cfg.genUI.legacyModeEnabled)
                        {
                            BvBinds.Cfg = new BindsConfig
                            {
                                modifierGroup = BindsConfig.DefaultModifiers,
                                mainGroup = BindsConfig.DefaultLegacyMain,
                                secondaryGroup = BindsConfig.DefaultLegacySecondary,
                                dupeGroup = BindsConfig.DefaultLegacyDupe
                            };
                        }
                        else
                        {
                            BvBinds.Cfg = new BindsConfig
                            {
                                modifierGroup = BindsConfig.DefaultModifiers,
                                mainGroup = BindsConfig.DefaultMain,
                                secondaryGroup = BindsConfig.DefaultSecondary,
                                dupeGroup = BindsConfig.DefaultDupe
                            };
                        }
                    }
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Makes old list menu the primary menu\n" +
                    "and reverts to the v2.5 control scheme.\n" +
                    "Changing this will overwrite your binds."
                }
            };

            // Cursor sensitivity
            var cursorSensitivity = new TerminalSlider()
            {
                Name = "Wheel Cursor Sensitivity",
                Min = .3f,
                Max = 2f,
                Value = BvConfig.Current.genUI.cursorSensitivity,
                ValueText = $"{(BvConfig.Current.genUI.hudScale * 100f).Round()}%",
                CustomValueGetter = () => BvConfig.Current.genUI.cursorSensitivity,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.genUI.cursorSensitivity = slider.Value;
                    slider.ValueText = $"{(slider.Value * 100f).Round()}%";
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Controls speed for scrolling using the mouse"
                }
            };

            var tile1 = new ControlTile()
            {
                legacyToggleBox,
                cursorSensitivity,
            };

            // Menu size
            var menuScale = new TerminalSlider()
            {
                Name = "Menu scale",
                Min = .75f,
                Max = 2f,
                Value = BvConfig.Current.genUI.hudScale,
                ValueText = $"{(BvConfig.Current.genUI.hudScale * 100f).Round()}%",
                CustomValueGetter = () => BvConfig.Current.genUI.hudScale,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.genUI.hudScale = slider.Value;
                    slider.ValueText = $"{(slider.Value * 100f).Round()}%";
                }
            };

            // Menu opacity
            var opacity = new TerminalSlider()
            {
                Name = "Menu opacity",
                Min = .5f,
                Max = 1f,
                Value = BvConfig.Current.genUI.hudOpacity,
                ValueText = $"{(BvConfig.Current.genUI.hudOpacity * 100f).Round()}%",
                CustomValueGetter = () => BvConfig.Current.genUI.hudOpacity,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.genUI.hudOpacity = slider.Value;
                    slider.ValueText = $"{(slider.Value * 100f).Round()}%";
                }
            };

            var tile2 = new ControlTile()
            {
                menuScale,
                opacity,
            };

            // Max visible properties
            var listMaxVisibleSlider = new TerminalSlider()
            {
                Name = "List Max Visible",
                Min = 6,
                Max = 40,
                Value = BvConfig.Current.genUI.listMaxVisible,
                ValueText = $"{BvConfig.Current.genUI.listMaxVisible}",
                CustomValueGetter = () => BvConfig.Current.genUI.listMaxVisible,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.genUI.listMaxVisible = (int)slider.Value;
                    slider.ValueText = $"{(int)slider.Value}";
                }
            };

            var wheelMaxVisibleSlider = new TerminalSlider()
            {
                Name = "Wheel Max Visible",
                Min = 10,
                Max = 30,
                Value = BvConfig.Current.genUI.wheelMaxVisible,
                ValueText = $"{BvConfig.Current.genUI.wheelMaxVisible}",
                CustomValueGetter = () => BvConfig.Current.genUI.wheelMaxVisible,
                ControlChangedHandler = (sender, args) =>
                {
                    var slider = sender as TerminalSlider;

                    BvConfig.Current.genUI.wheelMaxVisible = (int)slider.Value;
                    slider.ValueText = $"{(int)slider.Value}";
                }
            };

            var tile3 = new ControlTile()
            {
                listMaxVisibleSlider,
                wheelMaxVisibleSlider,
            };

            // Clamp to screen edges
            var clampToEdges = new TerminalCheckbox()
            {
                Name = "Clamp to screen edges",
                Value = BvConfig.Current.genUI.clampHudPos,
                CustomValueGetter = () => BvConfig.Current.genUI.clampHudPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.genUI.clampHudPos = (sender as TerminalCheckbox).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Prevents the menu from moving outside the bounds\n" +
                    "of the screen"
                },
            };

            // Use custom position
            var customPos = new TerminalCheckbox()
            {
                Name = "Use custom position",
                Value = BvConfig.Current.genUI.useCustomPos,
                CustomValueGetter = () => BvConfig.Current.genUI.useCustomPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.genUI.useCustomPos = (sender as TerminalCheckbox).Value),
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Sets menu to a user-defined fixed position"
                },
            };

            // Set custom position
            var setPosition = new TerminalDragBox()
            {
                Name = "Set custom position",
                AlignToEdge = true,
                CustomValueGetter = () => BvConfig.Current.genUI.hudPos,
                ControlChangedHandler = ((sender, args) => BvConfig.Current.genUI.hudPos = (sender as TerminalDragBox).Value),
                Value = BvConfig.Current.genUI.hudPos,
            };

            var tile4 = new ControlTile()
            {
                clampToEdges,
                customPos,
                setPosition,
            };

            var fontSelection = new TerminalDropdown<string>()
            {
                Name = "Font",
                ControlChangedHandler = (sender, args) =>
                {
                    var dd = sender as TerminalDropdown<string>;
                    BvConfig.Current.genUI.fontName = dd.Value.AssocObject;
                }
            };

            var loadFontsButton = new TerminalButton()
            {
                Name = "Load Fonts",
                ControlChangedHandler = (sender, args) =>
                {
                    fontSelection.List.Clear();

                    foreach (IFontMin font in FontManager.Fonts)
                        fontSelection.List.Add(font.Name, font.Name);
                }
            };

            var resetGuiButton = new TerminalButton()
            {
                Name = "Reset GUI settings",
                ControlChangedHandler = (sender, args) => BvConfig.Current.genUI = UIConfig.Defaults,
            };

            var tile5 = new ControlTile()
            {
                fontSelection,
                loadFontsButton,
                resetGuiButton,
            };

            return new ControlCategory()
            {
                HeaderText = "GUI Settings",
                SubheaderText = "Customize appearance and menu positioning",
                TileContainer = { tile1, tile2, tile3, tile4, tile5 }
            };
        }

        private ControlCategory GetPropertySettings()
        {
            Func<char, bool> NumFilterFunc = x => (x >= '0' && x <= '9') || x == '.';

            // Float divider
            var floatDiv = new TerminalTextField()
            {
                Name = "Base Float Divider",
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
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Used to define base increment for numerical values.\n" +
                    "The value's range ~|max - min| is divided by this value\n" +
                    "to obtain the base increment.\n" +
                    "Larger values = smaller increments and vice versa."
                },
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
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. X * base"
                },
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
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. Y * base"
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
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. Z * base"
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
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. X * base"
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
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. Y * base"
                },
            };

            // Z
            var colorMultZ = new TerminalTextField()
            {
                Name = "Color Mult Z",
                Value = PropertyBlock.Cfg.colorMult.Z.ToString(),
                CharFilterFunc = NumFilterFunc,
                CustomValueGetter = () => PropertyBlock.Cfg.colorMult.Z.ToString(),
                ControlChangedHandler = (sender, args) =>
                {
                    var textField = sender as TerminalTextField;

                    int.TryParse(textField.Value, out PropertyBlock.Cfg.colorMult.Z);
                    PropertyBlock.Cfg.Validate();
                },
                ToolTip = new RichText(ToolTip.DefaultText)
                {
                    "Scales the increment for numerical values e.g. Z * base"
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