using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using RichHudFramework.UI;
using RichHudFramework;

namespace DarkHelmet.BuildVision2
{
    public interface IPropertyBlock
    {
        /// <summary>
        /// Block type identifier. Uses IMyCubeBlock.BlockDefinition.TypeIdString.
        /// </summary>
        string TypeID { get; }

        /// <summary>
        /// Returns the position of the block in world space.
        /// </summary>
        Vector3D Position { get; }

        /// <summary>
        /// True if the block integrity is above its breaking threshold
        /// </summary>
        bool IsFunctional { get; }

        /// <summary>
        /// True if the block is functional and able to do work.
        /// </summary>
        bool IsWorking { get; }

        /// <summary>
        /// True if the local player has terminal access permissions
        /// </summary>
        bool CanLocalPlayerAccess { get; }

        /// <summary>
        /// Indicates the subtypes supported by the block.
        /// </summary>
        TBlockSubtypes SubtypeId { get; }

        /// <summary>
        /// Total number of block members currently enabled and visible
        /// </summary>
        int EnabledMemberCount { get; }

        /// <summary>
        /// The difference between the center of the bounding box and the position reported by
        /// GetPosition().
        /// </summary>
        Vector3D ModelOffset { get; }

        /// <summary>
        /// Controls serialization/deserialization of terminal block properties for duplication
        /// </summary>
        IReadOnlyBlockPropertyDuplicator Duplicator { get; }

        /// <summary>
        /// Controls prioritization of block properties
        /// </summary>
        IReadOnlyBlockPropertyPrioritizer Prioritizer { get; }

        /// <summary>
        /// Read-only collection of block members
        /// </summary>
        IReadOnlyList<IBlockMember> BlockMembers { get; }

        /// <summary>
        /// Appends a summary of the block's current configuration
        /// </summary>
        void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat);

        /// <summary>
        /// Exports block terminal settings as a serializable <see cref="BlockData"/>
        /// </summary>
        void ExportSettings(ref BlockData blockData);

        /// <summary>
        /// Applies property settings from block data and returns the number of properties successfully updated.
        /// </summary>
        int ImportSettings(BlockData src);
    }
}