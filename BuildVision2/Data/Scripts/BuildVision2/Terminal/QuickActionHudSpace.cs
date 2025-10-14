using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using VRage.Utils;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using VRage;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionHudSpace : HudSpaceNodeBase
    {
        /// <summary>
        /// Limits how frequently peek can switch targets
        /// </summary>
        private const int PeekRetargetTickInterval = 5;

        /// <summary>
        /// Currently targeted terminal block
        /// </summary>
        public static PropertyBlock Target  { get; private set; }

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get; private set; }

        /// <summary>
        /// Returns the menu's current state
        /// </summary>
        public static QuickActionMenuState MenuState => instance?.quickActionMenu.MenuState ?? QuickActionMenuState.Closed;

        /// <summary>
        /// If true, then the bounding box of the target block will be drawn. Used for debugging.
        /// </summary>
        public static bool DrawBoundingBox { get; set; }

        /// <summary>
        /// Anim speed normalized to 60fps
        /// </summary>
        public static float AnimScale => instance.lerpScale;

        private static QuickActionHudSpace instance;
        private static bool wasInitialized = false;

        private readonly QuickActionMenu quickActionMenu;
        private readonly BoundingBoard boundingBox;

        private readonly TerminalGrid targetGrid, tempGrid;
        private readonly List<IMySlimBlock> targetBuffer;

        private readonly IMyHudNotification hudNotification;
        private readonly Stopwatch frameTimer;

        private Vector2 lastPos;
        private float posLerpFactor, lerpScale;
        private int bpTick, bpMenuTick, targetTick;
        private bool isPlayerBlueprinting, isBpListOpen;
        private Vector2? lastSpecSpeeds;

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

            frameTimer = new Stopwatch();
            frameTimer.Start();

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

                if (!CanAccessTargetBlock() || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None 
                    || isPlayerBlueprinting || isBpListOpen)
                {
                    CloseMenuInternal();
                }
            }
        }

        protected override void Layout()
        {
            var specCon = MyAPIGateway.Session.CameraController as MySpectator;
            float scale = BvConfig.Current.genUI.hudScale;

            if (BvConfig.Current.genUI.resolutionScaling)
                scale *= HudMain.ResScale;

            if (Target.TBlock != null && Open)
            {
                Vector3D targetWorldPos, targetScreenPos;
                Vector2 screenBounds = new Vector2(HudMain.ScreenWidth, HudMain.ScreenHeight) / HudMain.ResScale * .5f,
                    menuPos;

                if (!BvConfig.Current.genUI.useCustomPos)
                {
                    if (LocalPlayer.IsLookingInBlockDir(Target.TBlock))
                    {
                        targetWorldPos = Target.Position + Target.ModelOffset * .75d;
                        targetScreenPos = LocalPlayer.GetWorldToScreenPos(targetWorldPos) * .5d;

                        menuPos = new Vector2((float)targetScreenPos.X, (float)targetScreenPos.Y);
                        menuPos = HudMain.GetPixelVector(menuPos) / scale;
                    }
                    else
                        menuPos = lastPos;
                }
                else
                {
                    menuPos = BvConfig.Current.genUI.hudPos;
                    menuPos = HudMain.GetPixelVector(menuPos) / scale;

                    if (menuPos.X < 0f)
                        menuPos.X += .5f * quickActionMenu.Width;
                    else
                        menuPos.X -= .5f * quickActionMenu.Width;

                    if (menuPos.Y < 0f)
                        menuPos.Y += .5f * quickActionMenu.Height;
                    else
                        menuPos.Y -= .5f * quickActionMenu.Height;
                }

                if (BvConfig.Current.genUI.clampHudPos || BvConfig.Current.genUI.useCustomPos)
                {
                    screenBounds -= .5f * quickActionMenu.Size;
                    menuPos.X = MathHelper.Clamp(menuPos.X, -screenBounds.X, screenBounds.X);
                    menuPos.Y = MathHelper.Clamp(menuPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                if ((lastPos - menuPos).LengthSquared() > 1f && !(HudMain.EnableCursor && SharedBinds.LeftButton.IsPressed))
                {
                    posLerpFactor = 0f;
                    lastPos = menuPos;
                }                

                if (BvConfig.Current.genUI.useCustomPos)
                    posLerpFactor = 1f;

                lerpScale = frameTimer.ElapsedMilliseconds / 16.6667f;
                posLerpFactor = Math.Min(posLerpFactor + .4f * lerpScale, 1f);

                quickActionMenu.Offset = Vector2.Lerp(quickActionMenu.Offset, lastPos, posLerpFactor);
                quickActionMenu.Visible = bpTick > 30;

                if (specCon != null && lastSpecSpeeds != null)
                {
                    specCon.SpeedModeAngular = lastSpecSpeeds.Value.X;
                    specCon.SpeedModeLinear = lastSpecSpeeds.Value.Y;
                }
            }
            else if (specCon != null)
            {
                lastSpecSpeeds = new Vector2(specCon.SpeedModeAngular, specCon.SpeedModeLinear);
            }

            // Rescale draw matrix based on config
            PlaneToWorldRef[0] = MatrixD.CreateScale(scale, scale, 1d) * HudMain.PixelToWorld;
            base.Layout();

            frameTimer.Restart();
        }

        protected override void Draw()
        {
            // Debug target bounding boxes
            if (DrawBoundingBox)
            {
                foreach (IMySlimBlock slimBlock in targetBuffer)
                {
                    if (slimBlock.FatBlock != null && slimBlock.FatBlock is IMyTerminalBlock)
                    {
                        boundingBox.Draw(slimBlock.FatBlock);
                    }
                }
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            bool isOpen = instance?.quickActionMenu.MenuState != QuickActionMenuState.Closed;

            UpdateBpInputMonitoring();
            
            quickActionMenu.InputEnabled = !RichHudTerminal.Open;
            bool tryOpen = BvBinds.OpenWheel.IsNewPressed || BvBinds.OpenList.IsNewPressed || BvBinds.StartDupe.IsNewPressed;

            if (BvConfig.Current.genUI.legacyModeEnabled || (MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (tryOpen)
                {
                    targetTick = 0;
                    TryOpenMenuInternal();
                }
            }

            if ((MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (BvBinds.MultXOrMouse.IsPressed && BvConfig.Current.targeting.enablePeek)
                {
                    if (BvBinds.MultXOrMouse.IsNewPressed)
                        targetTick = 0;

                    TryOpenMenuInternal();
                }
                else if ((quickActionMenu.MenuState & QuickActionMenuState.Peek) > 0)
                    CloseMenuInternal();
            }

            if (SharedBinds.Escape.IsNewPressed && isOpen)
                CloseMenuInternal();

            if (Open && !isOpen)
                Target.Reset();

            Open = isOpen;
            targetTick++;
        }

        /// <summary>
        /// Heuristics used to infer blueprint usage
        /// </summary>
        private void UpdateBpInputMonitoring()
        {
            bool canBp = MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None && !BindManager.IsChatOpen
                    && !MyAPIGateway.Gui.IsCursorVisible;

            if (!isBpListOpen)
            {
                if (!isPlayerBlueprinting)
                {
                    if (canBp && (SharedBinds.Paste.IsNewPressed || SharedBinds.Copy.IsNewPressed))
                        isPlayerBlueprinting = true;
                }
                else
                {
                    if (!canBp || SharedBinds.LeftButton.IsNewPressed || SharedBinds.Escape.IsNewPressed 
                        || MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("SLOT0")) )
                    {
                        isPlayerBlueprinting = false;
                    }
                }
            }

            if (!isBpListOpen)
            {
                if (canBp && BvBinds.OpenBpList.IsNewPressed)
                    isBpListOpen = true;
            }
            else if (bpMenuTick > 30)
            {
                if (MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None || !MyAPIGateway.Gui.IsCursorVisible 
                    || BindManager.IsChatOpen || SharedBinds.Escape.IsNewPressed)
                {
                    isBpListOpen = false;
                    isPlayerBlueprinting = !SharedBinds.Escape.IsNewPressed;
                }
            }

            if (!isBpListOpen)
                bpMenuTick = 0;
            else
                bpMenuTick++;

            if (BvBinds.Blueprint.IsNewPressed || isBpListOpen)
                bpTick = 0;
            else
                bpTick++;
        }

        /// <summary>
        /// Attempts to open the property menu
        /// </summary>
        private void TryOpenMenuInternal(QuickActionMenuState initialState = default(QuickActionMenuState))
        {
            if ((targetTick % PeekRetargetTickInterval) == 0 && !isPlayerBlueprinting && TryGetTargetWithPermission() && CanAccessTargetBlock())
            {
                quickActionMenu.OpenMenu(Target, initialState);

                if (!wasInitialized && !BvServer.IsAlive && BvServer.IsPlugin)
                {
                    ExceptionHandler.SendChatMessage(
                        "Build Vision has been launched in client-only mode by the Plugin Loader. " +
                        "Some functionality may be unavailable."
                    );

                    wasInitialized = true;
                }
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
        /// Tries to get terminal block being targeted by the local player if there is one while 
        /// respecting ownership permissions.
        /// </summary>
        private bool TryGetTargetWithPermission()
        {
            IMyTerminalBlock block;
            bool canDisplaceBlock = LocalPlayer.CurrentBuilderBlock != null && !BvConfig.Current.targeting.canOpenIfPlacing,
                canTarget = !canDisplaceBlock || BvConfig.Current.genUI.legacyModeEnabled;
                
            if (canTarget && TryGetTargetedBlockInternal(BvConfig.Current.targeting.maxOpenRange, out block))
            {
                if (block != null)
                {
                    TerminalPermissionStates permissions = block.GetAccessPermissions();

                    if ((permissions & TerminalPermissionStates.Granted) > 0 
                        || LocalPlayer.HasAdminSetting(MyAdminSettingsEnum.UseTerminals))
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
            MatrixD transform = MyAPIGateway.Session.Camera.WorldMatrix;
            Vector3D headPos = transform.Translation, forward = transform.Forward;

            LineD line = new LineD(headPos, headPos + forward * maxDist);
            target = null;

            if ((LocalPlayer.IsControllingCharacter || LocalPlayer.IsSpectating) 
                && LocalPlayer.TryGetTargetedGrid(line, out cubeGrid, out rayInfo))
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
                && (!BvConfig.Current.targeting.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(Target.TBlock))
                && (LocalPlayer.IsControllingCharacter || LocalPlayer.IsSpectating);
        }

        /// <summary>
        /// Returns true if the player is within maxControlRange meters of the block.
        /// </summary>
        private bool BlockInRange()
        {
            if (LocalPlayer.IsSpectating && !BvConfig.Current.targeting.isSpecRangeLimited)
            {
                return true;
            }
            else
            {
                double dist = double.PositiveInfinity;

                if (Target.TBlock != null)
                {
                    if (LocalPlayer.IsSpectating)
                        dist = (MyAPIGateway.Session.Camera.Position - Target.Position).LengthSquared();

                    dist = Math.Min(dist, (LocalPlayer.Position - Target.Position).LengthSquared());
                }

                return dist < (BvConfig.Current.targeting.maxControlRange * BvConfig.Current.targeting.maxControlRange);
            }                
        }
    }
}