using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;
    using Rendering.Client;
    using Rendering.Server;

    public class Label : HudElementBase
    {
        public RichText Text { get { return textBoard.GetText(); } set { textBoard.SetText(value); } }
        public ITextBoard TextBoard => textBoard;
        public GlyphFormat Format { get { return textBoard.Format; } set { textBoard.Format = value; } }

        public override float Width
        {
            get { return (AutoResize ? textBoard.Size.X : textBoard.FixedSize.X) + Padding.X; }
            set { textBoard.FixedSize = new Vector2(value - Padding.X, textBoard.FixedSize.Y); }
        }

        public override float Height
        {
            get { return (AutoResize ? textBoard.Size.Y : textBoard.FixedSize.Y) + Padding.Y; }
            set { textBoard.FixedSize = new Vector2(textBoard.FixedSize.X, value - Padding.Y); }
        }

        public override Vector2 Padding
        {
            get { return base.Padding; }
            set
            {
                textBoard.FixedSize += base.Padding - value;
                base.Padding = value;
            }
        }

        public TextBuilderModes BuilderMode { get { return textBoard.BuilderMode; } set { textBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize { get { return textBoard.AutoResize; } set { textBoard.AutoResize = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return textBoard.VertCenterText; } set { textBoard.VertCenterText = value; } }

        protected readonly TextBoard textBoard;

        public Label(IHudParent parent = null) : base(parent)
        {
            textBoard = new TextBoard();
        }

        protected override void Draw()
        {
            textBoard.Draw(Origin + Offset);
        }

        protected override void ScaleChanged(float change) =>
            textBoard.Scale = Scale;
    }
}
