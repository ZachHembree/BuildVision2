using RichHudFramework.UI.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

        /// <summary>
        /// A collection of rich strings. <see cref="RichString"/>s and <see cref="string"/>s can be implicitly
        /// cast to this type. Collection-initializer syntax can be used with this type.
        /// </summary>
        public class RichText : IEnumerable<RichString>
        {
            public GlyphFormat defaultFormat;
            public List<RichStringMembers> ApiData { get; }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object with the given default format.
            /// </summary>
            public RichText(GlyphFormat defaultFormat = null)
            {
                ApiData = new List<RichStringMembers>();
                this.defaultFormat = defaultFormat;
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object using a collection of rich string data.
            /// Used in conjunction with the Framework API.
            /// </summary>
            public RichText(IList<RichStringMembers> richStrings)
            {
                ApiData = new List<RichStringMembers>(richStrings);
                defaultFormat = GlyphFormat.Empty;
            }

            /// <summary>
            /// Initializes a <see cref="RichText"/> object with a <see cref="RichString"/> and sets the default format
            /// to that of the string.
            /// </summary>
            public RichText(RichString text)
            {
                this.defaultFormat = text.Format;
                this.ApiData = new List<RichStringMembers>();
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

                ApiData = new List<RichStringMembers>();
                Add(defaultFormat, text);
            }

            public IEnumerator<RichString> GetEnumerator()
            {
                throw new Exception("Enumerator not implemented for RichText.");
            }

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds a <see cref="string"/> to the text using the default format.
            /// </summary>
            public void Add(string text) =>
                ApiData.Add(new RichStringMembers(new StringBuilder(text), defaultFormat?.data ?? default(GlyphFormatMembers)));

            /// <summary>
            /// Adds a <see cref="RichText"/> to the collection using the formatting specified in the <see cref="RichText"/>.
            /// </summary>
            public void Add(RichText text) =>
                ApiData.AddRange(text.ApiData);

            /// <summary>
            /// Adds a <see cref="RichString"/> to the collection using the formatting specified in the <see cref="RichString"/>.
            /// </summary>
            public void Add(RichString text) =>
                ApiData.Add(text.ApiData);

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(GlyphFormat formatting, string text) =>
                ApiData.Add(new RichStringMembers(new StringBuilder(text), formatting.data));

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(string text, GlyphFormat formatting) =>
                Add(formatting, text);

            /// <summary>
            /// Returns the contents of the <see cref="RichText"/> as an unformatted <see cref="string"/>.
            /// </summary>
            public override string ToString()
            {
                StringBuilder rawText = new StringBuilder();

                for (int a = 0; a < ApiData.Count; a++)
                {
                    rawText.EnsureCapacity(rawText.Length + ApiData[a].Item1.Length);

                    for (int b = 0; b < ApiData[a].Item1.Length; b++)
                        rawText.Append(ApiData[a].Item1[b]);
                }

                return rawText.ToString();
            }

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
        }
    }
}