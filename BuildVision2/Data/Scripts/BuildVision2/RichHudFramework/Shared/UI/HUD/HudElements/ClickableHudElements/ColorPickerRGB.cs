using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A named color picker component utilizing RGB sliders, styled to mimic the Space Engineers terminal interface.
	/// <para>Operating in RGB mode. Alpha (transparency) is not supported.</para>
	/// </summary>
	public class ColorPickerRGB : HudElementBase, IValueControl<Color>
	{
		/// <summary>
		/// Event invoked when the selected color value changes.
		/// </summary>
		public event EventHandler ValueChanged;

		/// <summary>
		/// Utility property for registering a value update callback via object initializers.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { ValueChanged += value; }
		}

		/// <summary>
		/// Gets or sets the text content of the picker's label.
		/// </summary>
		public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

		/// <summary>
		/// Gets the text builder backing the label.
		/// </summary>
		public ITextBuilder NameBuilder => name.TextBoard;

		/// <summary>
		/// Gets or sets the glyph formatting used by the main label.
		/// </summary>
		public GlyphFormat NameFormat { get { return name.TextBoard.Format; } set { name.TextBoard.SetFormatting(value); } }

		/// <summary>
		/// Gets or sets the glyph formatting used by the numeric value labels next to the sliders.
		/// </summary>
		public GlyphFormat ValueFormat
		{
			get { return sliderText[0].Format; }
			set
			{
				foreach (Label label in sliderText)
					label.TextBoard.SetFormatting(value);
			}
		}

		/// <summary>
		/// Gets or sets the color currently specified by the picker.
		/// <para>Setting this value will automatically update the positions of the sliders.</para>
		/// </summary>
		public virtual Color Value
		{
			get { return _color; }
			set
			{
				sliders[0].Value = value.R;
				sliders[1].Value = value.G;
				sliders[2].Value = value.B;
				_color = value;
			}
		}

		// Header

		/// <summary>
		/// Color picker label located to the left of the color preview.
		/// </summary>
		/// <exclude/>
		protected readonly Label name;

		/// <summary>
		/// The visual preview box displaying the selected color.
		/// </summary>
		/// <exclude/>
		protected readonly TexturedBox display;

		/// <summary>
		/// Horizontal container layout for the name label and display box.
		/// </summary>
		/// <exclude/>
		protected readonly HudChain headerChain;

		// Slider text

		/// <summary>
		/// Array of labels displaying numeric values, arranged in a vertical column to the right of the sliders.
		/// </summary>
		/// <exclude/>
		protected readonly Label[] sliderText;

		/// <exclude/>
		protected readonly HudChain<HudElementContainer<Label>, Label> colorNameColumn;

		// Sliders

		/// <summary>
		/// Vertical column of slider controls located to the left of the value labels.
		/// </summary>
		/// <exclude/>
		public readonly SliderBox[] sliders;

		/// <exclude/>
		protected readonly HudChain<HudElementContainer<SliderBox>, SliderBox> colorSliderColumn;

		/// <summary>
		/// Layout chain combining the slider column and the value label column.
		/// </summary>
		/// <exclude/>
		protected readonly HudChain colorChain;

		/// <summary>
		/// Stores the current and previous color states. Previous color is used to detect changes for event firing.
		/// </summary>
		/// <exclude/>
		protected Color _color, lastColor;

		/// <summary>
		/// The index of the color slider that currently has input focus, or -1 if none.
		/// </summary>
		/// <exclude/>
		protected int focusedChannel;

		public ColorPickerRGB(HudParentBase parent) : base(parent)
		{
			// Header
			name = new Label()
			{
				Format = GlyphFormat.Blueish.WithSize(1.08f),
				Text = "NewColorPicker",
				AutoResize = false,
				Size = new Vector2(88f, 22f)
			};

			display = new TexturedBox()
			{
				Width = 231f,
				Color = Color.Black
			};

			var dispBorder = new BorderBox(display)
			{
				Color = Color.White,
				Thickness = 1f,
				DimAlignment = DimAlignments.Size,
			};

			headerChain = new HudChain(false)
			{
				Height = 22f,
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				CollectionContainer = { name, { display, 1f } }
			};

			// Color picker
			sliderText = new Label[]
			{
				new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f },
				new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f },
				new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f }
			};

			colorNameColumn = new HudChain<HudElementContainer<Label>, Label>(true)
			{
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				Width = 87f,
				Spacing = 5f,
				CollectionContainer =
				{
					{ sliderText[0], 1f },
					{ sliderText[1], 1f },
					{ sliderText[2], 1f }
				}
			};
			
			sliders = new SliderBox[]
			{
				new SliderBox()
				{
					Min = 0f, Max = 255f, Height = 47f,
					UpdateValueCallback = UpdateChannelR
				},
				new SliderBox()
				{
					Min = 0f, Max = 255f, Height = 47f,
					UpdateValueCallback = UpdateChannelG
				},
				new SliderBox()
				{
					Min = 0f, Max = 255f, Height = 47f,
					UpdateValueCallback = UpdateChannelB
				}
			};

			colorSliderColumn = new HudChain<HudElementContainer<SliderBox>, SliderBox>(true)
			{
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				Width = 231f,
				Spacing = 5f,
				CollectionContainer =
				{
					{ sliders[0], 1f },
					{ sliders[1], 1f },
					{ sliders[2], 1f }
				}
			};

			colorChain = new HudChain(false)
			{
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				CollectionContainer = { { colorNameColumn, 0f }, { colorSliderColumn, 1f } }
			};

			var mainChain = new HudChain(true, this)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				Spacing = 5f,
				CollectionContainer =
				{
					{ headerChain, 0f },
					{ colorChain, 1f },
				}
			};

			Size = new Vector2(318f, 163f);
			UseCursor = true;
			ShareCursor = true;
			focusedChannel = -1;
			Value = Color.White;
			lastColor = _color;
		}

		public ColorPickerRGB() : this(null)
		{ }

		/// <summary>
		/// Sets input focus to the slider corresponding to the given color channel index.
		/// </summary>
		/// <param name="channel">The index of the slider to focus (0 to 2).</param>
		public void SetChannelFocused(int channel)
		{
			channel = MathHelper.Clamp(channel, 0, 2);

			if (!sliders[channel].FocusHandler.HasFocus)
				focusedChannel = channel;
		}

		/// <summary>
		/// Updates the Red channel value and display when the first slider changes.
		/// </summary>
		/// <exclude/>
		protected virtual void UpdateChannelR(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_color.R = (byte)Math.Round(slider.Value);
			sliderText[0].TextBoard.SetText($"R: {_color.R}");
			display.Color = _color;
		}

		/// <summary>
		/// Updates the Green channel value and display when the second slider changes.
		/// </summary>
		/// <exclude/>
		protected virtual void UpdateChannelG(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_color.G = (byte)Math.Round(slider.Value);
			sliderText[1].TextBoard.SetText($"G: {_color.G}");
			display.Color = _color;
		}

		/// <summary>
		/// Updates the Blue channel value and display when the third slider changes.
		/// </summary>
		/// <exclude/>
		protected virtual void UpdateChannelB(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_color.B = (byte)Math.Round(slider.Value);
			sliderText[2].TextBoard.SetText($"B: {_color.B}");
			display.Color = _color;
		}

		/// <summary>
		/// Updates input handling for the picker.
		/// <para>Manages keyboard navigation (Up/Down arrows) between sliders and triggers value change events.</para>
		/// </summary>
		/// <param name="cursorPos">The current position of the cursor.</param>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (_color != lastColor)
			{
				ValueChanged?.Invoke(this, EventArgs.Empty);
				lastColor = _color;
			}

			if (focusedChannel != -1)
			{
				sliders[focusedChannel].FocusHandler.GetInputFocus();
				focusedChannel = -1;
			}

			for (int i = 0; i < sliders.Length; i++)
			{
				if (sliders[i].FocusHandler.HasFocus)
				{
					if (SharedBinds.UpArrow.IsNewPressed)
					{
						i = MathHelper.Clamp(i - 1, 0, sliders.Length - 1);
						sliders[i].FocusHandler.GetInputFocus();
					}
					else if (SharedBinds.DownArrow.IsNewPressed)
					{
						i = MathHelper.Clamp(i + 1, 0, sliders.Length - 1);
						sliders[i].FocusHandler.GetInputFocus();
					}

					break;
				}
			}
		}
	}
}