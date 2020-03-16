using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using HudElementMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        Action<bool>, // BeforeLayout
        Action<int>, // BeforeDraw
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMembers
    >;

    namespace UI
    {
        public enum HudParentAccessors : int
        {
            Add = 1,
            RemoveChild = 2,
            SetFocus = 3,
        }

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
            void RegisterChild(IHudNode child);

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

            void BeforeLayout(bool refresh);

            void BeforeDraw(HudLayers layer);

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudParent"/> through the API.
            /// </summary>
            HudElementMembers GetApiData();
        }
    }
}