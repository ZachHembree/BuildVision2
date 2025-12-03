using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A UI element that renders a textured rectangle. Supports coloring, transparency, 
	/// texture alignment/scaling, and masking.
	/// </summary>
	public class TexturedBox : HudElementBase
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
		/// The internal billboard logic used to render the textured quad.
		/// </summary>
		/// <exclude/>
		protected readonly MatBoard hudBoard;

		public TexturedBox(HudParentBase parent) : base(parent)
		{
			hudBoard = new MatBoard();
			Size = new Vector2(50f);
		}

		public TexturedBox() : this(null)
		{ }

		/// <summary>
		/// Renders the textured quad within the element's bounds, applying any active masking.
		/// </summary>
		/// <exclude/>
		protected override void Draw()
		{
			if (hudBoard.Color.A > 0)
			{
				CroppedBox box = default(CroppedBox);
				Vector2 halfSize = (UnpaddedSize) * .5f;

				box.bounds = new BoundingBox2(Position - halfSize, Position + halfSize);
				box.mask = MaskingBox;
				hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);
			}
		}
	}
}