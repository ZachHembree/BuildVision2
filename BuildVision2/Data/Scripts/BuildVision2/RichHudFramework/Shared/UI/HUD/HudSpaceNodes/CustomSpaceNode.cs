using System;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// HudSpace node that uses a completely user-supplied world matrix each frame.
		/// If no delegate is assigned, the node simply inherits its parent's Plane-to-World matrix.
		/// <para>Ideal for attaching UI to moving cockpits, turret bases, custom billboards, etc.</para>
		/// </summary>
		public class CustomSpaceNode : HudSpaceNodeBase
		{
			/// <summary>
			/// Delegate called every frame to retrieve the current Plane-to-World matrix.
			/// If null, the node falls back to its parent's matrix.
			/// </summary>
			public Func<MatrixD> UpdateMatrixFunc { get; set; }

			public CustomSpaceNode(HudParentBase parent = null) : base(parent)
			{ }

			/// <summary>
			/// Updates the node's Plane-to-World matrix either from <see cref="UpdateMatrixFunc"/>
			/// or by copying the parent's matrix.
			/// </summary>
			protected override void Layout()
			{
				if (UpdateMatrixFunc != null)
					PlaneToWorldRef[0] = UpdateMatrixFunc();
				else if (Parent?.HudSpace != null)
					PlaneToWorldRef[0] = Parent.HudSpace.PlaneToWorld;

				base.Layout();
			}
		}
	}
}