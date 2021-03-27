using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
    public enum ListBoxEntryAccessors : int
    {
        /// <summary>
        /// IList<RichStringMembers>
        /// </summary>
        Name = 1,

        /// <summary>
        /// bool
        /// </summary>
        Enabled = 2,

        /// <summary>
        /// Object
        /// </summary>
        AssocObject = 3,

        /// <summary>
        /// Object
        /// </summary>
        ID = 4,
    }


    /// <summary>
    /// Interface implemented by objects that function as list box entries.
    /// </summary>
    public interface IListBoxEntry<TElement, TValue>
        : ISelectionBoxEntryTuple<TElement, TValue>
        where TElement : HudElementBase, IMinLabelElement
    {
        object GetOrSetMember(object data, int memberEnum);
    }
}