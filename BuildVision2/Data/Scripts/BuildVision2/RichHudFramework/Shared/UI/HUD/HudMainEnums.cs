using RichHudFramework.UI.Client;
using System;

namespace RichHudFramework
{
	/// <summary>
	/// Represents a method that will handle an event, similar to System.EventHandler.
	/// <remarks>
	/// <para>For input or value change events on UI elements, the sender will usually be 
	/// <see cref="UI.IFocusHandler.InputOwner"/>.
	/// </para>
	/// </remarks>
	/// </summary>
	/// <param name="sender">Reference to the object invoking the event, if given.</param>
	/// <param name="e">Optional arguments</param>
	public delegate void EventHandler(object sender, EventArgs e);

	namespace UI
	{
		/// <summary>
		/// Used to indicate the global input state of the framework
		/// </summary>
		public enum HudInputMode : int
		{
			/// <summary>
			/// No UI nodes should process mouse or text input; only context-aware custom input should be used.
			/// </summary>
			NoInput = 0,

			/// <summary>
			/// Mouse cursor is visible and can interact with UI, but text input fields are disabled.
			/// </summary>
			CursorOnly = 1,

			/// <summary>
			/// Full interaction: cursor visible, clicking, scrolling, and text input all active.
			/// </summary>
			Full = 2
		}

		/// <summary>
		/// Internal API accessor indices for <see cref="HudMain"/>. 
		/// Used exclusively by the framework's internal APIs.
		/// </summary>
		/// <exclude/>
		public enum HudMainAccessors : int
		{
			/// <summary>
			/// out: float - Current viewport width in pixels
			/// </summary>
			ScreenWidth = 1,

			/// <summary>
			/// out: float - Current viewport height in pixels
			/// </summary>
			ScreenHeight = 2,

			/// <summary>
			/// out: float - ScreenWidth / ScreenHeight
			/// </summary>
			AspectRatio = 3,

			/// <summary>
			/// out: float - UI resolution scaling factor (usually matches game GUI scale)
			/// </summary>
			ResScale = 4,

			/// <summary>
			/// out: float - Current field of view in radians (affects 3D HUD projection)
			/// </summary>
			Fov = 5,

			/// <summary>
			/// out: float
			/// </summary>
			FovScale = 6,

			/// <summary>
			/// out: MatrixD
			/// </summary>
			PixelToWorldTransform = 7,

			/// <summary>
			/// in/out: RichText
			/// </summary>
			ClipBoard = 8,

			/// <summary>
			/// out: float
			/// </summary>
			UiBkOpacity = 9,

			/// <summary>
			/// in/out: bool
			/// </summary>
			EnableCursor = 10,

			/// <summary>
			/// [Deprecated] in/out: bool
			/// </summary>
			RefreshDrawList = 11,

			/// <summary>
			/// [Deprecated] in/out: Action&lt;List&lt;HudUpdateAccessors&gt;, byte&gt;
			/// </summary>
			GetUpdateAccessorsOld = 12,

			/// <summary>
			/// out: byte, in: Action&lt;byte&gt;
			/// </summary>
			GetFocusOffset = 13,

			/// <summary>
			/// out: HudSpaceDelegate
			/// </summary>
			GetPixelSpaceFunc = 14,

			/// <summary>
			/// out: Func&lt;Vector3D&gt;
			/// </summary>
			GetPixelSpaceOriginFunc = 15,

			/// <summary>
			/// in: Action
			/// </summary>
			GetInputFocus = 16,

			/// <summary>
			/// out: int
			/// </summary>
			TreeRefreshRate = 17,

			/// <summary>
			/// out: HudInputMode
			/// </summary>
			InputMode = 18,

			/// <summary>
			/// in: Action
			/// </summary>
			SetBeforeDrawCallback = 19,

			/// <summary>
			/// in: Action
			/// </summary>
			SetAfterDrawCallback = 20,

			/// <summary>
			/// in: Action
			/// </summary>
			SetBeforeInputCallback = 21,

			/// <summary>
			/// in: Action
			/// </summary>
			SetAfterInputCallback = 22,

			/// <summary>
			/// in/out: object - Gets/sets the root client HUD node
			/// </summary>
			ClientRootNode = 23,
		}

		/// <summary>
		/// Internal shared API accessors for list box entries used by the Rich HUD Terminal
		/// </summary>
		/// <exclude/>
		public enum ListBoxEntryAccessors : int
		{
			/// <summary>
			/// in/out: IList&lt;RichStringMembers&gt;
			/// </summary>
			Name = 1,

			/// <summary>
			/// in/out: bool
			/// </summary>
			Enabled = 2,

			/// <summary>
			/// in/out: object 
			/// </summary>
			AssocObject = 3,

			/// <summary>
			/// out: object
			/// </summary>
			ID = 4,
		}

		/// <summary>
		/// Internal API accessors for shared list box used by the Rich HUD Terminal
		/// </summary>
		/// <exclude/>
		public enum ListBoxAccessors : int
		{
			/// <summary>
			/// out: IReadOnlyList&lt;ListBoxEntry&gt; - Access to underlying collection and its API members
			/// </summary>
			ListMembers = 1,

			/// <summary>
			/// in: MyTuple&lt;IList&lt;RichStringMembers&gt;, T&gt; nameAndData, out: ApiMemberAccessor - Adds new entry and returns its API handle
			/// </summary>
			Add = 2,

			/// <summary>
			/// out: ListBoxEntry - Currently selected entry (or null)
			/// </summary>
			Selection = 3,

			/// <summary>
			/// out: int - Index of currently selected entry (-1 if none)
			/// </summary>
			SelectionIndex = 4,

			/// <summary>
			/// in: T assocObject - Selects the entry whose AssocObject matches the given value
			/// </summary>
			SetSelectionAtData = 5,

			/// <summary>
			/// in: MyTuple&lt;int index, IList&lt;RichStringMembers&gt;, T data&gt; - Inserts a new entry at the specified index
			/// </summary>
			Insert = 6,

			/// <summary>
			/// in: ListBoxEntry entry, out: bool - Removes the specified entry; returns true if found and removed
			/// </summary>
			Remove = 7,

			/// <summary>
			/// in: int index - Removes entry at the given index
			/// </summary>
			RemoveAt = 8,

			/// <summary>
			/// void - Removes all entries from the list
			/// </summary>
			ClearEntries = 9
		}
	}
}

// Empty namespaces for shared source with master module
namespace RichHudFramework.UI.Server { }
namespace RichHudFramework.UI.Rendering.Server { }