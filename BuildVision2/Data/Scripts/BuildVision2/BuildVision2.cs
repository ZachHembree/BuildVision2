using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Xml.Serialization;
using DarkHelmet.IO;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build vision main class; singleton.
    /// </summary>
    [ModMain]
    internal sealed class BuildVision2 : ModBase.Component<BuildVision2>
    {
        private const string configFileName = "BuildVision2Config.xml", logFileName = "bvLog.txt", cmdPrefix = "/bv2";

        private static ConfigIO<BvConfig> ConfigIO { get { return ConfigIO<BvConfig>.Instance; } }
        private static PropertiesMenu Menu { get { return PropertiesMenu.Instance; } }

        private bool initStarted, initFinished;

        public BvConfig Cfg
        {
            get { return cfg; }
            set
            {
                cfg = value;

                if (initFinished && cfg != null)
                {
                    cfg.Validate();
                    KeyBinds.Cfg = cfg.binds;
                    PropertiesMenu.Cfg = cfg.menu;
                    PropertyBlock.Cfg = cfg.propertyBlock;
                }
            }
        }

        private BvConfig cfg;
        private PropertyBlock target;

        static BuildVision2()
        {
            ModBase.ModName = "Build Vision";
            ModBase.RunOnServer = false;
            LogIO.FileName = logFileName;
            ConfigIO<BvConfig>.FileName = configFileName;

            UpdateActions.Add(() => Instance?.Update());
        }

        public BuildVision2()
        {
            initStarted = false;
            initFinished = false;
        }

        protected override void AfterInit()
        {
            initStarted = true;
            ConfigIO.LoadStart(InitFinish, true);
        }

        /// <summary>
        /// Finishes initialization upon retrieval of configuration information.
        /// </summary>
        private void InitFinish(BvConfig cfg)
        {
            if (!initFinished && initStarted)
            {
                this.Cfg = cfg;
                KeyBinds.Cfg = cfg.binds;
                PropertiesMenu.Cfg = cfg.menu;
                PropertyBlock.Cfg = cfg.propertyBlock;

                ChatCommands.Init();
                SettingsMenu.Init();
                initFinished = true;

                KeyBinds.Open.OnNewPress += TryOpenMenu;
                KeyBinds.Hide.OnNewPress += TryCloseMenu;
                MyAPIGateway.Utilities.ShowMessage(ModBase.ModName, $"Type {cmdPrefix} help for help. All settings are available through the mod menu.");
            }
        }

        /// <summary>
        /// Unloads all mod data.
        /// </summary>
        protected override void BeforeClose()
        {
            if (initFinished)
            {
                TryCloseMenu();
                ConfigIO?.Save(Cfg);
            }
        }

        /// <summary>
        /// Loads config from file and applies it. Runs in parallel.
        /// </summary>
        public void LoadConfig(bool silent = false)
        {
            if (initFinished)
                ConfigIO.LoadStart((BvConfig value) => Cfg = value, silent);
        }

        /// <summary>
        /// Gets the current configuration and writes it to the config file. 
        /// Runs in parallel.
        /// </summary>
        public void SaveConfig(bool silent = false)
        {
            if (initFinished)
                ConfigIO.SaveStart(Cfg, silent);
        }

        /// <summary>
        /// Resets the current configuration to the default settings and saves them.
        /// </summary>
        public void ResetConfig(bool silent = false)
        {
            if (initFinished)
                ConfigIO.SaveStart(BvConfig.Defaults, silent);

            Cfg = BvConfig.Defaults;
        }

        /// <summary>
        /// Mod main loop. This mod will not work if this isn't being called regularly.
        /// </summary>
        public void Update()
        {
            if (initFinished)
            {
                if (!CanAccessTargetBlock())
                    TryCloseMenu();
            }
        }

        /// <summary>
        /// Opens the menu and/or updates the current target if that target is valid. If it isn't, it closes the menu.
        /// </summary>
        public void TryOpenMenu()
        {
            if (initFinished)
            {
                if (TryGetTarget())
                {
                    Menu.SetTarget(target);
                    PropertiesMenu.menuOpen = true;
                }
                else
                    TryCloseMenu();
            }
        }

        /// <summary>
        /// Closes the menu and clears the current target.
        /// </summary>
        public void TryCloseMenu()
        {
            if (initFinished)
            {
                Menu.Hide();
                PropertiesMenu.menuOpen = false;
                target = null;
            }
        }

        /// <summary>
        /// Checks if the player can access the targeted block.
        /// </summary>
        private bool CanAccessTargetBlock() =>
            BlockInRange() && target.CanLocalPlayerAccess && (!Cfg.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(target.TBlock));

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyCubeBlock block;
            IMyTerminalBlock termBlock;

            if ((Cfg.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && LocalPlayer.TryGetTargetedBlock(8.0, out block))
            {
                termBlock = block as IMyTerminalBlock;

                if (termBlock != null)
                {
                    if (termBlock.HasLocalPlayerAccess())
                    {
                        if (target == null || termBlock != target.TBlock)
                            target = new PropertyBlock(termBlock);

                        return true;
                    }
                    else
                        MyAPIGateway.Utilities.ShowNotification("ACCESS DENIED", 1000, MyFontEnum.Red);
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = 10000.0;

            if (target != null)
                dist = (LocalPlayer.Position - target.GetPosition()).LengthSquared();

            return dist < 100.0;
        }
    }
}