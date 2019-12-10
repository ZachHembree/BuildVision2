using System;
using System.Collections.Generic;
using VRage;

namespace DarkHelmet
{
    using HudParentMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        object, // Add (Action<HudNodeMembers>)
        Action, // BeforeDraw
        Action, // BeforeInput
        MyTuple<
            Action<object>, // RemoveChild
            Action<object> // SetFocus
        >
    >;

    namespace UI
    {
        /// <summary>
        /// Interface for all types capable of serving as parent objects to <see cref="IHudNode"/>s.
        /// </summary>
        public interface IHudParent
        {
            /// <summary>
            /// Determines whether or not the element will be drawn and/or accept
            /// input.
            /// </summary>
            bool Visible { get; set; }

            /// <summary>
            /// Unique identifier.
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Registers a child node to the object.
            /// </summary>
            void Add(IHudNode child);

            /// <summary>
            /// Registers a collection of child nodes to the object.
            /// </summary>
            void RegisterChildren(IEnumerable<IHudNode> newChildren);

            /// <summary>
            /// Unregisters the specified node from the parent.
            /// </summary>
            void RemoveChild(IHudNode child);

            /// <summary>
            /// Moves the specified child element to the end of the update list in
            /// order to ensure that it's drawn on top/updated last.
            /// </summary>
            void SetFocus(IHudNode child);

            void BeforeInput();
            void BeforeDraw();

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudParent"/> through the API.
            /// </summary>
            HudParentMembers GetApiData();
        }
    }
}