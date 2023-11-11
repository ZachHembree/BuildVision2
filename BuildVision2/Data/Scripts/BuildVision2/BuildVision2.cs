using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.ModAPI;
using VRage.Game.Components;
using RichHudFramework;
using RichHudFramework.UI.Client;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build Vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, -1)]
    public sealed partial class BvMain : ModBase
    {
        public const string modName = "Build Vision";

        public static BvMain Instance { get; private set; }

        public static BvConfig Cfg => BvConfig.Current;

        public BvMain() : base(true, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of BvMain can exist at any given time.");

            LogIO.FileName = "bvLog.txt";
            BvConfig.FileName = "BuildVision2Config.xml";
            
            ExceptionHandler.ModName = modName;
            ExceptionHandler.PromptForReload = true;
            ExceptionHandler.RecoveryLimit = 3;
        }

        protected override void AfterLoadData()
        {
            BvApiMaster.Init();
            BvServer.Init();
        }

        protected override void AfterInit()
        {
            if (ExceptionHandler.IsClient)
            {
                CanUpdate = false;
                RichHudClient.Init(ExceptionHandler.ModName, HudInit, OnHudReset);
            }
        }

        private void HudInit()
        {
            if (ExceptionHandler.IsClient)
            {
                CanUpdate = true;

                BvBinds.Init();
                BvConfig.Load();
                AddChatCommands();
                InitSettingsMenu();
                TerminalUtilities.Init();
                QuickActionHudSpace.Init();                
                
                if (BvConfig.WasConfigOld)
                    RichHudTerminal.OpenToPage(helpMain);

                BvConfig.OnConfigLoad += UpdateBindPageVisibility;
                UpdateBindPageVisibility();

#if PLUGIN_LOADER
                ExceptionHandler.SendChatMessage(
                    "Build Vision has been launched in client-only mode by the Plugin Loader. " +
                    "Some functionality will be unavailable."
                );
#endif
            }
        }

        protected override void Update()
        {
            QuickActionHudSpace.Update();
        }

        private void OnHudReset() { }

        public override void BeforeClose()
        {
            if (ExceptionHandler.IsClient)
            {
                BvConfig.Save();
                QuickActionHudSpace.Close();
                BvConfig.OnConfigLoad -= UpdateBindPageVisibility;
            }

            if (ExceptionHandler.Unloading)
            {
                TerminalUtilities.Close();
                Instance = null;
            }
        }

        private void UpdateBindPageVisibility()
        {
            bindsPage.Enabled = !BvConfig.Current.genUI.legacyModeEnabled;
            legacyBindsPage.Enabled = BvConfig.Current.genUI.legacyModeEnabled;
        }
    }

    public abstract class BvComponentBase : ModBase.ModuleBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}