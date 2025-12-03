using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Scrollable list of text elements. Each list entry is associated with a <typeparamref name="TValue"/>
	/// <para>
	/// Alias of <see cref="ListBox{TContainer, TElement, TValue}"/> using 
	/// <see cref="ListBoxEntry{TValue}"/> and <see cref="Label"/> as the container and element, respectively.
	/// </para>
	/// </summary>
	/// <typeparam name="TValue">Value paired with the list entry</typeparam>
	public class ListBox<TValue> : ListBox<ListBoxEntry<TValue>, Label, TValue>
	{
		public ListBox(HudParentBase parent) : base(parent)
		{ }

		public ListBox() : base(null)
		{ }
	}

    /// <summary>
    /// Generic scrollable list of text elements. Allows use of custom entry element types.
    /// Each list entry is associated with a <typeparamref name="TValue"/>
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TValue">Value paired with the list entry</typeparam>
    public class ListBox<TContainer, TElement, TValue>
		: ScrollSelectionBox<TContainer, TElement, TValue>, IClickableElement
		where TContainer : class, IListBoxEntry<TElement, TValue>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// Color of the slider bar
		/// </summary>
		public Color BarColor { get { return EntryChain.BarColor; } set { EntryChain.BarColor = value; } }

		/// <summary>
		/// Bar color when moused over
		/// </summary>
		public Color BarHighlight { get { return EntryChain.BarHighlight; } set { EntryChain.BarHighlight = value; } }

		/// <summary>
		/// Color of the slider box when not moused over
		/// </summary>
		public Color SliderColor { get { return EntryChain.SliderColor; } set { EntryChain.SliderColor = value; } }

		/// <summary>
		/// Color of the slider button when moused over
		/// </summary>
		public Color SliderHighlight { get { return EntryChain.SliderHighlight; } set { EntryChain.SliderHighlight = value; } }

		/// <summary>
		/// Range of element indices representing the visible area, plus padding to allow for smooth clipping.
		/// Used for scissor rect masking.
		/// </summary>
		protected override Vector2I ListRange => EntryChain.ClipRange;

		/// <summary>
		/// Returns dimensions of the area occupied by list entries 
		/// </summary>
		protected override Vector2 ListSize
		{
			get
			{
				Vector2 listSize = EntryChain.Size;
				listSize.X -= EntryChain.ScrollBar.Width;
				return listSize;
			}
		}

		/// <summary>
		/// Returns the coordinates of the center of the list area
		/// </summary>
		protected override Vector2 ListPos
		{
			get
			{
				Vector2 listPos = EntryChain.Position;
				listPos.X -= EntryChain.ScrollBar.Width;

				return listPos;
			}
		}

		public ListBox(HudParentBase parent) : base(parent)
		{
			EntryChain.Padding = new Vector2(0f, 8f);
		}

		public ListBox() : this(null)
		{ }

		/// <summary>
		/// Locks the size of the listbox to the size of the entry container, before padding
		/// </summary>
		/// <exclude/>
		protected override void Measure()
		{
			UnpaddedSize = EntryChain.UnpaddedSize + EntryChain.Padding;
		}
	}

}