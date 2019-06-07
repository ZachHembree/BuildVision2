using DarkHelmet.Game;
using DarkHelmet.IO;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Build vision main class
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 1)]
    internal sealed partial class BvMain : ModBase
    {
        private const double maxDist = 10d, maxDistSquared = maxDist * maxDist;
        private new static BvMain Instance { get; set; }
        public static BvConfig Cfg { get { return BvConfig.Current; } }
        private static PropertiesMenu PropertiesMenu { get { return PropertiesMenu.Instance; } }
        private static CmdManager CmdManager { get { return CmdManager.Instance; } }
        private static BindManager BindManager { get { return BindManager.Instance; } }
        private static HudUtilities HudElements { get { return HudUtilities.Instance; } }

        private PropertyBlock target;
        private bool LoadFinished, LoadStarted;

        static BvMain()
        {
            ModName = "Build Vision";
            LogIO.FileName = "bvLog.txt";
            CmdManager.Prefix = "/bv2";
            BvConfig.FileName = "BuildVision2Config.xml";
        }

        public BvMain()
        {
            LoadStarted = false;
            LoadFinished = false;
        }

        protected override void AfterInit()
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
                Instance = (BvMain)ModBase.Instance;

                BindManager.RegisterBinds(new string[] { "open", "close", "select", "scrollup", "scrolldown", "multx", "multy", "multz" });
                KeyBinds.Cfg = Cfg.binds;

                CmdManager.AddCommands(GetChatCommands());
                MenuUtilities.AddMenuElements(GetSettingsMenuElements());

                KeyBinds.Open.OnNewPress += TryOpenMenu;
                KeyBinds.Hide.OnNewPress += TryCloseMenu;

                SendChatMessage($"Type {CmdManager.Prefix} help for help. All settings are available through the mod menu.");
            }
        }

        protected override void BeforeClose()
        {
            if (LoadFinished)
            {
                LoadStarted = false;
                LoadFinished = false;

                BvConfig.Save();
                TryCloseMenu();
            }
        }

        /// <summary>
        /// Mod main loop. This mod will not work if this isn't being called regularly.
        /// </summary>
        protected override void Update()
        {
            if (LoadFinished)
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
            if (LoadFinished)
            {
                if (TryGetTarget() && CanAccessTargetBlock())
                {
                    PropertiesMenu.SetTarget(target);
                    PropertiesMenu.MenuOpen = true;
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
            if (LoadFinished)
                PropertiesMenu.MenuOpen = false;
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

            if ((Cfg.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && LocalPlayer.TryGetTargetedBlock(maxDist, out block))
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

            return dist < maxDistSquared;
        }
    }
}