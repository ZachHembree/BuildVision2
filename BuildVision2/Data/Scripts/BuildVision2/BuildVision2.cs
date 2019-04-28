using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Xml.Serialization;
using DarkHelmet.IO;
using DarkHelmet.UI;
using DarkHelmet.Input;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class BuildVision2 : MySessionComponentBase
    {
        private static BvMain BuildVision { get { return BvMain.Instance; } }
        private bool crashed = false, isDedicated = false, isServer = false;

        public override void Draw()
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
                    if (BuildVision != null)
                        BuildVision.Draw();
                }
                catch (Exception e)
                {
                    crashed = true;
                    BvMain.Log?.TryWriteToLog("Build Vision has crashed!\n" + e.ToString());
                    MyAPIGateway.Utilities.ShowMissionScreen("Build Vision 2", "Debug", "",
                        "Build Vision has crashed! Press the X in the upper right hand corner if you don't want " +
                        "" + "it to reload.\n" + e.ToString(), AllowReload, "Reload");

                    TryUnload();
                }
            }
        }

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
                    if (BuildVision == null)
                        BvMain.Init();

                    if (BuildVision != null)
                        BuildVision.Update();
                }
                catch (Exception e)
                {
                    crashed = true;
                    BvMain.Log?.TryWriteToLog("Build Vision has crashed!\n" + e.ToString());
                    MyAPIGateway.Utilities.ShowMissionScreen("Build Vision 2", "Debug", "",
                        "Build Vision has crashed! Press the X in the upper right hand corner if you don't want " +
                        "" + "it to reload.\n" + e.ToString(), AllowReload, "Reload");

                    TryUnload();
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

        protected override void UnloadData() =>
            TryUnload();

        private void TryUnload()
        {
            try
            {
                BuildVision?.Close();
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

        private const string configFileName = "BuildVision2Config.xml", logFileName = "bvLog.txt", 
            senderName = "Build Vision 2", cmdPrefix = "/bv2";

        public static LogIO Log { get { return LogIO.Instance; } }
        private static ConfigIO<BvConfig> ConfigIO { get { return ConfigIO<BvConfig>.Instance; } }
        private static Binds Binds { get { return Binds.Instance; } }
        private static ChatCommands Cmd { get { return ChatCommands.Instance; } }
        private static PropertiesMenu Menu { get { return PropertiesMenu.Instance; } }
        private static HudUtilities HudElements { get { return HudUtilities.Instance; } }
        private static SettingsMenu Settings { get { return SettingsMenu.Instance; } }

        public BvConfig Cfg { get { return cfg; }
            set
            {
                cfg = value;

                if (init && cfg != null)
                {
                    cfg.Validate();
                    Binds.Cfg = cfg.binds;
                    Menu.Cfg = cfg.menu;
                    PropertyBlock.Cfg = cfg.propertyBlock;
                }
            }
        }

        private BvConfig cfg;
        private PropertyBlock target;
        private bool init, initStart, menuOpen;

        private BvMain()
        {
            init = false;
            initStart = false;
            menuOpen = false;
        }

        public static void Init()
        {
            if (Instance == null)
            {
                Instance = new BvMain();
                Instance.InitStart();
            }
        }

        private void InitStart()
        {
            if (!init && !initStart)
            {
                initStart = true;
                LogIO.Init(logFileName, SendChatMessage);
                ConfigIO<BvConfig>.Init(configFileName, Log, SendChatMessage);
                ConfigIO.LoadStart(InitFinish, true);
            }
        }

        /// <summary>
        /// Finishes initialization upon retrieval of configuration information.
        /// </summary>
        private void InitFinish(BvConfig cfg)
        {
            if (!init && initStart)
            {
                this.Cfg = cfg;
                Binds.Init(cfg.binds);
                ChatCommands.Init(cmdPrefix);
                HudUtilities.Init();
                SettingsMenu.Init();
                PropertiesMenu.Init(cfg.menu);
                PropertyBlock.Cfg = cfg.propertyBlock;

                init = true;
                MyAPIGateway.Utilities.ShowMessage("Build Vision 2", $"Type {cmdPrefix} help for help. All settings are available through the mod menu.");
            }
        }

        /// <summary>
        /// Unloads all mod data.
        /// </summary>
        public void Close()
        {
            if (init)
            {
                TryCloseMenu();
                ConfigIO.Save(Cfg);
            }

            init = false;
            initStart = false;

            Binds?.Close();
            Cmd?.Close();
            Menu?.Close();
            ConfigIO?.Close();
            Log?.Close();
            HudElements?.Close();
            Settings?.Close();
            Instance = null;
        }

        /// <summary>
        /// Loads config from file and applies it. Runs in parallel.
        /// </summary>
        public void LoadConfig(bool silent = false)
        {
            if (init)
                ConfigIO.LoadStart((BvConfig value) => Cfg = value, silent);
        }

        /// <summary>
        /// Gets the current configuration and writes it to the config file. 
        /// Runs in parallel.
        /// </summary>
        public void SaveConfig(bool silent = false)
        {
            if (init)
                ConfigIO.SaveStart(Cfg, silent);
        }

        /// <summary>
        /// Resets the current configuration to the default settings and saves them.
        /// </summary>
        public void ResetConfig(bool silent = false)
        {
            if (init)
                ConfigIO.SaveStart(BvConfig.Defaults, silent);

            Cfg = BvConfig.Defaults;
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
        /// Sends chat message using predetermined sender name.
        /// </summary>
        public void ShowMissionScreen(string subHeading, string message)
        {
            if (initStart)
                MyAPIGateway.Utilities.ShowMissionScreen(senderName, subHeading, null, message, null, "Close");
        }

        /// <summary>
        /// Mod main loop. This mod will not work if this isn't being called regularly.
        /// </summary>
        public void Update()
        {
            if (initStart)
            {
                Log.Update();
                ConfigIO.Update();
            }

            if (init)
            {
                Binds.Update();

                if (menuOpen)
                    Menu.Update(Cfg.general.forceFallbackHud);

                if (Binds.open.IsNewPressed)
                    TryOpenMenu();

                if (Binds.close.IsNewPressed || !CanAccessTargetBlock())
                    TryCloseMenu();
            }
        }

        /// <summary>
        /// Draws hud elements.
        /// </summary>
        public void Draw() =>
            HudElements?.Draw();

        /// <summary>
        /// Opens the menu and/or updates the current target if that target is valid. If it isn't, it closes the menu.
        /// </summary>
        public void TryOpenMenu()
        {
            if (init)
            {
                if (TryGetTarget())
                {
                    Menu.SetTarget(target);
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
                Menu.Hide();
                menuOpen = false;
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