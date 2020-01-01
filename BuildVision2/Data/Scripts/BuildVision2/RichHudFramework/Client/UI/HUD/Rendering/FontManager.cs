using System;
using System.Collections.Generic;
using VRageMath;
using VRage;
using VRage.Utils;
using AtlasMembers = VRage.MyTuple<string, VRageMath.Vector2>;
using GlyphMembers = VRage.MyTuple<int, VRageMath.Vector2, VRageMath.Vector2, float, float>;

namespace RichHudFramework
{
    using FontMembers = MyTuple<
        string, // Name
        int, // Index
        float, // PtSize
        float, // BaseScale
        Func<int, bool> // IsStyleDefined
    >;
    using FontStyleDefinition = MyTuple<
        int, // styleID
        float, // height
        float, // baseline
        AtlasMembers[], // atlases
        KeyValuePair<char, GlyphMembers>[], // glyphs
        KeyValuePair<uint, float>[] // kernings
    >;

    namespace UI
    {
        using FontDefinition = MyTuple<
            string, // Name
            float, // PtSize
            FontStyleDefinition[] // styles
        >;

        namespace Rendering.Client
        {
            using RichHudClient;
            using FontManagerMembers = MyTuple<
                MyTuple<Func<int, FontMembers>, Func<int>>, // Font List
                Func<FontDefinition, FontMembers?>, // TryAddFont
                Func<string, FontMembers?> // GetFont
            >;

            public sealed class FontManager : RichHudClient.ApiComponentBase
            {
                public static Vector2 Default => Vector2.Zero;
                public static IReadOnlyCollection<IFontMin> Fonts => Instance.fonts;
                private static FontManager Instance
                {
                    get { Init(); return instance; }
                    set { instance = value; }
                }
                private static FontManager instance;

                private readonly ReadOnlyCollectionData<IFontMin> fonts;
                private readonly Func<FontDefinition, FontMembers?> TryAddFontFunc;
                private readonly Func<string, FontMembers?> GetFontFunc;

                private FontManager() : base(ApiComponentTypes.FontManager, false, true)
                {
                    var members = (FontManagerMembers)GetApiData();

                    Func<int, IFontMin> fontGetter = x => new FontData(members.Item1.Item1(x));
                    fonts = new ReadOnlyCollectionData<IFontMin>(fontGetter, members.Item1.Item2);

                    TryAddFontFunc = members.Item2;
                    GetFontFunc = members.Item3;

                    Game.ModBase.SendChatMessage($"Font Client Init.");
                }

                private static void Init()
                {
                    if (instance == null)
                        instance = new FontManager();
                }

                public override void Close()
                {
                    instance = null;
                }

                public static bool TryAddFont(FontDefinition fontData) =>
                    Instance.TryAddFontFunc(fontData) != null;

                public static bool TryAddFont(FontDefinition fontData, out IFontMin font)
                {
                    FontMembers? members = Instance.TryAddFontFunc(fontData);

                    if (members != null)
                    {
                        font = new FontData(members.Value);
                        return true;
                    }
                    else
                    {
                        font = null;
                        return false;
                    }
                }

                public static IFontMin GetFont(string name)
                {
                    FontMembers? members = Instance.GetFontFunc(name);
                    IFontMin font = null;

                    if (members != null)
                        font = new FontData(members.Value);

                    return font;
                }

                private class FontData : IFontMin
                {
                    public string Name { get; }
                    public int Index { get; }
                    public float PtSize { get; }
                    public float BaseScale { get; }

                    private readonly Func<int, bool> IsFontDefinedFunc;

                    public FontData(FontMembers members)
                    {
                        Name = members.Item1;
                        Index = members.Item2;
                        PtSize = members.Item3;
                        BaseScale = members.Item4;
                        IsFontDefinedFunc = members.Item5;
                    }

                    public bool IsStyleDefined(FontStyleEnum styleEnum) =>
                        IsFontDefinedFunc((int)styleEnum);

                    public bool IsStyleDefined(int style) =>
                        IsFontDefinedFunc(style);
                }
            }
        }
    }
}