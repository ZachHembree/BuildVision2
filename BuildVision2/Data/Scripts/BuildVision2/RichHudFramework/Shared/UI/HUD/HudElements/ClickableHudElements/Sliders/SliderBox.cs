using VRageMath;
using System;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A horizontal slider control designed to mimic the appearance of the standard sliders found 
	/// in the Space Engineers terminal.
	/// </summary>
	public class SliderBox : HudElementBase, IClickableElement
	{
		/// <summary>
		/// Invoked when the current value of the slider changes.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { slide.ValueChanged += value; }
			remove { slide.ValueChanged -= value; }
		}

		/// <summary>
		/// Helper property for registering a value update callback during initialization.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { slide.ValueChanged += value; }
		}

		/// <summary>
		/// The minimum allowable value for the slider.
		/// </summary>
		public float Min { get { return slide.Min; } set { slide.Min = value; } }

		/// <summary>
		/// The maximum allowable value for the slider.
		/// </summary>
		public float Max { get { return slide.Max; } set { slide.Max = value; } }

		/// <summary>
		/// The current value of the slider, clamped between <see cref="Min"/> and <see cref="Max"/>.
		/// </summary>
		public float Current { get { return slide.Current; } set { slide.Current = value; } }

		/// <summary>
		/// The current value expressed as a normalized percentage (0.0 to 1.0) of the range between Min and Max.
		/// </summary>
		public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

		/// <summary>
		/// The color of the slider track (the background bar).
		/// </summary>
		public Color BarColor { get { return slide.BarColor; } set { slide.BarColor = value; lastBarColor = value; } }

		/// <summary>
		/// The color of the slider track when moused over.
		/// </summary>
		public Color BarHighlight { get { return slide.BarHighlight; } set { slide.BarHighlight = value; } }

		/// <summary>
		/// The color of the slider track when the control has input focus.
		/// </summary>
		public Color BarFocusColor { get; set; }

		/// <summary>
		/// The color of the slider thumb (the movable button) when not moused over.
		/// </summary>
		public Color SliderColor { get { return slide.SliderColor; } set { slide.SliderColor = value; lastSliderColor = value; } }

		/// <summary>
		/// The color of the slider thumb (the movable button) when moused over.
		/// </summary>
		public Color SliderHighlight { get { return slide.SliderHighlight; } set { slide.SliderHighlight = value; } }

		/// <summary>
		/// The color of the slider thumb (the movable button) when the control has input focus.
		/// </summary>
		public Color SliderFocusColor { get; set; }

		/// <summary>
		/// The color of the background container box.
		/// </summary>
		public Color BackgroundColor { get { return background.Color; } set { background.Color = value; lastBackgroundColor = value; } }

		/// <summary>
		/// The color of the background container box when moused over.
		/// </summary>
		public Color BackgroundHighlight { get; set; }

		/// <summary>
		/// The color of the background container box when the control has input focus.
		/// </summary>
		public Color BackgroundFocusColor { get; set; }

		/// <summary>
		/// The color of the border surrounding the background box.
		/// </summary>
		public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

		/// <summary>
		/// If true, the slider box will change visual appearance when moused over.
		/// </summary>
		public bool HighlightEnabled { get; set; }

		/// <summary>
		/// If true, the slider box will change visual appearance when it has input focus.
		/// </summary>
		public bool UseFocusFormatting { get; set; }

		/// <summary>
		/// Interface used to manage the element's input focus state.
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Mouse input interface for this clickable element.
		/// </summary>
		public IMouseInput MouseInput => slide;

		/// <summary>
		/// Indicates whether the cursor is currently over this element.
		/// </summary>
		public override bool IsMousedOver => slide.IsMousedOver;

		/// <summary>
		/// The background container behind the slider.
		/// </summary>
		/// <exclude/>
		protected readonly TexturedBox background;

		/// <summary>
		/// The border surrounding the background.
		/// </summary>
		/// <exclude/>
		protected readonly BorderBox border;

		/// <summary>
		/// The actual slider logic and rendering element.
		/// </summary>
		/// <exclude/>
		protected readonly SliderBar slide;

		/// <summary>
		/// Cached colors used to restore state after highlighting or focus changes.
		/// </summary>
		/// <exclude/>
		protected Color lastBarColor, lastSliderColor, lastBackgroundColor;

		public SliderBox(HudParentBase parent) : base(parent)
		{
			background = new TexturedBox(this)
			{
				DimAlignment = DimAlignments.Size
			};

			border = new BorderBox(background)
			{
				Thickness = 1f,
				DimAlignment = DimAlignments.Size,
			};

			FocusHandler = new InputFocusHandler(this)
			{
				GainedInputFocusCallback = GainFocus,
				LostInputFocusCallback = LoseFocus
			};
			slide = new SliderBar(this)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
				SliderSize = new Vector2(14f, 28f),
				BarHeight = 5f,
				MouseInput =
				{
					CursorEnteredCallback = CursorEnter,
					CursorExitedCallback = CursorExit
				}
			};

			BackgroundColor = TerminalFormatting.OuterSpace;
			BorderColor = TerminalFormatting.LimedSpruce;
			BackgroundHighlight = TerminalFormatting.Atomic;
			BackgroundFocusColor = TerminalFormatting.Mint;

			SliderColor = TerminalFormatting.MistBlue;
			SliderHighlight = Color.White;
			SliderFocusColor = TerminalFormatting.Cinder;

			BarColor = TerminalFormatting.MidGrey;
			BarHighlight = Color.White;
			BarFocusColor = TerminalFormatting.BlackPerl;

			UseFocusFormatting = true;
			HighlightEnabled = true;

			Padding = new Vector2(18f, 18f);
			Size = new Vector2(317f, 47f);
		}

		public SliderBox() : this(null)
		{ }

		/// <summary>
		/// Handles arrow key input for fine-tuning the slider value when focused.
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (FocusHandler.HasFocus)
			{
				if (SharedBinds.LeftArrow.IsNewPressed || SharedBinds.LeftArrow.IsPressedAndHeld)
				{
					Percent -= 0.01f;
				}
				else if (SharedBinds.RightArrow.IsNewPressed || SharedBinds.RightArrow.IsPressedAndHeld)
				{
					Percent += 0.01f;
				}
			}
		}

		/// <summary>
		/// Applies highlight and focus formatting when the cursor enters the slider area.
		/// </summary>
		/// <exclude/>
		protected virtual void CursorEnter(object sender, EventArgs args)
		{
			if (HighlightEnabled)
			{
				if (!(UseFocusFormatting && FocusHandler.HasFocus))
				{
					lastBarColor = slide.BarColor;
					lastSliderColor = slide.SliderColor;
					lastBackgroundColor = background.Color;
				}

				slide.SliderColor = SliderHighlight;
				slide.BarColor = BarHighlight;
				background.Color = BackgroundHighlight;
			}
		}

		/// <summary>
		/// Restores original colors when the cursor exits the slider area.
		/// </summary>
		/// <exclude/>
		protected virtual void CursorExit(object sender, EventArgs args)
		{
			if (HighlightEnabled)
			{
				if (UseFocusFormatting && FocusHandler.HasFocus)
				{
					slide.SliderColor = SliderFocusColor;
					slide.BarColor = BarFocusColor;
					background.Color = BackgroundFocusColor;
				}
				else
				{
					slide.SliderColor = lastSliderColor;
					slide.BarColor = lastBarColor;
					background.Color = lastBackgroundColor;
				}
			}
		}

		/// <summary>
		/// Applies focus-specific formatting when the slider gains input focus.
		/// </summary>
		/// <exclude/>
		protected virtual void GainFocus(object sender, EventArgs args)
		{
			if (UseFocusFormatting && !MouseInput.IsMousedOver)
			{
				lastBarColor = slide.BarColor;
				lastSliderColor = slide.SliderColor;
				lastBackgroundColor = background.Color;

				slide.SliderColor = SliderFocusColor;
				slide.BarColor = BarFocusColor;
				background.Color = BackgroundFocusColor;
			}
		}

		/// <summary>
		/// Clears focus-specific formatting when the slider loses input focus.
		/// </summary>
		/// <exclude/>
		protected virtual void LoseFocus(object sender, EventArgs args)
		{
			if (UseFocusFormatting)
			{
				slide.SliderColor = lastSliderColor;
				slide.BarColor = lastBarColor;
				background.Color = lastBackgroundColor;
			}
		}
	}
}