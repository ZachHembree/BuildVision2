using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Xml.Serialization;
using ParallelTasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    [XmlType(TypeName = "GeneralSettings")]
    public struct GeneralConfig
    {
        [XmlIgnore]
        public static readonly GeneralConfig defaults = new GeneralConfig(false, true, true);

        [XmlElement(ElementName = "ForceFallbackHud")]
        public bool forceFallbackHud;

        [XmlElement(ElementName = "CloseIfTargetNotInView")]
        public bool closeIfNotInView;

        [XmlElement(ElementName = "CanOpenIfHandsNotEmpty")]
        public bool canOpenIfHandsNotEmpty;

        public GeneralConfig(bool forceFallbackHud, bool closeIfNotInView, bool canOpenIfHandsNotEmpty)
        {
            this.forceFallbackHud = forceFallbackHud;
            this.closeIfNotInView = closeIfNotInView;
            this.canOpenIfHandsNotEmpty = canOpenIfHandsNotEmpty;
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class BuildVision2 : MySessionComponentBase
    {
        private BvMain buildVision;
        private bool crashed = false, isDedicated = false, isServer = false;

        public override void UpdateAfterSimulation()
        {
            if (!isDedicated)
            {
                isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);
            }

            if (!crashed && !isDedicated)
            {
                try
                {
                    if (buildVision == null)
                        buildVision = BvMain.GetInstance();

                    buildVision.Update();
                }
                catch (Exception e)
                {
                    crashed = true;
                    buildVision?.log?.TryWriteToLog("Build Vision has crashed!\n" + e.ToString());
                    MyAPIGateway.Utilities.ShowMissionScreen("Build Vision 2", "Debug", "", 
                        "Build Vision has crashed! Press the X in the upper right hand corner if you don't want " +
                        "" + "it to reload.\n" + e.ToString(), AllowReload, "Reload");
                }
            }
        }

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                crashed = false;
            else
                crashed = true;
        }

        protected override void UnloadData() 
        {
            try
            {
                buildVision?.Close();
                buildVision = null;
            }
            catch
            {
                // you're kinda screwed at this point
            }
        }
    }
    
    internal sealed class BvMain
    {
        public static BvMain Instance { get; private set; }
        public readonly LogIO log;
        public bool forceFallbackHud, closeIfNotInView, canOpenIfHandsNotEmpty;

        private const string configFileName = "BuildVision2Config.xml", logFileName = "bvLog.txt", 
            senderName = "Build Vision 2", cmdPrefix = "/bv2";
        
        private readonly ConfigIO config;
        private Binds binds;
        private ChatCommands cmd;
        private PropertyBlock target;
        private PropertiesMenu menu;
        private bool init, initStart, menuOpen;

        private BvMain()
        {
            init = false;
            menuOpen = false;
            log = LogIO.GetInstance(this, logFileName);
            config = ConfigIO.GetInstance(this, log, configFileName);
        }

        public static BvMain GetInstance()
        {
            if (Instance == null)
            {
                Instance = new BvMain();
                Instance.InitStart();
            }

            return Instance;
        }

        private void InitStart()
        {
            if (!init && !initStart)
            {
                initStart = true;
                config.LoadConfigStart(InitFinish, true, true);
            }
        }

        /// <summary>
        /// Finishes initialization upon retrieval of configuration information.
        /// </summary>
        /// <param name="cfg"></param>
        private void InitFinish(ConfigData cfg)
        {
            if (!init && initStart)
            {
                if (cfg != null)
                {
                    cfg.Validate();
                    UpdateGeneralConfig(cfg.general);
                    binds = Binds.GetInstance(cfg.binds);
                    cmd = ChatCommands.GetInstance(binds, cmdPrefix);
                    menu = PropertiesMenu.GetInstance(cfg.menu, binds);
                    init = true;
                }
                else
                {
                    UpdateGeneralConfig(GeneralConfig.defaults);
                    binds = Binds.GetInstance(BindsConfig.Defaults);
                    cmd = ChatCommands.GetInstance(binds, cmdPrefix);
                    menu = PropertiesMenu.GetInstance(PropMenuConfig.defaults, binds);

                    init = true;
                    SendChatMessage("Unable to load config file. Default settings loaded.");
                    ResetConfig();
                }

                MyAPIGateway.Utilities.ShowMessage("Build Vision 2", $"Type {cmdPrefix} help for help");
            }
        }

        /// <summary>
        /// Unloads all mod data.
        /// </summary>
        public void Close()
        {
            if (init) config.SaveConfig(GetConfig());
            init = false;
            initStart = false;

            binds?.Close();
            cmd?.Close();
            menu?.Close();
            config?.Close();
            log?.Close();
            Instance = null;
        }

        /// <summary>
        /// Loads config from file and applies it. Runs in parallel.
        /// </summary>
        public void LoadConfig()
        {
            if (init)
                config.LoadConfigStart(UpdateConfig, false);
        }

        /// <summary>
        /// Gets the current configuration and writes it to the config file. 
        /// Runs in parallel.
        /// </summary>
        public void SaveConfig()
        {
            if (init)
                config.SaveConfigStart(GetConfig());
        }

        /// <summary>
        /// Resets the current configuration to the default settings and saves them.
        /// </summary>
        public void ResetConfig()
        {
            if (init)
                config.SaveConfigStart(ConfigData.defaults);

            UpdateConfig(ConfigData.defaults);
        }

        /// <summary>
        /// Updates current configuration with given config data.
        /// </summary>
        public void UpdateConfig(ConfigData cfg)
        {
            if (init && cfg != null)
            {
                cfg.Validate();
                UpdateGeneralConfig(cfg.general);
                binds.UpdateConfig(cfg.binds);
                menu.UpdateConfig(cfg.menu);
            }
        }

        /// <summary>
        /// Updates the general configuration
        /// </summary>
        public void UpdateGeneralConfig(GeneralConfig cfg)
        {
            forceFallbackHud = cfg.forceFallbackHud;
            closeIfNotInView = cfg.closeIfNotInView;
            canOpenIfHandsNotEmpty = cfg.canOpenIfHandsNotEmpty;
        }

        /// <summary>
        /// Returns the currently loaded configuration.
        /// </summary>
        public ConfigData GetConfig()
        {
            if (init)
            {
                return new ConfigData
                {
                    general = new GeneralConfig(forceFallbackHud, closeIfNotInView, canOpenIfHandsNotEmpty),
                    binds = binds.GetConfig(),
                    menu = menu.GetConfig(),
                    propertyBlock = PropertyBlock.GetConfig()
                };
            }
            else
                return null;
        }

        /// <summary>
        /// Sends chat message using predetermined sender name.
        /// </summary>
        public void SendChatMessage(string message)
        {
            if (initStart)
                MyAPIGateway.Utilities.ShowMessage(senderName, message);
        }

        /// <summary>
        /// Mod main loop. This mod will not work if this isn't being called regularly.
        /// </summary>
        public void Update()
        {
            if (initStart)
            {
                log.Update();
                config.Update();
            }

            if (init)
            {
                binds.Update();

                if (menuOpen)
                    menu.Update(forceFallbackHud);

                if (binds.open.IsNewPressed)
                    TryOpenMenu();

                if (binds.close.IsNewPressed || !CanAccessTargetBlock())
                    TryCloseMenu();
            }
        }

        /// <summary>
        /// Opens the menu and/or updates the current target if that target is valid. If it isn't, it closes the menu.
        /// </summary>
        public void TryOpenMenu()
        {
            if (init)
            {
                if (TryGetTarget())
                {
                    menu.SetTarget(target);
                    menuOpen = true;
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
            if (init)
            {
                menu.Hide();
                menuOpen = false;
                target = null;
            }
        }

        /// <summary>
        /// Checks if the player can access the targeted block.
        /// </summary>
        private bool CanAccessTargetBlock() =>
            BlockInRange() && target.CanLocalPlayerAccess && (!closeIfNotInView || LocalPlayer.IsLookingInBlockDir(target.TBlock));

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyCubeBlock block;
            IMyTerminalBlock termBlock;

            if ((canOpenIfHandsNotEmpty || LocalPlayer.HasEmptyHands) && LocalPlayer.TryGetTargetedBlock(8.0, out block))
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
                        MyAPIGateway.Utilities.ShowNotification("ACCESS DENIED", font: MyFontEnum.Red);
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