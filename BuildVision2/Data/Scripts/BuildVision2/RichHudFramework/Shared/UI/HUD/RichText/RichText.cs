using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI
    {
        /// <summary>
        /// Reusable rich text builder designed for efficient construction and reuse of formatted text. 
        /// Internally minimises StringBuilder allocations by merging  consecutive text segments that 
        /// share identical formatting.
        /// </summary>
        public class RichText : IEnumerable<RichStringMembers>, IEquatable<RichText>
        {
            /// <summary>
            /// Default formatting applied to any added text that does not specify its own format.
            /// If null, the default format of the UI element this text is assigned to will be used 
            /// as the default when it is copied.
            /// </summary>
            public GlyphFormat? defaultFormat;

            /// <summary>
            /// Internal API-native storage for the rich text. Each entry is a <see cref="StringBuilder"/>
            /// paired with its <see cref="GlyphFormatMembers"/> formatting data.
            /// </summary>
            /// <exclude/>
            public readonly List<RichStringMembers> apiData;

            private ObjectPool<StringBuilder> sbPool;

            /// <summary>
            /// Initializes an empty <see cref="RichText"/> instance.
            /// </summary>
            /// <param name="defaultFormat">Optional default formatting applied to text added without explicit formatting.</param>
            public RichText(GlyphFormat? defaultFormat = null)
            {
                this.defaultFormat = defaultFormat ?? GlyphFormat.Empty;
                apiData = new List<RichStringMembers>();
            }

            /// <summary>
            /// Wraps an existing API-native rich text list in a new <see cref="RichText"/> instance.
            /// Used internally for client-master text sharing.
            /// </summary>
            /// <param name="apiData">The API data to wrap.</param>
            /// <param name="copy">If true, the data is deep-copied; otherwise the list is used directly (shared).</param>
            /// <exclude/>
            public RichText(List<RichStringMembers> apiData, bool copy = false)
            {
                this.apiData = copy ? GetDataCopy(apiData) : apiData;
                defaultFormat = GlyphFormat.Empty;
            }

            /// <summary>
            /// Creates a new <see cref="RichText"/> instance that is a deep copy of the given <see cref="RichText"/>.
            /// </summary>
            /// <param name="original">The rich text object to copy.</param>
            public RichText(RichText original)
            {
                apiData = new List<RichStringMembers>();
                defaultFormat = original.defaultFormat;
                Add(original);
            }

            /// <summary>
            /// Initializes a new <see cref="RichText"/> instance containing the specified plain string.
            /// </summary>
            /// <param name="text">Initial text content.</param>
            /// <param name="defaultFormat">Optional default formatting for the text.</param>
            public RichText(string text, GlyphFormat? defaultFormat = null)
            {
                this.defaultFormat = defaultFormat ?? GlyphFormat.Empty;
                apiData = new List<RichStringMembers>();
                apiData.Add(new RichStringMembers(new StringBuilder(text), this.defaultFormat.Value.Data));
            }

            /// <summary>
            /// Initializes a new <see cref="RichText"/> instance containing the contents of the given <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="text">Initial text content.</param>
            /// <param name="defaultFormat">Optional default formatting for the text.</param>
            public RichText(StringBuilder text, GlyphFormat? defaultFormat = null)
            {
                this.defaultFormat = defaultFormat ?? GlyphFormat.Empty;
                apiData = new List<RichStringMembers>();
                Add(text);
            }

            /// <summary>
            /// Returns an enumerator that iterates through the underlying rich string members.
            /// </summary>
            public IEnumerator<RichStringMembers> GetEnumerator() => apiData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => apiData.GetEnumerator();

            /// <summary>
            /// Appends a deep copy of another <see cref="RichText"/> object's contents to this instance.
            /// Consecutive segments with identical formatting are automatically merged into the same <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="text">The rich text to append.</param>
            public void Add(RichText text)
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> currentStrings = apiData,
                    newStrings = text.apiData;

                if (newStrings.Count > 0)
                {
                    int index = 0, end = newStrings.Count - 1;

                    // Attempt to use last StringBuilder if the formatting matches
                    if (currentStrings.Count > 0)
                    {
                        GlyphFormatMembers newFormat = newStrings[0].Item2;
                        StringBuilder sb;
                        bool formatEqual;

                        GetNextStringBuilder(newFormat, out sb, out formatEqual);

                        if (formatEqual)
                        {
                            StringBuilder newSb = newStrings[0].Item1;
                            sb.EnsureCapacity(sb.Length + newSb.Length);

                            for (int i = 0; i < newSb.Length; i++)
                                sb.Append(newSb[i]);

                            index++;
                        }
                    }

                    // Copy the remaining text
                    for (int i = index; i <= end; i++)
                    {
                        StringBuilder sb = sbPool.Get(),
                            newSb = newStrings[i].Item1;

                        sb.EnsureCapacity(sb.Length + newSb.Length);
                        currentStrings.Add(new RichStringMembers(sb, newStrings[i].Item2));

                        for (int j = 0; j < newSb.Length; j++)
                            sb.Append(newSb[j]);
                    }
                }
            }

            /// <summary>
            /// Appends a copy of the given <see cref="StringBuilder"/> using the specified formatting.
            /// If the new formatting matches the last segment's formatting, the text is merged into the existing <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="text">Text to append.</param>
            /// <param name="newFormat">Formatting to apply. If null, <see cref="defaultFormat"/> is used.</param>
            public void Add(StringBuilder text, GlyphFormat? newFormat = null)
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> richStrings = apiData;
                GlyphFormatMembers format = newFormat?.Data ?? defaultFormat?.Data ?? GlyphFormat.Empty.Data;
                StringBuilder sb;
                bool formatEqual;

                GetNextStringBuilder(newFormat?.Data ?? GlyphFormat.Empty.Data, out sb, out formatEqual);

                // If format is equal, reuse last StringBuilder
                if (!formatEqual)
                {
                    var richString = new RichStringMembers(sb, format);
                    richStrings.Add(richString);
                }

                sb.EnsureCapacity(sb.Length + text.Length);

                for (int i = 0; i < text.Length; i++)
                    sb.Append(text[i]);
            }

            /// <summary>
            /// Appends a copy of the given <see cref="StringBuilder"/> with explicit formatting (order reversed for convenience).
            /// </summary>
            /// <param name="newFormat">Formatting to apply.</param>
            /// <param name="text">Text to append.</param>
            public void Add(GlyphFormat newFormat, StringBuilder text) =>
                Add(text, newFormat);

            /// <summary>
            /// Appends a string using the specified formatting.
            /// If the new formatting matches the last segment's formatting, the text is merged into the existing <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="text">Text to append.</param>
            /// <param name="newFormat">Formatting to apply. If null, <see cref="defaultFormat"/> is used.</param>
            public void Add(string text, GlyphFormat? newFormat = null)
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> richStrings = apiData;
                GlyphFormatMembers format = newFormat?.Data ?? defaultFormat?.Data ?? GlyphFormat.Empty.Data;
                StringBuilder sb;
                bool formatEqual;

                GetNextStringBuilder(newFormat?.Data ?? GlyphFormat.Empty.Data, out sb, out formatEqual);

                // If format is equal, reuse last StringBuilder
                if (!formatEqual)
                {
                    var richString = new RichStringMembers(sb, format);
                    richStrings.Add(richString);
                }

                sb.Append(text);
            }

            /// <summary>
            /// Appends a string with explicit formatting (order reversed for convenience).
            /// </summary>
            /// <param name="newFormat">Formatting to apply.</param>
            /// <param name="text">Text to append.</param>
            public void Add(GlyphFormat newFormat, string text) => Add(text, newFormat);

            /// <summary>
            /// Appends a single character using the specified formatting.
            /// If the new formatting matches the last segment's formatting, the character is added to the existing <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="ch">Character to append.</param>
            /// <param name="newFormat">Formatting to apply. If null, <see cref="defaultFormat"/> is used.</param>
            public void Add(char ch, GlyphFormat? newFormat = null)
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> richStrings = apiData;
                GlyphFormatMembers format = newFormat?.Data ?? defaultFormat?.Data ?? GlyphFormat.Empty.Data;
                StringBuilder sb;
                bool formatEqual;

                GetNextStringBuilder(newFormat?.Data ?? GlyphFormat.Empty.Data, out sb, out formatEqual);

                // If format is equal, reuse last StringBuilder
                if (!formatEqual)
                {
                    var richString = new RichStringMembers(sb, format);
                    richStrings.Add(richString);
                }

                sb.Append(ch);
            }

            private void GetNextStringBuilder(GlyphFormatMembers newFormat, out StringBuilder sb, out bool formatEqual)
            {
                List<RichStringMembers> richStrings = apiData;
                int last = richStrings.Count - 1;
                formatEqual = false;

                // Test formatting
                if (richStrings.Count > 0)
                {
                    GlyphFormatMembers lastFormat = richStrings[last].Item2;
                    formatEqual = newFormat.Item1 == lastFormat.Item1
                        && newFormat.Item2 == lastFormat.Item2
                        && newFormat.Item3 == lastFormat.Item3
                        && newFormat.Item4 == lastFormat.Item4;
                }

                sb = formatEqual ? richStrings[last].Item1 : sbPool.Get();
            }

            /// <summary>
            /// Reduces memory usage by trimming excess capacity from all internal <see cref="StringBuilder"/> instances
            /// and the object pool.
            /// </summary>
            public void TrimExcess()
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> text = apiData;

                for (int n = 0; n < text.Count; n++)
                    text[n].Item1.Capacity = text[n].Item1.Length;

                sbPool.TrimExcess();
                text.TrimExcess();
            }

            /// <summary>
            /// Removes all text from this instance and returns all pooled <see cref="StringBuilder"/> objects.
            /// </summary>
            public void Clear()
            {
                if (sbPool == null)
                    sbPool = new ObjectPool<StringBuilder>(new StringBuilderPoolPolicy());

                List<RichStringMembers> text = apiData;
                sbPool.ReturnRange(text, 0, text.Count);
                text.Clear();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            /// <summary>
            /// Determines whether this instance and a specified object, which must also be a <see cref="RichText"/>
            /// object, have the same value.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            public override bool Equals(object obj)
            {
                RichText other = obj as RichText;

                if (apiData == other?.apiData)
                    return true;
                if (other != null)
                    return Equals(other);
                else
                    return false;
            }

            /// <summary>
            /// Determines whether this <see cref="RichText"/> instance contains identical text and formatting
            /// to another <see cref="RichText"/> instance.
            /// </summary>
            /// <param name="other">The object to compare with the current object.</param>
            public bool Equals(RichText other)
            {
                bool isFormatEqual = true,
                    isTextEqual = true,
                    isLengthEqual = true;

                if (other == null)
                    return false;
                else if (apiData == other.apiData)
                    return true;
                else if (apiData.Count == other.apiData.Count)
                {
                    for (int i = 0; i < apiData.Count; i++)
                    {
                        if (apiData[i].Item1.Length != other.apiData[i].Item1.Length)
                        {
                            isLengthEqual = false;
                            break;
                        }
                    }

                    if (isLengthEqual)
                    {
                        for (int i = 0; i < apiData.Count; i++)
                        {
                            GlyphFormatMembers fmt = apiData[i].Item2,
                                otherFmt = other.apiData[i].Item2;

                            if (fmt.Item1 != otherFmt.Item1 ||
                                fmt.Item2 != otherFmt.Item2 ||
                                fmt.Item3 != otherFmt.Item3 ||
                                fmt.Item4 != otherFmt.Item4)
                            {
                                isFormatEqual = false;
                                break;
                            }
                        }
                    }
                    else
                        isFormatEqual = false;

                    if (isFormatEqual)
                    {
                        for (int i = 0; i < apiData.Count; i++)
                        {
                            for (int j = 0; j < apiData[i].Item1.Length; j++)
                            {
                                if (apiData[i].Item1[j] != other.apiData[i].Item1[j])
                                {
                                    isTextEqual = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                    isLengthEqual = false;

                return isLengthEqual && isFormatEqual && isTextEqual;
            }

            /// <summary>
            /// Returns a concatenated, unformatted copy of the entire text content as a <see cref="string"/>.
            /// </summary>
            public override string ToString()
            {
                StringBuilder rawText = new StringBuilder();
                List<RichStringMembers> richText = apiData;
                int charCount = 0;

                for (int i = 0; i < richText.Count; i++)
                    charCount += richText[i].Item1.Length;

                rawText.EnsureCapacity(charCount);

                for (int i = 0; i < richText.Count; i++)
                {
                    for (int b = 0; b < richText[i].Item1.Length; b++)
                        rawText.Append(richText[i].Item1[b]);
                }

                return rawText.ToString();
            }

            /// <summary>
            /// Creates and returns a new <see cref="RichText"/> instance that is a deep copy of this object.
            /// </summary>
            public RichText GetCopy() =>
                new RichText(GetDataCopy(apiData));

            /// <summary>
            /// Creates a deep copy of the given API-native rich text data list.
            /// </summary>
            /// <param name="original">Source data to copy.</param>
            /// <returns>A new list containing independent <see cref="StringBuilder"/> instances.</returns>
            /// <exclude/>
            public static List<RichStringMembers> GetDataCopy(List<RichStringMembers> original)
            {
                var newData = new List<RichStringMembers>(original.Count);

                for (int i = 0; i < original.Count; i++)
                {
                    StringBuilder oldSb = original[i].Item1,
                        sb = new StringBuilder(oldSb.Length);

                    for (int j = 0; j < oldSb.Length; j++)
                        sb.Append(oldSb[j]);

                    newData.Add(new RichStringMembers(sb, original[i].Item2));
                }

                return newData;
            }

            #region Operators

            /// <summary>
            /// Appends a plain string to the left <see cref="RichText"/> instance and returns the modified instance.
            /// </summary>
            public static RichText operator +(RichText left, string right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Appends a <see cref="StringBuilder"/> to the left <see cref="RichText"/> instance and returns the modified instance.
            /// </summary>
            public static RichText operator +(RichText left, StringBuilder right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Appends a copy of the right <see cref="RichText"/> to the left <see cref="RichText"/> and returns the modified left instance.
            /// </summary>
            public static RichText operator +(RichText left, RichText right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Implicitly converts a <see cref="string"/> to a <see cref="RichText"/> instance using the default formatting.
            /// </summary>
            public static implicit operator RichText(string text) => new RichText(text);

            /// <summary>
            /// Implicitly converts a <see cref="StringBuilder"/> to a <see cref="RichText"/> instance using the default formatting.
            /// </summary>
            public static implicit operator RichText(StringBuilder text) => new RichText(text);

            /// <summary>
            /// Implicitly wraps API-native rich text data in a <see cref="RichText"/> instance (internal use).
            /// </summary>
            /// <exclude/>
            public static implicit operator RichText(List<RichStringMembers> text) => new RichText(text);

            #endregion
        }
    }
}