using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using AtlasMembers = VRage.MyTuple<string, VRageMath.Vector2>;
using GlyphMembers = VRage.MyTuple<int, VRageMath.Vector2, VRageMath.Vector2, float, float>;

namespace RichHudFramework
{
	using Client;
	using FontMembers = MyTuple<
		string, // Name
		int, // Index
		float, // PtSize
		float, // BaseScale
		Func<int, bool>, // IsStyleDefined
		ApiMemberAccessor
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
			using FontManagerMembers = MyTuple<
				MyTuple<Func<int, FontMembers>, Func<int>>, // Font List
				Func<FontDefinition, FontMembers?>, // TryAddFont
				Func<string, FontMembers?>, // GetFont
				ApiMemberAccessor
			>;

			/// <summary>
			/// Client-side API for the font management system. Allows mods to query registered 
			/// fonts and register custom fonts at runtime.
			/// <para>
			/// Most frequently used by <see cref="GlyphFormat"/> internally when setting font or style.
			/// </para>
			/// </summary>
			public sealed partial class FontManager : RichHudClient.ApiModule
			{
				/// <summary>
				/// Index representing the default Space Engineers font with regular styling.
				/// Equivalent to <c>(0, 0)</c>.
				/// </summary>
				public static Vector2I Default => Vector2I.Zero;

				/// <summary>
				/// Read-only list of all fonts currently registered with RHM.
				/// Includes both built-in fonts and any fonts added by mods.
				/// </summary>
				public static IReadOnlyList<IFontMin> Fonts => Instance.fonts;

				private static FontManager Instance
				{
					get { Init(); return instance; }
					set { instance = value; }
				}
				private static FontManager instance;

				private readonly ReadOnlyApiCollection<IFontMin> fonts;
				private readonly Func<FontDefinition, FontMembers?> TryAddFontFunc;
				private readonly Func<string, FontMembers?> GetFontFunc;

				private FontManager() : base(ApiModuleTypes.FontManager, false, true)
				{
					var members = (FontManagerMembers)GetApiData();

					// members.Item1 gives access to the font list (getter by index + count)
					Func<int, IFontMin> fontGetter = x => new FontData(members.Item1.Item1(x));
					fonts = new ReadOnlyApiCollection<IFontMin>(fontGetter, members.Item1.Item2);

					TryAddFontFunc = members.Item2;
					GetFontFunc = members.Item3;
				}

				private static void Init()
				{
					if (instance == null)
						instance = new FontManager();
				}

				/// <exclude/>
				public override void Close()
				{
					instance = null;
				}

				/// <summary>
				/// Attempts to register a new custom font with RHM.
				/// </summary>
				/// <param name="fontData">Complete font definition including name, base size, and all style data.</param>
				/// <returns><c>true</c> if the font was successfully registered</returns>
				public static bool TryAddFont(FontDefinition fontData) =>
					Instance.TryAddFontFunc(fontData) != null;

				/// <summary>
				/// Attempts to register a new custom font and returns the registered font interface if successful.
				/// </summary>
				/// <param name="fontData">Complete font definition.</param>
				/// <param name="font">The newly registered <see cref="IFontMin"/> instance, or <c>null</c> on failure.</param>
				/// <returns><c>true</c> if registration succeeded</returns>
				public static bool TryAddFont(FontDefinition fontData, out IFontMin font)
				{
					FontMembers? members = Instance.TryAddFontFunc(fontData);

					if (members != null)
					{
						font = Instance.fonts[members.Value.Item2];
						return true;
					}
					else
					{
						font = null;
						return false;
					}
				}

				/// <summary>
				/// Retrieves a registered font by its exact name (case insensitive).
				/// </summary>
				/// <param name="name">The unique name the font was registered with.</param>
				/// <returns>The <see cref="IFontMin"/> interface for the font, or <c>null</c> if not found.</returns>
				public static IFontMin GetFont(string name)
				{
					if (name == null)
						return null;

					FontMembers? members = Instance.GetFontFunc(name);

					if (members != null)
						return Instance.fonts[members.Value.Item2];
					else
						return null;
				}

				/// <summary>
				/// Retrieves a registered font by its index in the global font list.
				/// </summary>
				/// <exception cref="IndexOutOfRangeException">Thrown if the index is invalid.</exception>
				public static IFontMin GetFont(int index) =>
					Instance.fonts[index];

				/// <summary>
				/// Returns a <see cref="Vector2I"/> that uniquely identifies a specific style of a font.
				/// </summary>
				/// <param name="name">Name of the font.</param>
				/// <param name="style">Desired style (Regular, Bold, Italic, etc.). Defaults to <see cref="FontStyles.Regular"/>.</param>
				/// <returns>
				/// A <c>Vector2I(x, y)</c> where <c>x</c> is the font index and <c>y</c> is the style index.
				/// Returns <c>(0, 0)</c> (default font, regular) if the font or style is not found.
				/// </returns>
				public static Vector2I GetStyleIndex(string name, FontStyles style = FontStyles.Regular)
				{
					IFontMin font = GetFont(name);
					return new Vector2I(font?.Index ?? 0, (int)style);
				}
			}
		}
	}
}