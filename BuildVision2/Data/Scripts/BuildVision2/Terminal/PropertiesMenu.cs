using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class PropertiesMenu : BvComponentBase
    {
        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target => Instance.targetBlock;

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get { return Instance.scrollMenu.Visible; } set { Instance.scrollMenu.Visible = value; } }

        /// <summary>
        /// Returns the menu's current mode (peek/control/copy)
        /// </summary>
        public static ScrollMenuModes MenuMode => Instance.scrollMenu.MenuMode;

        /// <summary>
        /// Draws the bounding box of the target. Used for debugging.
        /// </summary>
        public static bool DrawBoundingBox { get; set; }

        private static PropertiesMenu Instance
        {
            get { Init(); return _instance; }
            set { _instance = value; }
        }
        private static PropertiesMenu _instance;
        private const long peekTime = 100 * TimeSpan.TicksPerMillisecond;

        private readonly BvScrollMenu scrollMenu;
        private readonly TerminalGrid targetGrid;
        private PropertyBlock targetBlock;
        private IMyTerminalBlock lastPastedTarget;
        private BlockData clipboard, pasteBackup;
        private Utils.Stopwatch peekRefresh;

        private readonly BlockBoard boundsTest;

        private PropertiesMenu() : base(false, true)
        {
            DrawBoundingBox = false;
            scrollMenu = new BvScrollMenu() { Visible = false };
            targetGrid = new TerminalGrid();

            RichHudCore.LateMessageEntered += MessageHandler;
            peekRefresh = new Utils.Stopwatch();
            peekRefresh.Start();

            SharedBinds.Escape.OnNewPress += Hide;

            boundsTest = new BlockBoard();
            boundsTest.Front.Color = Color.Blue.SetAlphaPct(0.7f);
            boundsTest.Back.Color = Color.LightBlue.SetAlphaPct(0.7f);
            boundsTest.Top.Color = Color.Red.SetAlphaPct(0.7f);
            boundsTest.Bottom.Color = Color.Orange.SetAlphaPct(0.7f);
            boundsTest.Left.Color = Color.Green.SetAlphaPct(0.7f);
            boundsTest.Right.Color = Color.DarkOliveGreen.SetAlphaPct(0.7f);
        }

        public static void Init()
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
            RichHudCore.LateMessageEntered -= MessageHandler;
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

            if (targetBlock != null && Open)
                scrollMenu.UpdateText();
        }

        public override void HandleInput()
        {
            if (BvBinds.Open.IsNewPressed && BvBinds.Hide.IsNewPressed)
                ToggleOpen();
            else if (BvBinds.Open.IsNewPressed)
                TryOpen();
            else if (BvBinds.Hide.IsNewPressed)
                Hide();

            if (BvConfig.Current.general.enablePeek)
            {
                if (BvBinds.Peek.IsPressed && (!Open || scrollMenu.MenuMode == ScrollMenuModes.Peek))
                {
                    if (BvBinds.Peek.IsNewPressed || peekRefresh.ElapsedTicks > peekTime)
                    {
                        TryPeek();
                        peekRefresh.Reset();
                    }
                }
                else if (BvBinds.Peek.IsReleased && Open && scrollMenu.MenuMode == ScrollMenuModes.Peek)
                    Hide();
            }

            if (targetBlock != null && Open && scrollMenu.MenuMode != ScrollMenuModes.Peek)
            {
                if (BvBinds.CopySelection.IsNewPressed && scrollMenu.MenuMode == ScrollMenuModes.Dupe)
                {
                    clipboard = new BlockData(targetBlock.TypeID, scrollMenu.GetDuplicationRange());
                    scrollMenu.ShowNotification($"Copied {clipboard.terminalProperties.Count} Properties");
                }

                if (BvBinds.PasteProperties.IsNewPressed && !clipboard.Equals(default(BlockData)) && clipboard.terminalProperties.Count > 0)
                {
                    if (clipboard.blockTypeID == targetBlock.TypeID)
                    {
                        pasteBackup = targetBlock.ExportSettings();
                        lastPastedTarget = targetBlock.TBlock;

                        int importCount = targetBlock.ImportSettings(clipboard);
                        scrollMenu.ShowNotification($"Pasted {importCount} Properties");
                    }
                    else
                        scrollMenu.ShowNotification($"Paste Incompatible");
                }

                if (BvBinds.UndoPaste.IsNewPressed && targetBlock.TBlock == lastPastedTarget)
                {
                    targetBlock.ImportSettings(pasteBackup);
                    scrollMenu.ShowNotification("Paste Undone");
                    lastPastedTarget = null;
                }
            }
        }

        public override void Draw()
        {
            if (BvConfig.Current.hudConfig.resolutionScaling)
                scrollMenu.Scale = BvConfig.Current.hudConfig.hudScale * HudMain.ResScale;
            else
                scrollMenu.Scale = BvConfig.Current.hudConfig.hudScale;

            scrollMenu.BgOpacity = BvConfig.Current.hudConfig.hudOpacity;
            scrollMenu.MaxVisible = BvConfig.Current.hudConfig.maxVisible;

            if (targetBlock != null && Open)
            {
                Vector3D targetPos, worldPos;
                Vector2 screenPos, screenBounds = Vector2.One / 2f;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !BvConfig.Current.hudConfig.useCustomPos)
                {
                    targetPos = Target.Position + Target.modelOffset * .75;
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos) / 2d;

                    screenPos = new Vector2((float)worldPos.X, (float)worldPos.Y);
                    screenBounds -= HudMain.GetRelativeVector(scrollMenu.Size / 2f);
                    scrollMenu.AlignToEdge = false;
                }
                else
                {
                    screenPos = BvConfig.Current.hudConfig.hudPos;
                    scrollMenu.AlignToEdge = true;
                }

                if (BvConfig.Current.hudConfig.clampHudPos)
                {
                    screenPos.X = MathHelper.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                    screenPos.Y = MathHelper.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                scrollMenu.Offset = HudMain.GetPixelVector(screenPos);

                if (DrawBoundingBox)
                    DrawTestCube();
            }
        }

        private void DrawTestCube()
        {
            BoundingBoxD box = targetBlock.TBlock.WorldAABB;
            MatrixD matrix = MatrixD.Orthogonalize(box.Matrix);
            boundsTest.Size = box.Size;
            boundsTest.Draw(ref matrix);
        }

        private void ToggleOpen()
        {
            if (Open && scrollMenu.MenuMode != ScrollMenuModes.Peek)
                Hide();
            else
                TryOpen();
        }

        private void TryPeek()
        {
            scrollMenu.MenuMode = ScrollMenuModes.Peek;

            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.SetTarget(targetBlock);
                Open = true;
            }
        }

        private void TryOpen()
        {
            scrollMenu.MenuMode = ScrollMenuModes.Control;

            if ((scrollMenu.MenuMode == ScrollMenuModes.Peek && targetBlock?.TBlock != null || TryGetTarget()) && CanAccessTargetBlock())
            {
                if (targetBlock?.TBlock != scrollMenu?.Target?.TBlock)
                {
                    scrollMenu.SetTarget(targetBlock);
                    Open = true;
                }
            }
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        private void Hide()
        {
            Open = false;
            targetBlock = null;
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
                    TerminalPermissionStates permissions = LocalPlayer.GetBlockAccessPermissions(block);

                    if ((permissions & TerminalPermissionStates.Granted) > 0)
                    {
                        if (targetBlock == null || block != targetBlock.TBlock)
                        {
                            targetGrid.SetGrid(block.CubeGrid);
                            targetBlock = new PropertyBlock(targetGrid, block);
                        }

                        return true;
                    }
                    else if (scrollMenu.MenuMode != ScrollMenuModes.Peek)
                    {
                        if ((permissions & TerminalPermissionStates.GridUnfriendly) > 0)
                            MyAPIGateway.Utilities.ShowNotification($"Access denied. Grid unfriendly.", 1000, MyFontEnum.Red);
                        else
                            MyAPIGateway.Utilities.ShowNotification("Access denied", 1000, MyFontEnum.Red);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to retrieve targeted <see cref="IMyTerminalBlock"/> on a grid within a given distance.
        /// </summary>
        public static bool TryGetTargetedBlock(double maxDist, out IMyTerminalBlock target)
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
                    IMyCubeBlock cubeBlock = slimBlock?.FatBlock;

                    if (cubeBlock != null)
                    {
                        BoundingBoxD box = cubeBlock.WorldAABB;
                        double newDist = box.DistanceSquared(rayInfo.Position),
                            newCenterDist = Vector3D.DistanceSquared(box.Center, rayInfo.Position);
                        var tBlock = cubeBlock as IMyTerminalBlock;

                        if ((tBlock != null || currentDist > 0d) && (newDist < currentDist || (newDist == 0d && newCenterDist < currentCenterDist)))
                        {
                            target = tBlock;
                            currentDist = newDist;
                            currentCenterDist = newCenterDist;
                        }
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
            targetBlock?.TBlock != null && BlockInRange() && targetBlock.CanLocalPlayerAccess && (!BvConfig.Current.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(targetBlock.TBlock));

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (targetBlock != null)
                dist = (LocalPlayer.Position - targetBlock.Position).LengthSquared();

            return dist < (BvConfig.Current.general.maxControlRange * BvConfig.Current.general.maxControlRange);
        }
    }
}