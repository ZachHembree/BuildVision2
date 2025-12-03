using RichHudFramework.Internal;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Diagnostics;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

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
        public static PropertyBlock Target { get; private set; }

        /// <summary>
        /// If true, then the menu is open
        /// </summary>
        public static bool Open { get; private set; }

        /// <summary>
        /// Returns the menu's current state
        /// </summary>
        public static QuickActionMenuState MenuState => instance?.quickActionMenu.MenuState ?? QuickActionMenuState.Closed;

        /// <summary>
        /// Enables/disables debug targeting visualization
        /// </summary>
        public static bool EnableTargetDebugVis { get; set; }

        /// <summary>
        /// Anim speed normalized to 60fps
        /// </summary>
        public static float AnimScale => instance.lerpScale;

        // Singleton instance
        private static QuickActionHudSpace instance;
        private static bool wasInitialized = false;

        // UI
        private readonly QuickActionMenu quickActionMenu;
        private readonly IMyHudNotification hudNotification;

        // Animation lerp
        private Vector2 lastPos;
        private float posLerpFactor, lerpScale;
        private readonly Stopwatch frameTimer;

        // Block targeting
        private readonly TerminalGrid targetGrid;
        private readonly BlockFinder blockFinder;

        // Debug targeting visualization
        private readonly BoundingBoard boundingBox; 
        private readonly LineBoard targetLineBoard;

        // BP detection heuristics
        private int bpTick, bpMenuTick, targetTick;
        private bool isPlayerBlueprinting, isBpListOpen;

        // Stores last spectator camera speeds, before opening the menu
        private Vector2? lastSpecSpeeds;

        private QuickActionHudSpace() : base(HudMain.Root)
        {
            EnableTargetDebugVis = false;

            quickActionMenu = new QuickActionMenu(this);
            hudNotification = MyAPIGateway.Utilities.CreateNotification("", 1000, MyFontEnum.Red);

            frameTimer = new Stopwatch();
            frameTimer.Start();

            Target = new PropertyBlock();
            targetGrid = new TerminalGrid();
            blockFinder = new BlockFinder();
            boundingBox = new BoundingBoard();
            targetLineBoard = new LineBoard();

			RichHudCore.LateMessageEntered += MessageHandler;
        }

        public static void Init()
        {
            Close();
            instance = new QuickActionHudSpace();
        }

		/// <summary>
		/// Tries to retrieve targeted <see cref="IMyTerminalBlock"/> on a grid within a given distance.
		/// </summary>
		public static bool TryGetTargetedBlock(double maxDist, out IMyTerminalBlock target)
		{
			target = null;
			return instance?.TryGetTargetedBlockInternal(maxDist, out target) ?? false;
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

                if (!GetCanAccessTargetBlock())
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
                        menuPos = (menuPos * HudMain.ScreenDim) / scale;
                    }
                    else
                        menuPos = lastPos;
                }
                else
                {
                    menuPos = BvConfig.Current.genUI.hudPos;
                    menuPos = (menuPos * HudMain.ScreenDim) / scale;

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
            if (EnableTargetDebugVis)
            {
                foreach (var target in blockFinder.SortedTargets)
                {
                    boundingBox.Draw(target.block);
                    ExceptionHandler.SendDebugNotification(
                        $"Dist: {target.distance:G5} " +
                        $" - {target.block?.CustomName ?? target.block?.Name ?? "?"}");
                }

                var camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation;
                var rayPos = blockFinder.LastTargetLine.From;

                if (Vector3D.DistanceSquared(camPos, rayPos) > .01d)
                    targetLineBoard.Draw(blockFinder.LastTargetLine);
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            bool isOpen = instance?.quickActionMenu.MenuState != QuickActionMenuState.Closed;

            UpdateBpInputMonitoring();
            quickActionMenu.InputEnabled = !RichHudTerminal.Open && MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.None;

            if (!quickActionMenu.InputEnabled)
                return;

            if (BvConfig.Current.genUI.legacyModeEnabled || (MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (BvBinds.OpenWheel.IsNewPressed || BvBinds.OpenList.IsNewPressed || BvBinds.StartDupe.IsNewPressed)
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
                        || MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("SLOT0")))
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
            if ((targetTick % PeekRetargetTickInterval) == 0 && !isPlayerBlueprinting && TryGetTargetWithPermission() && GetCanAccessTargetBlock())
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
		/// Checks if the player can access the targeted block.
		/// </summary>
		private bool GetCanAccessTargetBlock()
		{
			return Target.TBlock != null
				&& Target.CanLocalPlayerAccess
                && GetIsBlockInRange()
				&& (!BvConfig.Current.targeting.closeIfNotInView || LocalPlayer.IsLookingInBlockDir(Target.TBlock))
				&& (LocalPlayer.IsControllingCharacter || LocalPlayer.IsSpectating) &&
				!(MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.None || isPlayerBlueprinting || isBpListOpen);
		}

		/// <summary>
		/// Returns true if the player is within maxControlRange meters of the block.
		/// </summary>
		private bool GetIsBlockInRange()
		{
			if (LocalPlayer.IsSpectating && !BvConfig.Current.targeting.isSpecRangeLimited)
				return true;
			else if (Target.TBlock != null)
			{
				double distSq = (MyAPIGateway.Session.Camera.Position - Target.Position).LengthSquared(),
					maxConDistSq = (BvConfig.Current.targeting.maxControlRange * BvConfig.Current.targeting.maxControlRange);

				return distSq < maxConDistSq;
			}
			else
				return false;
		}

		/// <summary>
		/// Hide the menu and clear target
		/// </summary>
		private void CloseMenuInternal()
        {
            Target.Reset();
            targetGrid.Reset();

            if (!EnableTargetDebugVis)
                blockFinder.Clear();

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
            bool isCharOrSpectator = LocalPlayer.IsControllingCharacter || LocalPlayer.IsSpectating,
                canOpenAndPlace = (BvConfig.Current.targeting.canOpenIfPlacing || BvConfig.Current.genUI.legacyModeEnabled),
                isHandEmpty = LocalPlayer.CurrentBuilderBlock == null,
                canTarget = isCharOrSpectator && (isHandEmpty || canOpenAndPlace);

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
                        }
                        
                        return Target.TBlock != null;
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
        private bool TryGetTargetedBlockInternal(double maxDist, out IMyTerminalBlock target)
        {
            MatrixD camMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            LineD line;
            line.From = camMatrix.Translation;
            line.To = camMatrix.Translation + camMatrix.Forward * maxDist;
            line.Length = maxDist;
            line.Direction = camMatrix.Forward;

            if (blockFinder.TryUpdateTargets(line))
            {
                target = blockFinder.SortedTargets[0].block;
                return true;
            }

			target = null;
			return false;
        }
    }
}