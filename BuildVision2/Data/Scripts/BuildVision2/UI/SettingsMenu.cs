using System.Collections.Generic;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    internal sealed class SettingsMenu
    {
        public static SettingsMenu Instance { get; private set; }

        private static BvMain Main { get { return BvMain.Instance; } }
        private static Binds Binds { get { return Binds.Instance; } }
        private static PropertiesMenu Menu { get { return PropertiesMenu.Instance; } }
        private static ChatCommands Cmd { get { return ChatCommands.Instance; } }

        private HudUtilities.MenuRoot settingsMenuRoot;

        private SettingsMenu()
        {
            settingsMenuRoot = new HudUtilities.MenuRoot("Build Vision", "Build Vision Settings", new List<HudUtilities.IMenuElement>()
            {
                new HudUtilities.MenuButton(
                    () => $"Force Fallback Hud: {Main.Cfg.forceFallbackHud}",
                    () => Main.Cfg.forceFallbackHud = !Main.Cfg.forceFallbackHud),
                new HudUtilities.MenuButton(
                    () => $"Close If Not In View: {Main.Cfg.closeIfNotInView}",
                    () => Main.Cfg.closeIfNotInView = !Main.Cfg.closeIfNotInView),
                new HudUtilities.MenuButton(
                    () => $"Can Open While Holding Tools: {Main.Cfg.canOpenIfHolding}",
                    () => Main.Cfg.canOpenIfHolding = !Main.Cfg.canOpenIfHolding),

                // GUI Settings
                new HudUtilities.MenuCategory("GUI Settings", "GUI Settings", GetGuiSettings()),

                // Bind Settings
                new HudUtilities.MenuCategory("Bind Settings", "Key Binds", GetBindSettings()),

                new HudUtilities.MenuButton(
                    () => "Open Help Menu",
                    () => Main.ShowMissionScreen("Help", Cmd.GetHelpMessage())),
                new HudUtilities.MenuButton(
                    () => "Reset Config",
                    () => Main.ResetConfig()),
                new HudUtilities.MenuButton(
                    () => "Save Config",
                    () => Main.SaveConfig()),
                new HudUtilities.MenuButton(
                    () => "Load Config",
                    () => Main.LoadConfig())
            });
        }

        private List<HudUtilities.IMenuElement> GetGuiSettings()
        {
            return new List<HudUtilities.IMenuElement>()
            {
                new HudUtilities.MenuSliderInput(0.8f, 1.6f,
                    () => Menu.ApiHudConfig.hudScale,
                    () => $"Hud Scale: {Math.Round(Menu.ApiHudConfig.hudScale, 2)}",
                    "Hud Scale",
                    (float scale) => Menu.ApiHudConfig.hudScale = scale),
                new HudUtilities.MenuSliderInput(6, 20,
                    () => Menu.ApiHudConfig.maxVisible,
                    () => $"Max Visible Properties: {Menu.ApiHudConfig.maxVisible}",
                    "Max Visible Properties",
                    (float maxVisible) => Menu.ApiHudConfig.maxVisible = (int)maxVisible),
                new HudUtilities.MenuButton(
                    () => $"Hide If Out  of View: {Menu.ApiHudConfig.hideIfNotVis}",
                    () => Menu.ApiHudConfig.hideIfNotVis = !Menu.ApiHudConfig.hideIfNotVis),
                new HudUtilities.MenuButton(
                    () => $"Clamp To Screen Edges: {Menu.ApiHudConfig.clampHudPos}",
                    () => Menu.ApiHudConfig.clampHudPos = !Menu.ApiHudConfig.clampHudPos),
                new HudUtilities.MenuButton(
                    () => $"Lock To Screen Center: {Menu.ApiHudConfig.forceToCenter}",
                    () => Menu.ApiHudConfig.forceToCenter = !Menu.ApiHudConfig.forceToCenter),
                /*
                // Text Colors
                new HudUtilities.MenuCategory("Text Colors", "Text Colors", new HudUtilities.IMenuElement[]
                {
                    new HudUtilities.MenuColorInput("Body Text Color",
                        () => Utilities.ParseColor(Menu.ApiHudConfig.colors.bodyText),
                        (Color color) => Menu.ApiHudConfig.colors.bodyText = Utilities.GetColorString(color, false), false),
                    new HudUtilities.MenuColorInput("Header Text Color",
                        () => Utilities.ParseColor(Menu.ApiHudConfig.colors.headerText),
                        (Color color) => Menu.ApiHudConfig.colors.headerText = Utilities.GetColorString(color, false), false),
                    new HudUtilities.MenuColorInput("Block Inc Text Color",
                        () => Utilities.ParseColor(Menu.ApiHudConfig.colors.blockIncText),
                        (Color color) => Menu.ApiHudConfig.colors.blockIncText = Utilities.GetColorString(color, false), false),
                    new HudUtilities.MenuColorInput("Highlight Text Color",
                        () => Utilities.ParseColor(Menu.ApiHudConfig.colors.highlightText),
                        (Color color) => Menu.ApiHudConfig.colors.highlightText = Utilities.GetColorString(color, false), false),
                    new HudUtilities.MenuColorInput("Selection Text Color",
                        () => Utilities.ParseColor(Menu.ApiHudConfig.colors.selectedText),
                        (Color color) => Menu.ApiHudConfig.colors.selectedText = Utilities.GetColorString(color, false), false),
                }),

                // Background Colors
                new HudUtilities.MenuCategory("Background Colors", "Background Colors", new HudUtilities.IMenuElement[]
                {
                    new HudUtilities.MenuColorInput("List Bg Color",
                        () => Menu.ApiHudConfig.colors.backgroundColor,
                        (Color color) => Menu.ApiHudConfig.colors.backgroundColor = color),
                    new HudUtilities.MenuColorInput("Selection Bg Color",
                        () => Menu.ApiHudConfig.colors.selectionBoxColor,
                        (Color color) => Menu.ApiHudConfig.colors.selectionBoxColor = color),
                    new HudUtilities.MenuColorInput("Header Bg Color",
                        () => Menu.ApiHudConfig.colors.headerColor,
                        (Color color) => Menu.ApiHudConfig.colors.headerColor = color),
                }),*/
            };
        }

        private List<HudUtilities.IMenuElement> GetBindSettings()
        {
            List<HudUtilities.IMenuElement> bindSettings = new List<HudUtilities.IMenuElement>(Binds.Count + 2);

            foreach (IKeyBind bind in Binds.KeyBinds)
            {
                bindSettings.Add(new HudUtilities.MenuTextInput(() => bind.Name, "Enter Key Combination", 
                    (string input) =>
                    {
                        string[] args;
                        input = input.ToLower();
                        Cmd.TryParseCommand(bind.Name + input, out args);

                        if (args.Length > 1)
                            Binds.TryUpdateBind(args[0], Utilities.GetSubarray(args, 1));
                    }));
            }

            bindSettings.Add(new HudUtilities.MenuButton(() => "Open Bind Help Menu",
                () => Main.ShowMissionScreen("Bind Help", Cmd.GetBindHelpMessage())));

            bindSettings.Add(new HudUtilities.MenuButton(() => "Reset Binds",
                () => Binds.TryUpdateConfig(BindsConfig.Defaults)));

            return bindSettings;
        }

        public static void Init()
        {
            if (Instance == null)
                Instance = new SettingsMenu();
        }

        public void Close()
        {
            Instance = null;
        }
    }
}