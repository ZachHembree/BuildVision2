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
    public sealed partial class MenuManager : BvComponentBase
    {
        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target  { get; private set; }

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get; set; }

        /// <summary>
        /// Returns the menu's current mode (peek/control/copy)
        /// </summary>
        public static ScrollMenuModes MenuMode { get; private set; }

        /// <summary>
        /// If true, then the bounding box of the target block will be drawn. Used for debugging.
        /// </summary>
        public static bool DrawBoundingBox { get; set; }

        private static MenuManager Instance
        {
            get { Init(); return _instance; }
            set { _instance = value; }
        }
        private static MenuManager _instance;

        private readonly QuickActionMenu quickActionMenu;
        private readonly CustomSpaceNode hudSpace;
        private readonly BoundingBoard boundingBox;

        private readonly TerminalGrid targetGrid, tempGrid;
        private readonly List<IMySlimBlock> targetBuffer;

        private Stopwatch peekRefresh;
        private readonly IMyHudNotification hudNotification;

        private MenuManager() : base(false, true)
        {
            DrawBoundingBox = false;
            targetGrid = new TerminalGrid();
            tempGrid = new TerminalGrid();
            targetBuffer = new List<IMySlimBlock>();
            Target = new PropertyBlock();

            hudSpace = new CustomSpaceNode(HudMain.Root) { UpdateMatrixFunc = UpdateHudSpace };
            quickActionMenu = new QuickActionMenu(hudSpace) { Visible = false };
            boundingBox = new BoundingBoard();
            hudNotification = MyAPIGateway.Utilities.CreateNotification("", 1000, MyFontEnum.Red);

            RichHudCore.LateMessageEntered += MessageHandler;
            peekRefresh = new Stopwatch();
            peekRefresh.Start();
        }

        public static void Init()
        {
            if (_instance == null)
                _instance = new MenuManager();
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
            if (Open)
                sendToOthers = false;
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public override void Update()
        {
            if (Open && (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None))
                Hide();
        }

        public override void HandleInput()
        {
            if (BvBinds.Peek.IsPressed && !Open)
            {
                // Acquire peek target on new press and reacquire every 100ms until bind is released
                if (BvBinds.Peek.IsNewPressed || peekRefresh.ElapsedMilliseconds > 100)
                {
                    TryOpen();
                    peekRefresh.Restart();
                }
            }
            else if (!BvBinds.Peek.IsPressed && Open)
                Hide();
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

                Vector3D targetWorldPos, targetScreenPos;
                Vector2 menuPos, screenBounds = Vector2.One / 2f;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !BvConfig.Current.hudConfig.useCustomPos)
                {
                    targetWorldPos = Target.Position + Target.ModelOffset * .75d;
                    targetScreenPos = LocalPlayer.GetWorldToScreenPos(targetWorldPos) / 2d;

                    menuPos = new Vector2((float)targetScreenPos.X, (float)targetScreenPos.Y);
                    screenBounds -= HudMain.GetAbsoluteVector(quickActionMenu.Size * scale / 2f);
                }
                else
                {
                    menuPos = BvConfig.Current.hudConfig.hudPos;
                }

                if (BvConfig.Current.hudConfig.clampHudPos)
                {
                    menuPos.X = MathHelper.Clamp(menuPos.X, -screenBounds.X, screenBounds.X);
                    menuPos.Y = MathHelper.Clamp(menuPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                quickActionMenu.Offset = HudMain.GetPixelVector(menuPos) / scale;
            }

            // Rescale draw matrix based on config
            return MatrixD.CreateScale(scale, scale, 1d) * HudMain.PixelToWorld;
        }

        /// <summary>
        /// Attempts to open the menu and set it to peek
        /// </summary>
        private void TryOpen()
        {
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                if (!quickActionMenu.Visible)
                    quickActionMenu.UpdateTarget();

                quickActionMenu.Visible = true;
                HudMain.EnableCursor = true;
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

            quickActionMenu.Clear();
            quickActionMenu.Visible = false;
            HudMain.EnableCursor = false;
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