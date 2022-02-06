using RichHudFramework;
using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

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
    public sealed class BlockPropertyDuplicator : IReadOnlyBlockPropertyDuplicator
    {
        /// <summary>
        /// Read-only list parallel to block members in <see cref="PropertyBlock"/> indicating which
        /// properties are selected for duplication, and which can be duplicated.
        /// </summary>
        public IReadOnlyList<BlockPropertyDupeEntry> DupeEntries { get; }

        /// <summary>
        /// Returns the block targeted for property duplication.
        /// </summary>
        public PropertyBlock Block { get; private set; }

        private readonly List<BlockPropertyDupeEntry> dupeEntries;
        private BlockData copiedProperties;
        private MyTuple<IMyTerminalBlock, BlockData> backup;

        public BlockPropertyDuplicator()
        {
            dupeEntries = new List<BlockPropertyDupeEntry>();
            DupeEntries = dupeEntries;

            copiedProperties.propertyList = new List<PropertyData>();
            backup.Item2.propertyList = new List<PropertyData>();
        }

        /// <summary>
        /// Assigns the given <see cref="PropertyBlock"/> as the current target and generates
        /// a parallel list of dupe entries correspondin to its block members.
        /// </summary>
        public void SetBlockMembers(PropertyBlock block)
        {
            Reset();
            Block = block;

            // Clear backup if target changes
            if (backup.Item1 != block.TBlock)
            {
                backup.Item1 = null;
                backup.Item2.blockTypeID = null;
                backup.Item2.propertyList.Clear();
            }

            for (int i = 0; i < block.BlockMembers.Count; i++)
            {
                IBlockMember member = block.BlockMembers[i];
                bool canDuplicate = member is IBlockProperty;
                var entry = new BlockPropertyDupeEntry(canDuplicate);

                dupeEntries.Add(entry);
            }
        }

        /// <summary>
        /// Clears current block target and corresponding dupe entries
        /// </summary>
        public void Reset()
        {
            dupeEntries.Clear();
            Block = null;
        }

        /// <summary>
        /// Returns number of selected entries
        /// </summary>
        public int GetSelectedEntryCount()
        {
            int count = 0;

            foreach (BlockPropertyDupeEntry entry in dupeEntries)
            {
                if (entry.isSelectedForDuplication)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Sets the selection <see cref="BlockPropertyDupeEntry"/> at the corresponding index.
        /// Will not allow unsupported/disabled properties to be selected for duplication.
        /// </summary>
        public void SetMemberSelection(int index, bool isSelected)
        {
            BlockPropertyDupeEntry entry = dupeEntries[index];
            IBlockMember member = Block.BlockMembers[index];

            entry.isSelectedForDuplication = isSelected && entry.canDuplicate && member.Enabled;
            dupeEntries[index] = entry;
        }

        /// <summary>
        /// Creates a copy of all valid block properties.
        /// </summary>
        public int CopyAllProperties(bool includeName = true)
        {
            SelectAllProperties(includeName);
            return CopySelectedProperties();
        }

        /// <summary>
        /// Selects all valid block properties for duplication. If includeName == false, the name property
        /// will be excluded, where applicable.
        /// </summary>
        public int SelectAllProperties(bool includeName = true)
        {
            ClearSelection();
            int count = 0;

            for (int i = 0; i < dupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry entry = dupeEntries[i];
                IBlockMember member = Block.BlockMembers[i];

                if (!includeName && i == 0 && (member.PropName == "Name" || member.PropName == "CustomName"))
                {
                    entry.isSelectedForDuplication = false;
                }
                else if (entry.canDuplicate && member.Enabled)
                {
                    entry.isSelectedForDuplication = true;
                    count++;
                }

                dupeEntries[i] = entry;
            }

            return count;
        }

        /// <summary>
        /// Saves serialized copies of the properties currently selcted for duplication.
        /// </summary>
        public int CopySelectedProperties()
        {
            var propertyList = copiedProperties.propertyList;
            propertyList.Clear();

            copiedProperties.blockTypeID = Block.TypeID;

            for (int i = 0; i < dupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry entry = dupeEntries[i];
                IBlockMember member = Block.BlockMembers[i];

                if (entry.isSelectedForDuplication && entry.canDuplicate && member.Enabled)
                {
                    var property = member as IBlockProperty;
                    propertyList.Add(property.GetPropertyData());
                }
            }

            ClearSelection();

            return propertyList.Count;
        }

        /// <summary>
        /// Clears all property selections by setting isSelectedForDuplication to false.
        /// </summary>
        public void ClearSelection()
        {
            for (int i = 0; i < dupeEntries.Count; i++)
            {
                BlockPropertyDupeEntry entry = dupeEntries[i];
                entry.isSelectedForDuplication = false;

                dupeEntries[i] = entry;
            }
        }

        /// <summary>
        /// Writes previously copied properties to the current target block, while saving
        /// the current configuration to allow for undo. If successful, this will return the 
        /// number of properties successfully written; if it fails, this will return -1;
        /// </summary>
        public int TryPasteCopiedProperties()
        {
            if (copiedProperties.propertyList.Count > 0 && copiedProperties.blockTypeID == Block.TypeID)
            {
                backup.Item1 = Block.TBlock;
                Block.ExportSettings(ref backup.Item2);

                return TryPasteSerializedProperties(copiedProperties);
            }
            else
                return 0;
        }

        /// <summary>
        /// Tries to restore backup made before previous paste. If successful, this will return 
        /// the number of properties successfully written; if it fails, this will return
        /// -1;
        /// </summary>
        public int TryUndoPaste()
        {
            if (backup.Item1 == Block.TBlock && backup.Item2.blockTypeID == Block.TypeID)
                return TryPasteSerializedProperties(backup.Item2);
            else
                return -1;
        }

        /// <summary>
        /// Attempts to write the given serialized properties to the current target <see cref="PropertyBlock"/>.
        /// If successful, this will return the number of properties successfully written; if it fails, this will return
        /// -1;
        /// </summary>
        private int TryPasteSerializedProperties(BlockData blockData)
        {
            return Block.ImportSettings(blockData);
        }
    }
}