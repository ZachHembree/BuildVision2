using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Types of data used for <see cref="PropertyBlock"/> members
    /// </summary>
    public enum BlockMemberValueTypes : int
    {
        None = 0,
        Bool = 1,
        Color = 2,
        ColorChannel = 3,
        Combo = 4,
        Float = 5,
        Text = 6,
        ColorHSV = 7,
    }

    /// <summary>
    /// Flags assigned to block member wrappers used w/<see cref="PropertyBlock"/>
    /// </summary>
    [Flags]
    public enum BlockPropertyFlags : int
    {
        None = 0,
        IsIntegral = 0x1,
        CanUseMultipliers = 0x2,
    }

    public interface IBlockMember
    {
        /// <summary>
        /// Unique identifier associated with the property
        /// </summary>
        string PropName { get; }

        /// <summary>
        /// Retrieves the name of the block property
        /// </summary>
        StringBuilder Name { get; }

        /// <summary>
        /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
        /// </summary>
        StringBuilder FormattedValue { get; }

        /// <summary>
        /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
        /// </summary>
        StringBuilder ValueText { get; }

        /// <summary>
        /// Additional information following the value of the member.
        /// </summary>
        StringBuilder StatusText { get; }

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Returns the type of data stored by this member, if any.
        /// </summary>
        BlockMemberValueTypes ValueType { get; }
    }

    public interface IBlockAction : IBlockMember
    {
        void Action();
    }

    public interface IBlockProperty : IBlockMember
    {
        /// <summary>
        /// Returns flags associated with the property
        /// </summary>
        BlockPropertyFlags Flags { get; }

        /// <summary>
        /// Returns a serializable representation of the property.
        /// </summary>
        PropertyData? GetPropertyData();
    }

    /// <summary>
    /// Interface for block terminal settings that support text input.
    /// </summary>
    public interface IBlockTextMember : IBlockProperty
    {
        /// <summary>
        /// Delegate used for filtering text input. Returns true if a given character is in the accepted range.
        /// </summary>
        Func<char, bool> CharFilterFunc { get; }

        /// <summary>
        /// Assigns value as string, parses if nececessary.
        /// </summary>
        void SetValueText(string text);
    }

    /// <summary>
    /// Interface for terminal settings that store data
    /// </summary>
    public interface IBlockValue<T> : IBlockProperty
    {
        /// <summary>
        /// Gets/sets the member's value
        /// </summary>
        T Value { get; set; }   
    }

    public interface IBlockNumericValue<T> : IBlockValue<T>, IBlockTextMember
        where T : struct, IEquatable<T>
    {
        /// <summary>
        /// Minimum allowable value
        /// </summary>
        T MinValue { get; }

        /// <summary>
        /// Maximum allowable value
        /// </summary>
        T MaxValue{ get; }

        /// <summary>
        /// Standard increment
        /// </summary>
        T Increment { get; }
    }

    public interface IBlockColor : IBlockNumericValue<Color>
    {
        /// <summary>
        /// RGB Color channels presented as individual byte properties
        /// </summary>
        IReadOnlyList<IBlockNumericValue<byte>> ColorChannels { get; }
    }

    public interface IBlockColorHSV : IBlockNumericValue<Vector3>
    {
        /// <summary>
        /// HSV Color channels presented as individual float properties
        /// </summary>
        IReadOnlyList<IBlockNumericValue<float>> ColorChannels { get; }
    }

    public interface IBlockComboBox : IBlockValue<long>
    {
        /// <summary>
        /// Localized selection options for combo box
        /// </summary>
        IReadOnlyList<KeyValuePair<long, StringBuilder>> ComboEntries { get; }
    }
}