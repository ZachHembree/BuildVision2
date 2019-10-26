using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using System;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>;

namespace DarkHelmet
{
    using Vec2Prop = MyTuple<Func<Vector2>, Action<Vector2>>;
    using FloatProp = MyTuple<Func<float>, Action<float>>;
    using RichCharMembers = MyTuple<char, GlyphFormatMembers>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI
    {
        using LineMembers = MyTuple<
            Func<int, RichCharMembers>, // GetChar
            Func<int> // Count
        >;

        namespace Rendering
        {
            using TextBoardMembers = MyTuple<
                MyTuple<Func<int, LineMembers>, Func<int>>, // GetLine, GetCount
                FloatProp, // Scale
                Func<Vector2>, // TextSize
                Vec2Prop, // MaxSize
                FloatProp, // MaxLineWidth
                MyTuple<
                    Func<bool>, // WordWrapping
                    Action<Vector2>, // Draw
                    Func<Vector2I, Vector2I, List<RichStringMembers>>, // GetRange
                    Action<Vector2I, Vector2I, GlyphFormatMembers>, // SetFormatting
                    Action<IList<RichStringMembers>, Vector2I>, // Insert
                    MyTuple<
                        Action<RichStringMembers, Vector2I>, // Insert
                        Action<Vector2I, Vector2I>, // RemoveRange
                        Action // Clear
                    >
                >
            >;

            public interface IRichChar
            {
                char Ch { get; }
                GlyphFormat Format { get; }

                RichCharMembers GetApiData();
            }

            public interface ILine : IIndexedCollection<IRichChar>
            { }

            public interface ITextBoard
            {
                ILine this[int index] { get; }
                int Count { get; }

                GlyphFormat Format { get; set; }
                bool WordWrapping { get; }
                Vector2 TextSize { get; }
                Vector2 MaxSize { get; set; }
                float Scale { get; set; }
                float MaxLineWidth { get; }

                void SetText(RichString text);
                void SetText(RichText text);
                void SetText(string text);
                void Append(RichString text);
                void Append(RichText text);
                void Append(string text);
                void Insert(RichText text, Vector2I start);
                void Insert(RichString text, Vector2I start);
                RichText GetText();
                RichText GetTextRange(Vector2I start, Vector2I end);
                void RemoveRange(Vector2I start, Vector2I end);
                void SetLineWrapWidth(float width);
                void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format);
                void Clear();
                TextBoardMembers GetApiData();
            }
        }
    }
}