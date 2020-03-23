using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Creates a colored box of a given width and height using a given material. The default material is just a plain color.
    /// </summary>
    public class TexturedBox : HudElementBase
    {
        /// <summary>
        /// Material applied to the box.
        /// </summary>
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }

        /// <summary>
        /// Determines how the material reacts to changes in element size/aspect ratio.
        /// </summary>
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }

        /// <summary>
        /// Coloring applied to the material.
        /// </summary>
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }

        /// <summary>
        /// Width of the hud element in pixels.
        /// </summary>
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

        /// <summary>
        /// Height of the hud element in pixels.
        /// </summary>
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

        private float lastScale;
        private readonly MatBoard hudBoard;

        public TexturedBox(IHudParent parent = null) : base(parent)
        {
            hudBoard = new MatBoard();
            lastScale = Scale;
        }

        protected override void Layout()
        {
            if (Scale != lastScale)
            {
                hudBoard.Size *= Scale / lastScale;
                Offset *= Scale / lastScale;
                lastScale = Scale;
            }
        }

        protected override void Draw()
        {
            if (Color.A > 0)
            {
                hudBoard.Draw(cachedPosition);
            }
        }
    }
}
