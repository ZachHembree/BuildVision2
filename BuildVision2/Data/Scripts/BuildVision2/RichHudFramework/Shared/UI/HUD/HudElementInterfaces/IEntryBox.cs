using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    /// <summary>
    /// An interface for clickable UI elements that represent of ListBoxEntry elements.
    /// </summary>
    public interface IEntryBox<T> : IEnumerable<ListBoxEntry<T>>, IReadOnlyHudElement
    {
        /// <summary>
        /// Invoked when a member of the list is selected.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        IReadOnlyList<ListBoxEntry<T>> ListEntries { get; }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        ListBoxEntry<T> Selection { get; }
    }
}