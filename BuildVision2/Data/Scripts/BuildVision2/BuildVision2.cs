using RichHudFramework;
using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

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

        private PropertyBlock target;

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

            BvBinds.Open.OnNewPress += TryOpenMenu;
            BvBinds.Hide.OnNewPress += TryCloseMenu;
            SharedBinds.Escape.OnNewPress += TryCloseMenu;
        }

        public override void BeforeClose()
        {
            BvConfig.Save();
            TryCloseMenu();

            if (!ExceptionHandler.Unloading)
                RichHudClient.Reset();
            else
                Instance = null;
        }

        protected override void Update()
        {
            if (PropertiesMenu.Open && (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None))
                TryCloseMenu();
        }

        /// <summary>
        /// Checks if the player can access the targeted block.
        /// </summary>
        private bool CanAccessTargetBlock() =>
            target?.TBlock != null && BlockInRange() && target.CanLocalPlayerAccess && (!Cfg.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(target.TBlock));

        /// <summary>
        /// Opens the menu and/or updates the current target if that target is valid. If it isn't, it closes the menu.
        /// </summary>
        private void TryOpenMenu()
        {
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                PropertiesMenu.Target = target;
                PropertiesMenu.Show();
            }
            else
                TryCloseMenu();
        }

        /// <summary>
        /// Closes the menu and clears the current target.
        /// </summary>
        private void TryCloseMenu()
        {
            PropertiesMenu.Hide();
        }

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyTerminalBlock block;

            if ((Cfg.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && TryGetTargetedBlock(Cfg.general.maxOpenRange, out block))
            {
                if (block != null)
                {
                    if (block.HasLocalPlayerAccess())
                    {
                        if (target == null || block != target.TBlock)
                        {
                            target = new PropertyBlock(block);
                        }
                        return true;
                    }
                    else
                        MyAPIGateway.Utilities.ShowNotification("Access denied", 1000, MyFontEnum.Red);
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to retrieve targeted <see cref="IMyTerminalBlock"/> on a grid within a given distance.
        /// </summary>
        private static bool TryGetTargetedBlock(double maxDist, out IMyTerminalBlock target)
        {
            IMyCubeGrid grid;
            IHitInfo rayInfo;
            Vector3D headPos = LocalPlayer.HeadTransform.Translation, forward = LocalPlayer.HeadTransform.Forward;
            LineD line = new LineD(headPos, headPos + forward * maxDist);
            target = null;

            if (LocalPlayer.TryGetTargetedGrid(line, out grid, out rayInfo))
            {
                double currentDist = double.PositiveInfinity, currentCenterDist = double.PositiveInfinity;
                var sphere = new BoundingSphereD(rayInfo.Position, (grid.GridSizeEnum == MyCubeSize.Large) ? 1.3 : .3);
                List<IMySlimBlock> blocks = grid.GetBlocksInsideSphere(ref sphere);

                foreach (IMySlimBlock slimBlock in blocks)
                {
                    BoundingBoxD box; slimBlock.GetWorldBoundingBox(out box);
                    double newDist = box.DistanceSquared(rayInfo.Position),
                        newCenterDist = Vector3D.DistanceSquared(box.Center, rayInfo.Position);
                    var fatBlock = slimBlock?.FatBlock as IMyTerminalBlock;

                    if ((fatBlock != null || currentDist > 0d) && (newDist < currentDist || (newDist == 0d && newCenterDist < currentCenterDist)))
                    {
                        target = fatBlock;
                        currentDist = newDist;
                        currentCenterDist = newCenterDist;
                    }
                }

                if (target == null)
                {
                    IMySlimBlock slimBlock;
                    double dist;
                    grid.GetLineIntersectionExactAll(ref line, out dist, out slimBlock);
                    target = slimBlock?.FatBlock as IMyTerminalBlock;
                }
            }

            return target != null;
        }

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (target != null)
                dist = (LocalPlayer.Position - target.Position).LengthSquared();

            return dist < (Cfg.general.maxControlRange * Cfg.general.maxControlRange);
        }
    }

    public abstract class BvComponentBase : ModBase.ComponentBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}