using RichHudFramework.UI.Rendering;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Abstract base class for UI elements responsible for rendering text.
		/// </summary>
		public abstract class LabelElementBase : HudElementBase, IMinLabelElement
		{
			/// <summary>
			/// Gets the <see cref="ITextBoard"/> responsible for managing text layout, formatting, and rendering.
			/// </summary>
			public abstract ITextBoard TextBoard { get; }

			public LabelElementBase(HudParentBase parent = null) : base(parent)
			{ }
		}
	}
}