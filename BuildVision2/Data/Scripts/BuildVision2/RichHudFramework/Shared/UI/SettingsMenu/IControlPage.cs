using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        public enum ControlPageAccessors : int
        {
            /// <summary>
            /// MemberAccessor
            /// </summary>
            AddCategory = 10,

            CategoryData = 11,
        }

        /// <summary>
        /// Vertically scrolling collection of control categories.
        /// </summary>
        public interface IControlPage : ITerminalPage, IEnumerable<IControlCategory>
        {
            /// <summary>
            /// Read only collection of <see cref="IControlCategory"/>s assigned to this object.
            /// </summary>
            IReadOnlyList<IControlCategory> Categories { get; }

            /// <summary>
            /// Used to allow the addition of category elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            IControlPage CategoryContainer { get; }

            /// <summary>
            /// Adds a given <see cref="IControlCategory"/> to the page
            /// </summary>
            void Add(ControlCategory category);
        }
    }
}