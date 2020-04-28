namespace DarkHelmet.BuildVision2
{

    public partial class PropertyBlock
    {
        private abstract class BlockMemberBase : IBlockMember
        {
            /// <summary>
            /// Retrieves the name of the block property
            /// </summary>
            public virtual string Name { get; protected set; }

            /// <summary>
            /// Retrieves the value as a <see cref="string"/> using formatting specific to the member.
            /// </summary>
            public abstract string Display { get; }

            /// <summary>
            /// Retrieves the current value of the block member as an unformatted <see cref="string"/>
            /// </summary>
            public virtual string Value => Display;

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public abstract string Status { get; }

            /// <summary>
            /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
            /// </summary>
            public virtual bool Enabled { get; protected set; }
        }
    }
}