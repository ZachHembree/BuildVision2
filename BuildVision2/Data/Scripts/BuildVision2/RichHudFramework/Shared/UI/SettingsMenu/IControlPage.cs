using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

		/// <summary>
		/// Internal API accessor enums for control page configuration
		/// </summary>
		/// <exclude/>
		public enum ControlPageAccessors : int
        {
            /// <summary>
            /// MemberAccessor
            /// </summary>
            AddCategory = 10,

            CategoryData = 11,
        }

		/// <summary>
		/// Internal interface for a list of horizontally scrolling control categories,
        /// implemented in both client and master modules.
		/// </summary>
		/// <exclude/>
		public interface IControlPage : IControlPage<ControlCategory, ControlTile>
        { }

		/// <summary>
		/// Internal interface for RHF terminal control pages implemented in both client and master modules.
		/// </summary>
		/// <exclude/>
		public interface IControlPage<TCategory, TMember> : ITerminalPage, IEnumerable<TCategory>
            where TCategory : IControlCategory<TMember>, new()
        {
            /// <summary>
            /// Read only collection of <see cref="IControlCategory"/>s assigned to this object.
            /// </summary>
            IReadOnlyList<TCategory> Categories { get; }

            /// <summary>
            /// Used to allow the addition of category elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            IControlPage<TCategory, TMember> CategoryContainer { get; }

            /// <summary>
            /// Adds a given category to the page
            /// </summary>
            void Add(TCategory category);
        }
    }
}