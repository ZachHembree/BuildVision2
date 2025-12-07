using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		namespace Rendering
		{
			/// <summary>
			/// Read-only interface defining the mapping of a material to a quad.
			/// </summary>
			public interface IReadOnlyMaterialFrame
			{
				/// <summary>
				/// The texture and sprite data associated with the frame.
				/// </summary>
				Material Material { get; }

				/// <summary>
				/// The alignment mode determining how the material is scaled or cropped to fit the element's aspect ratio.
				/// </summary>
				MaterialAlignment Alignment { get; }

				/// <summary>
				/// Calculates the UV bounds required to align the material based on the given aspect ratio.
				/// </summary>
				BoundingBox2 GetMaterialAlignment(float bbAspectRatio);

				/// <summary>
				/// Calculates the geometric scaling required to maintain the material's aspect ratio.
				/// </summary>
				Vector2 GetAlignmentScale(float bbAspectRatio);
			}

			/// <summary>
			/// Manages the positioning, scaling, and alignment of a <see cref="Material"/> on a QuadBoard.
			/// Handles calculations for aspect-ratio preservation via UV cropping or geometry scaling.
			/// </summary>
			public class MaterialFrame : IReadOnlyMaterialFrame
			{
				/// <summary>
				/// The texture and sprite data associated with the frame.
				/// </summary>
				public Material Material { get; set; }

				/// <summary>
				/// The alignment mode determining how the material is scaled or cropped to fit the element's aspect ratio.
				/// </summary>
				public MaterialAlignment Alignment { get; set; }

				public MaterialFrame()
				{
					Material = Material.Default;
					Alignment = MaterialAlignment.StretchToFit;
				}

				/// <summary>
				/// Calculates the modified UV (texture) coordinates needed to fit the material to the billboard 
				/// according to the specified <see cref="Alignment"/>.
				/// </summary>
				/// <param name="bbAspectRatio">The aspect ratio (Width/Height) of the target billboard.</param>
				/// <returns>A BoundingBox2 representing the Min/Max UV coordinates.</returns>
				public BoundingBox2 GetMaterialAlignment(float bbAspectRatio)
				{
					BoundingBox2 bounds = Material.UVBounds;

					if (Alignment != MaterialAlignment.StretchToFit)
					{
						Vector2 uvScale = new Vector2(1f);
						float matAspectRatio = Material.Size.X / Material.Size.Y;

						if (Alignment == MaterialAlignment.FitAuto)
						{
							if (matAspectRatio > bbAspectRatio) // Material is wider than target; crop width (U)
								uvScale = new Vector2(1f, matAspectRatio / bbAspectRatio);
							else // Material is taller than target; crop height (V)
								uvScale = new Vector2(bbAspectRatio / matAspectRatio, 1f);
						}
						else if (Alignment == MaterialAlignment.FitVertical)
						{
							uvScale = new Vector2(bbAspectRatio / matAspectRatio, 1f);
						}
						else if (Alignment == MaterialAlignment.FitHorizontal)
						{
							uvScale = new Vector2(1f, matAspectRatio / bbAspectRatio);
						}

						bounds.Scale(uvScale);
					}

					return bounds;
				}

				/// <summary>
				/// Calculates the scaling vector required to resize a billboard to match the material's aspect ratio.
				/// Used when the geometry itself should change size rather than cropping the texture.
				/// </summary>
				/// <param name="bbAspectRatio">The current aspect ratio of the billboard.</param>
				/// <returns>A scalar vector to apply to the billboard dimensions.</returns>
				public Vector2 GetAlignmentScale(float bbAspectRatio)
				{
					if (Alignment != MaterialAlignment.StretchToFit)
					{
						float matAspectRatio = Material.Size.X / Material.Size.Y;
						Vector2 bbScale = new Vector2(1f);

						if (Alignment == MaterialAlignment.FitAuto)
						{
							if (matAspectRatio < bbAspectRatio)
								bbScale = new Vector2(matAspectRatio / bbAspectRatio, 1f);
							else
								bbScale = new Vector2(1f, bbAspectRatio / matAspectRatio);
						}
						else if (Alignment == MaterialAlignment.FitHorizontal)
						{
							bbScale = new Vector2(1f, bbAspectRatio / matAspectRatio);
						}
						else if (Alignment == MaterialAlignment.FitVertical)
						{
							bbScale = new Vector2(matAspectRatio / bbAspectRatio, 1f);
						}

						return bbScale;
					}
					else
						return Vector2.One;
				}
			}
		}
	}
}