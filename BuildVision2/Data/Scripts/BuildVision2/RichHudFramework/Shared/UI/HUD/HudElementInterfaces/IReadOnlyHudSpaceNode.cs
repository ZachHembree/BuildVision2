using System;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Interface for all HUD nodes that define their own custom coordinate space.
		/// </summary>
		public interface IReadOnlyHudSpaceNode : IReadOnlyHudParent
		{
			/// <summary>
			/// Position of the HUD cursor projected onto this node's plane.
			/// <para>
			/// X/Y = local plane coordinates
			/// Z = approximate squared distance from the camera to the intersection point (for depth sorting).
			/// </para>
			/// </summary>
			Vector3 CursorPos { get; }

			/// <summary>
			/// Delegate used by the cursor system to query this node's current HUD space properties.
			/// Returns (drawCursorInThisSpace, cursorBillboardScale, planeToWorldMatrix).
			/// </summary>
			HudSpaceDelegate GetHudSpaceFunc { get; }

			/// <summary>
			/// Current Plane-to-World transformation matrix used for drawing this subtree.
			/// Transforms from local coordinates to world space.
			/// </summary>
			MatrixD PlaneToWorld { get; }

			/// <summary>
			/// Reference to the current Plane-to-World matrix as a single-element array.
			/// </summary>
			MatrixD[] PlaneToWorldRef { get; }

			/// <summary>
			/// Function that returns the current world-space position of this node's origin.
			/// </summary>
			Func<Vector3D> GetNodeOriginFunc { get; }

			/// <summary>
			/// If true, when a child of this node captures the cursor, the cursor will be drawn
			/// using this node's PlaneToWorld matrix instead of the default screen-space matrix.
			/// Useful for 3D / diegetic UI where the cursor should appear on the custom plane.
			/// </summary>
			bool DrawCursorInHudSpace { get; }

			/// <summary>
			/// True if the node's origin is in front of the camera (i.e. in the camera's forward hemisphere).
			/// </summary>
			bool IsInFront { get; }

			/// <summary>
			/// True if the node is in front of the camera AND its forward vector roughly faces the camera
			/// (dot product > 0). Used for culling back-facing HUD elements.
			/// </summary>
			bool IsFacingCamera { get; }
		}
	}
}
