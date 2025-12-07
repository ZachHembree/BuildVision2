namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Read-only interface for hud elements that can be parented to another element.
        /// </summary>
        public interface IReadOnlyHudNode : IReadOnlyHudParent
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            IReadOnlyHudParent Parent { get; }

			/// <summary>
			/// Returns true if the node has been registered to a parent. Does not necessarilly indicate that 
			/// the parent is registered or that the node is active.
			/// </summary>
			bool Registered { get; }
        }
    }
}