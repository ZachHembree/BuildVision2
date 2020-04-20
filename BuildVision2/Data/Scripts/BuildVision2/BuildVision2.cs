using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using VRage.Game.Components;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build Vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, -1)]
    public sealed partial class BvMain : ModBase
    {
        public static BvMain Instance { get; private set; }
        public static BvConfig Cfg => BvConfig.Current;

        public BvMain() : base(false, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of BvMain can exist at any given time.");

            LogIO.FileName = "bvLog.txt";
            BvConfig.FileName = "BuildVision2Config.xml";

            ExceptionHandler.ModName = "Build Vision";
            ExceptionHandler.PromptForReload = true;
            ExceptionHandler.RecoveryLimit = 3;
        }

        protected override void AfterInit()
        {
            CanUpdate = false;
            RichHudClient.Init(ExceptionHandler.ModName, HudInit, Reload);
        }

        private void HudInit()
        {
            CanUpdate = true;

            BvConfig.Load(true);
            CmdManager.AddOrGetCmdGroup("/bv2", GetChatCommands());
            InitSettingsMenu();
        }

        public override void BeforeClose()
        {
            BvConfig.Save();

            if (!ExceptionHandler.Unloading)
                RichHudClient.Reset();
            else
                Instance = null;
        }
    }

    public abstract class BvComponentBase : ModBase.ComponentBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}