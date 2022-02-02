using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public interface IReadOnlyBlockPropertyDuplicator
    {
        /// <summary>
        /// Returns the block targeted for property duplication.
        /// </summary>
        PropertyBlock Block { get; }

        /// <summary>
        /// Read-only list parallel to block members in <see cref="PropertyBlock"/> indicating which
        /// properties are selected for duplication, and which can be duplicated.
        /// </summary>
        IReadOnlyList<BlockPropertyDupeEntry> DupeEntries { get; }

        /// <summary>
        /// Clears all property selections by setting isSelectedForDuplication to false.
        /// </summary>
        void ClearSelection();

        /// <summary>
        /// Creates a copy of all valid block properties.
        /// </summary>
        int CopyAllProperties(bool includeName = true);

        /// <summary>
        /// Saves serialized copies of the properties currently selcted for duplication.
        /// </summary>
        int CopySelectedProperties();

        /// <summary>
        /// Selects all valid block properties for duplication. If includeName == false, the name property
        /// will be excluded, where applicable.
        /// </summary>
        int SelectAllProperties(bool includeName = true);

        /// <summary>
        /// Sets the selection <see cref="BlockPropertyDupeEntry"/> at the corresponding index.
        /// Will not allow unsupported/disabled properties to be selected for duplication.
        /// </summary>
        void SetMemberSelection(int index, bool isSelected);

        /// <summary>
        /// Writes previously copied properties to the current target block, while saving
        /// the current configuration to allow for undo;
        /// </summary>
        int TryPasteCopiedProperties();

        /// <summary>
        /// Tries to restore backup made before previous paste
        /// </summary>
        int TryUndoPaste();
    }
}