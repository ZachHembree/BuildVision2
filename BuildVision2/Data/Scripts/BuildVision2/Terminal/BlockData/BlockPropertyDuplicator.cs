using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Entry for <see cref="BlockPropertyDuplicator"/> used to track which properties in its associated
    /// <see cref="PropertyBlock"/> can be/are being duplicated.
    /// </summary>
    public struct BlockPropertyDupeEntry
    {
        /// <summary>
        /// Set to true if the block member corresponding to the given index has been
        /// selected for duplication.
        /// </summary>
        public bool isSelectedForDuplication;

        /// <summary>
        /// Returns true if the member is a valid target for duplication. Block actions,
        /// for instance, are not.
        /// </summary>
        public readonly bool canDuplicate;

        public BlockPropertyDupeEntry(bool canDuplicate)
        {
            isSelectedForDuplication = false;
            this.canDuplicate = canDuplicate;
        }
    }

    /// <summary>
    /// Class responsible for managing duplication of block properties in <see cref="PropertyBlock"/>s
    /// </summary>
    public sealed class BlockPropertyDuplicator
    {
        /// <summary>
        /// Read-only list parallel to block members in <see cref="PropertyBlock"/> indicating which
        /// properties are selected for duplication, and which can be duplicated.
        /// </summary>
        public IReadOnlyList<BlockPropertyDupeEntry> PropertyDupeEntries { get; }

        /// <summary>
        /// Read-only list of block members contained by the associated <see cref="PropertyBlock"/>.
        /// </summary>
        public IReadOnlyList<IBlockMember> BlockMembers { get; private set; }

        /// <summary>
        /// Returns the block targeted for property duplication.
        /// </summary>
        public PropertyBlock Block { get; private set; }

        private readonly List<BlockPropertyDupeEntry> propertyDupeEntries;
        private BlockData copiedProperties, backup;

        public BlockPropertyDuplicator()
        {
            propertyDupeEntries = new List<BlockPropertyDupeEntry>();
            PropertyDupeEntries = propertyDupeEntries;

            copiedProperties.propertyList = new List<PropertyData>();
            backup.propertyList = new List<PropertyData>();
        }

        /// <summary>
        /// Assigns the given <see cref="PropertyBlock"/> as the current target and generates
        /// a parallel list of dupe entries correspondin to its block members.
        /// </summary>
        public void SetBlockMembers(PropertyBlock block)
        {
            Reset();
            Block = block;
            BlockMembers = block.BlockMembers;

            for (int i = 0; i < block.BlockMembers.Count; i++)
            {
                IBlockMember member = block.BlockMembers[i];
                bool canDuplicate = member is IBlockProperty;
                var dupeContainer = new BlockPropertyDupeEntry(canDuplicate);
            }
        }

        /// <summary>
        /// Clears current block target and corresponding dupe entries
        /// </summary>
        public void Reset()
        {
            propertyDupeEntries.Clear();
            Block = null;
            BlockMembers = null;

            // Reset backup
            backup.propertyList.Clear();
            backup.blockTypeID = null;
        }

        /// <summary>
        /// Sets the selection <see cref="BlockPropertyDupeEntry"/> at the corresponding index.
        /// Will not allow unsupported/disabled properties to be selected for duplication.
        /// </summary>
        public void SetMemberSelection(int index, bool isSelected)
        {
            BlockPropertyDupeEntry entry = propertyDupeEntries[index];
            IBlockMember member = Block.BlockMembers[index];

            entry.isSelectedForDuplication = isSelected && entry.canDuplicate && member.Enabled;
        }

        /// <summary>
        /// Creates a copy of all valid block properties.
        /// </summary>
        public void CopyAllProperties(bool includeName = true)
        {
            SelectAllProperties(includeName);
            CopySelectedProperties();
        }

        /// <summary>
        /// Selects all valid block properties for duplication. If includeName == false, the name property
        /// will be excluded, where applicable.
        /// </summary>
        public void SelectAllProperties(bool includeName = true)
        {
            ClearSelection();

            for (int i = 0; i < propertyDupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry container = propertyDupeEntries[i];
                IBlockMember member = Block.BlockMembers[i];

                if (includeName && i == 0 && (member.PropName == "Name" || member.PropName == "CustomName"))
                {
                    container.isSelectedForDuplication = false;
                }
                else if (container.canDuplicate && member.Enabled)
                {
                    container.isSelectedForDuplication = true;
                }

                propertyDupeEntries[i] = container;
            }
        }

        /// <summary>
        /// Saves serialized copies of the properties currently selcted for duplication.
        /// </summary>
        public void CopySelectedProperties()
        {
            var propertyList = copiedProperties.propertyList;

            copiedProperties.blockTypeID = Block.TypeID;
            propertyList.Clear();

            for (int i = 0; i < propertyDupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry entry = propertyDupeEntries[i];
                IBlockMember member = Block.BlockMembers[i];

                if (entry.isSelectedForDuplication && entry.canDuplicate && member.Enabled)
                {
                    var property = member as IBlockProperty;
                    propertyList.Add(property.GetPropertyData());
                }
            }

            ClearSelection();
        }

        /// <summary>
        /// Clears all property selections by setting isSelectedForDuplication to false.
        /// </summary>
        public void ClearSelection()
        {
            for (int i = 0; i < propertyDupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry container = propertyDupeEntries[i];
                container.isSelectedForDuplication = false;

                propertyDupeEntries[i] = container;
            }
        }

        /// <summary>
        /// Writes previously copied properties to the current target block, while saving
        /// the current configuration to allow for undo;
        /// </summary>
        public int TryPasteCopiedProperties()
        {
            Block.ExportSettings(ref backup);
            return TryPasteSerializedProperties(copiedProperties);
        }

        /// <summary>
        /// Restores backup made before previous paste
        /// </summary>
        public int TryUndoPaste()
        {
            return TryPasteSerializedProperties(backup);
        }

        /// <summary>
        /// Attempts to write the given serialized properties to the current target <see cref="PropertyBlock"/>.
        /// If successful, this will return the number of properties successfully written; if it fails, this will return
        /// -1;
        /// </summary>
        private int TryPasteSerializedProperties(BlockData blockData)
        {
            if (blockData.blockTypeID == Block.TypeID)
            {
                return Block.ImportSettings(blockData);
            }
            else
                return -1;
        }
    }
}