using System;
namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// A specialized HUD space node that rescales the X/Y (Right/Up) plane of its parent's 
		/// plane-to-world transformation matrix, enlarging or shrinking all child UI elements 
		/// attached to it uniformly.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This node creates a modified copy of the parent <see cref="IReadOnlyHudSpaceNode.PlaneToWorldRef"/> 
		/// transform and applies a uniform scale factor (<see cref="PlaneScale"/>) to the Right and Up vectors.
		/// </para>
		/// </remarks>
		public class ScaledSpaceNode : HudSpaceNodeBase
		{
			/// <summary>
			/// Uniform scale factor applied to the X/Y plane (Right / Up) of the parent's 
			/// plane-to-world matrix. A value of 1.0f represents no scaling, &gt;1.0f enlarges, &lt;1.0f shrinks.
			/// </summary>
			public float PlaneScale { get; set; } = 1f;

			/// <summary>
			/// Optional delegate that, when assigned, is invoked every frame during <see cref="Layout"/> 
			/// to dynamically update <see cref="PlaneScale"/>.
			/// </summary>
			public Func<float> UpdateScaleFunc { get; set; }

			public ScaledSpaceNode(HudParentBase parent = null) : base(parent)
			{ }

			/// <exclude/>
			protected override void Layout()
			{
				// Dynamically update scale if a callback is provided
				if (UpdateScaleFunc != null)
					PlaneScale = UpdateScaleFunc();

				IReadOnlyHudSpaceNode parentSpace = Parent.HudSpace;

				// Copy and scale only the planar (X/Y) components
				PlaneToWorldRef[0] = parentSpace.PlaneToWorldRef[0];
				PlaneToWorldRef[0].Right *= PlaneScale;
				PlaneToWorldRef[0].Up *= PlaneScale;

				// Preserve depth and camera-facing behavior
				IsInFront = parentSpace.IsInFront;
				IsFacingCamera = parentSpace.IsFacingCamera;

				// Compensate cursor position for scale so mouse interaction remains intuitive
				CursorPos = parentSpace.CursorPos / PlaneScale;
			}
		}
	}
}