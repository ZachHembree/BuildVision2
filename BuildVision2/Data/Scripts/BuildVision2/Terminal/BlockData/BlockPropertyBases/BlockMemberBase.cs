namespace DarkHelmet.BuildVision2
{

    internal partial class PropertyBlock
    {
        private abstract class BlockMemberBase : IBlockMember
        {
            /// <summary>
            /// Retrieves the name of the block property
            /// </summary>
            public virtual string Name { get; protected set; }

            /// <summary>
            /// Retrieves the current value of the block member as a <see cref="string"/>
            /// </summary>
            public abstract string Value { get; }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public abstract string Postfix { get; }

            /// <summary>
            /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
            /// </summary>
            public virtual bool Enabled { get; protected set; }
        }
    }
}