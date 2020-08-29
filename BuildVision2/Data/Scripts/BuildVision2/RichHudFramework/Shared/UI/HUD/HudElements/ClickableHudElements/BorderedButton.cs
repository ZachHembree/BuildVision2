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
        public override Color HighlightColor { get { return highlight.Color; } set { highlight.Color = value; } }

        private readonly BorderBox border;
        private readonly TexturedBox highlight;

        public BorderedButton(HudParentBase parent = null) : base(parent)
        {
            border = new BorderBox(this)
            {
                Color = TerminalFormatting.BorderColor,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            highlight = new TexturedBox(this)
            {
                Color = TerminalFormatting.HighlightOverlayColor,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Visible = false,
            };

            AutoResize = false;
            Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center);
            Text = "NewTerminalButton";

            Color = new Color(42, 55, 63);
            TextPadding = new Vector2(32f, 0f);
            Padding = new Vector2(37f, 0f);
            Size = new Vector2(253f, 50f);
            HighlightEnabled = true;
        }

        protected override void CursorEntered(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                highlight.Visible = true;
            }
        }

        protected override void CursorExited(object sender, EventArgs args)
        {
            highlight.Visible = false;
        }
    }
}