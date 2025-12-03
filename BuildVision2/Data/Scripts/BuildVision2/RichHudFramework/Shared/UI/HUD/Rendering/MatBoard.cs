using System;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		namespace Rendering
		{
			/// <summary>
			/// A high-level wrapper for drawing textured, colored, and optionally clipped rectangular billboards.
			/// Utilizes <see cref="VRage.Game.MyTransparentGeometry"/> for rendering.
			/// </summary>
			public class MatBoard
			{
				/// <summary>
				/// The color tint applied to the material.
				/// </summary>
				public Color Color
				{
					get { return color; }
					set
					{
						if (value != color)
							minBoard.materialData.bbColor = value.GetBbColor();

						color = value;
					}
				}

				/// <summary>
				/// The texture or sprite atlas region applied to the billboard.
				/// </summary>
				public Material Material
				{
					get { return matFrame.Material; }
					set
					{
						if (value != matFrame.Material)
						{
							bbAspect = -1f;
							matFrame.Material = value;
							minBoard.materialData.textureID = value.TextureID;
						}
					}
				}

				/// <summary>
				/// Determines how the texture is scaled or cropped relative to the billboard's dimensions.
				/// </summary>
				public MaterialAlignment MatAlignment
				{
					get { return matFrame.Alignment; }
					set
					{
						if (value != matFrame.Alignment)
						{
							bbAspect = -1f;
							matFrame.Alignment = value;
						}
					}
				}

				private Color color;
				private float bbAspect;
				private Vector2 matScale;

				private QuadBoard minBoard;
				private readonly MaterialFrame matFrame;

				/// <summary>
				/// Initializes a new MatBoard with zero size and a default white material.
				/// </summary>
				public MatBoard()
				{
					matFrame = new MaterialFrame();
					minBoard = QuadBoard.Default;

					color = Color.White;
					bbAspect = -1f;
				}

				/// <summary>
				/// Renders the billboard in world space using a pre-calculated quad.
				/// </summary>
				public void Draw(ref MyQuadD quad)
				{
					BillBoardUtils.AddQuad(ref minBoard.materialData, ref quad);
				}

				/// <summary>
				/// Renders the billboard in world space, facing the +Z direction of the provided matrix.
				/// Handles masking (cropping) if a mask is provided in the <paramref name="box"/>.
				/// </summary>
				/// <param name="box">Defines the bounds and the optional mask for the billboard.</param>
				/// <param name="matrixRef">Matrix defining position and orientation.</param>
				public void Draw(ref CroppedBox box, MatrixD[] matrixRef)
				{
					bool isDisjoint = false;

					// Check if the billboard is completely outside the mask area
					if (box.mask != null)
					{
						isDisjoint =
							(box.bounds.Max.X < box.mask.Value.Min.X) ||
							(box.bounds.Min.X > box.mask.Value.Max.X) ||
							(box.bounds.Max.Y < box.mask.Value.Min.Y) ||
							(box.bounds.Min.Y > box.mask.Value.Max.Y);
					}

					if (!isDisjoint)
					{
						if (matFrame.Material != Material.Default)
						{
							Vector2 boxSize = box.bounds.Size;
							float newAspect = (boxSize.X / boxSize.Y);

							if (Math.Abs(bbAspect - newAspect) > 1E-5f)
							{
								bbAspect = newAspect;
								minBoard.materialData.texBounds = Material.UVBounds;

								// Calculate scaling required to fit the material to the billboard
								if (matFrame.Alignment != MaterialAlignment.StretchToFit)
									matScale = matFrame.GetAlignmentScale(bbAspect);
							}

							if (matFrame.Alignment != MaterialAlignment.StretchToFit)
								box.bounds.Scale(matScale);
						}

						FlatQuad quad = new FlatQuad()
						{
							Point0 = box.bounds.Max,
							Point1 = new Vector2(box.bounds.Max.X, box.bounds.Min.Y),
							Point2 = box.bounds.Min,
							Point3 = new Vector2(box.bounds.Min.X, box.bounds.Max.Y),
						};

						// Apply skew (rhombus effect) if defined
						if (minBoard.skewRatio != 0f)
						{
							Vector2 start = quad.Point0, end = quad.Point3,
								offset = (end - start) * minBoard.skewRatio * .5f;

							quad.Point0 = Vector2.Lerp(start, end, minBoard.skewRatio) - offset;
							quad.Point3 = Vector2.Lerp(start, end, 1f + minBoard.skewRatio) - offset;
							quad.Point1 -= offset;
							quad.Point2 -= offset;
						}

						BillBoardUtils.AddQuad(ref quad, ref minBoard.materialData, matrixRef, box.mask);
					}
				}
			}
		}
	}
}