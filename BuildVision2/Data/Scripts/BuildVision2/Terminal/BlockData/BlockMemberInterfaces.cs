using System;
using System.Collections.Generic;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public enum BlockMemberValueTypes
    {
        None,
        Bool,
        Color,
        Combo,
        Float,
        Text
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
        /// Returns a serializable representation of the property.
        /// </summary>
        PropertyData GetPropertyData();
    }

    /// <summary>
    /// Interface for block terminal settings that support text input.
    /// </summary>
    public interface IBlockTextMember : IBlockProperty
    {
        Func<char, bool> CharFilterFunc { get; }

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

    public interface IBlockNumericValue<T> : IBlockValue<T>
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

    public interface IBlockComboBox : IBlockValue<long>
    {
        /// <summary>
        /// Selection options for combo box
        /// </summary>
        IReadOnlyList<KeyValuePair<long, StringBuilder>> ComboEntries { get; }
    }
}