﻿using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using VRageMath;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using System;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class PropertiesMenu : BvComponentBase
    {
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
        private const long peekTime = 100 * TimeSpan.TicksPerMillisecond;

        private readonly BvScrollMenu scrollMenu;
        private PropertyBlock target;
        private IMyTerminalBlock lastPastedTarget;
        private BlockData clipboard, pasteBackup;
        private Utils.Stopwatch peakRefresh;

        private PropertiesMenu() : base(false, true)
        {
            scrollMenu = new BvScrollMenu() { Visible = false };
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
            peakRefresh = new Utils.Stopwatch();
            peakRefresh.Start();

            SharedBinds.Escape.OnNewPress += Hide;
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
                    if (BvBinds.Peek.IsNewPressed || peakRefresh.ElapsedTicks > peekTime)
                    {
                        TryPeek();
                        peakRefresh.Reset();
                    }
                }
                else if (BvBinds.Peek.IsReleased && Open && scrollMenu.MenuMode == ScrollMenuModes.Peek)
                    Hide();
            }

            if (target != null && Open && scrollMenu.MenuMode != ScrollMenuModes.Peek)
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
            if (BvConfig.Current.hudConfig.resolutionScaling)
                scrollMenu.Scale = BvConfig.Current.hudConfig.hudScale * HudMain.ResScale;
            else
                scrollMenu.Scale = BvConfig.Current.hudConfig.hudScale;

            scrollMenu.BgOpacity = BvConfig.Current.hudConfig.hudOpacity;
            scrollMenu.MaxVisible = BvConfig.Current.hudConfig.maxVisible;

            if (target != null && Open)
            {
                Vector3D targetPos, worldPos;
                Vector2 screenPos, screenBounds = Vector2.One;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !BvConfig.Current.hudConfig.useCustomPos)
                {
                    targetPos = Target.Position + Target.modelOffset * .75;
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);

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
            }
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
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.MenuMode = ScrollMenuModes.Peek;

                scrollMenu.SetTarget(target);
                Open = true;   
            }
        }

        private void TryOpen()
        {
            if (TryGetTarget() && CanAccessTargetBlock())
            {
                scrollMenu.MenuMode = ScrollMenuModes.Control;

                if (target?.TBlock != scrollMenu?.Target?.TBlock)
                {
                    scrollMenu.SetTarget(target);
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
                    else if (scrollMenu.MenuMode != ScrollMenuModes.Peek)
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