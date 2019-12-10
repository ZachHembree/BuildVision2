using System;
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
        using HudNodeMembers = MyTuple<
            HudParentMembers, // Base members
            Func<object>, // GetParentID
            object, // GetParentData (Func<HudParentMembers?>)
            Action, // GetFocus
            Action<object>, // Register
            Action // Unregister
        >;

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

            /// <summary>
            /// Retrieves the information necessary to access the <see cref="IHudNode"/> through the API.
            /// </summary>
            new HudNodeMembers GetApiData();
        }
    }
}