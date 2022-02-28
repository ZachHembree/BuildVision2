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
    public sealed partial class QuickActionHudSpace : HudSpaceNodeBase
    {
        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target  { get; private set; }

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open => instance?.quickActionMenu.MenuState != QuickActionMenuState.Closed;

        /// <summary>
        /// Returns the menu's current state
        /// </summary>
        public static QuickActionMenuState MenuState => instance?.quickActionMenu.MenuState ?? QuickActionMenuState.Closed;

        /// <summary>
        /// If true, then the bounding box of the target block will be drawn. Used for debugging.
        /// </summary>
        public static bool DrawBoundingBox { get; set; }

        private static QuickActionHudSpace instance;

        private readonly QuickActionMenu quickActionMenu;
        private readonly BoundingBoard boundingBox;

        private readonly TerminalGrid targetGrid, tempGrid;
        private readonly List<IMySlimBlock> targetBuffer;

        private readonly IMyHudNotification hudNotification;
        private Vector2 lastPos;
        private int bpTick;

        private QuickActionHudSpace() : base(HudMain.Root)
        {
            DrawBoundingBox = false;
            targetGrid = new TerminalGrid();
            tempGrid = new TerminalGrid();
            targetBuffer = new List<IMySlimBlock>();
            Target = new PropertyBlock();

            quickActionMenu = new QuickActionMenu(this);
            boundingBox = new BoundingBoard();
            hudNotification = MyAPIGateway.Utilities.CreateNotification("", 1000, MyFontEnum.Red);

            RichHudCore.LateMessageEntered += MessageHandler;
        }

        public static void Init()
        {
            Close();
            instance = new QuickActionHudSpace();
        }

        public static void TryOpenMenu() =>
            instance?.TryOpenMenuInternal(QuickActionMenuState.WheelMenuControl);

        public static void CloseMenu() =>
            instance?.CloseMenuInternal();

        public static void Update() =>
            instance?.UpdateInternal();

        public static void Close()
        {
            instance?.CloseMenuInternal();
            RichHudCore.LateMessageEntered -= MessageHandler;
            Target = null;
            instance = null;
        }

        /// <summary>
        /// Intercepts chat input when a property is open
        /// </summary>
        private static void MessageHandler(string message, ref bool sendToOthers)
        {
            if (Open)
                sendToOthers = false;
        }

        private void UpdateInternal()
        {
            if (Open)
            {
                Target.Update();

                if (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None)
                    CloseMenuInternal();
            }
        }

        protected override void Layout()
        {
            float scale = BvConfig.Current.genUI.hudScale;

            if (BvConfig.Current.genUI.resolutionScaling)
                scale *= HudMain.ResScale;

            if (Target.TBlock != null && Open)
            {
                if (BvBinds.Blueprint.IsNewPressed)
                    bpTick = 0;
                else
                    bpTick++;

                Vector3D targetWorldPos, targetScreenPos;
                Vector2 menuPos, screenBounds = Vector2.One / 2f;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !BvConfig.Current.genUI.useCustomPos)
                {
                    targetWorldPos = Target.Position + Target.ModelOffset * .75d;
                    targetScreenPos = LocalPlayer.GetWorldToScreenPos(targetWorldPos) / 2d;

                    menuPos = new Vector2((float)targetScreenPos.X, (float)targetScreenPos.Y);
                    screenBounds -= HudMain.GetAbsoluteVector(quickActionMenu.Size * scale / 2f);
                }
                else
                {
                    menuPos = BvConfig.Current.genUI.hudPos;
                }

                if (BvConfig.Current.genUI.clampHudPos)
                {
                    menuPos.X = MathHelper.Clamp(menuPos.X, -screenBounds.X, screenBounds.X);
                    menuPos.Y = MathHelper.Clamp(menuPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                menuPos = HudMain.GetPixelVector(menuPos) / scale;

                if (BvConfig.Current.genUI.useCustomPos)
                {
                    if (menuPos.X < 0)
                        menuPos.X += .5f * quickActionMenu.Width;
                    else
                        menuPos.X -= .5f * quickActionMenu.Width;

                    if (menuPos.Y < 0)
                        menuPos.Y += .5f * quickActionMenu.Height;
                    else
                        menuPos.Y -= .5f * quickActionMenu.Height;
                }

                if ((lastPos - menuPos).LengthSquared() > 16f)
                {
                    quickActionMenu.Offset = menuPos;
                    lastPos = menuPos;
                }

                quickActionMenu.Visible = bpTick > 30;
            }

            // Rescale draw matrix based on config
            PlaneToWorldRef[0] = MatrixD.CreateScale(scale, scale, 1d) * HudMain.PixelToWorld;
            base.Layout();
        }

        protected override void Draw()
        {
            // Debug target bounding box
            if (Target?.TBlock != null && DrawBoundingBox)
                boundingBox.Draw(Target.TBlock);
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            quickActionMenu.InputEnabled = !RichHudTerminal.Open;
            bool tryOpen = BvBinds.OpenWheel.IsNewPressed || BvBinds.OpenList.IsNewPressed || BvBinds.StartDupe.IsNewPressed;

            if (BvConfig.Current.genUI.legacyModeEnabled || (MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (tryOpen)
                    TryOpenMenuInternal();
            }

            if ((MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (BvBinds.MultXOrMouse.IsPressed && BvConfig.Current.targeting.enablePeek)
                    TryOpenMenuInternal();
                else if ((quickActionMenu.MenuState & QuickActionMenuState.Peek) > 0)
                    CloseMenuInternal();
            }

            if (SharedBinds.Escape.IsNewPressed && Open)
                CloseMenuInternal();

            if (!Open)
                Target.Reset();
        }

        /// <summary>
        /// Attempts to open the property menu
        /// </summary>
        private void TryOpenMenuInternal(QuickActionMenuState initialState = default(QuickActionMenuState))
        {
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                quickActionMenu.OpenMenu(Target, initialState);
            }
        }

        /// <summary>
        /// Hide the menu and clear target
        /// </summary>
        private void CloseMenuInternal()
        {
            Target.Reset();
            targetGrid.Reset();

            quickActionMenu.CloseMenu();
            HudMain.EnableCursor = false;
        }

        /// <summary>
        /// Tries to get terminal block being targeted by the local player if there is one.
        /// </summary>
        private bool TryGetTarget()
        {
            IMyTerminalBlock block;

            if (
                (BvConfig.Current.targeting.canOpenIfHolding || LocalPlayer.HasEmptyHands) 
                && TryGetTargetedBlockInternal(BvConfig.Current.targeting.maxOpenRange, out block)
            )
            {
                if (block != null)
                {
                    TerminalPermissionStates permissions = block.GetAccessPermissions();
                    
                    if ((permissions & TerminalPermissionStates.Granted) > 0)
                    {
                        if (Target.TBlock == null || block != Target.TBlock)
                        {
                            targetGrid.SetGrid(block.CubeGrid);
                            Target.SetBlock(targetGrid, block);

                            return true;
                        }
                        else
                            return false;
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
            target = null;
            return instance?.TryGetTargetedBlockInternal(maxDist, out target) ?? false;
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
                && (!BvConfig.Current.targeting.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(Target.TBlock));
        }

        /// <summary>
        /// Determines whether the player is within 10 units of the block.
        /// </summary>
        private bool BlockInRange()
        {
            double dist = double.PositiveInfinity;

            if (Target.TBlock != null)
                dist = (LocalPlayer.Position - Target.Position).LengthSquared();

            return dist < (BvConfig.Current.targeting.maxControlRange * BvConfig.Current.targeting.maxControlRange);
        }
    }
}