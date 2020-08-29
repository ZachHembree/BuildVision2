using System;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// Bordered checkbox designed to mimic the appearance of the checkbox used in the SE terminal
    /// (sans name tag).
    /// </summary>
    public class BorderedCheckBox : Button
    {
        /// <summary>
        /// Indicates whether or not the box is checked.
        /// </summary>
        public bool BoxChecked { get { return box.Visible; } set { box.Visible = value; } }

        private readonly TexturedBox box, highlight;

        private static readonly Color BoxColor = new Color(114, 121, 139);

        public BorderedCheckBox(HudParentBase parent = null) : base(parent)
        {
            var border = new BorderBox(this)
            {
                Color = TerminalFormatting.BorderColor,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            highlight = new TexturedBox(this)
            {
                Color = TerminalFormatting.HighlightOverlayColor,
                DimAlignment = DimAlignments.Both,
                Visible = false,
            };

            box = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Both,
                Padding = new Vector2(16f),
                Color = BoxColor,
            };

            Size = new Vector2(37f);
            Color = new Color(39, 52, 60);
            highlightColor = new Color(50, 60, 70);
            highlightEnabled = false;

            MouseInput.OnLeftClick += ToggleValue;
        }

        private void ToggleValue(object sender, EventArgs args)
        {
            BoxChecked = !BoxChecked;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsMousedOver)
            {
                highlight.Visible = true;
                box.Color = TerminalFormatting.HighlightColor;
            }
            else
            {
                highlight.Visible = false;
                box.Color = BoxColor;
            }
        }
    }
}