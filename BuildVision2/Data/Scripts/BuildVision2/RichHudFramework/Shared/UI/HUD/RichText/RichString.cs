using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

        /// <summary>
        /// A <see cref="string"/> associated with a <see cref="GlyphFormat"/>. Implicitly castable to <see cref="RichText"/>.
        /// </summary>
        public struct RichString
        {
            public char this[int index] => ApiData.Item1[index];

            public int Length => ApiData.Item1.Length;

            public GlyphFormat Format { get { return new GlyphFormat(ApiData.Item2); } set { apiData.Item2 = value.data; } }

            public RichStringMembers ApiData => apiData;

            private RichStringMembers apiData;

            /// <summary>
            /// Initializes a <see cref="RichString"/> with the given capacity.
            /// </summary>
            public RichString(int capacity = 6)
            {
                apiData = new RichStringMembers(new StringBuilder(capacity), GlyphFormat.Empty.data);
            }

            /// <summary>
            /// Initializes a <see cref="RichString"/> using the given API data.
            /// </summary>
            public RichString(RichStringMembers data)
            {
                apiData = data;
            }

            /// <summary>
            /// Initializes a <see cref="RichString"/> with a string and given formatting.
            /// </summary>
            public RichString(string text, GlyphFormat format = null)
            {
                if (format == null)
                    format = GlyphFormat.Empty;

                var sb = new StringBuilder(text.Length);
                sb.Append(text);

                apiData = new RichStringMembers(sb, format.data);
            }

            /// <summary>
            /// Appends the contents of the given <see cref="StringBuilder"/> to the <see cref="RichString"/>.
            /// </summary>
            public void Append(StringBuilder text) =>
                apiData.Item1.Append(text);

            /// <summary>
            /// Appends the string to the <see cref="RichString"/>.
            /// </summary>
            public void Append(string text) =>
                apiData.Item1.Append(text);

            /// <summary>
            /// Appends the character to the <see cref="RichString"/>.
            /// </summary>
            public void Append(char ch) =>
                apiData.Item1.Append(ch);

            public void Clear() =>
                apiData.Item1.Clear();

            /// <summary>
            /// Returns the contents of the <see cref="RichString"/> as an unformatted <see cref="string"/>.
            /// </summary>
            public override string ToString() =>
                apiData.Item1.ToString();

        }
    }
}