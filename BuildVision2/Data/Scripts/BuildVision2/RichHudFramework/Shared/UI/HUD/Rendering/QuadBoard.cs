using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		namespace Rendering
		{
			/// <summary>
			/// Represents a bounding box paired with a masking box. The mask is used to clip
			/// the rendering of the associated billboard, acting as a scissor rectangle.
			/// </summary>
			public struct CroppedBox
			{
				/// <summary>
				/// Represents an infinite bounding box, effectively disabling clipping when used as a mask.
				/// </summary>
				public static readonly BoundingBox2 defaultMask =
					new BoundingBox2(-Vector2.PositiveInfinity, Vector2.PositiveInfinity);

				/// <summary>
				/// The geometric bounds of the UI element to be drawn.
				/// </summary>
				public BoundingBox2 bounds;

				/// <summary>
				/// The bounds used to clip the element. Pixels outside this box will not be rendered.
				/// If null, no clipping is applied.
				/// </summary>
				public BoundingBox2? mask;
			}

			/// <summary>
			/// Contains the final geometry and material data for a <see cref="QuadBoard"/>, 
			/// ready to be submitted to the renderer.
			/// </summary>
			public struct QuadBoardData
			{
				public BoundedQuadMaterial material;
				public MyQuadD positions;
			}

			/// <summary>
			/// A container struct associating a <see cref="QuadBoard"/> definition with specific 2D bounds.
			/// </summary>
			public struct BoundedQuadBoard
			{
				public BoundingBox2 bounds;
				public QuadBoard quadBoard;
			}

			/// <summary>
			/// Defines the visual properties of a rectangular billboard, including its texture, color, 
			/// texture coordinates, and skew.
			/// </summary>
			public struct QuadBoard
			{
				/// <summary>
				/// A default white, untextured quad definition.
				/// </summary>
				public static readonly QuadBoard Default;

				/// <summary>
				/// Defines the horizontal skew applied to the quad, transforming the rectangle into a parallelogram (rhombus).
				/// Useful for italicized styling or oblique geometric shapes.
				/// </summary>
				public float skewRatio;

				/// <summary>
				/// Contains the underlying material data, including the Texture ID, UV bounds, and color tint.
				/// </summary>
				public BoundedQuadMaterial materialData;

				static QuadBoard()
				{
					var matFit = new BoundingBox2(new Vector2(0f, 0f), new Vector2(1f, 1f));
					Default = new QuadBoard(Material.Default.TextureID, matFit, Color.White);
				}

				public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Vector4 bbColor, float skewRatio = 0f)
				{
					materialData.textureID = textureID;
					materialData.texBounds = matFit;
					materialData.bbColor = bbColor;
					this.skewRatio = skewRatio;
				}

				public QuadBoard(MyStringId textureID, BoundingBox2 matFit, Color color, float skewRatio = 0f)
				{
					materialData.textureID = textureID;
					materialData.texBounds = matFit;
					materialData.bbColor = color.GetBbColor();
					this.skewRatio = skewRatio;
				}
			}
		}
	}
}