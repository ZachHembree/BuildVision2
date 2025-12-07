using System;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// <see cref="LabelBoxButton"/> styled to closely match the appearance of buttons in the SE terminal.
    /// <para>Formatting temporarily changes when it gains input focus.</para>
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
        /// Background highlight color
        /// </summary>
        public override Color HighlightColor { get; set; }

        /// <summary>
        /// Text color used when the control gains focus.
        /// </summary>
        public Color FocusTextColor { get; set; }

        /// <summary>
        /// Background color used when the control gains focus.
        /// </summary>
        public Color FocusColor { get; set; } 

        /// <summary>
        /// If true, then the button will change formatting when it takes focus.
        /// Enabled by default.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

		/// <summary>
		/// Renders a colored border around the button
		/// </summary>
		/// <exclude/>
		protected readonly BorderBox border;

        /// <summary>
        /// Background and text colors last used before highlighting
        /// </summary>
        /// <exclude/>
        protected Color lastColor, lastTextColor;

        public BorderedButton(HudParentBase parent) : base(parent)
        {
            border = new BorderBox(this)
            {
                Thickness = 1f,
                DimAlignment = DimAlignments.UnpaddedSize,
            };

            AutoResize = false;
            Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center);
            FocusTextColor = TerminalFormatting.Charcoal;
            Text = "NewBorderedButton";

            TextPadding = new Vector2(32f, 0f);
            Padding = new Vector2(37f, 0f);
            Size = new Vector2(253f, 50f);
            HighlightEnabled = true;

            Color = TerminalFormatting.OuterSpace;
            HighlightColor = TerminalFormatting.Atomic;
            BorderColor = TerminalFormatting.LimedSpruce;
            FocusColor = TerminalFormatting.Mint;
            UseFocusFormatting = true;

			FocusHandler.GainedInputFocus += GainFocus;
			FocusHandler.LostInputFocus += LoseFocus;
        }

        public BorderedButton() : this(null)
        { }

        /// <summary>
        /// Handles keyboard input when the button has input focus
        /// </summary>
        /// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
        {
            if (FocusHandler.HasFocus)
            {
                if (SharedBinds.Space.IsNewPressed)
                {
                    _mouseInput.LeftClick();
                }
			}
		}

		/// <summary>
		/// Invoked when the cursor first howvers over the button
		/// </summary>
		/// <exclude/>
		protected override void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (!UseFocusFormatting || !FocusHandler.HasFocus)
                    lastColor = Color;

				if (UseFocusFormatting)
				{
					if (!FocusHandler.HasFocus)
						lastTextColor = TextBoard.Format.Color;

					TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
				}

                Color = HighlightColor;
            }
        }

		/// <summary>
		/// Invoked when the cursor moves out of the button
		/// </summary>
		/// <exclude/>
		protected override void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting && FocusHandler.HasFocus)
                {
                    Color = FocusColor;
                    TextBoard.SetFormatting(TextBoard.Format.WithColor(FocusTextColor));
                }
                else
                {
                    Color = lastColor;

					if (UseFocusFormatting)
						TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
                }
            }
        }

		/// <summary>
		/// Invoked when the button has input focus
		/// </summary>
		/// <exclude/>
		protected virtual void GainFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting)
            {
                if (!MouseInput.IsMousedOver)
                {
                    lastColor = Color;
                    lastTextColor = TextBoard.Format.Color;
                }

                Color = FocusColor;
                TextBoard.SetFormatting(TextBoard.Format.WithColor(FocusTextColor));
            }
        }

		/// <summary>
		/// Invoked when the button loses input focus
		/// </summary>
		/// <exclude/>
		protected virtual void LoseFocus(object sender, EventArgs args)
        {
            if (UseFocusFormatting)
            {
                Color = lastColor;
                TextBoard.SetFormatting(TextBoard.Format.WithColor(lastTextColor));
            }
        }
    }
}