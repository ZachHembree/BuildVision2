using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Read-only interface for hud elements with definite size and position.
		/// </summary>
		public interface IReadOnlyHudElement : IReadOnlyHudNode
		{
			/// <summary>
			/// Size of the element. Units in pixels with HudMain.Root.
			/// </summary>
			Vector2 Size { get; }

			/// <summary>
			/// Height of the element. Units in pixels with HudMain.Root.
			/// </summary>
			float Height { get; }

			/// <summary>
			/// Width of the element. Units in pixels with HudMain.Root.
			/// </summary>
			float Width { get; }

			/// <summary>
			/// Starting/anchoring position of the hud element. Starts in the center of the parent node 
			/// by default. This behavior can be modified with ParentAlignment flags.
			/// </summary>
			Vector2 Origin { get; }

			/// <summary>
			/// Position of the center of the UI element relative to its origin.
			/// </summary>
			Vector2 Offset { get; }

			/// <summary>
			/// Determines the starting position/anchoring behavior of the hud element relative to its parent.
			/// </summary>
			ParentAlignments ParentAlignment { get; }

			/// <summary>
			/// Determines how/if an element will copy its parent's dimensions. 
			/// </summary>
			DimAlignments DimAlignment { get; }

			/// <summary>
			/// Enables or disables cursor input and capture
			/// </summary>
			bool UseCursor { get; }

			/// <summary>
			/// If set to true the hud element will share the cursor with its child elements.
			/// </summary>
			bool ShareCursor { get; }

			/// <summary>
			/// Indicates whether or not the cursor is currently over the element. The element must
			/// be set to capture the cursor for this to work.
			/// </summary>
			bool IsMousedOver { get; }
		}
	}
}