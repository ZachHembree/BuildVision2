using System;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// LabelBoxButton modified to roughly match the appearance of buttons in the SE terminal.
    /// </summary>
    public class BorderedButton : LabelBoxButton
    {
        /// <summary>
        /// Color of the border surrounding the button
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Thickness of the border surrounding the button
        /// </summary>
        public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

        /// <summary>
        /// Color of the button's highlight overlay
        /// </summary>
        public override Color HighlightColor { get; set; }

        private readonly BorderBox border;
        private readonly TexturedBox highlight;

        public BorderedButton(HudParentBase parent) : base(parent)
        {
            border = new BorderBox(this)
            {
                Color = TerminalFormatting.BorderColor,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            highlight = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Visible = false,
            };

            AutoResize = false;
            Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center);
            Text = "NewBorderedButton";

            Color = new Color(42, 55, 63);
            HighlightColor = TerminalFormatting.HighlightOverlayColor;
            TextPadding = new Vector2(32f, 0f);
            Padding = new Vector2(37f, 0f);
            Size = new Vector2(253f, 50f);
            HighlightEnabled = true;
        }

        public BorderedButton() : this(null)
        { }

        protected override void CursorEntered(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                highlight.Color = HighlightColor;
                highlight.Visible = true;
            }
        }

        protected override void CursorExited(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                highlight.Visible = false;
            }
        }
    }
}