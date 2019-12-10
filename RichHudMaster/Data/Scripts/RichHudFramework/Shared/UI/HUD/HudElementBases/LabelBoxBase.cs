using System;
using VRage;
using VRageMath;

namespace DarkHelmet
{
    namespace UI
    {
        using Client;
        using Server;

        /// <summary>
        /// Base type for hud elements that have text elements and a <see cref="TexturedBox"/> background.
        /// </summary>
        public abstract class LabelBoxBase : PaddedElementBase
        {
            public override float Width
            {
                get { return TextSize.X + Padding.X; }
                set
                {
                    TextSize = new Vector2(value - Padding.X, TextSize.Y);
                    background.Width = value;
                }
            }

            public override float Height
            {
                get { return TextSize.Y + Padding.Y; }
                set
                {
                    TextSize = new Vector2(TextSize.X, value - Padding.Y);
                    background.Height = value;
                }
            }

            public abstract Vector2 TextSize { get; protected set; }
            public abstract bool AutoResize { get; set; }

            public virtual Color Color { get { return background.Color; } set { background.Color = value; } }
            public IHudElement Background => background;

            protected readonly TexturedBox background;

            public LabelBoxBase(IHudParent parent) : base(parent)
            {
                background = new TexturedBox(this);
            }

            protected override void ScaleChanged(float change)
            {
                base.ScaleChanged(change);
                Padding *= change;
            }

            protected override void Draw()
            {
                if (background.Width != Width)
                    background.Width = Width;

                if (background.Height != Height)
                    background.Height = Height;
            }
        }
    }
}