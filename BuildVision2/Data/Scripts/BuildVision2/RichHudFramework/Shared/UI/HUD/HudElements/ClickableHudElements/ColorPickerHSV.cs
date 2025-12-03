using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A named color picker using sliders designed to mimic the appearance of the Space Engineers terminal color picker.
	/// <para>Operating in HSV mode (Hue, Saturation, Value). Alpha (transparency) is not supported.</para>
	/// </summary>
	public class ColorPickerHSV : ColorPickerRGB
	{
		/// <exclude/>
		protected static readonly Vector3 HSVScale = new Vector3(360f, 100f, 100f);
		/// <exclude/>
		protected static readonly Vector3 RcpHSVScale = 1f / new Vector3(360f, 100f, 100f);

		/// <summary>
		/// Gets or sets the color currently specified by the picker.
		/// Setting this value will automatically update the positions of the sliders.
		/// </summary>
		public override Color Color
		{
			get { return _color; }
			set { ColorHSV = value.ColorToHSV() * HSVScale; }
		}

		/// <summary>
		/// Gets or sets the currently selected color in HSV format. 
		/// Setting this value will automatically update the positions of the sliders.
		/// <para>X = Hue (0-360), Y = Saturation (0-100), Z = Value (0-100).</para>
		/// </summary>
		public Vector3 ColorHSV
		{
			get { return _hsvColor; }
			set
			{
				sliders[0].Current = value.X;
				sliders[1].Current = value.Y;
				sliders[2].Current = value.Z;
				_hsvColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the currently selected color in normalized HSV format.
		/// Setting this value will automatically update the positions of the sliders.
		/// <para>X = Hue (0-1), Y = Saturation (0-1), Z = Value (0-1).</para>
		/// </summary>
		public Vector3 ColorHSVNorm
		{
			get { return _hsvColor * RcpHSVScale; }
			set { ColorHSV = value * HSVScale; }
		}

		/// <exclude/>
		protected Vector3 _hsvColor;

		public ColorPickerHSV(HudParentBase parent = null) : base(parent)
		{
			sliders[0].Max = 360f;
			sliders[1].Max = 100f;
			sliders[2].Max = 100f;
		}

		/// <summary>
		/// Updates the Hue value and display when the first slider (channel R) changes.
		/// </summary>
		/// <exclude/>
		protected override void UpdateChannelR(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_hsvColor.X = (float)Math.Round(slider.Current);
			sliderText[0].TextBoard.SetText($"H: {_hsvColor.X}");

			_color = (_hsvColor * RcpHSVScale).HSVtoColor();
			display.Color = _color;
		}

		/// <summary>
		/// Updates the Saturation value and display when the second slider (channel G) changes.
		/// </summary>
		/// <exclude/>
		protected override void UpdateChannelG(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_hsvColor.Y = (float)Math.Round(slider.Current);
			sliderText[1].TextBoard.SetText($"S: {_hsvColor.Y}");

			_color = (_hsvColor * RcpHSVScale).HSVtoColor();
			display.Color = _color;
		}

		/// <summary>
		/// Updates the Value (brightness) and display when the third slider (channel B) changes.
		/// </summary>
		/// <exclude/>
		protected override void UpdateChannelB(object sender, EventArgs args)
		{
			var slider = sender as SliderBox;
			_hsvColor.Z = (float)Math.Round(slider.Current);
			sliderText[2].TextBoard.SetText($"V: {_hsvColor.Z}");

			_color = (_hsvColor * RcpHSVScale).HSVtoColor();
			display.Color = _color;
		}
	}
}