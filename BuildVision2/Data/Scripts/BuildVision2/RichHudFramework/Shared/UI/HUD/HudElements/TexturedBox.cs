using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Creates a colored box of a given width and height using a given material. The default material is just a plain color.
    /// </summary>
    public class TexturedBox : HudElementBase
    {
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }

        public override float Width
        {
            get { return hudBoard.Width + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                hudBoard.Width = value;
            }
        }

        public override float Height
        {
            get { return hudBoard.Height + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                hudBoard.Height = value;
            }
        }

        public override Vector2 Offset { get { return hudBoard.offset; } set { hudBoard.offset = value; } }

        private float lastScale;
        protected readonly MatBoard hudBoard;

        public TexturedBox(IHudParent parent = null) : base(parent)
        {
            hudBoard = new MatBoard();
            lastScale = Scale;
        }

        protected override void Draw()
        {
            if (Scale != lastScale)
            {
                hudBoard.Size *= Scale / lastScale;
                Offset *= Scale / lastScale;
                lastScale = Scale;
            }

            if (Color.A > 0)
            {
                hudBoard.Draw(Origin);
            }
        }
    }
}
