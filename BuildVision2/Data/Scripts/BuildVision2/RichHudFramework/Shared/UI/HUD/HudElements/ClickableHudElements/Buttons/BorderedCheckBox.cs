using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Bordered checkbox designed to mimic the appearance of the checkbox used in the SE terminal.
	/// <para>Does not have a label. Use <see cref="NamedCheckBox"/> for a version with a label.</para>
	/// <para>Formatting temporarily changes when it gains input focus.</para>
	/// </summary>
	public class BorderedCheckBox : Button
    {
		/// <summary>
		/// Invoked when the current value (<see cref="IsBoxChecked"/>) changes
		/// </summary>
		public event EventHandler ValueChanged;

		/// <summary>
		/// Registers a value (<see cref="IsBoxChecked"/>) update callback. Useful in initializers.
		/// </summary>
		public EventHandler UpdateValueCallback { set { ValueChanged += value; } }

		/// <summary>
		/// Indicates whether or not the box is checked.
		/// </summary>
		public bool IsBoxChecked { get; set; }

        /// <summary>
        /// Color of the border surrounding the button
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Thickness of the border surrounding the button
        /// </summary>
        public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

        /// <summary>
        /// Tickbox default color
        /// </summary>
        public Color TickBoxColor { get { return tickBox.Color; } set { tickBox.Color = value; } }

        /// <summary>
        /// Tickbox highlight color
        /// </summary>
        public Color TickBoxHighlightColor { get; set; }

        /// <summary>
        /// Tickbox focus color
        /// </summary>
        public Color TickBoxFocusColor { get; set; }

        /// <summary>
        /// Background color used when the control gains focus.
        /// </summary>
        public Color FocusColor { get; set; }

        /// <summary>
        /// If true, then the button will change formatting when it takes focus.
        /// </summary>
        public bool UseFocusFormatting { get; set; }

		/// <summary>
		/// Renders a colored border around the checkbox
		/// </summary>
		/// <exclude/>
		protected readonly BorderBox border;

        /// <summary>
        /// Renders the checkbox tick
        /// </summary>
        /// <exclude/>
        protected readonly TexturedBox tickBox;

        /// <summary>
        /// Last tick color before highlighting
        /// </summary>
        /// <exclude/>
        protected Color lastTickColor;

        /// <summary>
        /// Last checkbox value, used for event updates
        /// </summary>
        /// <exclude/>
        protected bool lastValue;

        public BorderedCheckBox(HudParentBase parent) : base(parent)
        {
            border = new BorderBox(this)
            {
                Thickness = 1f,
                DimAlignment = DimAlignments.Size,
            };

            tickBox = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.UnpaddedSize,
                Padding = new Vector2(17f),
            };

            IsBoxChecked = true;
			Size = new Vector2(37f);

            Color = TerminalFormatting.OuterSpace;
            HighlightColor = TerminalFormatting.Atomic;
            FocusColor = TerminalFormatting.Mint;

            TickBoxColor = TerminalFormatting.StormGrey;
            TickBoxHighlightColor = Color.White;
            TickBoxFocusColor = TerminalFormatting.Cinder;

            BorderColor = TerminalFormatting.LimedSpruce;
            UseFocusFormatting = true;
            lastValue = IsBoxChecked;

            MouseInput.LeftClicked += ToggleValue;
            FocusHandler.GainedInputFocus += GainFocus;
			FocusHandler.LostInputFocus += LoseFocus;
        }

        public BorderedCheckBox() : this(null)
        { }

		/// <summary>
		/// Handles keyboard input when focused and fires value changed events
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
        {
            tickBox.Visible = IsBoxChecked;

            if (FocusHandler.HasFocus)
            {
                if (SharedBinds.Space.IsNewPressed)
                {
                    _mouseInput.LeftClick();
                }
            }

            if (lastValue != IsBoxChecked)
            {
                ValueChanged?.Invoke(FocusHandler?.InputOwner, EventArgs.Empty);
                lastValue = IsBoxChecked;
            }
        }

		/// <summary>
		/// Inverts checkbox value on click
		/// </summary>
		/// <exclude/>
		protected virtual void ToggleValue(object sender, EventArgs args)
        {
            IsBoxChecked = !IsBoxChecked;
        }

		/// <summary>
		/// Sets highlight formatting when the cursor enters
		/// </summary>
		/// <exclude/>
		protected override void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (!(UseFocusFormatting && FocusHandler.HasFocus))
                {
                    lastBackgroundColor = Color;
                    lastTickColor = TickBoxColor;
                }

                Color = HighlightColor;
                TickBoxColor = TickBoxHighlightColor;
            }
        }

		/// <summary>
		/// Resets highlight formatting when the cursor leaves
		/// </summary>
		/// <exclude/>
		protected override void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting && FocusHandler.HasFocus)
                {
                    Color = FocusColor;
                    TickBoxColor = TickBoxFocusColor;
                }
                else
                {
                    Color = lastBackgroundColor;
                    TickBoxColor = lastTickColor;
                }
            }
        }

		/// <summary>
		/// Sets focus formatting
		/// </summary>
		/// <exclude/>
		protected virtual void GainFocus(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting && !MouseInput.IsMousedOver)
                {
                    Color = FocusColor;
                    TickBoxColor = TickBoxFocusColor;
                }
            }
        }

		/// <summary>
		/// Restores formatting to non-focused state
		/// </summary>
		/// <exclude/>
		protected virtual void LoseFocus(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                if (UseFocusFormatting)
                {
                    Color = lastBackgroundColor;
                    TickBoxColor = lastTickColor;
                }
            }
        }
    }
}