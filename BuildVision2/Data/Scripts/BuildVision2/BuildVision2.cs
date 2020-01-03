using RichHudFramework.Game;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace DarkHelmet.BuildVision2
{
    using RichHudFramework;
    using RichHudFramework.Client;

    /// <summary>
    /// Build vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 1)]
    internal sealed partial class BvMain : RichHudClient
    {
        public static BvConfig Cfg { get { return BvConfig.Current; } }

        private PropertyBlock target;
        private CmdManager.Group bvCommands;
        private bool LoadFinished, LoadStarted;

        static BvMain()
        {
            ModName = "Build Vision";
            LogFileName = "bvLog.txt";
            BvConfig.FileName = "BuildVision2Config.xml";
            TaskPool.MaxTasksRunning = 2;
        }

        public BvMain()
        { }

        protected override void HudInit()
        {
            LoadStarted = true;
            LoadFinished = false;
            BvConfig.LoadStart(InitFinish, true);
        }

        /// <summary>
        /// Finishes initialization upon retrieval of configuration information.
        /// </summary>
        private void InitFinish()
        {
            if (!LoadFinished && LoadStarted)
            {
                LoadFinished = true;
                bvCommands = CmdManager.AddOrGetCmdGroup("/bv2", GetChatCommands());
                InitSettingsMenu();

                if (MenuUtilities.CanAddElements)
                    MenuUtilities.AddMenuElements(GetSettingsMenuElements());

                BvBinds.Open.OnNewPress += TryOpenMenu;
                BvBinds.Hide.OnNewPress += TryCloseMenu;
                SharedBinds.Escape.OnNewPress += TryCloseMenu;

                SendChatMessage($"Type {bvCommands.prefix} help for help. Settings are available through the mod menu.");
            }
        }

        protected override void HudClose()
        {
            if (LoadFinished)
            {
                LoadStarted = false;
                LoadFinished = false;

                BvConfig.Save();
                TryCloseMenu();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (LoadFinished)
            {
                if (PropertiesMenu.Open && (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None))
                    TryCloseMenu();
            }
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
            if (LoadFinished)
            {
                if (TryGetTarget() && CanAccessTargetBlock())
                {
                    PropertiesMenu.Target = target;
                    PropertiesMenu.Show();
                }
                else
                    TryCloseMenu();
            }
        }

        /// <summary>
        /// Closes the menu and clears the current target.
        /// </summary>
        private void TryCloseMenu()
        {
            if (LoadFinished)
                PropertiesMenu.Hide();
        }

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyCubeBlock block;
            IMyTerminalBlock termBlock;

            if ((Cfg.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && LocalPlayer.TryGetTargetedBlock(Cfg.general.maxOpenRange, out block, 2))
            {
                termBlock = block as IMyTerminalBlock;

                if (termBlock != null)
                {
                    if (termBlock.HasLocalPlayerAccess())
                    {
                        if (target == null || termBlock != target.TBlock)
                        {
                            target = new PropertyBlock(termBlock);
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
}