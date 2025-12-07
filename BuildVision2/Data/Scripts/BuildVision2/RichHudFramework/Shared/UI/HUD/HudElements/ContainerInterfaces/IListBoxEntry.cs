namespace RichHudFramework.UI
{
	/// <summary>
	/// Interface implemented by objects that function as list box entries, 
	/// pairing specific data types with a label-based UI element.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TValue">Data type associated with the entry</typeparam>
	public interface IListBoxEntry<TElement, TValue>
		: ISelectionBoxEntryTuple<TElement, TValue>
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// API interop method used by the Rich HUD Terminal to access member data dynamically.
		/// </summary>
		/// <exclude/>
		object GetOrSetMember(object data, int memberEnum);
	}
}