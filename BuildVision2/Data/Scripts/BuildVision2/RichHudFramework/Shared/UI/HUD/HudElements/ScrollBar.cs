using VRageMath;

namespace RichHudFramework.UI
{
    public class ScrollBar : HudElementBase
    {
        public override float Width
        {
            get { return slide.Width + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                slide.bar.Width = value;
                slide.button.Width = value;
            }
        }

        public override float Height
        {
            get { return slide.Height + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                slide.bar.Height = value;
                slide.button.Height = value;
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
        public bool Vertical { get { return slide.Vertical; } set { slide.Vertical = value; slide.Reverse = value; } }

        public readonly SliderBar slide;

        public ScrollBar(IHudParent parent = null) : base(parent)
        {
            slide = new SliderBar(this) { Reverse = true, Vertical = true };
            slide.button.Width = 12f;
            slide.button.Color = new Color(103, 109, 124);

            slide.bar.Width = 12f;
            slide.bar.Color = new Color(41, 51, 61);
            slide.bar.highlightEnabled = false;

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }

        protected override void Draw()
        {
            if (Vertical)
            {
                slide.button.Width = slide.bar.Width;
            }
            else
            {
                slide.bar.Height = slide.button.Height;
            }
        }
    }    
}