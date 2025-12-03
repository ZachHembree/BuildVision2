using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Internal API accessor indices for querying the contents of a textbuilder line
            /// </summary>
            /// <exclude/>
            public enum LineAccessors : int
            {
                /// <summary>
                /// out: int
                /// </summary>
                Count = 1,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Size = 2,

                /// <summary>
                /// out: float
                /// </summary>
                VerticalOffset = 3,
            }

            /// <summary>
            /// Represents a single horizontal line of text within a <see cref="ITextBuilder"/>.
            /// <para>
            /// This interface serves as a collection of <see cref="IRichChar"/> and stores line-specific layout properties, 
            /// such as the line's dimensions and vertical offset.
            /// </para>
            /// </summary>
            public interface ILine : IIndexedCollection<IRichChar>
            {
				/// <summary>
				/// Retrieves the <see cref="IRichChar"/> at the specified index.
				/// <para>
				/// Note: This creates temporary index based wrapper objects. Reference equality checks between calls 
                /// may fail.
				/// </para>
				/// </summary>
				new IRichChar this[int index] { get; }

				/// <summary>
				/// The dimensions of the line as rendered.
				/// X = Width (sum of character advances), Y = Max height of characters in the line.
				/// </summary>
				Vector2 Size { get; }

                /// <summary>
                /// The vertical position of this line relative to the center of the text element.
                /// This value does not include the global <see cref="ITextBoard.TextOffset"/>.
                /// </summary>
                float VerticalOffset { get; }
            }
        }
    }
}