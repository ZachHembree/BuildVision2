using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
	using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

	/// <summary>
	/// A List Box entry that associates a <see cref="Label"/> with an object of type <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TValue">Data type associated with the entry</typeparam>
	public class ListBoxEntry<TValue> : ListBoxEntry<Label, TValue>
	{ }

	/// <summary>
	/// A concrete implementation of a List Box entry, pairing a text element with a data value.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TValue">Data type associated with the entry</typeparam>
	public class ListBoxEntry<TElement, TValue>
		: SelectionBoxEntryTuple<TElement, TValue>, IListBoxEntry<TElement, TValue>
		where TElement : HudElementBase, IMinLabelElement, new()
	{
		public ListBoxEntry()
		{
			SetElement(new TElement());
			Element.TextBoard.AutoResize = false;
		}

		public override void Reset()
		{
			Enabled = true;
			AllowHighlighting = true;
			AssocMember = default(TValue);
			Element.TextBoard.Clear();
		}

		/// <inheritdoc/>
		public object GetOrSetMember(object data, int memberEnum)
		{
			var member = (ListBoxEntryAccessors)memberEnum;

			switch (member)
			{
				case ListBoxEntryAccessors.Name:
					{
						if (data != null)
							Element.TextBoard.SetText(data as List<RichStringMembers>);
						else
							return Element.TextBoard.GetText().apiData;

						break;
					}
				case ListBoxEntryAccessors.Enabled:
					{
						if (data != null)
							Enabled = (bool)data;
						else
							return Enabled;

						break;
					}
				case ListBoxEntryAccessors.AssocObject:
					{
						if (data != null)
							AssocMember = (TValue)data;
						else
							return AssocMember;

						break;
					}
				case ListBoxEntryAccessors.ID:
					return this;
			}

			return null;
		}
	}
}