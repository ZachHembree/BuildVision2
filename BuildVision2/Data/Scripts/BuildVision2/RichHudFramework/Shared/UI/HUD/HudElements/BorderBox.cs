using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A UI element that renders a textured frame. Supports coloring, transparency, 
	/// texture alignment/scaling, and masking.
	/// </summary>
	public class BorderBox : HudElementBase
    {
		/// <summary>
		/// Gets or sets the texture material applied to the background of this element.
		/// </summary>
		public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }

		/// <summary>
		/// Determines how the texture is scaled and positioned within the element's bounds 
		/// (e.g., stretch to fit, preserve aspect ratio, etc.).
		/// </summary>
		public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }

		/// <summary>
		/// Gets or sets the tint color applied to the texture. 
		/// <para>Note: Alpha affects opacity.</para>
		/// </summary>
		public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }

        /// <summary>
        /// Size of the border on all four sides in
        /// </summary>
        public float Thickness { get; set; }

		/// <summary>
		/// The internal billboard logic used to render the textured quad.
		/// </summary>
		/// <exclude/>
		protected readonly MatBoard hudBoard;

		public BorderBox(HudParentBase parent) : base(parent)
        {
            hudBoard = new MatBoard();
            Thickness = 1f;
        }

        public BorderBox() : this(null)
        { }

        /// <summary>
        /// Renders the frame using four textured quads
        /// </summary>
        /// <exclude/>
        protected override void Draw()
        {
            if (Color.A > 0)
            {
                CroppedBox box = default(CroppedBox);
                box.mask = MaskingBox;

                float height = UnpaddedSize.Y, 
                    width = UnpaddedSize.X;
                Vector2 halfSize, pos;

                // Left
                halfSize = new Vector2(Thickness, height) * .5f;
                pos = Position + new Vector2((-width + Thickness) * .5f, 0f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Top
                halfSize = new Vector2(width, Thickness) * .5f;
                pos = Position + new Vector2(0f, (height - Thickness) * .5f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Right
                halfSize = new Vector2(Thickness, height) * .5f;
                pos = Position + new Vector2((width - Thickness) * .5f, 0f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

                // Bottom
                halfSize = new Vector2(width, Thickness) * .5f;
                pos = Position + new Vector2(0f, (-height + Thickness) * .5f);
                box.bounds = new BoundingBox2(pos - halfSize, pos + halfSize);
                hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);
            }
        }
    }
}
