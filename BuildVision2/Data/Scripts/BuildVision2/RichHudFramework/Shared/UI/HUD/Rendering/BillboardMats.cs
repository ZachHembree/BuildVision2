using System.Collections.Generic;
using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		namespace Rendering
		{
			/// <summary>
			/// Defines the visual properties (texture, color, and UV mapping) for rendering a single triangle billboard.
			/// </summary>
			public struct TriMaterial
			{
				public static readonly TriMaterial Default = new TriMaterial
				{
					textureID = Material.Default.TextureID,
					bbColor = Vector4.One,
					texCoords = new Triangle(
						new Vector2(0f, 0f),
						new Vector2(0f, 1f),
						new Vector2(1f, 0f)
					)
				};

				/// <summary>
				/// The unique identifier of the texture to be applied.
				/// </summary>
				public MyStringId textureID;

				/// <summary>
				/// The color tint applied to the triangle vertices. 
				/// Expected format is Normalized Linear RGB.
				/// </summary>
				public Vector4 bbColor;

				/// <summary>
				/// The normalized texture coordinates (UVs) corresponding to the triangle's three vertices.
				/// </summary>
				public Triangle texCoords;
			}

			/// <summary>
			/// Defines the visual properties for rendering a quadrilateral using explicit texture coordinates 
			/// for each corner, allowing for non-rectangular UV mappings (e.g., distortion or rotation).
			/// </summary>
			public struct QuadMaterial
			{
				public static readonly QuadMaterial Default = new QuadMaterial()
				{
					textureID = Material.Default.TextureID,
					bbColor = Vector4.One,
					texCoords = new FlatQuad(
						new Vector2(0f, 0f),
						new Vector2(0f, 1f),
						new Vector2(1f, 0f),
						new Vector2(1f, 1f)
					)
				};

				/// <summary>
				/// The unique identifier of the texture to be applied.
				/// </summary>
				public MyStringId textureID;

				/// <summary>
				/// The color tint applied to the quad. 
				/// Expected format is Normalized Linear RGB.
				/// </summary>
				public Vector4 bbColor;

				/// <summary>
				/// The normalized texture coordinates (UVs) for the four corners of the quad.
				/// </summary>
				public FlatQuad texCoords;
			}

			/// <summary>
			/// Defines the visual properties for rendering a quad where the texture coordinates are 
			/// axis-aligned and defined by a bounding box (Min/Max). This is optimized for standard 
			/// sprites or full textures without distortion.
			/// </summary>
			public struct BoundedQuadMaterial
			{
				public static readonly BoundedQuadMaterial Default = new BoundedQuadMaterial()
				{
					textureID = Material.Default.TextureID,
					bbColor = Vector4.One,
					texBounds = new BoundingBox2(Vector2.Zero, Vector2.One)
				};

				/// <summary>
				/// The unique identifier of the texture to be applied.
				/// </summary>
				public MyStringId textureID;

				/// <summary>
				/// The color tint applied to the quad. 
				/// Expected format is Normalized Linear RGB.
				/// </summary>
				public Vector4 bbColor;

				/// <summary>
				/// Defines the region of the texture to be used (UV Min/Max).
				/// Determines the scale and aspect ratio of the texture as rendered.
				/// </summary>
				public BoundingBox2 texBounds;
			}

			/// <summary>
			/// Defines the visual properties for rendering arbitrary N-sided polygons, including 
			/// a list of specific texture coordinates matching the polygon's vertices.
			/// </summary>
			public struct PolyMaterial
			{
				public static readonly PolyMaterial Default = new PolyMaterial()
				{
					textureID = Material.Default.TextureID,
					bbColor = Vector4.One,
					texCoords = null
				};

				/// <summary>
				/// The unique identifier of the texture to be applied.
				/// </summary>
				public MyStringId textureID;

				/// <summary>
				/// The color tint applied to the polygon. 
				/// Expected format is Normalized Linear RGB.
				/// </summary>
				public Vector4 bbColor;

				/// <summary>
				/// The overall UV bounds of the texture on the polygon. 
				/// </summary>
				public BoundingBox2 texBounds;

				/// <summary>
				/// A list of normalized texture coordinates (UVs) corresponding 1:1 with the polygon's vertices.
				/// </summary>
				public List<Vector2> texCoords;
			}

			/// <summary>
			/// Represents a 2D quadrilateral defined by four points. 
			/// Used for both screen-space geometry definitions and texture coordinate (UV) mapping.
			/// </summary>
			public struct FlatQuad
			{
				public Vector2 Point0, Point1, Point2, Point3;

				public FlatQuad(Vector2 Point0, Vector2 Point1, Vector2 Point2, Vector2 Point3)
				{
					this.Point0 = Point0;
					this.Point1 = Point1;
					this.Point2 = Point2;
					this.Point3 = Point3;
				}
			}

			/// <summary>
			/// Represents a 2D triangle defined by three points.
			/// Used primarily for texture coordinate (UV) mapping or screen-space geometry.
			/// </summary>
			public struct Triangle
			{
				public Vector2 Point0, Point1, Point2;

				public Triangle(Vector2 Point0, Vector2 Point1, Vector2 Point2)
				{
					this.Point0 = Point0;
					this.Point1 = Point1;
					this.Point2 = Point2;
				}
			}

			/// <summary>
			/// Represents a 3D triangle defined by three double-precision world coordinates.
			/// Used for defining world-space geometry.
			/// </summary>
			public struct TriangleD
			{
				public Vector3D Point0, Point1, Point2;

				public TriangleD(Vector3D Point0, Vector3D Point1, Vector3D Point2)
				{
					this.Point0 = Point0;
					this.Point1 = Point1;
					this.Point2 = Point2;
				}
			}
		}
	}
}