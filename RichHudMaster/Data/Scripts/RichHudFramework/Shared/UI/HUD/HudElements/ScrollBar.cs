using VRageMath;

namespace DarkHelmet.UI
{
    public class ScrollBar : PaddedElementBase
    {
        public override float Width
        {
            set
            {
                base.Width = value;
                slide.Width = value - Padding.X;
            }
        }
        public override float Height
        {
            set
            {
                base.Height = value;
                slide.Height = value - Padding.Y;
            }
        }
        public override Vector2 Padding
        {
            set
            {
                slide.Size = Size - value;
                base.Padding = value;
            }
        }

        public float Min
        {
            get { return slide.Min; }
            set { slide.Min = value; }
        }
        public float Max
        {
            get { return slide.Max; }
            set { slide.Max = value; }
        }

        public float Current { get { return slide.Current; } set { slide.Current = value; } }
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        public readonly SliderBar slide;

        public ScrollBar(IHudParent parent = null) : base(parent)
        {
            slide = new SliderBar(this, true) { Reverse = true };
            slide.button.Width = 12f;
            slide.button.Color = new Color(103, 109, 124);

            slide.bar.Width = 12f;
            slide.bar.Color = new Color(41, 51, 61);
            slide.bar.highlightEnabled = false;

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }
    }    
}