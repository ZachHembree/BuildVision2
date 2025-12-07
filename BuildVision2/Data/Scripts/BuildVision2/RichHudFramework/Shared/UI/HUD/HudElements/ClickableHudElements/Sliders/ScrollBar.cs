using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A clickable scrollbar designed to approximate the appearance of standard Space Engineers scrollbars.
	/// </summary>
	public class ScrollBar : HudElementBase, IClickableElement, IValueControl<float>
    {
		/// <summary>
		/// Invoked when the scrollbar value changes.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { SlideInput.ValueChanged += value; }
			remove { SlideInput.ValueChanged -= value; }
		}

		/// <summary>
		/// Helper property for registering a value update callback during initialization.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { SlideInput.ValueChanged += value; }
		}

		/// <summary>
		/// The minimum allowable value.
		/// </summary>
		public float Min
		{
			get { return SlideInput.Min; }
			set { SlideInput.Min = value; }
		}

		/// <summary>
		/// The maximum allowable value.
		/// </summary>
		public float Max
		{
			get { return SlideInput.Max; }
			set { SlideInput.Max = value; }
		}

		/// <summary>
		/// The currently set value, clamped between <see cref="Min"/> and <see cref="Max"/>.
		/// </summary>
		public float Value { get { return SlideInput.Value; } set { SlideInput.Value = value; } }

		/// <summary>
		/// The current value expressed as a normalized value on [0, 1] of the range between Min and Max.
		/// </summary>
		public float Percent { get { return SlideInput.Percent; } set { SlideInput.Percent = value; } }

		/// <summary>
		/// The proportion of the total range that is currently visible as a normalized value on [0, 1].
		/// <para>Automatically hides sliders if <see cref="VisiblePercent"/> >= 1f.</para>
		/// </summary>
		public float VisiblePercent { get; set; }

		/// <summary>
		/// Determines whether the scrollbar is oriented vertically. If true, the slider operates on the Y-axis.
		/// <para>True by default.</para>
		/// </summary>
		public bool Vertical { get { return SlideInput.Vertical; } set { SlideInput.Vertical = value; SlideInput.Reverse = value; } }

		/// <summary>
		/// Indicates whether the cursor is currently over the scrollbar.
		/// </summary>
		public override bool IsMousedOver => SlideInput.IsMousedOver;

		/// <summary>
		/// Interface used to manage the element's input focus state.
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Mouse input interface for this clickable element.
		/// </summary>
		public IMouseInput MouseInput => SlideInput.MouseInput;

		/// <summary>
		/// The internal slider element functioning as the scrollbar.
		/// </summary>
		public readonly SliderBar SlideInput;

		public ScrollBar(HudParentBase parent) : base(parent)
		{
			FocusHandler = new InputFocusHandler(this);
			SlideInput = new SliderBar(this)
			{
				Reverse = true,
				Vertical = true,
				SliderWidth = 13f,
				BarWidth = 13f,

				SliderColor = new Color(78, 87, 101),
				SliderHighlight = new Color(136, 140, 148),

				BarColor = new Color(41, 51, 61),
			};

			Size = new Vector2(13f, 300f);
			Padding = new Vector2(30f, 10f);
			SlideInput.SliderVisible = false;
			VisiblePercent = 0.2f;
		}

		public ScrollBar() : this(null)
		{ }

		/// <summary>
		/// Updates the scrollbar's slider size and visibility based on whether the content fits within the visible area.
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			Vector2 size = UnpaddedSize;
			SlideInput.BarSize = size;

			if (Vertical)
			{
				SlideInput.SliderWidth = size.X;
				SlideInput.SliderHeight = size.Y * VisiblePercent;
				SlideInput.SliderVisible = SlideInput.SliderHeight < SlideInput.BarHeight;
			}
			else
			{
				SlideInput.SliderHeight = size.Y;
				SlideInput.SliderWidth = size.X * VisiblePercent;
				SlideInput.SliderVisible = SlideInput.SliderWidth < SlideInput.BarWidth;
			}
		}
	}
}