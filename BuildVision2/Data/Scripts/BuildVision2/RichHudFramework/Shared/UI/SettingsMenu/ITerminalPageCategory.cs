using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        /// <summary>
        /// Internal text page category member accessor enums
        /// </summary>
        /// <exclude/>
        public enum TerminalPageCategoryAccessors : int
        {
            /// <summary>
            /// string
            /// </summary>
            Name = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// out: ControlMembers
            /// </summary>
            Selection = 4,

            /// <summary>
            /// in: TerminalPageBase
            /// </summary>
            AddPage = 5,

            /// <summary>
            /// in: IReadOnlyList{TerminalPageBase}
            /// </summary>
            AddPageRange = 6,
        }

        /// <summary>
        /// Internal iterface for indented treebox dropdown boxes used for page selection.
        /// Implemented by master and client modules.
        /// </summary>
        /// <exclude/>
        public interface ITerminalPageCategory : IEnumerable<TerminalPageBase>, IModRootMember
        {
            /// <summary>
            /// Read only collection of <see cref="TerminalPageBase"/>s assigned to this object.
            /// </summary>
            IReadOnlyList<TerminalPageBase> Pages { get; }

            /// <summary>
            /// Used to allow the addition of category elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            ITerminalPageCategory PageContainer { get; }

            /// <summary>
            /// Currently selected <see cref="TerminalPageBase"/>.
            /// </summary>
            TerminalPageBase SelectedPage { get; }

            /// <summary>
            /// Adds a terminal page to the category
            /// </summary>
            void Add(TerminalPageBase page);

            /// <summary>
            /// Adds a range of terminal pages to the category
            /// </summary>
            void AddRange(IReadOnlyList<TerminalPageBase> pages);
        }
    }
}