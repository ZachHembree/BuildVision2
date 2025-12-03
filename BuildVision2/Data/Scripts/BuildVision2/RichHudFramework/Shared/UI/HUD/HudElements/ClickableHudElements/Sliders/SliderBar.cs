using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A clickable slider bar consisting of a track (Bar) and a movable thumb (Slider), based on <see cref="MouseInputElement"/>. 
	/// It can be oriented vertically or horizontally, and the current value is automatically clamped between min and max.
	/// <para>
	/// Size is determined by the slider and bar, not <see cref="HudElementBase.Size"/> or <see cref="HudElementBase.UnpaddedSize"/>.
	/// </para>
	/// </summary>
	public class SliderBar : MouseInputElement, IClickableElement
	{
		/// <summary>
		/// Invoked when the <see cref="Current"/> value changes.
		/// </summary>
		public event EventHandler ValueChanged;

		/// <summary>
		/// Helper property for registering a <see cref="Current"/> value update callback during initialization.
		/// </summary>
		public EventHandler UpdateValueCallback
		{
			set { ValueChanged += value; }
		}

		/// <summary>
		/// The lower limit of the value range.
		/// </summary>
		public float Min
		{
			get { return _min; }
			set
			{
				_min = value;

				if (_max > _min)
					Percent = (_current - _min) / (_max - _min);
				else
					Percent = 0;
			}
		}

		/// <summary>
		/// The upper limit of the value range.
		/// </summary>
		public float Max
		{
			get { return _max; }
			set
			{
				_max = value;

				if (_max > _min)
					Percent = (_current - _min) / (_max - _min);
				else
					Percent = 0;
			}
		}

		/// <summary>
		/// The currently selected value, bounded by the Min and Max values.
		/// </summary>
		public float Current
		{
			get { return _current; }
			set
			{
				if (_max > _min)
					Percent = (value - _min) / (_max - _min);
				else
					Percent = 0f;
			}
		}

		/// <summary>
		/// The position of the slider thumb expressed as a percentage (0 to 1). 
		/// At 0, the slider is at the minimum value; at 1, it is at the maximum.
		/// </summary>
		public float Percent
		{
			get { return _percent; }
			set
			{
				_percent = MathHelper.Clamp(value, 0f, 1f);
				_current = _percent * (_max - _min) + _min;
			}
		}

		/// <summary>
		/// If true, the slider thumb (and optionally the track bar) will change to their highlight colors when moused over.
		/// </summary>
		public bool EnableHighlight { get; set; }

		/// <summary>
		/// The color of the track bar (background).
		/// </summary>
		public Color BarColor { get; set; }

		/// <summary>
		/// The color of the track bar when moused over.
		/// </summary>
		public Color BarHighlight { get; set; }

		/// <summary>
		/// The color of the slider thumb (button) when not moused over.
		/// </summary>
		public Color SliderColor { get; set; }

		/// <summary>
		/// The color of the slider thumb (button) when moused over.
		/// </summary>
		public Color SliderHighlight { get; set; }

		/// <summary>
		/// The size of the track bar (background).
		/// </summary>
		public Vector2 BarSize
		{
			get { return _barSize; }
			set
			{
				_barSize = value;
				UnpaddedSize = Vector2.Max(_barSize, _sliderSize);
			}
		}

		/// <summary>
		/// The width of the track bar.
		/// </summary>
		public float BarWidth
		{
			get { return _barSize.X; }
			set
			{
				_barSize.X = value;
				value = Math.Max(_barSize.X, _sliderSize.X);
				UnpaddedSize = new Vector2(value, UnpaddedSize.Y);
			}
		}

		/// <summary>
		/// The height of the track bar.
		/// </summary>
		public float BarHeight
		{
			get { return _barSize.Y; }
			set
			{
				_barSize.Y = value;
				value = Math.Max(_barSize.Y, _sliderSize.Y);
				UnpaddedSize = new Vector2(UnpaddedSize.X, value);
			}
		}

		/// <summary>
		/// The size of the slider thumb (movable button).
		/// </summary>
		public Vector2 SliderSize
		{
			get { return _sliderSize; }
			set
			{
				_sliderSize = value;
				UnpaddedSize = Vector2.Max(_barSize, _sliderSize);
			}
		}

		/// <summary>
		/// The width of the slider thumb (movable button).
		/// </summary>
		public float SliderWidth
		{
			get { return _sliderSize.X; }
			set
			{
				_sliderSize.X = value;
				value = Math.Max(_barSize.X, _sliderSize.X);
				UnpaddedSize = new Vector2(value, UnpaddedSize.Y);
			}
		}

		/// <summary>
		/// The height of the slider thumb (movable button).
		/// </summary>
		public float SliderHeight
		{
			get { return _sliderSize.Y; }
			set
			{
				_sliderSize.Y = value;
				value = Math.Max(_barSize.Y, _sliderSize.Y);
				UnpaddedSize = new Vector2(UnpaddedSize.X, value);
			}
		}

		/// <summary>
		/// Determines whether the slider thumb (button) is currently visible.
		/// </summary>
		public bool SliderVisible { get; set; }

		/// <summary>
		/// If true, the slider will be oriented vertically (moves up/down). If false, it is horizontal.
		/// </summary>
		public bool Vertical { get; set; }

		/// <summary>
		/// Reverses the direction of the slider value. 
		/// <para>Normal: Left/Top is Min, Right/Bottom is Max. Reverse: Left/Top is Max, Right/Bottom is Min.</para>
		/// </summary>
		public bool Reverse { get; set; }

		/// <summary>
		/// Handles mouse input for the slider bar.
		/// </summary>
		public IMouseInput MouseInput { get; }

		/// <summary>
		/// Textured boxes for rendering the slider thumb and the track bar.
		/// </summary>
		/// <exclude/>
		protected readonly TexturedBox slider, bar;

		/// <exclude/>
		protected Vector2 _barSize, _sliderSize;

		/// <summary>
		/// Cursor position when first clicked, used to prevent the slider from 
		/// jumping when the drag begins.
		/// </summary>
		/// <exclude/>
		protected Vector2 startCursorOffset;

		/// <summary>
		/// Cursor position when the slider was last dragged. 
		/// Used for calculating movement deltas.
		/// </summary>
		/// <exclude/>
		protected Vector2 lastPos;

		/// <exclude/>
		protected float _min, _max, _current, _percent, lastValue;

		/// <exclude/>
		protected bool canMoveSlider;

		public SliderBar(HudParentBase parent) : base(parent)
		{
			bar = new TexturedBox(this);
			slider = new TexturedBox(bar) { UseCursor = true, ShareCursor = true };
			MouseInput = this;

			_barSize = new Vector2(100f, 12f);
			_sliderSize = new Vector2(6f, 12f);
			UnpaddedSize = _barSize;
			SliderVisible = true;

			bar.Size = _barSize;
			slider.Size = _sliderSize;

			SliderColor = new Color(180, 180, 180, 255);
			BarColor = new Color(140, 140, 140, 255);
			SliderHighlight = new Color(200, 200, 200, 255);
			EnableHighlight = true;

			_min = 0f;
			_max = 1f;

			lastValue = float.PositiveInfinity;
			Current = 0f;
			Percent = 0f;

			ShareCursor = false;
			UseCursor = true;
			DimAlignment = DimAlignments.None;
		}

		public SliderBar() : this(null)
		{ }

		/// <summary>
		/// Updates slider dragging logic, value calculation, and cursor sharing.
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			base.HandleInput(cursorPos);

			ShareCursor = Min == Max;

			if (!canMoveSlider && IsNewLeftClicked)
			{
				canMoveSlider = true;

				if (slider.IsMousedOver)
					startCursorOffset = cursorPos - slider.Position;
				else
					startCursorOffset = Vector2.Zero;
			}
			else if (canMoveSlider && !SharedBinds.LeftButton.IsPressed)
				canMoveSlider = false;

			if (canMoveSlider && (cursorPos - lastPos).LengthSquared() > 4f)
			{
				float minOffset, maxOffset, pos;
				lastPos = cursorPos;
				cursorPos -= startCursorOffset;

				if (Vertical)
				{
					minOffset = -((_barSize.Y - _sliderSize.Y) * .5f);
					maxOffset = -minOffset;
					pos = MathHelper.Clamp(cursorPos.Y - Origin.Y, minOffset, maxOffset);
				}
				else
				{
					minOffset = -((_barSize.X - _sliderSize.X) * .5f);
					maxOffset = -minOffset;
					pos = MathHelper.Clamp(cursorPos.X - Origin.X, minOffset, maxOffset);
				}

				if (Reverse)
					Percent = 1f - ((pos - minOffset) / (maxOffset - minOffset));
				else
					Percent = (pos - minOffset) / (maxOffset - minOffset);
			}

			_current = (float)Math.Round(_current, 6);

			if (Math.Abs(_current - lastValue) > 1e-6f)
			{
				ValueChanged?.Invoke(FocusHandler?.InputOwner ?? this, EventArgs.Empty);
				lastValue = _current;
			}
		}

		/// <summary>
		/// Updates slider visibility, highlighting colors, sizing, and the position of the thumb.
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			slider.Visible = SliderVisible;

			if (EnableHighlight && (IsMousedOver || canMoveSlider))
			{
				slider.Color = SliderHighlight;

				if (BarHighlight != default(Color))
					bar.Color = BarHighlight;
			}
			else
			{
				slider.Color = SliderColor;
				bar.Color = BarColor;
			}

			Vector2 size = UnpaddedSize;

			if (_barSize.X >= _sliderSize.X)
			{
				_barSize.X = size.X;
				_sliderSize.X = Math.Min(_sliderSize.X, _barSize.X);
			}
			else
			{
				_sliderSize.X = size.X;
				_barSize.X = Math.Min(_sliderSize.X, _barSize.X);
			}

			if (_barSize.Y >= _sliderSize.Y)
			{
				_barSize.Y = size.Y;
				_sliderSize.Y = Math.Min(_sliderSize.Y, _barSize.Y);
			}
			else
			{
				_sliderSize.Y = size.Y;
				_barSize.Y = Math.Min(_sliderSize.Y, _barSize.Y);
			}

			bar.UnpaddedSize = _barSize;
			slider.UnpaddedSize = _sliderSize;

			UpdateButtonOffset();
		}

		/// <summary>
		/// Calculates and updates the visual offset of the slider thumb based on the current Percent.
		/// </summary>
		private void UpdateButtonOffset()
		{
			if (Vertical)
			{
				if (Reverse)
					slider.Offset = new Vector2(0f, -(Percent - .5f) * (_barSize.Y - _sliderSize.Y));
				else
					slider.Offset = new Vector2(0f, (Percent - .5f) * (_barSize.Y - _sliderSize.Y));
			}
			else
			{
				if (Reverse)
					slider.Offset = new Vector2(-(Percent - .5f) * (_barSize.X - _sliderSize.X), 0f);
				else
					slider.Offset = new Vector2((Percent - .5f) * (_barSize.X - _sliderSize.X), 0f);
			}
		}
	}
}