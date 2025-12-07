using System;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Internal UI node configuration flags
		/// </summary>
		/// <exclude/>
		[Flags]
		public enum HudElementStates : uint
		{
			/// <summary>
			/// No flags set. Represents an unset or default state with no properties enabled.
			/// </summary>
			None = 0x0,

			/// <summary>
			/// Indicates whether the HUD element should be rendered and is considered visible. 
			/// This is a core flag for drawing; elements without it are skipped in visibility checks 
			/// (e.g., during tree iteration in HudNodeIterator). It's often combined in masks like 
			/// NodeVisibleMask for final visibility decisions.
			/// </summary>
			IsVisible = 1 << 0,

			/// <summary>
			/// Tracks whether the element's parent was visible in the previous update cycle. 
			/// This propagates visibility down the hierarchy (top-down) during layout updates. 
			/// It's used to ensure children inherit parent visibility without recalculating it every 
			/// frame, and it's part of visibility masks (e.g., cleared or set in UpdateNodeLayout).
			/// </summary>
			WasParentVisible = 1 << 1,

			/// <summary>
			/// Indicates whether the element has been successfully registered to a parent in the UI 
			/// tree (via methods like Register in HudNodeBase). Unregistered elements aren't processed 
			/// in the update tree. This is set after adding to a parent's child list and cleared on 
			/// unregistration.
			/// </summary>
			IsRegistered = 1 << 2,

			/// <summary>
			/// Allows the element to capture or interact with the cursor (mouse). If set, the element 
			/// can participate in input depth testing and mouse-over checks. Without it, depth updates 
			/// are disabled.
			/// </summary>
			CanUseCursor = 1 << 3,

			/// <summary>
			/// Permits the element to share cursor capture with other elements (e.g., allowing multiple 
			/// overlapping elements to respond to input). This modifies how mouse-over states are 
			/// handled when multiple elements could claim the cursor.
			/// </summary>
			CanShareCursor = 1 << 4,

			/// <summary>
			/// Set when the cursor is directly over the element and not obstructed by higher-depth elements. 
			/// This is determined during input handling (bottom-up order) and cleared/reset each frame. 
			/// It's used for hover effects or interactions.
			/// </summary>
			IsMousedOver = 1 << 5,

			/// <summary>
			/// Indicates the mouse cursor is within the element's bounding box (after depth testing). This 
			/// is checked during input depth updates (front-to-back order) and is a prerequisite for 
			/// IsMousedOver. It's cleared each frame before re-testing.
			/// </summary>
			IsMouseInBounds = 1 << 6,

			/// <summary>
			/// Internal flag used to indicate when the structure of the active UI tree may have changed.
			/// </summary>
			IsStructureStale = 1 << 7,

			/// <summary>
			/// Indicates the element is being clipped (masked) by its parent's bounding box or masking configuration. 
			/// This is set during masking updates (e.g., in UpdateMasking in HudElementBase) and affects drawing 
			/// and visibility propagation.
			/// </summary>
			IsMasked = 1 << 8,

			/// <summary>
			/// Configures the element to act as a clipping mask for its children (e.g., children outside its bounds 
			/// aren't drawn). This is checked when updating child masking boxes and can be combined with parent 
			/// masks for intersection-based clipping.
			/// </summary>
			IsMasking = 1 << 9,

			/// <summary>
			/// Forces the element to treat its parent as a masking boundary, even if the parent isn't explicitly 
			/// set as masking (IsMasking). This overrides default behavior for selective clipping without affecting 
			/// the parent's general config.
			/// </summary>
			IsSelectivelyMasked = 1 << 10,

			/// <summary>
			/// Allows the element to bypass any masking imposed by its parents (supersedes IsSelectivelyMasked). 
			/// Useful for overlays or elements that should always be fully visible regardless of hierarchy.
			/// </summary>
			CanIgnoreMasking = 1 << 11,

			/// <summary>
			/// Enables input processing for the element (e.g., responding to mouse/keyboard). Without it, input 
			/// updates are skipped. This is part of input masks like NodeInputMask and propagates similarly to 
			/// visibility.
			/// </summary>
			IsInputEnabled = 1 << 12,

			/// <summary>
			/// Tracks whether the element's parent had input enabled in the previous cycle. This propagates input 
			/// enablement down the hierarchy (top-down).
			/// </summary>
			WasParentInputEnabled = 1 << 13,

			/// <summary>
			/// Indicates the element defines its own HUD space (coordinate system, e.g., via HudSpace for 
			/// pixel-to-world transformations). If not set, it inherits from the parent. This affects origin 
			/// functions and space readiness.
			/// </summary>
			IsSpaceNode = 1 << 15,

			/// <summary>
			/// Set when the element's HUD space is initialized and ready (e.g., HudSpace != null). This is 
			/// checked in layout beginnings and combined in readiness flags to ensure elements aren't processed 
			/// without a valid coordinate space.
			/// </summary>
			IsSpaceNodeReady = 1 << 16,

			/// <summary>
			/// Indicates the element's bounding box does not intersect with its parent's masking box 
			/// (or effective bounds). This is an optimization flag set during alignment/masking updates to
			/// skip processing for fully obscured or out-of-bounds elements (e.g., in UpdateAlignment).
			/// </summary>
			IsDisjoint = 1 << 17,

			/// <summary>
			/// Internal flag used to tag inactive leaf nodes that need to be monitored for visibility
			/// transitions.
			/// </summary>
			IsInactiveLeaf = 1 << 18,

			/// <summary>
			/// Internal optimization flag used to indicate whether HandleInput() has been overridden on a node.
			/// </summary>
			IsInputHandlerCustom = 1 << 19,

			/// <summary>
			/// Internal optimization flag used to indicate whether Layout() has been overridden on a node.
			/// </summary>
			IsLayoutCustom = 1 << 20,
		}

		/// <summary>
		/// Internal debug enums
		/// </summary>
		/// <exclude/>
		public enum HudElementAccessors : int
		{
			/// <summary>
			/// out: string
			/// </summary>
			ModName = 0,

			/// <summary>
			/// out: System.Type
			/// </summary>
			GetType = 1,

			/// <summary>
			/// out: byte
			/// </summary>
			ZOffset = 2,

			/// <summary>
			/// out: ushort
			/// </summary>
			FullZOffset = 3,

			/// <summary>
			/// out: Vector2
			/// </summary>
			Position = 4,

			/// <summary>
			/// out: Vector2
			/// </summary>
			Size = 5,

			/// <summary>
			/// out: Vector3
			/// </summary>
			LocalCursorPos = 6,

			/// <summary>
			/// out: bool
			/// </summary>
			DrawCursorInHudSpace = 7,

			/// <summary>
			/// out: HudSpaceDelegate
			/// </summary>
			GetHudSpaceFunc = 8,

			/// <summary>
			/// out: Vector3D
			/// </summary>
			NodeOrigin = 9,

			/// <summary>
			/// out: MatrixD
			/// </summary>
			PlaneToWorld = 10,

			/// <summary>
			/// out: bool
			/// </summary>
			IsInFront = 11,

			/// <summary>
			/// out: bool
			/// </summary>
			IsFacingCamera = 12,
		}

		/// <summary>
		/// Read-only interface for types capable of serving as parent objects to <see cref="HudNodeBase"/>s.
		/// </summary>
		public interface IReadOnlyHudParent
		{
			/// <summary>
			/// Node defining the coordinate space used to render the UI element
			/// </summary>
			IReadOnlyHudSpaceNode HudSpace { get; }

			/// <summary>
			/// Returns true if the element is enabled and able to be drawn and accept input.
			/// </summary>
			bool Visible { get; }

			/// <summary>
			/// Returns true if input is enabled can update
			/// </summary>
			bool InputEnabled { get; }

			/// <summary>
			/// Moves the UI element up or down in draw order. -1 will darw an element behind its immediate 
			/// parent. +1 will draw it on top of siblings. Higher values will allow it to draw behind or over 
			/// more distantly related elements.
			/// </summary>
			sbyte ZOffset { get; }
		}
	}
}