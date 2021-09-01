using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class PropertiesMenu : BvComponentBase
    {
        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target  { get; private set; }

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get { return Instance.scrollMenu.Visible; } set { Instance.scrollMenu.Visible = value; } }

        /// <summary>
        /// Returns the menu's current mode (peek/control/copy)
        /// </summary>
        public static ScrollMenuModes MenuMode => Instance.scrollMenu.MenuMode;

        /// <summary>
        /// If true, then the bounding box of the target block will be drawn. Used for debugging.
        /// </summary>
        public static bool DrawBoundingBox { get; set; }

        private static PropertiesMenu Instance
        {
            get { Init(); return _instance; }
            set { _instance = value; }
        }
        private static PropertiesMenu _instance;

        private readonly BvScrollMenu scrollMenu;
        private readonly CustomSpaceNode hudSpace;
        private readonly BoundingBoard boundingBox;

        private readonly TerminalGrid targetGrid, tempGrid;
        private readonly List<IMySlimBlock> targetBuffer;
        private IMyTerminalBlock lastPastedTarget;

        private BlockData clipboard, pasteBackup;
        private Stopwatch peekRefresh;
        private readonly IMyHudNotification hudNotification;

        private PropertiesMenu() : base(false, true)
        {
            DrawBoundingBox = false;
            targetGrid = new TerminalGrid();
            tempGrid = new TerminalGrid();
            targetBuffer = new List<IMySlimBlock>();
            Target = new PropertyBlock();

            hudSpace = new CustomSpaceNode(HudMain.Root) { UpdateMatrixFunc = UpdateHudSpace };
            scrollMenu = new BvScrollMenu(hudSpace) { Visible = false };
            boundingBox = new BoundingBoard();
            hudNotification = MyAPIGateway.Utilities.CreateNotification("", 1000, MyFontEnum.Red);

            RichHudCore.LateMessageEntered += MessageHandler;
            peekRefresh = new Stopwatch();
            peekRefresh.Start();
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
            Target = null;
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

            if (Target.TBlock != null && Open)
                scrollMenu.UpdateText();
        }

        public override void HandleInput()
        {
            if (!HudMain.Cursor.Visible)
            {
                // Open/Hide
                if (BvBinds.Open.IsNewPressed && BvBinds.Hide.IsNewPressed)
                    ToggleOpen();
                else if (BvBinds.Open.IsNewPressed)
                    TryOpen();
                else if (BvBinds.Hide.IsNewPressed || SharedBinds.Escape.IsNewPressed)
                    Hide();

                // Peek
                if (BvConfig.Current.general.enablePeek)
                {
                    if (BvBinds.Peek.IsPressed && (!Open || scrollMenu.MenuMode == ScrollMenuModes.Peek))
                    {
                        // Acquire peek target on new press and reacquire every 100ms until bind is released
                        if (BvBinds.Peek.IsNewPressed || peekRefresh.ElapsedMilliseconds > 100)
                        {
                            TryPeek();
                            peekRefresh.Restart();
                        }
                    }
                    else if (BvBinds.Peek.IsReleased && Open && scrollMenu.MenuMode == ScrollMenuModes.Peek)
                        Hide();
                }

                // Copy/paste
                if (Target.TBlock != null && Open)
                {
                    // Copy properties
                    if (BvBinds.CopySelection.IsNewPressed && scrollMenu.MenuMode == ScrollMenuModes.Dupe)
                    {
                        clipboard = new BlockData(Target.TypeID, scrollMenu.GetDuplicationRange());
                        scrollMenu.ShowNotification($"Copied {clipboard.terminalProperties.Count} Properties");
                    }

                    // Attempt to paste copied properties
                    if (BvBinds.PasteProperties.IsNewPressed && !clipboard.Equals(default(BlockData)) && clipboard.terminalProperties.Count > 0)
                    {
                        if (clipboard.blockTypeID == Target.TypeID)
                        {
                            pasteBackup = Target.ExportSettings();
                            lastPastedTarget = Target.TBlock;

                            int importCount = Target.ImportSettings(clipboard);
                            scrollMenu.ShowNotification($"Pasted {importCount} Properties");
                        }
                        else
                            scrollMenu.ShowNotification($"Paste Incompatible");
                    }

                    // Undo paste if the last pasted block is selected
                    if (BvBinds.UndoPaste.IsNewPressed && Target.TBlock == lastPastedTarget)
                    {
                        Target.ImportSettings(pasteBackup);
                        scrollMenu.ShowNotification("Paste Undone");
                        lastPastedTarget = null;
                    }
                }
            }
        }

        private MatrixD UpdateHudSpace()
        {
            float scale = BvConfig.Current.hudConfig.hudScale;

            if (BvConfig.Current.hudConfig.resolutionScaling)
                scale *= HudMain.ResScale;

            if (Target.TBlock != null && Open)
            {
                if (DrawBoundingBox) // Debug target bounding box
                    boundingBox.Draw(Target.TBlock);

                // Update opacity, visible range and UI scale based on config
                scrollMenu.BgOpacity = BvConfig.Current.hudConfig.hudOpacity;
                scrollMenu.MaxVisible = BvConfig.Current.hudConfig.maxVisible;

                Vector3D targetWorldPos, targetScreenPos;
                Vector2 menuPos, screenBounds = Vector2.One / 2f;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !BvConfig.Current.hudConfig.useCustomPos)
                {
                    targetWorldPos = Target.Position + Target.ModelOffset * .75d;
                    targetScreenPos = LocalPlayer.GetWorldToScreenPos(targetWorldPos) / 2d;

                    menuPos = new Vector2((float)targetScreenPos.X, (float)targetScreenPos.Y);
                    screenBounds -= HudMain.GetAbsoluteVector(scrollMenu.Size * scale / 2f);
                    scrollMenu.AlignToEdge = false;
                }
                else
                {
                    menuPos = BvConfig.Current.hudConfig.hudPos;
                    scrollMenu.AlignToEdge = true;
                }

                if (BvConfig.Current.hudConfig.clampHudPos)
                {
                    menuPos.X = MathHelper.Clamp(menuPos.X, -screenBounds.X, screenBounds.X);
                    menuPos.Y = MathHelper.Clamp(menuPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                scrollMenu.Offset = HudMain.GetPixelVector(menuPos) / scale;
            }

            // Rescale draw matrix based on config
            return MatrixD.CreateScale(scale, scale, 1d) * HudMain.PixelToWorld;
        }

        /// <summary>
        /// Toggles the menu open/closed
        /// </summary>
        private void ToggleOpen()
        {
            if (Open && scrollMenu.MenuMode != ScrollMenuModes.Peek)
                Hide();
            else
                TryOpen();
        }

        /// <summary>
        /// Attempts to open the menu and set it to peek
        /// </summary>
        private void TryPeek()
        {
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.MenuMode = ScrollMenuModes.Peek;
                scrollMenu.UpdateTarget();
                Open = true;
            }
        }

        /// <summary>
        /// Attempts to open the menu and set it to control
        /// </summary>
        private void TryOpen()
        {
            if (Open && scrollMenu.MenuMode == ScrollMenuModes.Peek)
                scrollMenu.MenuMode = ScrollMenuModes.Control;
            else if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.MenuMode = ScrollMenuModes.Control;
                scrollMenu.UpdateTarget();
                Open = true;
            }
        }

        /// <summary>
        /// Hide the scroll menu and clear target
        /// </summary>
        private void Hide()
        {
            Open = false;
            Target.Reset();
            targetGrid.Reset();
            scrollMenu.Clear();
        }

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyTerminalBlock block;

            if (
                (LocalPlayer.GetHudState() != HudState.Hidden) 
                && (BvConfig.Current.general.canOpenIfHolding || LocalPlayer.HasEmptyHands) 
                && TryGetTargetedBlockInternal(BvConfig.Current.general.maxOpenRange, out block)
            )
            {
                if (block != null)
                {
                    TerminalPermissionStates permissions = LocalPlayer.GetBlockAccessPermissions(block);
                    
                    if ((permissions & TerminalPermissionStates.Granted) > 0)
                    {
                        if (Target.TBlock == null || block != Target.TBlock)
                        {
                            targetGrid.SetGrid(block.CubeGrid);
                            Target.SetBlock(targetGrid, block);
                        }

                        return true;
                    }
                    else
                    {
                        if ((permissions & TerminalPermissionStates.GridUnfriendly) > 0)
                            hudNotification.Text = $"Access denied. Grid unfriendly.";
                        else
                            hudNotification.Text = "Access denied";

                        hudNotification.Show();
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
            return Instance.TryGetTargetedBlockInternal(maxDist, out target);
        }

        /// <summary>
        /// Tries to retrieve targeted <see cref="IMyTerminalBlock"/> on a grid within a given distance.
        /// </summary>
        private bool TryGetTargetedBlockInternal(double maxDist, out IMyTerminalBlock target)
        {
            IMyCubeGrid cubeGrid;
            IHitInfo rayInfo;
            Vector3D headPos = LocalPlayer.HeadTransform.Translation, forward = LocalPlayer.HeadTransform.Forward;
            LineD line = new LineD(headPos, headPos + forward * maxDist);
            target = null;

            if (LocalPlayer.TryGetTargetedGrid(line, out cubeGrid, out rayInfo))
            {
                // Retrieve blocks within about half a block of the ray intersection point.
                var sphere = new BoundingSphereD(rayInfo.Position, (cubeGrid.GridSizeEnum == MyCubeSize.Large) ? 1.3 : .3);
                double currentDist = double.PositiveInfinity, currentCenterDist = double.PositiveInfinity;

                tempGrid.SetGrid(cubeGrid, true);
                targetBuffer.Clear();
                tempGrid.GetBlocksInsideSphere(cubeGrid, targetBuffer, ref sphere);

                foreach (IMySlimBlock slimBlock in targetBuffer)
                {
                    IMyCubeBlock cubeBlock = slimBlock?.FatBlock;

                    if (cubeBlock != null)
                    {
                        var topBlock = cubeBlock as IMyAttachableTopBlock;

                        if (topBlock != null)
                            cubeBlock = topBlock.Base;
                    }

                    var tBlock = cubeBlock as IMyTerminalBlock;

                    if (tBlock != null)
                    {
                        // Find shortest dist between the bb and the intersection.
                        BoundingBoxD box = cubeBlock.WorldAABB;
                        double newDist = Math.Round(box.DistanceSquared(rayInfo.Position), 3), 
                            newCenterDist = Math.Round(Vector3D.DistanceSquared(box.Center, rayInfo.Position), 3);

                        // If this is a terminal block, check to see if this block is any closer than the last.
                        // If the distance to the bb is zero, use the center dist, favoring smaller blocks.
                        if (
                            (currentDist > 0d && newDist < currentDist) 
                            || (Math.Abs(currentDist - newDist) < 0.02 && newCenterDist < currentCenterDist)
                        )
                        {
                            target = tBlock;
                            currentDist = newDist;
                            currentCenterDist = newCenterDist;
                        }
                    }
                }
            }

            tempGrid.Reset();
            return target != null;
        }

        /// <summary>
        /// Checks if the player can access the targeted block.
        /// </summary>
        private bool CanAccessTargetBlock()
        {
            return Target.TBlock != null
            && BlockInRange()
            && Target.CanLocalPlayerAccess
            && (!BvConfig.Current.general.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(Target.TBlock));
        }

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (Target.TBlock != null)
                dist = (LocalPlayer.Position - Target.Position).LengthSquared();

            return dist < (BvConfig.Current.general.maxControlRange * BvConfig.Current.general.maxControlRange);
        }
    }
}