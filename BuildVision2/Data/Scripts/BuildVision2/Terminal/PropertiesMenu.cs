using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using DarkHelmet.UI.Client;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class PropertiesMenu : ModBase.ComponentBase
    {
        public static HudConfig Cfg { get { return BvConfig.Current.menu.hudConfig; } set { BvConfig.Current.menu.hudConfig = value; } }
        public static PropertyBlock Target
        {
            get { return Instance.target; }
            set
            {
                if (Instance.target != null)
                {
                    Instance.scrollMenu.Clear();
                    Instance.scrollMenu.SetTarget(value);
                }

                Instance.target = value;
            }
        }
        public static bool Open { get { return Instance.scrollMenu.Visible; } set { Instance.scrollMenu.Visible = value; } }

        private static PropertiesMenu Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static PropertiesMenu instance;

        private readonly BvScrollMenu scrollMenu;
        private PropertyBlock target;

        private PropertiesMenu() : base(false, true)
        {
            scrollMenu = new BvScrollMenu() { Visible = false };
            target = null;
        }

        private static void Init()
        {
            if (instance == null)
                instance = new PropertiesMenu();
        }

        public override void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

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

        public override void Draw()
        {
            if (target != null && Open)
            {
                Vector3D targetPos, worldPos;
                Vector2 screenPos, screenBounds = Vector2.One;

                if (LocalPlayer.IsLookingInBlockDir(Target.TBlock) && !Cfg.useCustomPos)
                {
                    scrollMenu.OriginAlignment = OriginAlignment.Center;
                    targetPos = Target.GetPosition() + Target.modelOffset * .75;
                    worldPos = LocalPlayer.GetWorldToScreenPos(targetPos);
                    screenPos = new Vector2((float)worldPos.X, (float)worldPos.Y);
                    screenBounds -= scrollMenu.NativeSize * scrollMenu.Scale / 2f;
                }
                else
                {
                    scrollMenu.OriginAlignment = OriginAlignment.Auto;
                    screenPos = Cfg.hudPos;
                }

                if (Cfg.clampHudPos)
                {
                    screenPos.X = Utils.Math.Clamp(screenPos.X, -screenBounds.X, screenBounds.X);
                    screenPos.Y = Utils.Math.Clamp(screenPos.Y, -screenBounds.Y, screenBounds.Y);
                }

                scrollMenu.Offset += HudMain.GetPixelVector(Utils.Math.Round(screenPos, 3));
                scrollMenu.Offset /= 2f;
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