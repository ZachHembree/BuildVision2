using System;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public interface IBlockMember
    {
        /// <summary>
        /// Retrieves the name of the block property
        /// </summary>
        StringBuilder Name { get; }

        /// <summary>
        /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
        /// </summary>
        StringBuilder Display { get; }

        /// <summary>
        /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
        /// </summary>
        StringBuilder Value { get; }

        /// <summary>
        /// Additional information following the value of the member.
        /// </summary>
        StringBuilder Status { get; }

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }
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
    /// Interface for block terminal settings that support scrolling. Usually used for incrementing/decrementing
    /// values.
    /// </summary>
    public interface IBlockScrollable : IBlockProperty
    {
        void ScrollUp();

        void ScrollDown();
    }
}