using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using HudElementMembers = MyTuple<
        Func<bool>, // Visible
        object, // ID
        Action, // BeforeDraw
        Action, // Draw
        Action, // HandleInput
        ApiMemberAccessor // GetOrSetMembers
    >;

    namespace UI
    {
        /// <summary>
        /// Interface for all hud elements that can be parented to another element.
        /// </summary>
        public interface IHudNode : IHudParent
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            IHudParent Parent { get; }

            /// <summary>
            /// Registers the element to the given parent object.
            /// </summary>
            void Register(IHudParent parent);

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            void Unregister();

            /// <summary>
            /// Moves the element to the end of its parent's update list in order to ensure
            /// that it's drawn/updated last.
            /// </summary>
            void GetFocus();
        }
    }
}