using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
{
    namespace UI
    {
        using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

        /// <summary>
        /// A <see cref="string"/> associated with a <see cref="GlyphFormat"/>. Implicitly castable to <see cref="RichText"/>.
        /// </summary>
        public class RichString
        {
            public char this[int index] => text[index];
            public int Length => text.Length;

            public GlyphFormat format;
            public readonly StringBuilder text;

            /// <summary>
            /// Initializes a <see cref="RichString"/> with the given capacity.
            /// </summary>
            public RichString(int capacity = 6)
            {
                text = new StringBuilder(capacity);
            }

            /// <summary>
            /// Initializes a <see cref="RichString"/> using the given API data.
            /// </summary>
            public RichString(RichStringMembers data)
            {
                text = data.Item1;
                format = new GlyphFormat(data.Item2);
            }

            /// <summary>
            /// Initializes a <see cref="RichString"/> with the given string builder and formatting.
            /// </summary>
            public RichString(StringBuilder text, GlyphFormat format = null)
            {
                if (format != null)
                    this.format = format;
                else
                    this.format = GlyphFormat.Empty;

                this.text = text;
            }

            /// <summary>
            /// Initializes a <see cref="RichString"/> with a string and given formatting.
            /// </summary>
            public RichString(string text, GlyphFormat format = null)
            {
                if (format != null)
                    this.format = format;
                else
                    this.format = GlyphFormat.Empty;

                this.text = new StringBuilder(text.Length);
                this.text.Append(text);
            }

            /// <summary>
            /// Creates a shallow copy of the given <see cref="RichString"/>.
            /// </summary>
            public RichString(RichString original)
            {
                this.format = original.format;
                this.text = original.text;
            }

            /// <summary>
            /// Appends the contents of the given <see cref="StringBuilder"/> to the <see cref="RichString"/>.
            /// </summary>
            public void Append(StringBuilder text) =>
                this.text.Append(text);

            /// <summary>
            /// Appends the string to the <see cref="RichString"/>.
            /// </summary>
            public void Append(string text) =>
                this.text.Append(text);

            /// <summary>
            /// Appends the character to the <see cref="RichString"/>.
            /// </summary>
            public void Append(char ch) =>
                text.Append(ch);

            public static implicit operator RichString(string text) =>
                new RichString(text, null);

            /// <summary>
            /// Retrieves data used by the Framework API.
            /// </summary>
            public RichStringMembers GetApiData() =>
                new RichStringMembers(text, format.data);

            /// <summary>
            /// Returns the contents of the <see cref="RichString"/> as an unformatted <see cref="string"/>.
            /// </summary>
            public override string ToString() =>
                text.ToString();

        }
    }
}