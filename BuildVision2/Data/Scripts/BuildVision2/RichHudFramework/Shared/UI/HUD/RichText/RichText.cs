using RichHudFramework.UI.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
{
    namespace UI
    {
        using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

        /// <summary>
        /// A collection of rich strings. <see cref="RichString"/>s and <see cref="string"/>s can be implicitly
        /// cast to this type. Collection-initializer syntax can be used with this type.
        /// </summary>
        public class RichText : IReadOnlyCollection<RichString>
        {
            public RichString this[int index] => text[index];
            public int Count => text.Count;

            public GlyphFormat defaultFormat;
            private readonly List<RichString> text;

            /// <summary>
            /// Creates a shallow copy of the given <see cref="RichText"/>
            /// </summary>
            public RichText(RichText original)
            {
                this.defaultFormat = original.defaultFormat;
                this.text = original.text;
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object with the given default format.
            /// </summary>
            public RichText(GlyphFormat defaultFormat = null)
            {
                text = new List<RichString>();
                this.defaultFormat = defaultFormat;
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object using a collection of rich string data.
            /// Used in conjunction with the Framework API.
            /// </summary>
            public RichText(IList<RichStringMembers> richStrings)
            {
                text = new List<RichString>(richStrings.Count);

                for (int n = 0; n < richStrings.Count; n++)
                    text.Add(new RichString(richStrings[n]));
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object with a <see cref="RichString"/> and sets the default format
            /// to that of the string.
            /// </summary>
            public RichText(RichString text)
            {
                this.defaultFormat = text.format;
                this.text = new List<RichString>();
                Add(text);
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object with the given string and formatting.
            /// </summary>
            public RichText(string text, GlyphFormat defaultFormat = null)
            {
                if (defaultFormat != null)
                    this.defaultFormat = defaultFormat;
                else
                    this.defaultFormat = GlyphFormat.Empty;

                this.text = new List<RichString>();
                Add(defaultFormat, text);
            }

            public IEnumerator<RichString> GetEnumerator() =>
                text.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds a <see cref="string"/> to the text using the default format.
            /// </summary>
            public void Add(string text) =>
                this.text.Add(new RichString(text, defaultFormat));

            /// <summary>
            /// Adds a <see cref="RichText"/> to the collection using the formatting specified in the <see cref="RichText"/>.
            /// </summary>
            public void Add(RichText text) =>
                this.text.AddRange(text);

            /// <summary>
            /// Adds a <see cref="RichString"/> to the collection using the formatting specified in the <see cref="RichString"/>.
            /// </summary>
            /// <param name="text"></param>
            public void Add(RichString text) =>
                this.text.Add(text);

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(GlyphFormat formatting, string text) =>
                this.text.Add(new RichString(text, formatting));

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(string text, GlyphFormat formatting) =>
                Add(formatting, text);

            /// <summary>
            /// Adds a <see cref="string"/> to the text using the default format.
            /// </summary>
            public static RichText operator +(RichText left, string right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Adds a <see cref="RichString"/> to the collection using the formatting specified in the <see cref="RichString"/>.
            /// </summary>
            /// <param name="text"></param>
            public static RichText operator +(RichText left, RichString right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Adds a <see cref="RichText"/> to the collection using the formatting specified in the <see cref="RichText"/>.
            /// </summary>
            public static RichText operator +(RichText left, RichText right)
            {
                left.Add(right);
                return left;
            }

            public static implicit operator RichText(string text) =>
                new RichText(text, GlyphFormat.Empty);

            public static implicit operator RichText(RichString text) =>
                new RichText(text);

            /// <summary>
            /// Returns the contents of the <see cref="RichText"/> as an unformatted <see cref="string"/>.
            /// </summary>
            public override string ToString()
            {
                StringBuilder rawText = new StringBuilder();

                for (int a = 0; a < text.Count; a++)
                {
                    for (int b = 0; b < text[a].Length; b++)
                        rawText.Append(text[a][b]);
                }

                return rawText.ToString();
            }

            /// <summary>
            /// Returns data used to share data between mods using the Framework API.
            /// </summary>
            public RichStringMembers[] GetApiData()
            {
                RichStringMembers[] data = new RichStringMembers[text.Count];

                for (int n = 0; n < data.Length; n++)
                    data[n] = text[n].GetApiData();

                return data;
            }
        }
    }
}