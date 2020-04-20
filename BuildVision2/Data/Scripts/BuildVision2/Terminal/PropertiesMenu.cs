using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using RichHudFramework.UI.Client;
using RichHudFramework.IO;
using VRageMath;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class PropertiesMenu : BvComponentBase
    {
        /// <summary>
        /// Current UI configuration
        /// </summary>
        public static HudConfig Cfg { get { return BvConfig.Current.menu.hudConfig; } set { BvConfig.Current.menu.hudConfig = value; } }

        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target => Instance.target;

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get { return Instance.scrollMenu.Visible; } set { Instance.scrollMenu.Visible = value; } }

        private static PropertiesMenu Instance
        {
            get { Init(); return _instance; }
            set { _instance = value; }
        }
        private static PropertiesMenu _instance;

        private readonly BvScrollMenu scrollMenu;
        private PropertyBlock target;
        private IMyTerminalBlock lastPastedTarget;
        private BlockData clipboard, pasteBackup;
        private int peakTick;
        private bool canPeak;

        private PropertiesMenu() : base(false, true)
        {
            scrollMenu = new BvScrollMenu() { Visible = false };
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;

            BvBinds.Hide.OnNewPress += Hide;
            SharedBinds.Escape.OnNewPress += Hide;
        }

        private static void Init()
        {
            if (_instance == null)
                _instance = new PropertiesMenu();
        }

        public static void TryOpenMenu() =>
            Instance.TryOpen();

        public static void HideMenu() =>
            Instance.Hide();

        public override void Close()
        {
            Hide();
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

        /// <summary>
        /// Intercepts chat input when a property is open
        /// </summary>
        private void MessageHandler(string message, ref bool sendToOthers)
        {
            if (scrollMenu.Visible)
                sendToOthers = false;
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public override void Update()
        {
            if (Open && (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None))
                Hide();

            if (target != null && Open)
            {
                scrollMenu.UpdateText();
            }
        }

        public override void HandleInput()
        {
            if (BvBinds.MultX.IsNewPressed)
            {
                canPeak = true;
                peakTick = 0;
            }
            else if (BvBinds.MultX.IsReleased)
            {
                canPeak = false;

                if (Open && scrollMenu.MenuMode == ScrollMenuModes.Peak)
                    Hide();
            }

            if (BvBinds.Open.IsNewPressed)
                TryOpen();
            else if (BvBinds.MultX.IsPressed && ((!Open && canPeak) || scrollMenu.MenuMode == ScrollMenuModes.Peak))
            {
                if (peakTick == 0)
                    TryPeak();

                peakTick++;

                if (peakTick == 15)
                    peakTick = 0;
            }

            if (target != null && Open)
            {
                if (BvBinds.CopySelection.IsNewPressed && scrollMenu.MenuMode == ScrollMenuModes.Copy)
                {
                    clipboard = new BlockData(target.TypeID, scrollMenu.GetReplicationRange());
                    scrollMenu.ShowNotification($"Copied {clipboard.terminalProperties.Count} Properties");
                }

                if (BvBinds.PasteProperties.IsNewPressed && !clipboard.Equals(default(BlockData)) && clipboard.terminalProperties.Count > 0)
                {
                    if (clipboard.blockTypeID == target.TypeID)
                    {
                        pasteBackup = target.ExportSettings();
                        lastPastedTarget = target.TBlock;

                        int importCount = target.ImportSettings(clipboard);
                        scrollMenu.ShowNotification($"Pasted {importCount} Properties");
                    }
                    else
                        scrollMenu.ShowNotification($"Paste Incompatible");
                }

                if (BvBinds.UndoPaste.IsNewPressed && target.TBlock == lastPastedTarget)
                {
                    target.ImportSettings(pasteBackup);
                    scrollMenu.ShowNotification("Paste Undone");
                    lastPastedTarget = null;
                }
            }
        }

        public override void Draw()
        {
            if (Cfg.resolutionScaling)
                scrollMenu.Scale = Cfg.hudScale * HudMain.ResScale;
            else
                scrollMenu.Scale = Cfg.hudScale;

            scrollMenu.BgOpacity = Cfg.hudOpacity;
            scrollMenu.MaxVisible = Cfg.maxVisible;

            if (target != null && Open)
            {
                Vector3D targetPos, worldPos;
                Vector2 screenPos, screenBounds = Vector2.One;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !Cfg.useCustomPos)
                {
                    targetPos = Target.Position + Target.modelOffset * .75;
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);

                    screenPos = new Vector2((float)worldPos.X, (float)worldPos.Y);
                    screenBounds -= HudMain.GetRelativeVector(scrollMenu.Size / 2f);
                    scrollMenu.AlignToEdge = false;
                }
                else
                {
                    screenPos = Cfg.hudPos;
                    scrollMenu.AlignToEdge = true;
                }

                if (Cfg.clampHudPos)
                {
                    screenPos.X = MathHelper.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                    screenPos.Y = MathHelper.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                scrollMenu.Offset = HudMain.GetPixelVector(screenPos);
            }
        }

        private void TryPeak()
        {
            scrollMenu.MenuMode = ScrollMenuModes.Peak;

            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.SetTarget(target);
                Open = true;
            }
        }

        private void TryOpen()
        {
            scrollMenu.MenuMode = ScrollMenuModes.Control;
            canPeak = false;

            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.SetTarget(target);
                Open = true;
            }
            else
                Hide();
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        private void Hide()
        {
            Open = false;
            target = null;
            scrollMenu.Clear();
        }

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyTerminalBlock block;

            if ((BvConfig.Current.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) && TryGetTargetedBlock(BvConfig.Current.general.maxOpenRange, out block))
            {
                if (block != null)
                {
                    if (block.HasLocalPlayerAccess())
                    {
                        if (target == null || block != target.TBlock)
                            target = new PropertyBlock(block);

                        return true;
                    }
                    else if (scrollMenu.MenuMode != ScrollMenuModes.Peak)
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
        /// Checks if the player can access the targeted block.
        /// </summary>
        private bool CanAccessTargetBlock() =>
            target?.TBlock != null && BlockInRange() && target.CanLocalPlayerAccess && (!BvConfig.Current.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(target.TBlock));

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (target != null)
                dist = (LocalPlayer.Position - target.Position).LengthSquared();

            return dist < (BvConfig.Current.general.maxControlRange * BvConfig.Current.general.maxControlRange);
        }
    }
}