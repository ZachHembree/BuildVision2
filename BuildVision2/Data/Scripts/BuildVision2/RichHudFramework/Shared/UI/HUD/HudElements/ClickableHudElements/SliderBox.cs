using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Horizontal slider designed to mimic the appearance of the slider in the SE terminal.
    /// </summary>
    public class SliderBox : HudElementBase
    {
        public override float Width
        {
            get { return background.Width; }
            set
            {
                background.Width = value;

                if (value > Padding.X)
                    value -= Padding.X;

                slide.Width = value;
            }
        }
        public override float Height
        {
            get { return background.Height; }
            set
            {
                background.Height = value;

                if (value > Padding.Y)
                    value -= Padding.Y;

                slide.Height = value;
            }
        }

        public float Min { get { return slide.Min; } set { slide.Min = value; } }
        public float Max { get { return slide.Max; } set { slide.Max = value; } }
        public float Current { get { return slide.Current; } set { slide.Current = value; } }
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        private readonly TexturedBox background;
        private readonly BorderBox border;
        private readonly SliderBar slide;

        public SliderBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            { Color = new Color(41, 54, 62) };

            border = new BorderBox(background)
            { Color = new Color(53, 66, 75), Thickness = 1f, DimAlignment = DimAlignments.Both, };

            slide = new SliderBar(background);

            slide.button.Size = new Vector2(14f, 28f);
            slide.button.Color = new Color(103, 109, 124);

            slide.bar.Height = 5f;
            slide.bar.Color = new Color(103, 109, 124);

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }
    }
}