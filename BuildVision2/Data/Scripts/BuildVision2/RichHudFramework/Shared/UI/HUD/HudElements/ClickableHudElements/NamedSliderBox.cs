using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// Horizontal sliderbox with a name and value label. Value label is not updated automatically.
    /// </summary>
    public class NamedSliderBox : HudElementBase
    {
        /// <summary>
        /// The name of the control as rendred in the terminal.
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Text indicating the current value of the slider. Does not automatically reflect changes to the slider value.
        /// </summary>
        public RichText ValueText { get { return current.TextBoard.ToString(); } set { current.TextBoard.SetText(value); } }

        /// <summary>
        /// Minimum configurable value for the slider.
        /// </summary>
        public float Min { get { return sliderBox.Min; } set { sliderBox.Min = value; } }

        /// <summary>
        /// Maximum configurable value for the slider.
        /// </summary>
        public float Max { get { return sliderBox.Max; } set { sliderBox.Max = value; } }

        /// <summary>
        /// Value currently set on the slider.
        /// </summary>
        public float Current { get { return sliderBox.Current; } set { sliderBox.Current = value; } }

        /// <summary>
        /// Current slider value expreseed as a percentage between the min and maximum values.
        /// </summary>
        public float Percent { get { return sliderBox.Percent; } set { sliderBox.Percent = value; } }

        public override float Width
        {
            get { return sliderBox.Width + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                sliderBox.Width = value;
            }
        }

        public override float Height
        {
            get { return sliderBox.Height + Math.Max(name.Height, current.Height) + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                sliderBox.Height = value - Math.Max(name.Height, current.Height);
            }
        }

        protected readonly Label name, current;
        protected readonly SliderBox sliderBox;

        public NamedSliderBox(HudParentBase parent = null) : base(parent)
        {
            sliderBox = new SliderBox(this)
            {
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                UseCursor = true,
            };

            name = new Label(this)
            {
                Format = TerminalFormatting.ControlFormat,
                Text = "NewSlideBox",
                Offset = new Vector2(0f, -18f),
                ParentAlignment = ParentAlignments.InnerH | ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.UsePadding
            };

            current = new Label(this)
            {
                Format = TerminalFormatting.ControlFormat,
                Text = "Value",
                Offset = new Vector2(0f, -18f),
                ParentAlignment = ParentAlignments.InnerH | ParentAlignments.Top | ParentAlignments.Right | ParentAlignments.UsePadding
            };

            Padding = new Vector2(40f, 0f);
        }
    }
}