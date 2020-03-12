using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using RichHudFramework.UI.Client;
using RichHudFramework.IO;
using VRageMath;
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
        public static PropertyBlock Target
        {
            get { return Instance.target; }
            set
            {
                if (value != null)
                    Instance.scrollMenu.SetTarget(value);                 

                Instance.target = value;
            }
        }

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

        private PropertiesMenu() : base(false, true)
        {
            scrollMenu = new BvScrollMenu() { Visible = false };
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
        }

        private static void Init()
        {
            if (_instance == null)
                _instance = new PropertiesMenu();
        }

        public override void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

        /// <summary>
        /// Intercepts chat input when a property is open
        /// </summary>
        private void MessageHandler(string message, ref bool sendToOthers)
        {
            if (scrollMenu.Visible && scrollMenu.PropOpen)
                sendToOthers = false;
        }

        /// <summary>
        /// Updates the menu each time it's called. Reopens menu if closed.
        /// </summary>
        public override void Update()
        {
            if (target != null && Open)
            {
                scrollMenu.UpdateText();
            }
        }

        public override void HandleInput()
        {
            if (target != null && Open)
            {
                if (BvBinds.CopySelection.IsNewPressed && scrollMenu.ReplicationMode)
                {
                    clipboard = new BlockData(target.TypeID, scrollMenu.GetReplicationRange());
                    scrollMenu.ShowNotification($"Copied {clipboard.terminalProperties.Count} Properties");
                }

                if (BvBinds.PasteSelection.IsNewPressed && !clipboard.Equals(default(BlockData)) && clipboard.terminalProperties.Count > 0)
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
                    targetPos = Target.GetPosition() + Target.modelOffset * .75;
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

        /// <summary>
        /// Shows the menu if its hidden.
        /// </summary>
        public static void Show()
        {
            Open = true;
        }

        /// <summary>
        /// Hides all menu elements.
        /// </summary>
        public static void Hide()
        {
            Open = false;
        }
    }
}