using VRage.Utils;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Used to determine how a given <see cref="Material"/> is scaled on a given Billboard.
            /// </summary>
            public enum MaterialAlignment : int
            {
                /// <summary>
                /// Stretches/compresses the material to cover the whole billboard. Default behavior.
                /// </summary>
                StretchToFit = 0,

                /// <summary>
                ///  Rescales the material so that it matches the height of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitVertical = 1,

                /// <summary>
                /// Rescales the material so that it matches the width of the Billboard while maintaining its aspect ratio
                /// </summary>
                FitHorizontal = 2,

                /// <summary>
                /// Maintains the material's aspect ratio and size at the given scale without regard
                /// to the size of the billboard.
                /// </summary>
                Fixed = 3,
            }

            /// <summary>
            /// Defines a texture used by <see cref="MatBoard"/>s. Supports sprite sheets.
            /// </summary>
            public class Material
            {
                public static readonly Material Default = new Material(MyStringId.GetOrCompute("RichHudDefault"), new Vector2(4f, 4f));

                /// <summary>
                /// ID of the Texture the <see cref="Material"/> is based on.
                /// </summary>
                public readonly MyStringId TextureID;
                /// <summary>
                /// The dimensions, in pixels, of the <see cref="Material"/>.
                /// </summary>
                public readonly Vector2 size;
                /// <summary>
                /// The dimensions of the <see cref="Material"/> relative to the size of the texture its based on.
                /// </summary>
                public readonly Vector2 scaledSize;
                /// <summary>
                /// Center of the <see cref="Material"/> on the texture scaled relative to the size of the texture.
                /// </summary>
                public readonly Vector2 scaledOrigin;

                /// <summary>
                /// Creates a <see cref="Material"/> using the name of the Texture's ID and its size in pixels.
                /// </summary>
                public Material(string TextureName, Vector2 size) : this(MyStringId.GetOrCompute(TextureName), size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> based on a Texture Atlas/Sprite with a given offset and size.
                /// </summary>
                public Material(string TextureName, Vector2 textureSize, Vector2 offset, Vector2 size)
                    : this(MyStringId.GetOrCompute(TextureName), textureSize, offset, size)
                { }

                /// <summary>
                /// Creates a <see cref="Material"/> using the <see cref="MyStringId"/> of the texture and its size in pixels.
                /// </summary>
                public Material(MyStringId TextureID, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    scaledSize = Vector2.One;
                    scaledOrigin = scaledSize / 2f;
                }

                /// <summary>
                /// Creates a <see cref="Material"/> based on an Atlas/Sprite with a given offset and size.
                /// </summary>
                public Material(MyStringId TextureID, Vector2 textureSize, Vector2 offset, Vector2 size)
                {
                    this.TextureID = TextureID;
                    this.size = size;

                    size.X /= textureSize.X;
                    size.Y /= textureSize.Y;

                    scaledSize = size;

                    offset.X /= textureSize.X;
                    offset.Y /= textureSize.Y;

                    scaledOrigin = offset + (scaledSize / 2f);
                }
            }

        }
    }
}