using System;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace RichHudFramework.UI
{
	using Rendering;
	using System.Collections;

	/// <summary>
	/// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
	/// <para>
	/// Alias of <see cref="Dropdown{TContainer, TElement, TValue}"/> using 
	/// <see cref="ListBoxEntry{TValue}"/> and <see cref="Label"/> as the container and element, respectively.
	/// </para>
	/// </summary>
	/// <typeparam name="TValue">Value paired with the list entry</typeparam>
	public class Dropdown<TValue> : Dropdown<ListBoxEntry<TValue>, Label, TValue>
	{
		public Dropdown(HudParentBase parent) : base(parent)
		{ }

		public Dropdown() : base(null)
		{ }
	}

	/// <summary>
	/// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
	/// <para>
	/// Alias of <see cref="Dropdown{TContainer, TElement, TValue}"/> using 
	/// <see cref="ListBoxEntry{TValue, TValue}"/> as the container.
	/// </para>
	/// </summary>
	/// <typeparam name="TElement">UI element in the list</typeparam>
	/// <typeparam name="TValue">Value paired with the list entry</typeparam>
	public class Dropdown<TElement, TValue> : Dropdown<ListBoxEntry<TElement, TValue>, TElement, TValue>
		where TElement : HudElementBase, IMinLabelElement, new()
	{
		public Dropdown(HudParentBase parent) : base(parent)
		{ }

		public Dropdown() : base(null)
		{ }
	}

	/// <summary>
	/// Generic collapsable list box. Allows use of custom entry element types.
	/// Designed to mimic the appearance of the dropdown in the SE terminal.
	/// </summary>
	/// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
	/// <typeparam name="TElement">UI element in the list</typeparam>
	/// <typeparam name="TValue">Value paired with the list entry</typeparam>
	public class Dropdown<TContainer, TElement, TValue>
		: HudElementBase, IClickableElement, IEntryBox<TContainer, TElement>
		where TContainer : class, IListBoxEntry<TElement, TValue>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// Invoked when a member of the list is selected.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { listBox.ValueChanged += value; }
			remove { listBox.ValueChanged -= value; }
		}

		/// <summary>
		/// Event initializer utility for SelectionChanged
		/// </summary>
		public EventHandler UpdateValueCallback { set { listBox.ValueChanged += value; } }

		/// <summary>
		/// List of entries in the dropdown.
		/// </summary>
		public IReadOnlyList<TContainer> EntryList => listBox.EntryList;

		/// <summary>
		/// Read-only collection of list entries.
		/// </summary>
		public IReadOnlyHudCollection<TContainer, TElement> HudCollection => listBox.EntryChain;

		/// <summary>
		/// Used to allow the addition of list entries using collection-initializer syntax in
		/// conjunction with normal initializers.
		/// </summary>
		public Dropdown<TContainer, TElement, TValue> ListContainer => this;

		/// <summary>
		/// Height of the dropdown list
		/// </summary>
		public float DropdownHeight { get { return listBox.Height; } set { listBox.Height = value; } }

		/// <summary>
		/// Padding applied to list members.
		/// </summary>
		public Vector2 MemberPadding { get { return listBox.MemberPadding; } set { listBox.MemberPadding = value; } }

		/// <summary>
		/// Height of entries in the dropdown.
		/// </summary>
		public float LineHeight { get { return listBox.LineHeight; } set { listBox.LineHeight = value; } }

		/// <summary>
		/// Default format for member text;
		/// </summary>
		public GlyphFormat Format { get { return listBox.Format; } set { listBox.Format = value; display.Format = value; } }

		/// <summary>
		/// Background color of the dropdown list
		/// </summary>
		public Color Color { get { return listBox.Color; } set { listBox.Color = value; } }

		/// <summary>
		/// Color of the slider bar
		/// </summary>
		public Color BarColor { get { return listBox.BarColor; } set { listBox.BarColor = value; } }

		/// <summary>
		/// Bar color when moused over
		/// </summary>
		public Color BarHighlight { get { return listBox.BarHighlight; } set { listBox.BarHighlight = value; } }

		/// <summary>
		/// Color of the slider box when not moused over
		/// </summary>
		public Color SliderColor { get { return listBox.SliderColor; } set { listBox.SliderColor = value; } }

		/// <summary>
		/// Color of the slider button when moused over
		/// </summary>
		public Color SliderHighlight { get { return listBox.SliderHighlight; } set { listBox.SliderHighlight = value; } }

		/// <summary>
		/// Background color of the highlight box
		/// </summary>
		public Color HighlightColor { get { return listBox.HighlightColor; } set { listBox.HighlightColor = value; } }

		/// <summary>
		/// Color of the highlight box's tab
		/// </summary>
		public Color TabColor { get { return listBox.TabColor; } set { listBox.TabColor = value; } }

		/// <summary>
		/// Padding applied to the highlight box.
		/// </summary>
		public Vector2 HighlightPadding { get { return listBox.HighlightPadding; } set { listBox.HighlightPadding = value; } }

		/// <summary>
		/// Minimum number of elements visible in the list at any given time.
		/// </summary>
		public int MinVisibleCount { get { return listBox.MinVisibleCount; } set { listBox.MinVisibleCount = value; } }

		/// <summary>
		/// Current selection. Null if empty.
		/// </summary>
		public TContainer Value => listBox.Value;

		/// <summary>
		/// Index of the current selection. -1 if empty.
		/// </summary>
		public int SelectionIndex => listBox.SelectionIndex;

		/// <summary>
		/// Interface used to manage the element's input focus state
		/// </summary>
		public IFocusHandler FocusHandler => display.FocusHandler;

		/// <summary>
		/// Mouse input for the dropdown display.
		/// </summary>
		public IMouseInput MouseInput => display.MouseInput;

		/// <summary>
		/// Indicates whether or not the dropdown is moused over.
		/// </summary>
		public override bool IsMousedOver => display.IsMousedOver || listBox.IsMousedOver;

		/// <summary>
		/// Indicates whether or not the list is open.
		/// </summary>
		public bool Open => listBox.Visible;

		/// <summary>
		/// Selection box attached to the dropdown button
		/// </summary>
		/// <exclude/>
		protected readonly ListBox<TContainer, TElement, TValue> listBox;

		/// <summary>
		/// Dropdown display/button
		/// </summary>
		/// <exclude/>
		protected readonly DropdownDisplay display;

		/// <summary>
		/// Flag to grab input focus on next input update
		/// </summary>
		/// <exclude/>
		protected bool getDispFocus;

		public Dropdown(HudParentBase parent) : base(parent)
		{
			display = new DropdownDisplay(this)
			{
				DimAlignment = DimAlignments.UnpaddedSize,
				Text = "None"
			};
			
			listBox = new ListBox<TContainer, TElement, TValue>(this)
			{
				Visible = false,
				CanIgnoreMasking = true,
				ZOffset = 3,
				DimAlignment = DimAlignments.Width,
				ParentAlignment = ParentAlignments.Bottom,
				TabColor = new Color(0, 0, 0, 0),
			};
			listBox.FocusHandler.InputOwner = this;

			Size = new Vector2(300f, 43f);
			DropdownHeight = 100f;

			display.MouseInput.LeftClicked += ClickDisplay;
			ValueChanged += UpdateDisplay;
		}

		public Dropdown() : this(null)
		{ }

		/// <summary>
		/// Updates mouse input and display focus
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (SharedBinds.LeftButton.IsNewPressed && !(display.IsMousedOver || listBox.IsMousedOver))
				CloseList();

			if (getDispFocus)
			{
				display.FocusHandler.GetInputFocus();
				getDispFocus = false;
			}
		}

		/// <summary>
		/// Updates display formatting and value to match the current selection
		/// </summary>
		/// <exclude/>
		protected virtual void UpdateDisplay(object sender, EventArgs args)
		{
			if (Value != null)
			{
				var fmt = display.FocusHandler.HasFocus ? Format.WithColor(listBox.FocusTextColor) : Format;
				display.name.TextBoard.SetText(Value.Element.TextBoard.ToString(), fmt);
				CloseList();
			}
		}

		/// <summary>
		/// Handles dropdown display click callback
		/// </summary>
		protected virtual void ClickDisplay(object sender, EventArgs args)
		{
			if (!listBox.Visible)
				OpenList();
			else
				CloseList();
		}

		/// <summary>
		/// Expands the dropdown list and captures input focus
		/// </summary>
		public void OpenList()
		{
			if (!listBox.Visible)
			{
				listBox.Visible = true;
				listBox.FocusHandler.GetInputFocus();
			}
		}

		/// <summary>
		/// Collapses the dropdown list and returns focus to the display button
		/// </summary>
		public void CloseList()
		{
			if (listBox.Visible)
			{
				listBox.Visible = false;
				getDispFocus = true;
			}
		}

		/// <summary>
		/// Adds a new entry to the dropdown with the given name, associated value, and enabled state.
		/// </summary>
		/// <param name="name">Text to display for this entry</param>
		/// <param name="assocMember">Value associated with this entry</param>
		/// <param name="enabled">Determines if the entry is selectable and visible</param>
		public TContainer Add(RichText name, TValue assocMember, bool enabled = true) =>
			listBox.Add(name, assocMember, enabled);

		/// <summary>
		/// Adds a range of entries to the dropdown from a list of tuples.
		/// </summary>
		public void AddRange(IReadOnlyList<MyTuple<RichText, TValue, bool>> entries) =>
			listBox.AddRange(entries);

		/// <summary>
		/// Inserts a new entry at the specified index.
		/// </summary>
		/// <param name="index">Index at which to insert the entry</param>
		/// <param name="name">Text to display for this entry</param>
		/// <param name="assocMember">Value associated with this entry</param>
		/// <param name="enabled">Determines if the entry is selectable and visible</param>
		public void Insert(int index, RichText name, TValue assocMember, bool enabled = true) =>
			listBox.Insert(index, name, assocMember, enabled);

		/// <summary>
		/// Removes the entry at the specified index.
		/// </summary>
		public void RemoveAt(int index) =>
			listBox.RemoveAt(index);

		/// <summary>
		/// Removes the specified container entry from the dropdown.
		/// </summary>
		/// <returns>True if the entry was successfully removed</returns>
		public bool Remove(TContainer entry) =>
			listBox.Remove(entry);

		/// <summary>
		/// Removes a range of entries starting from the specified index.
		/// </summary>
		public void RemoveRange(int index, int count) =>
			listBox.RemoveRange(index, count);

		/// <summary>
		/// Removes all entries from the dropdown.
		/// </summary>
		public void ClearEntries() =>
			listBox.ClearEntries();

		/// <summary>
		/// Sets the selection to the entry at the specified index.
		/// </summary>
		public void SetSelectionAt(int index) =>
			listBox.SetSelectionAt(index);

		/// <summary>
		/// Sets the selection to the first entry associated with the given value.
		/// </summary>
		public void SetSelection(TValue assocMember) =>
			listBox.SetSelection(assocMember);

		/// <summary>
		/// Sets the selection to the specified container object.
		/// </summary>
		public void SetSelection(TContainer member) =>
			listBox.SetSelection(member);

		/// <summary>
		/// Internal API interop method
		/// </summary>
		/// <exclude/>
		public object GetOrSetMember(object data, int memberEnum) =>
		 listBox.GetOrSetMember(data, memberEnum);

		public IEnumerator<TContainer> GetEnumerator() =>
			listBox.EntryList.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		/// <summary>
		/// Custom button used for dropdown label
		/// </summary>
		/// <exclude/>
		protected class DropdownDisplay : Button
		{
			private static readonly Material arrowMat = new Material("RichHudDownArrow", new Vector2(64f, 64f));

			public RichText Text { get { return name.Text; } set { name.Text = value; } }

			public GlyphFormat Format
			{
				get { return name.Format; }
				set { name.Format = value; }
			}

			/// <summary>
			/// Color of the border surrounding the button
			/// </summary>
			public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

			/// <summary>
			/// Thickness of the border surrounding the button
			/// </summary>
			public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

			/// <summary>
			/// Text color used when the control gains focus.
			/// </summary>
			public Color FocusTextColor { get; set; }

			/// <summary>
			/// Background color used when the control gains focus.
			/// </summary>
			public Color FocusColor { get; set; }

			/// <summary>
			/// If true, then the button will change formatting when it takes focus.
			/// </summary>
			public bool UseFocusFormatting { get; set; }

			public readonly Label name;
			public readonly TexturedBox arrow, divider;

			private readonly BorderBox border;
			private Color lastTextColor;

			public DropdownDisplay(HudParentBase parent = null) : base(parent)
			{
				border = new BorderBox(this)
				{
					Thickness = 1f,
					DimAlignment = DimAlignments.UnpaddedSize,
				};

				name = new Label()
				{
					AutoResize = false,
					Padding = new Vector2(10f, 0f)
				};

				divider = new TexturedBox()
				{
					Padding = new Vector2(4f, 17f),
					Width = 2f,
					Color = new Color(104, 113, 120),
				};

				arrow = new TexturedBox()
				{
					Width = 38f,
					MatAlignment = MaterialAlignment.FitVertical,
					Material = arrowMat,
				};

				var layout = new HudChain(false, this)
				{
					SizingMode = HudChainSizingModes.FitMembersOffAxis,
					DimAlignment = DimAlignments.UnpaddedSize,
					CollectionContainer = { { name, 1f }, divider, arrow }
				};

				Format = TerminalFormatting.ControlFormat;
				FocusTextColor = TerminalFormatting.Charcoal;

				Color = TerminalFormatting.OuterSpace;
				HighlightColor = TerminalFormatting.Atomic;
				FocusColor = TerminalFormatting.Mint;
				BorderColor = TerminalFormatting.LimedSpruce;

				HighlightEnabled = true;
				UseFocusFormatting = true;

				FocusHandler.GainedInputFocus += GainFocus;
				FocusHandler.LostInputFocus += LoseFocus;
			}

			protected override void HandleInput(Vector2 cursorPos)
			{
				if (FocusHandler.HasFocus)
				{
					if (SharedBinds.Space.IsNewPressed)
					{
						_mouseInput.LeftClick();
					}
				}
				else if (!MouseInput.IsMousedOver)
				{
					lastBackgroundColor = Color;
					lastTextColor = name.Format.Color;
				}
			}

			protected override void CursorEnter(object sender, EventArgs args)
			{
				if (HighlightEnabled)
				{
					if (!UseFocusFormatting || !FocusHandler.HasFocus)
						lastBackgroundColor = Color;

					if (UseFocusFormatting)
					{
						if (!FocusHandler.HasFocus)
							lastTextColor = name.Format.Color;

						name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));
					}

					Color = HighlightColor;
					divider.Color = lastTextColor.SetAlphaPct(0.8f);
					arrow.Color = lastTextColor;
				}
			}

			protected override void CursorExit(object sender, EventArgs args)
			{
				if (HighlightEnabled)
				{
					if (UseFocusFormatting && FocusHandler.HasFocus)
					{
						Color = FocusColor;
						name.TextBoard.SetFormatting(name.Format.WithColor(FocusTextColor));

						divider.Color = FocusTextColor.SetAlphaPct(0.8f);
						arrow.Color = FocusTextColor;
					}
					else
					{
						Color = lastBackgroundColor;

						if (UseFocusFormatting)
							name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));

						divider.Color = lastTextColor.SetAlphaPct(0.8f);
						arrow.Color = lastTextColor;
					}
				}
			}

			private void GainFocus(object sender, EventArgs args)
			{
				if (UseFocusFormatting)
				{
					if (!MouseInput.IsMousedOver)
					{
						lastBackgroundColor = Color;
						lastTextColor = name.Format.Color;
					}

					Color = FocusColor;
					name.TextBoard.SetFormatting(name.Format.WithColor(FocusTextColor));

					divider.Color = FocusTextColor.SetAlphaPct(0.8f);
					arrow.Color = FocusTextColor;
				}
			}

			private void LoseFocus(object sender, EventArgs args)
			{
				if (UseFocusFormatting)
				{
					Color = lastBackgroundColor;
					name.TextBoard.SetFormatting(name.Format.WithColor(lastTextColor));

					divider.Color = lastTextColor.SetAlphaPct(0.8f);
					arrow.Color = lastTextColor;
				}
			}
		}
	}
}