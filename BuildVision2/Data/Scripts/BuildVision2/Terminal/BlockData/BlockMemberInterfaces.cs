using System;

namespace DarkHelmet.BuildVision2
{
    internal interface IBlockMember
    {
        /// <summary>
        /// Retrieves the name of the block property
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the current value of the block member as a <see cref="string"/>
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Additional information following the value of the member.
        /// </summary>
        string Postfix { get; }

        /// <summary>
        /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
        /// </summary>
        bool Enabled { get; }
    }

    internal interface IBlockAction : IBlockMember
    {
        void Action();
    }

    /// <summary>
    /// Interface for block terminal settings that support text input.
    /// </summary>
    internal interface IBlockTextMember : IBlockMember
    {
        Func<char, bool> CharFilterFunc { get; }

        void SetValueText(string text);
    }

    /// <summary>
    /// Interface for block terminal settings that support scrolling. Usually used for incrementing/decrementing
    /// values.
    /// </summary>
    internal interface IBlockScrollable : IBlockMember
    {
        void ScrollUp();

        void ScrollDown();
    }
}