using System;
using VRageMath;

namespace RichHudFramework.UI
{
	using UI.Rendering;

	/// <summary>
	/// A composite control containing a horizontal slider box, a name label, and a value label. 
	/// <para>
	/// Note: The value label is not updated automatically; it must be updated manually via the ValueChanged event.
	/// </para>
	/// </summary>
	public class NamedSliderBox : HudElementBase, IClickableElement, IValueControl<float>
    {
		/// <summary>
		/// Invoked when the current value of the slider changes.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { SliderBox.ValueChanged += value; }
			remove { SliderBox.ValueChanged -= value; }
		}

		/// <summary>
		/// Helper property for registering a value update callback during initialization.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { SliderBox.ValueChanged += value; }
		}

		/// <summary>
		/// The text displayed in the name label.
		/// </summary>
		public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

		/// <summary>
		/// The text displayed in the value label. 
		/// <para>Note: This does not automatically reflect changes to the slider value; you must update this string manually.</para>
		/// </summary>
		public RichText ValueText { get { return current.TextBoard.GetText(); } set { current.TextBoard.SetText(value); } }

		/// <summary>
		/// Accessor for the text builder of the name label.
		/// </summary>
		public ITextBuilder NameBuilder => name.TextBoard;

		/// <summary>
		/// Accessor for the text builder of the value label.
		/// </summary>
		public ITextBuilder ValueBuilder => current.TextBoard;

		/// <summary>
		/// The minimum configurable value for the slider.
		/// </summary>
		public float Min { get { return SliderBox.Min; } set { SliderBox.Min = value; } }

		/// <summary>
		/// The maximum configurable value for the slider.
		/// </summary>
		public float Max { get { return SliderBox.Max; } set { SliderBox.Max = value; } }

		/// <summary>
		/// The value currently set on the slider.
		/// </summary>
		public float Value { get { return SliderBox.Value; } set { SliderBox.Value = value; } }

		/// <summary>
		/// The current slider value expressed as a percentage (0 to 1) of the range between the Min and Max values.
		/// </summary>
		public float Percent { get { return SliderBox.Percent; } set { SliderBox.Percent = value; } }

		/// <summary>
		/// Interface used to manage the element's input focus state.
		/// </summary>
		public IFocusHandler FocusHandler => SliderBox.FocusHandler;

		/// <summary>
		/// Mouse input interface for this clickable element.
		/// </summary>
		public IMouseInput MouseInput => SliderBox.MouseInput;

		/// <summary>
		/// Indicates whether the cursor is currently over the slider box.
		/// </summary>
		public override bool IsMousedOver => SliderBox.IsMousedOver;

		/// <summary>
		/// Slider control inside the labeled slider box. 
		/// This field can be used to customize the formatting of the slider box.
		/// </summary>
		public readonly SliderBox SliderBox;

		/// <summary>
		/// Labels for the name and current value display.
		/// </summary>
		/// <exclude/>
		protected readonly Label name, current;

		public NamedSliderBox(HudParentBase parent) : base(parent)
		{
			SliderBox = new SliderBox(this)
			{
				DimAlignment = DimAlignments.UnpaddedWidth,
				ParentAlignment = ParentAlignments.InnerBottom,
				UseCursor = true,
			};

			name = new Label(this)
			{
				AutoResize = false,
				Format = TerminalFormatting.ControlFormat,
				Text = "NewSlideBox",
				Offset = new Vector2(0f, -18f),
				ParentAlignment = ParentAlignments.PaddedInnerLeft | ParentAlignments.Top
			};

			current = new Label(this)
			{
				AutoResize = false,
				Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
				Text = "Value",
				Offset = new Vector2(0f, -18f),
				ParentAlignment = ParentAlignments.PaddedInnerRight | ParentAlignments.Top
			};

			FocusHandler.InputOwner = this;
			Padding = new Vector2(40f, 0f);
			Size = new Vector2(317f, 70f);
		}

		public NamedSliderBox() : this(null)
		{ }

		/// <summary>
		/// Sizes the labels and slider box to fit within the element's bounds.
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			Vector2 size = UnpaddedSize;
			current.UnpaddedSize = current.TextBoard.TextSize;
			name.UnpaddedSize = name.TextBoard.TextSize;
			SliderBox.Height = size.Y - Math.Max(name.Height, current.Height);
			current.Width = Math.Max(size.X - name.Width - 10f, 0f);
		}
	}
}