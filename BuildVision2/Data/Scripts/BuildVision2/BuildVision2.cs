using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.UI.Client;
using System;
using VRage.Game.Components;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build Vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, -1)]
    public sealed partial class BvMain : ModBase
    {
        public const string ModName = "Build Vision";

        public static BvMain Instance { get; private set; }

        public static BvConfig Cfg => BvConfig.Current;

        public BvMain() : base(true, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of BvMain can exist at any given time.");

            BvConfig.FileName = "BuildVision2Config.xml";

            ExceptionHandler.ModName = ModName;
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