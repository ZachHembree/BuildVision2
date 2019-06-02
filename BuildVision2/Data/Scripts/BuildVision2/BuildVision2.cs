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
    internal sealed partial class BvMain : ConfigurableMod<BvConfig>
    {
        private const double maxDist = 10d, maxDistSquared = maxDist * maxDist;
        private new static BvMain Instance { get; set; }
        private static PropertiesMenu Menu { get { return PropertiesMenu.Instance; } }
        private static CmdManager CmdManager { get { return CmdManager.Instance; } }
        private static BindManager BindManager { get { return BindManager.Instance; } }
        private static HudUtilities HudElements { get { return HudUtilities.Instance; } }

        public static BvConfig Cfg
        {
            get { return cfg; }
            set
            {
                cfg = value;

                if (Instance.initFinished && cfg != null)
                {
                    cfg.Validate();
                    KeyBinds.Cfg = cfg.binds;
                    PropertiesMenu.Cfg = cfg.menu;
                    PropertyBlock.Cfg = cfg.propertyBlock;
                }
            }
        }

        private static BvConfig cfg;
        private PropertyBlock target;

        static BvMain()
        {
            ModName = "Build Vision";
            CmdManager.Prefix = "/bv2";
            LogIO.FileName = "bvLog.txt";
            ConfigIO<BvConfig>.FileName = "BuildVision2Config.xml";

            UpdateActions.Add(() => Instance?.Update());
        }

        /// <summary>
        /// Finishes initialization upon retrieval of configuration information.
        /// </summary>
        protected override void InitFinish()
        {
            if (!initFinished && initStarted)
            {
                Instance = (BvMain)ModBase.Instance;
                BindManager.RegisterBinds(new string[] { "open", "close", "select", "scrollup", "scrolldown", "multx", "multy", "multz" });
                //Cfg = cfg;

                CmdManager.AddCommands(GetChatCommands());
                MenuUtilities.AddMenuElements(GetSettingsMenuElements());
                PropertiesMenu.Init();

                KeyBinds.Open.OnNewPress += TryOpenMenu;
                KeyBinds.Hide.OnNewPress += TryCloseMenu;

                SendChatMessage($"Type {CmdManager.Prefix} help for help. All settings are available through the mod menu.");
            }
        }

        /// <summary>
        /// Unloads all mod data.
        /// </summary>
        protected override void BeforeUnload()
        {
            TryCloseMenu();
            PropertiesMenu.Close();
        }

        /// <summary>
        /// Mod main loop. This mod will not work if this isn't being called regularly.
        /// </summary>
        private void Update()
        {
            if (initFinished)
            {
                if (!CanAccessTargetBlock())
                    TryCloseMenu();

                PropertiesMenu.Instance?.Update();
            }
        }

        /// <summary>
        /// Opens the menu and/or updates the current target if that target is valid. If it isn't, it closes the menu.
        /// </summary>
        public void TryOpenMenu()
        {
            if (initFinished)
            {
                if (TryGetTarget() && CanAccessTargetBlock())
                {
                    Menu.SetTarget(target);
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
            if (initFinished)
            {
                //Menu.Hide();
                PropertiesMenu.MenuOpen = false;
                //target = null;
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

            if ((Cfg.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && LocalPlayer.TryGetTargetedBlock(maxDist, out block))
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