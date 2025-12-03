using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		namespace Rendering
		{
			/// <summary>
			/// Defines how a <see cref="Material"/> texture is mapped to the geometry of the UI element.
			/// </summary>
			public enum MaterialAlignment : int
			{
				/// <summary>
				/// Stretches or squeezes the material to fill the element's bounds exactly. 
				/// This behaves like CSS "fill". Aspect ratio is ignored, which may result in distortion.
				/// </summary>
				StretchToFit = 0,

				/// <summary>
				/// Scales the material to match the height of the element. The width is adjusted to maintain aspect ratio.
				/// If the material is wider than the element, it will be cropped horizontally.
				/// </summary>
				FitVertical = 1,

				/// <summary>
				/// Scales the material to match the width of the element. The height is adjusted to maintain aspect ratio.
				/// If the material is taller than the element, it will be cropped vertically.
				/// </summary>
				FitHorizontal = 2,

				/// <summary>
				/// Scales the material to cover the entire element while maintaining aspect ratio.
				/// This behaves like CSS "contain". The texture is not cropped, but may leave empty space.
				/// </summary>
				FitAuto = 3,
			}

			/// <summary>
			/// Represents a handle to a Space Engineers Transparent Material.
			/// Supports defining full textures or specific sprites (texture regions) within a texture atlas.
			/// </summary>
			public class Material
			{
				/// <summary>
				/// A plain white 4x4 texture used for solid color UI elements.
				/// </summary>
				public static readonly Material Default = new Material("RichHudDefault", new Vector2(4f, 4f));

				/// <summary>
				/// High-resolution circle texture (1024x1024).
				/// </summary>
				public static readonly Material CircleMat = new Material("RhfCircle", new Vector2(1024f));

				/// <summary>
				/// High-resolution ring/donut texture (1024x1024).
				/// </summary>
				public static readonly Material AnnulusMat = new Material("RhfAnnulus", new Vector2(1024f));

				/// <summary>
				/// The unique SubtypeId of the underlying Transparent Material.
				/// </summary>
				public readonly MyStringId TextureID;

				/// <summary>
				/// The dimensions of the material sprite in pixels.
				/// </summary>
				public readonly Vector2 Size;

				/// <summary>
				/// The normalized UV coordinates defining the region of the texture to be used.
				/// (0,0 is top-left, 1,1 is bottom-right relative to the atlas).
				/// </summary>
				public readonly BoundingBox2 UVBounds;

				/// <summary>
				/// Creates a Material using the SubtypeId of a Transparent Material.
				/// Assumes the material utilizes the full texture dimensions.
				/// </summary>
				/// <param name="SubtypeId">The string name of the texture SubtypeId.</param>
				/// <param name="size">The resolution of the texture in pixels.</param>
				public Material(string SubtypeId, Vector2 size) : this(MyStringId.GetOrCompute(SubtypeId), size)
				{ }

				/// <summary>
				/// Creates a Material representing a specific region (sprite) within a larger texture atlas.
				/// </summary>
				/// <param name="SubtypeId">The string name of the texture SubtypeId.</param>
				/// <param name="texSize">The total resolution of the source texture atlas in pixels.</param>
				/// <param name="texCoords">The pixel offset of the sprite starting from the top-left corner.</param>
				/// <param name="size">The size of the specific sprite region in pixels.</param>
				public Material(string SubtypeId, Vector2 texSize, Vector2 texCoords, Vector2 size)
					: this(MyStringId.GetOrCompute(SubtypeId), texSize, texCoords, size)
				{ }

				/// <summary>
				/// Creates a Material using the hashed SubtypeId of a Transparent Material.
				/// Assumes the material utilizes the full texture dimensions.
				/// </summary>
				/// <param name="TextureID">The hashed <see cref="MyStringId"/> of the texture.</param>
				/// <param name="size">The resolution of the texture in pixels.</param>
				public Material(MyStringId TextureID, Vector2 size)
				{
					this.TextureID = TextureID;
					this.Size = size;
					UVBounds = new BoundingBox2(Vector2.Zero, Vector2.One);
				}

				/// <summary>
				/// Creates a Material representing a specific region (sprite) within a larger texture atlas
				/// using a hashed SubtypeId.
				/// </summary>
				/// <param name="SubtypeId">The hashed <see cref="MyStringId"/> of the texture.</param>
				/// <param name="texSize">The total resolution of the source texture atlas in pixels.</param>
				/// <param name="offset">The pixel offset of the sprite starting from the top-left corner.</param>
				/// <param name="size">The size of the specific sprite region in pixels.</param>
				public Material(MyStringId SubtypeId, Vector2 texSize, Vector2 offset, Vector2 size)
				{
					this.TextureID = SubtypeId;
					this.Size = size;

					Vector2 rcpTexSize = 1f / texSize,
						halfUVSize = .5f * size * rcpTexSize,
						uvOffset = (offset * rcpTexSize) + halfUVSize;

					UVBounds = new BoundingBox2(uvOffset - halfUVSize, uvOffset + halfUVSize);
				}
			}
		}
	}
}