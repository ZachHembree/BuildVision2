using System;
using System.Text;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    using UI;

    /// <summary>
    /// Named color picker using sliders designed to mimic the appearance of the color picker in the SE terminal.
    /// HSV only. Alpha not supported.
    /// </summary>
    public class ColorPickerHSV : ColorPickerRGB
    {
        protected override void Layout()
        {
            _color = new Color()
            {
                R = (byte)Math.Round(sliders[0].Current),
                G = (byte)Math.Round(sliders[1].Current),
                B = (byte)Math.Round(sliders[2].Current),
                A = 255
            };

            valueBuilder.Clear();
            valueBuilder.Append("H: ");
            valueBuilder.Append(_color.R);
            sliderText[0].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("S: ");
            valueBuilder.Append(_color.G);
            sliderText[1].TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("V: ");
            valueBuilder.Append(_color.B);
            sliderText[2].TextBoard.SetText(valueBuilder);

            display.Color = (_color / new Vector3(360f, 100f, 100f)).HSVtoColor();
        }
    }
}