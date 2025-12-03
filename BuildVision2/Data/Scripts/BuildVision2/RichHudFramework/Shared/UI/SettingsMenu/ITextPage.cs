using RichHudFramework.UI.Rendering;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Internal text page data accessor enums
		/// </summary>
		/// <exclude/>
		public enum TextPageAccessors : int
		{
			/// <summary>
			/// in/out: IList(RichStringMembers)
			/// </summary>
			GetOrSetHeader = 10,

			/// <summary>
			/// in/out: IList(RichStringMembers)
			/// </summary>
			GetOrSetSubheader = 11,

			/// <summary>
			/// in/out: IList(RichStringMembers)
			/// </summary>
			GetOrSetText = 12,

			/// <summary>
			/// out: TextBuilderMembers
			/// </summary>
			GetTextBuilder = 13,
		}

		/// <summary>
		/// Internal TextPage interface implemented by client and master modules
		/// </summary>
		/// <exclude/>
		public interface ITextPage : ITerminalPage
		{
			/// <summary>
			/// Gets/sets header text
			/// </summary>
			RichText HeaderText { get; set; }

			/// <summary>
			/// Gets/sets subheader text
			/// </summary>
			RichText SubHeaderText { get; set; }

			/// <summary>
			/// Contents of the text box.
			/// </summary>
			RichText Text { get; set; }

			/// <summary>
			/// Text builder used to control the contents of the page
			/// </summary>
			ITextBuilder TextBuilder { get; }
		}
	}
}