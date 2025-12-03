using Sandbox.ModAPI;
using VRageMath;

namespace RichHudFramework
{
	namespace UI
	{
		using Client;
		using Server;

		/// <summary>
		/// HudSpace node that builds its Plane-to-World matrix directly from the player camera.
		/// <para>In its default state this exactly replicates the vanilla <see cref="HudMain.HighDpiRoot"/> 
		/// transform (screen-space UI). By modifying its properties you can create rotated, offset,
		/// or world-space (non-screen-space) HUDs that still follow the camera.</para>
		/// </summary>
		public class CamSpaceNode : HudSpaceNodeBase
		{
			/// <summary>
			/// Uniform scaling factor applied to the HUD plane (X/Y axes only). Default = 1.
			/// </summary>
			public float PlaneScale { get; set; }

			/// <summary>
			/// Axis around which the HUD plane is rotated relative to the camera. Default = (0,0,1).
			/// </summary>
			public Vector3 RotationAxis { get; set; }

			/// <summary>
			/// Rotation angle in radians around <see cref="RotationAxis"/>.
			/// </summary>
			public float RotationAngle { get; set; }

			/// <summary>
			/// World-space offset applied to the camera matrix before scaling/rotation.
			/// Default places the plane exactly on the camera's near clip plane.
			/// </summary>
			public Vector3D TransformOffset { get; set; }

			/// <summary>
			/// If true (default), the plane is automatically scaled so that 1 unit on the HUD
			/// equals 1 DPI-scaled point at the center of the screen, regardless of FOV or resolution.
			/// This reproduces normal screen-space behavior.
			/// </summary>
			public bool IsScreenSpace { get; set; }

			/// <summary>
			/// If true (default) and <see cref="HudMain.ResScale"/> is applied when
			/// <see cref="IsScreenSpace"/> is true. Compensates for high-DPI displays.
			/// </summary>
			public bool UseResScaling { get; set; }

			public CamSpaceNode(HudParentBase parent = null) : base(parent)
			{
				PlaneScale = 1f;
				TransformOffset = new Vector3D(0.0, 0.0, -MyAPIGateway.Session.Camera.NearPlaneDistance);

				IsScreenSpace = true;
				UseResScaling = true;
			}

			/// <summary>
			/// Rebuilds <see cref="HudSpaceNodeBase.PlaneToWorldRef"/> every frame from the current
			/// camera matrix and the node's transformation properties.
			/// </summary>
			protected override void Layout()
			{
				double finalScale = PlaneScale;

				if (IsScreenSpace)
				{
					// Makes 1 unit == 1 pixel at screen center
					finalScale *= HudMain.FovScale / HudMain.ScreenHeight;

					if (UseResScaling)
						finalScale *= HudMain.ResScale;
				}

				var scaling = MatrixD.CreateScale(finalScale, finalScale, 1.0);
				var rotation = MatrixD.CreateFromAxisAngle(RotationAxis, RotationAngle);
				var translation = MatrixD.CreateTranslation(TransformOffset);

				// Order: scale -> rotate -> translate -> multiply by camera matrix
				PlaneToWorldRef[0] = scaling * rotation * translation * MyAPIGateway.Session.Camera.WorldMatrix;

				base.Layout();
			}
		}
	}
}