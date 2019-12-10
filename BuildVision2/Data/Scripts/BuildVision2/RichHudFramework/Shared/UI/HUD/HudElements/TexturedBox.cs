using DarkHelmet.UI.Rendering;
using VRageMath;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Creates a colored box of a given width and height using a given material. The default material is just a plain color.
    /// </summary>
    public class TexturedBox : PaddedElementBase
    {
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }
        public override float Width { get { return hudBoard.Width + Padding.X; } set {hudBoard.Width = value - Padding.X; } }
        public override float Height { get { return hudBoard.Height + Padding.Y; } set { hudBoard.Height = value - Padding.Y; } }
        public override Vector2 Offset { get { return hudBoard.offset; } set { hudBoard.offset = value; } }

        protected readonly MatBoard hudBoard;

        public TexturedBox(IHudParent parent = null) : base(parent)
        {
            hudBoard = new MatBoard();
        }

        protected override void Draw()
        {
            if (Color.A > 0)
            {
                hudBoard.Draw(Origin);
            }
        }
    }
}
