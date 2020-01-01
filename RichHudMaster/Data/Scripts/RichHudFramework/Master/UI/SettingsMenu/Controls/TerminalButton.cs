using System;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    public class TerminalButton : TerminalControlBase
    {
        public override event Action OnControlChanged;

        public override float Width
        {
            get { return button.Width; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                button.Width = value;
            }
        }

        public override float Height
        {
            get { return button.Height; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                button.Height = value;
            }
        }

        public override RichText Name { get { return button.TextBoard.GetText(); } set { button.TextBoard.SetText(value); } }
        public GlyphFormat Format { get { return button.Format; } set { button.Format = value; } }
        public bool HighlightEnabled { get { return button.HighlightEnabled; } set { button.HighlightEnabled = value; } }
        public IClickableElement MouseInput => button.MouseInput;

        private readonly TextBoxButton button;
        private readonly BorderBox border;

        public TerminalButton(IHudParent parent = null) : base(parent)
        {
            button = new TextBoxButton(this)
            {
                AutoResize = false,
                Color = new Color(42, 55, 63),
                HighlightColor = new Color(66, 75, 82),
                HighlightEnabled = true,
            };

            border = new BorderBox(button)
            {
                Color = new Color(53, 66, 75),
                Thickness = 2f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            Padding = new Vector2(37f, 0f);
            Size = new Vector2(253f, 50f);
            MouseInput.OnLeftClick += () => OnControlChanged?.Invoke();

            Format = ModMenu.ControlText.WithAlignment(TextAlignment.Center);
            Name = "NewTerminalButton";
        }
    }
}