using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A labeled pair of horizontally aligned on and off bordered buttons used to indicate a boolean value. 
	/// Made to resemble on/off toggle used in the SE terminal.
	/// <para>Adds a label to <see cref="OnOffButton"/>.</para>
	/// <para>Formatting temporarily changes when it gains input focus.</para>
	/// </summary>
	public class NamedOnOffButton : HudElementBase, IClickableElement, IValueControl<bool>
    {
		/// <summary>
		/// Invoked when <see cref="Value"/> changes
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { onOffButton.ValueChanged += value; }
			remove { onOffButton.ValueChanged -= value; }
		}

		/// <summary>
		/// Registers a <see cref="Value"/> update callback. Useful in initializers.
		/// </summary>
		public EventHandler UpdateValueCallback { set { onOffButton.ValueChanged += value; } }

		/// <summary>
		/// The name of the control as it appears in the terminal.
		/// </summary>
		public RichText Name { get { return name.Text; } set { name.Text = value; } }

		/// <summary>
		/// Distance between the on and off buttons
		/// </summary>
		public float ButtonSpacing { get { return onOffButton.ButtonSpacing; } set { onOffButton.ButtonSpacing = value; } }

		/// <summary>
		/// Padding around on/off button block
		/// </summary>
		public Vector2 ButtonPadding { get { return onOffButton.Padding; } set { onOffButton.Padding = value; } }

		/// <summary>
		/// Color of the border surrounding the on and off buttons
		/// </summary>
		public Color BorderColor { get { return onOffButton.BorderColor; } set { onOffButton.BorderColor = value; } }

		/// <summary>
		/// On button text
		/// </summary>
		public RichText OnText { get { return onOffButton.OnText; } set { onOffButton.OnText = value; } }

		/// <summary>
		/// Off button text
		/// </summary>
		public RichText OffText { get { return onOffButton.OnText; } set { onOffButton.OnText = value; } }

		/// <summary>
		/// Default glyph format used by the on and off buttons
		/// </summary>
		public GlyphFormat Format { get { return onOffButton.Format; } set { onOffButton.Format = value; } }

		/// <summary>
		/// Current value of the on/off button
		/// </summary>
		public bool Value { get { return onOffButton.Value; } set { onOffButton.Value = value; } }

		/// <summary>
		/// Interface used to manage the element's input focus state.
		/// </summary>
		public IFocusHandler FocusHandler => onOffButton.FocusHandler;

		/// <summary>
		/// Mouse input element for the button
		/// </summary>
		public IMouseInput MouseInput => onOffButton.MouseInput;

		/// <summary>
		/// Label element over the buttons
		/// </summary>
		/// <exclude/>
		protected readonly Label name;

		/// <summary>
		/// On/Off button pair
		/// </summary>
		/// <exclude/>
		protected readonly OnOffButton onOffButton;

		/// <summary>
		/// Stacking container for label and buttons
		/// </summary>
		/// <exclude/>
		protected readonly HudChain layout;

		public NamedOnOffButton(HudParentBase parent) : base(parent)
		{
			name = new Label()
			{
				Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
				Text = "NewOnOffButton",
				Height = 22f,
			};

			onOffButton = new OnOffButton();

			layout = new HudChain(true, this)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
				Spacing = 2f,
				CollectionContainer = { { name, 0f }, { onOffButton, 0f } }
			};

			FocusHandler.InputOwner = this;
			Padding = new Vector2(40f, 0f);
			Size = new Vector2(300f, 84f);
		}

		public NamedOnOffButton() : this(null)
		{ }
	}
}