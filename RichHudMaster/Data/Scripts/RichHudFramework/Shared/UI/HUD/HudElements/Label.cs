using DarkHelmet.UI.Rendering;
using VRageMath;

namespace DarkHelmet.UI
{
    using Rendering;
    using Rendering.Client;
    using Rendering.Server;

    public class Label : PaddedElementBase
    {
        public ITextBoard Text => text;
        public GlyphFormat Format { get { return text.Format; } set { text.Format = value; } }

        public override float Width
        {
            get { return (AutoResize ? text.Size.X : text.FixedSize.X) + Padding.X; }
            set { text.FixedSize = new Vector2(value - Padding.X, text.FixedSize.Y); }
        }

        public override float Height
        {
            get { return (AutoResize ? text.Size.Y : text.FixedSize.Y) + Padding.Y; }
            set { text.FixedSize = new Vector2(text.FixedSize.X, value - Padding.Y); }
        }

        public override Vector2 Padding
        {
            get { return base.Padding; }
            set
            {
                text.FixedSize += base.Padding - value;
                base.Padding = value;
            }
        }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize { get { return text.AutoResize; } set { text.AutoResize = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return text.VertCenterText; } set { text.VertCenterText = value; } }

        protected readonly TextBoard text;

        public Label(RichText text, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            Text.Append(text);
        }

        public Label(RichString text, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            Text.Append(text);
        }

        public Label(string text, GlyphFormat format, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            Text.Append(new RichString(text, format));
        }

        public Label(IHudParent parent = null, bool wordWrapping = false) : base(parent)
        {
            text = new TextBoard(wordWrapping);
        }

        protected override void Draw()
        {
            text.Draw(Origin + Offset);
        }

        protected override void ScaleChanged(float change) =>
            text.Scale = Scale;
    }
}
