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
using ProtoBuf;

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

        public BvMain() : base(true, true)
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

                BvConfig.Load();
                AddChatCommands();
                InitSettingsMenu();
                PropertiesMenu.Init();
            }
        }

        private void OnHudReset() { }

        public override void BeforeClose()
        {
            if (ExceptionHandler.IsClient)
            {
                BvConfig.Save();

                if (ExceptionHandler.Unloading)
                    Instance = null;
            }
        }
    }

    public abstract class BvComponentBase : ModBase.ModuleBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}