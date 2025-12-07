using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A pair of horizontally aligned on and off bordered buttons used to indicate a boolean value. 
	/// Styled to resemble on/off button used in the SE terminal.
	/// <para>Does not have a label. Use <see cref="NamedOnOffButton"/> for a version with a label.</para>
	/// <para>Formatting temporarily changes when it gains input focus.</para>
	/// </summary>
	public class OnOffButton : HudElementBase, IClickableElement, IValueControl<bool>
    {
		/// <summary>
		/// Invoked when <see cref="Value"/> changes
		/// </summary>
		public event EventHandler ValueChanged;

		/// <summary>
		/// Registers a <see cref="Value"/> update callback. Useful in initializers.
		/// </summary>
		public EventHandler UpdateValueCallback { set { ValueChanged += value; } }

		/// <summary>
		/// Spacing between the on and off buttons
		/// </summary>
		public float ButtonSpacing { get { return buttonChain.Spacing; } set { buttonChain.Spacing = value; } }

		/// <summary>
		/// Color of the border surrounding the on and off buttons
		/// </summary>
		public Color BorderColor
		{
			get { return onBorder.Color; }
			set
			{
				onBorder.Color = value;
				offBorder.Color = value;
				bgBorder.Color = value;
			}
		}

		/// <summary>
		/// Padding between background and button pair
		/// </summary>
		public Vector2 BackgroundPadding { get { return buttonChain.Padding; } set { buttonChain.Padding = value; } }

		/// <summary>
		/// Color used for the background behind the button pair
		/// </summary>
		public Color BackgroundColor { get { return _backgroundColor; } set { background.Color = value; _backgroundColor = value; } }

		/// <summary>
		/// Focus color used for the background behind the button pair
		/// </summary>
		public Color FocusColor { get; set; }

		/// <summary>
		/// Highlight color used for the background behind the button pair
		/// </summary>
		public Color HighlightColor { get; set; }

		/// <summary>
		/// Color used for the background of the unselected button
		/// </summary>
		public Color UnselectedColor { get; set; }

		/// <summary>
		/// Background color used to indicate the current selection
		/// </summary>
		public Color SelectionColor { get; set; }

		/// <summary>
		/// On button text
		/// </summary>
		public RichText OnText { get { return on.Text; } set { on.Text = value; } }

		/// <summary>
		/// Off button text
		/// </summary>
		public RichText OffText { get { return off.Text; } set { off.Text = value; } }

		/// <summary>
		/// Default glyph format used by the on and off buttons
		/// </summary>
		public GlyphFormat Format { get { return on.Format; } set { on.Format = value; off.Format = value; } }

		/// <summary>
		/// Current value of the on/off button
		/// </summary>
		public bool Value { get; set; }

		/// <summary>
		/// If true, then the button will change formatting when it takes focus.
		/// </summary>
		public bool UseFocusFormatting { get; set; }

		/// <summary>
		/// Determines whether or not the button will highlight when moused over.
		/// </summary>
		public virtual bool HighlightEnabled { get; set; }

		/// <summary>
		/// Interface for managing gaining/losing input focus
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Mouse input element for the button
		/// </summary>
		public IMouseInput MouseInput { get; }

		/// <summary>
		/// On and off labels with backgrounds
		/// </summary>
		/// <exclude/>
		protected readonly LabelBox on, off;

		/// <summary>
		/// Borders drawn around on and off buttons
		/// </summary>
		/// <exclude/>
		protected readonly BorderBox onBorder, offBorder;

		/// <summary>
		/// Linear stacking container for placing and sizing the buttons
		/// </summary>
		/// <exclude/>
		protected readonly HudChain buttonChain;

		/// <summary>
		/// Main background behind the button pair
		/// </summary>
		/// <exclude/>
		protected readonly TexturedBox background;

		/// <summary>
		/// Border around the main background
		/// </summary>
		/// <exclude/>
		protected readonly BorderBox bgBorder;

		/// <exclude/>
		protected readonly MouseInputElement _mouseInput;
		/// <exclude/>
		protected Color _backgroundColor;

		/// <summary>
		/// Previously set value, used for event updates
		/// </summary>
		/// <exclude/>
		protected bool lastValue;

		public OnOffButton(HudParentBase parent) : base(parent)
		{
			FocusHandler = new InputFocusHandler(this);
			_mouseInput = new MouseInputElement(this);
			MouseInput = _mouseInput;

			background = new TexturedBox(this)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
			};

			bgBorder = new BorderBox(background)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
			};

			on = new LabelBox()
			{
				AutoResize = false,
				Size = new Vector2(71f, 49f),
				Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
				Text = "On"
			};

			onBorder = new BorderBox(on)
			{
				Thickness = 2f,
				DimAlignment = DimAlignments.UnpaddedSize,
			};

			off = new LabelBox()
			{
				AutoResize = false,
				Size = new Vector2(71f, 49f),
				Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Center),
				Text = "Off"
			};

			offBorder = new BorderBox(off)
			{
				Thickness = 2f,
				DimAlignment = DimAlignments.UnpaddedSize,
			};

			buttonChain = new HudChain(false, bgBorder)
			{
				DimAlignment = DimAlignments.Size,
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				Padding = new Vector2(20f, 10f),
				Spacing = 9f,
				CollectionContainer = { { on, 1f }, { off, 1f } }
			};

			Size = new Vector2(166f, 59f);

			BackgroundColor = TerminalFormatting.Cinder.SetAlphaPct(0.8f);
			HighlightColor = TerminalFormatting.Atomic;
			FocusColor = TerminalFormatting.Mint;
			BorderColor = TerminalFormatting.LimedSpruce;

			UnselectedColor = TerminalFormatting.OuterSpace;
			SelectionColor = TerminalFormatting.DullMint;

			HighlightEnabled = true;
			UseFocusFormatting = true;

			_mouseInput.LeftClicked += LeftClick;
			lastValue = Value;
		}

		public OnOffButton() : this(null)
		{ }

		/// <summary>
		/// Inverts the current value on click
		/// </summary>
		/// <exclude/>
		protected virtual void LeftClick(object sender, EventArgs args) => Value = !Value;

		/// <summary>
		/// Updates the formatting based on the current value and highlight state
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			if (Value)
			{
				on.Color = SelectionColor;
				off.Color = UnselectedColor;
			}
			else
			{
				off.Color = SelectionColor;
				on.Color = UnselectedColor;
			}

			if (HighlightEnabled && _mouseInput.IsMousedOver)
				background.Color = HighlightColor;
			else if (UseFocusFormatting && FocusHandler.HasFocus)
				background.Color = FocusColor;
			else
				background.Color = BackgroundColor;
		}

		/// <summary>
		/// Handles keyboard input when focused and fires value change events
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (lastValue != Value)
			{
				ValueChanged?.Invoke(FocusHandler?.InputOwner, EventArgs.Empty);
				lastValue = Value;
			}

			if (FocusHandler.HasFocus && SharedBinds.Space.IsNewPressed)
			{
				_mouseInput.LeftClick();
			}
		}
	}
}