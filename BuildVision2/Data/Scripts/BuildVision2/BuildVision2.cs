using RichHudFramework.Game;
using RichHudFramework.Client;
using RichHudFramework.UI;
using RichHudFramework.IO;
using RichHudFramework;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using System;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 0)]
    internal sealed partial class BvMain : ModBase
    {
        public static BvMain Instance { get; private set; }
        public static BvConfig Cfg { get { return BvConfig.Current; } }

        private PropertyBlock target;
        private CmdManager.Group bvCommands;

        public BvMain() : base(false, true)
        {
            if (Instance == null)
                Instance = this;
            else
                throw new Exception("Only one instance of BvMain can exist at any given time.");

            ModName = "Build Vision";
            LogIO.FileName = "bvLog.txt";
            BvConfig.FileName = "BuildVision2Config.xml";

            promptForReload = true;
            recoveryLimit = 2;
        }

        protected override void AfterInit()
        {
            RichHudClient.Init(ModName, HudInit, Reload);
            BvConfig.Load(true);
        }

        private void HudInit()
        {
            bvCommands = CmdManager.AddOrGetCmdGroup("/bv2", GetChatCommands());
            InitSettingsMenu();

            BvBinds.Open.OnNewPress += TryOpenMenu;
            BvBinds.Hide.OnNewPress += TryCloseMenu;
            SharedBinds.Escape.OnNewPress += TryCloseMenu;
        }

        protected override void BeforeClose()
        {
            BvConfig.Save();
            TryCloseMenu();

            if (Reloading)
                RichHudClient.Reset();

            if (Unloading)
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
            target != null && BlockInRange() && target.CanLocalPlayerAccess && (!Cfg.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(target.TBlock));

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
        private static bool TryGetTargetedBlock(double maxDist, out IMyTerminalBlock tBlock)
        {
            IMyCubeGrid grid;
            IHitInfo rayInfo;
            Vector3D headPos = LocalPlayer.HeadTransform.Translation, forward = LocalPlayer.HeadTransform.Forward;
            LineD lineA = new LineD(headPos, headPos + forward * maxDist);

            double dist;
            tBlock = null;

            if (LocalPlayer.TryGetTargetedGrid(lineA, out grid, out rayInfo))
            {
                IMySlimBlock blockA, blockB;
                LineD lineB = new LineD(rayInfo.Position - (rayInfo.Normal * .3f), rayInfo.Position);

                grid.GetLineIntersectionExactAll(ref lineA, out dist, out blockA);
                grid.GetLineIntersectionExactAll(ref lineB, out dist, out blockB);

                var fatA = blockA?.FatBlock as IMyTerminalBlock;
                var fatB = blockB?.FatBlock as IMyTerminalBlock;

                if (fatA != null && fatB != null)
                {
                    BoundingBoxD boundA, boundB;

                    blockA.GetWorldBoundingBox(out boundA);
                    blockB.GetWorldBoundingBox(out boundB);

                    if (boundB.Distance(rayInfo.Position) < boundA.Distance(rayInfo.Position))
                        tBlock = fatB;
                    else
                        tBlock = fatA;
                }
                else if (fatA != null)
                    tBlock = fatA;
                else
                    tBlock = fatB;
            }

            return tBlock != null;
        }

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (target != null)
                dist = (LocalPlayer.Position - target.GetPosition()).LengthSquared();

            return dist < (Cfg.general.maxControlRange * Cfg.general.maxControlRange);
        }        
    }

    public abstract class BvComponentBase : ModBase.ComponentBase
    {
        public BvComponentBase(bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient, BvMain.Instance)
        { }
    }
}