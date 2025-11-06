using Sandbox.ModAPI;
using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
	namespace UI
	{
		using static NodeConfigIndices;
		using Server;
		using Client;

		/// <summary>
		/// Base class for hud nodes used to replace standard Pixel to World matrix with an arbitrary
		/// world matrix transform.
		/// </summary>
		public abstract class HudSpaceNodeBase : HudNodeBase, IReadOnlyHudSpaceNode
		{
			/// <summary>
			/// Node defining the coordinate space used to render the UI element
			/// </summary>
			public override IReadOnlyHudSpaceNode HudSpace => this;

			/// <summary>
			/// Returns the current draw matrix
			/// </summary>
			public MatrixD PlaneToWorld => PlaneToWorldRef[0];

			/// <summary>
			/// Returns the current draw matrix by reference as an array of length 1
			/// </summary>
			public MatrixD[] PlaneToWorldRef { get; }

			/// <summary>
			/// Cursor position on the XY plane defined by the HUD space. Z == dist from screen.
			/// </summary>
			public Vector3 CursorPos { get; protected set; }

			/// <summary>
			/// Delegate used to retrieve current hud space. Used with cursor.
			/// </summary>
			public HudSpaceDelegate GetHudSpaceFunc { get; protected set; }

			/// <summary>
			/// Returns the world space position of the node's origin.
			/// </summary>
			public Func<Vector3D> GetNodeOriginFunc
			{
				get { return DataHandle[0].Item2[0]; }
				protected set { DataHandle[0].Item2[0] = value; }
			}

			/// <summary>
			/// If true, then the cursor will be drawn using the PTW matrix of this HUD space when
			/// captured by one of its children.
			/// </summary>
			public bool DrawCursorInHudSpace { get; set; }

			/// <summary>
			/// True if the origin of the HUD space is in front of the camera
			/// </summary>
			public bool IsInFront { get; protected set; }

			/// <summary>
			/// True if the XY plane of the HUD space is in front and facing toward the camera
			/// </summary>
			public bool IsFacingCamera { get; protected set; }

			public HudSpaceNodeBase(HudParentBase parent = null) : base(parent)
			{
				GetHudSpaceFunc = () => new MyTuple<bool, float, MatrixD>(DrawCursorInHudSpace, 1f, PlaneToWorldRef[0]);
				GetNodeOriginFunc = () => PlaneToWorldRef[0].Translation;
				PlaneToWorldRef = new MatrixD[1];
				Config[StateID] |= (uint)HudElementStates.IsSpaceNode;
			}

			protected override void Layout()
			{
				// Determine whether the node is in front of the camera and pointed toward it
				MatrixD camMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
				Vector3D camOrigin = camMatrix.Translation,
					camForward = camMatrix.Forward,
					nodeOrigin = PlaneToWorldRef[0].Translation,
					nodeForward = PlaneToWorldRef[0].Forward;

				IsInFront = Vector3D.Dot((nodeOrigin - camOrigin), camForward) > 0;
				IsFacingCamera = IsInFront && Vector3D.Dot(nodeForward, camForward) > 0;

				MatrixD worldToPlane;
				MatrixD.Invert(ref PlaneToWorldRef[0], out worldToPlane);
				LineD cursorLine = HudMain.Cursor.WorldLine;

				PlaneD plane = new PlaneD(PlaneToWorldRef[0].Translation, PlaneToWorldRef[0].Forward);
				Vector3D worldPos = plane.Intersection(ref cursorLine.From, ref cursorLine.Direction);

				Vector3D planePos;
				Vector3D.TransformNoProjection(ref worldPos, ref worldToPlane, out planePos);

				CursorPos = new Vector3()
				{
					X = (float)planePos.X,
					Y = (float)planePos.Y,
					Z = (float)Math.Round(Vector3D.DistanceSquared(worldPos, cursorLine.From), 6)
				};
			}
		}
	}
}
