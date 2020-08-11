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

        protected float lastScale;
        protected readonly MatBoard hudBoard;

        public TexturedBox(HudParentBase parent = null) : base(parent)
        {
            hudBoard = new MatBoard();
            lastScale = Scale;
        }

        protected override void Layout()
        {
            hudBoard.Size = cachedSize - cachedPadding;
        }

        protected override void Draw(object planeToWorld)
        {
            if (hudBoard.Color.A > 0)
            {
                var matrix = (MatrixD)planeToWorld;
                hudBoard.Draw(cachedPosition, ref matrix);
            }
        }
    }
}
