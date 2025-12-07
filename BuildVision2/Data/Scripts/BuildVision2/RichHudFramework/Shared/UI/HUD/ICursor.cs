using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Internal API accessor indices for the Rich HUD cursor
		/// </summary>
		/// <exclude/>
		public enum HudCursorAccessors : int
		{
			/// <summary>
			/// out: bool - Cursor is currently visible on screen
			/// </summary>
			Visible = 0,

			/// <summary>
			/// out: bool - Cursor is currently captured by any UI element
			/// </summary>
			IsCaptured = 1,

			/// <summary>
			/// out: Vector2 - Cursor position in screen-space pixels (origin top-left)
			/// </summary>
			ScreenPos = 2,

			/// <summary>
			/// out: Vector3D - Cursor position in world space (at the drawn plane or captured depth)
			/// </summary>
			WorldPos = 3,

			/// <summary>
			/// out: LineD - Raycast-equivalent line starting at the cursor and extending into the world along the drawn cursor direction
			/// </summary>
			WorldLine = 4,

			/// <summary>
			/// in: Action&lt;ToolTip&gt; - Registers a tooltip callback for the current frame
			/// </summary>
			RegisterToolTip = 5,

			/// <summary>
			/// out: bool - True if any tooltip has been registered this frame
			/// </summary>
			IsToolTipRegistered = 6,
		}

		/// <summary>
		/// Represents the global mouse cursor managed by Rich HUD Framework.
		/// Provides screen/world position, capture state, and tooltip registration functionality.
		/// Used by all clickable UI elements for correct hit-testing and interaction within and 
		/// between HUD spaces.
		/// </summary>
		public interface ICursor
		{
			/// <summary>
			/// True if the cursor is currently visible (not hidden by game UI or capture state).
			/// </summary>
			bool Visible { get; }

			/// <summary>
			/// True if any UI element is currently capturing the cursor
			/// </summary>
			bool IsCaptured { get; }

			/// <summary>
			/// True if a tooltip has been registered for the current frame.
			/// </summary>
			bool IsToolTipRegistered { get; }

			/// <summary>
			/// Cursor position in screen-space pixels
			/// </summary>
			Vector2 ScreenPos { get; }

			/// <summary>
			/// Cursor position in world space
			/// </summary>
			Vector3D WorldPos { get; }

			/// <summary>
			/// Unwarped line starting at the camera and passing through the cursor position.
			/// Corrects for perspective distortion so that a straight line in world space remains straight on screen.
			/// </summary>
			LineD WorldLine { get; }

			/// <summary>
			/// Determines whether the cursor is currently being drawn in (and captured by) the given HUD space.
			/// </summary>
			/// <param name="GetHudSpaceFunc">
			/// Delegate that returns (bool isValid, float depthSquared, MatrixD worldMatrix) for the HUD space node.
			/// </param>
			bool IsCapturingSpace(HudSpaceDelegate GetHudSpaceFunc);

			/// <summary>
			/// Attempts to capture the cursor for a custom 3D HUD space at the specified depth.
			/// Returns true if capture succeeded (i.e., this space is closer than any existing capturer).
			/// </summary>
			/// <param name="depthSquared">Squared distance from camera to the plane/node. Smaller = closer.</param>
			/// <param name="GetHudSpaceFunc">Delegate returning plane validity, depth, and world matrix.</param>
			/// <remarks>
			/// Only the closest (smallest depthSquared) valid plane will capture the cursor each frame.
			/// </remarks>
			bool TryCaptureHudSpace(float depthSquared, HudSpaceDelegate GetHudSpaceFunc);

			/// <summary>
			/// Returns true if the cursor is currently captured by the given API-wrapped element.
			/// </summary>
			bool IsCapturing(ApiMemberAccessor capturedElement);

			/// <summary>
			/// Attempts to capture the cursor capture using a standard API-wrapped element.
			/// Returns true on success. Usually succeeds unless already captured by a higher-priority element.
			/// </summary>
			bool TryCapture(ApiMemberAccessor capturedElement);

			/// <summary>
			/// Attempts to release cursor capture held by the given API-wrapped element.
			/// Returns true if the element was actually holding the capture.
			/// </summary>
			bool TryRelease(ApiMemberAccessor capturedElement);

			/// <summary>
			/// Registers a tooltip that will be displayed when the cursor is over the calling element.
			/// Tooltips are cleared every frame — this must be called every tick inside <c>HandleInput()</c>
			/// if you want a persistent tooltip.
			/// The first registered tooltip (in registration order) wins if multiple elements try to show one.
			/// </summary>
			/// <param name="toolTip">The <see cref="ToolTip"/> instance to display.</param>
			void RegisterToolTip(ToolTip toolTip);
		}
	}
}