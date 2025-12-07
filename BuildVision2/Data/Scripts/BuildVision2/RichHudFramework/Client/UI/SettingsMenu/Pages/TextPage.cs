using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
	using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

	namespace UI.Client
	{
		using TextBuilderMembers = MyTuple<
			MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
			Func<Vector2I, int, object>, // GetCharMember
			Func<object, int, object>, // GetOrSetMember
			Action<IList<RichStringMembers>, Vector2I>, // Insert
			Action<IList<RichStringMembers>>, // SetText
			Action // Clear
		>;

		/// <summary>
		/// A terminal page dedicated to displaying read-only rich text. 
		/// <para>Useful for help screens, changelogs, or information displays.</para>
		/// </summary>
		public class TextPage : TerminalPageBase, ITextPage
		{
			/// <summary>
			/// Gets or sets the formatted header text displayed at the top of the page.
			/// </summary>
			public RichText HeaderText
			{
				get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetHeader) as List<RichStringMembers>); }
				set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetHeader); }
			}

			/// <summary>
			/// Gets or sets the formatted subheader text displayed below the header.
			/// </summary>
			public RichText SubHeaderText
			{
				get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetSubheader) as List<RichStringMembers>); }
				set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetSubheader); }
			}

			/// <summary>
			/// Gets or sets the main body content of the text page.
			/// </summary>
			public RichText Text
			{
				get { return new RichText(GetOrSetMemberFunc(null, (int)TextPageAccessors.GetOrSetText) as List<RichStringMembers>); }
				set { GetOrSetMemberFunc(value.apiData, (int)TextPageAccessors.GetOrSetText); }
			}

			/// <summary>
			/// Provides direct access to the underlying text builder for advanced manipulation (e.g., appending, inserting).
			/// </summary>
			public ITextBuilder TextBuilder { get; }

			public TextPage() : base(ModPages.TextPage)
			{
				TextBuilder = new BasicTextBuilder((TextBuilderMembers)GetOrSetMemberFunc(null, (int)TextPageAccessors.GetTextBuilder));
			}

			/// <summary>
			/// Wrapper for the internal API TextBuilder.
			/// </summary>
			private class BasicTextBuilder : TextBuilder
			{
				public BasicTextBuilder(TextBuilderMembers members) : base(members)
				{ }
			}
		}
	}
}