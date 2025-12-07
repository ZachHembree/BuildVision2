using Sandbox.ModAPI;
using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
	namespace UI
	{
		using Client;
		using Server;
		using static NodeConfigIndices;

		/// <summary>
		/// Abstract base for all HUD nodes that define their own custom coordinate space
		/// (replacing the default screen-space Pixel-to-World transform with an arbitrary world matrix).
		/// <para>Provides cursor projection, facing/in-front detection, and origin retrieval for derived classes.</para>
		/// </summary>
		public abstract class HudSpaceNodeBase : HudNodeBase, IReadOnlyHudSpaceNode
		{
			/// <summary>
			/// This node itself defines the HUD space for itself and its children.
			/// </summary>
			public override IReadOnlyHudSpaceNode HudSpace => this;

			/// <summary>
			/// Current Plane-to-World transformation matrix used for drawing this subtree.
			/// Transforms from local coordinates to world space.
			/// </summary>
			public MatrixD PlaneToWorld => PlaneToWorldRef[0];

			/// <summary>
			/// Reference to the current Plane-to-World matrix as a single-element array.
			/// </summary>
			public MatrixD[] PlaneToWorldRef { get; }

			/// <summary>
			/// Position of the HUD cursor projected onto this node's plane.
			/// <para>
			/// X/Y = local plane coordinates
			/// Z = approximate squared distance from the camera to the intersection point (for depth sorting).
			/// </para>
			/// </summary>
			public Vector3 CursorPos { get; protected set; }

			/// <summary>
			/// Delegate used by the cursor system to query this node's current HUD space properties.
			/// Returns (drawCursorInThisSpace, cursorBillboardScale, planeToWorldMatrix).
			/// </summary>
			public HudSpaceDelegate GetHudSpaceFunc { get; protected set; }

			/// <summary>
			/// Function that returns the current world-space position of this node's origin.
			/// Default implementation returns <see cref="PlaneToWorldRef"/>[0].Translation.
			/// </summary>
			public Func<Vector3D> GetNodeOriginFunc
			{
				get { return DataHandle[0].Item2[0]; }
				protected set { DataHandle[0].Item2[0] = value; }
			}

			/// <summary>
			/// If true, when a child of this node captures the cursor, the cursor will be drawn
			/// using this node's PlaneToWorld matrix instead of the default screen-space matrix.
			/// Useful for 3D / diegetic UI where the cursor should appear on the custom plane.
			/// </summary>
			public bool DrawCursorInHudSpace { get; set; }

			/// <summary>
			/// True if the node's origin is in front of the camera (i.e. in the camera's forward hemisphere).
			/// </summary>
			public bool IsInFront { get; protected set; }

			/// <summary>
			/// True if the node is in front of the camera AND its forward vector roughly faces the camera
			/// (dot product > 0). Used for culling back-facing HUD elements.
			/// </summary>
			public bool IsFacingCamera { get; protected set; }

			public HudSpaceNodeBase(HudParentBase parent = null) : base(parent)
			{
				PlaneToWorldRef = new MatrixD[1];

				GetHudSpaceFunc = () => new MyTuple<bool, float, MatrixD>(DrawCursorInHudSpace, 1f, PlaneToWorldRef[0]);
				GetNodeOriginFunc = () => PlaneToWorldRef[0].Translation;

				_config[StateID] |= (uint)HudElementStates.IsSpaceNode;
			}

			/// <summary>
			/// Updates visibility flags (<see cref="IsInFront"/>, <see cref="IsFacingCamera"/>)
			/// and projects the current cursor ray onto this node's plane to calculate <see cref="CursorPos"/>.
			/// Called automatically every frame before drawing.
			/// </summary>
			protected override void Layout()
			{
				MatrixD camMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
				Vector3D camPos = camMatrix.Translation;
				Vector3D camForward = camMatrix.Forward;

				Vector3D nodeOrigin = PlaneToWorldRef[0].Translation;
				Vector3D nodeForward = PlaneToWorldRef[0].Forward;

				// Is the node origin in the forward hemisphere of the camera?
				IsInFront = Vector3D.Dot(nodeOrigin - camPos, camForward) > 0;
				// Is the plane facing the camera?
				IsFacingCamera = IsInFront && Vector3D.Dot(nodeForward, camForward) > 0;

				// Project cursor ray onto the plane
				MatrixD worldToPlane;
				MatrixD.Invert(ref PlaneToWorldRef[0], out worldToPlane);

				LineD cursorLine = HudMain.Cursor.WorldLine;
				PlaneD plane = new PlaneD(nodeOrigin, nodeForward);

				// Find intersection point in world space
				Vector3D worldIntersection = plane.Intersection(ref cursorLine.From, ref cursorLine.Direction);

				// Transform intersection point into local plane space (no perspective divide needed)
				Vector3D localPos;
				Vector3D.TransformNoProjection(ref worldIntersection, ref worldToPlane, out localPos);

				CursorPos = new Vector3(
					(float)localPos.X,
					(float)localPos.Y,
					// Squared distance used only for rough depth comparison – avoids sqrt
					(float)Math.Round(Vector3D.DistanceSquared(worldIntersection, cursorLine.From), 6)
				);
			}
		}
	}
}