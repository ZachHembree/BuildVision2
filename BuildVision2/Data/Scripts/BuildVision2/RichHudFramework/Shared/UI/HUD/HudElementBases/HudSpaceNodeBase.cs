using Sandbox.ModAPI;
using System;
using VRage;
using VRage.ModAPI;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;
        using System.Collections.Generic;
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

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
            /// Returns true if the space node is visible and rendering.
            /// </summary>
            public override bool Visible => _visible && parentVisible && IsInFront;

            /// <summary>
            /// Returns the current draw matrix
            /// </summary>
            public MatrixD PlaneToWorld { get; protected set; }

            /// <summary>
            /// Cursor position on the XY plane defined by the HUD space. Z == dist from screen.
            /// </summary>
            public Vector3 CursorPos { get; protected set; }

            /// <summary>
            /// If set to true, then the cursor will be drawn in the node's HUD space when being captured by thsi node.
            /// True by default.
            /// </summary>
            public bool DrawCursorInHudSpace { get; set; }

            /// <summary>
            /// Delegate used to retrieve current hud space. Used with cursor.
            /// </summary>
            public HudSpaceDelegate GetHudSpaceFunc { get; protected set; }

            /// <summary>
            /// Returns the world space position of the node's origin.
            /// </summary>
            public Func<Vector3D> GetNodeOriginFunc { get; protected set; }

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
                GetHudSpaceFunc = () => new MyTuple<bool, float, MatrixD>(DrawCursorInHudSpace, Scale, PlaneToWorld);
                DrawCursorInHudSpace = true;
                GetNodeOriginFunc = () => PlaneToWorld.Translation;
            }

            protected override void Layout()
            {
                // Determine whether the node is in front of the camera and pointed toward it
                MatrixD camMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                Vector3D camOrigin = camMatrix.Translation,
                    camForward = camMatrix.Forward,
                    nodeOrigin = PlaneToWorld.Translation,
                    nodeForward = PlaneToWorld.Forward;

                IsInFront = Vector3D.Dot((nodeOrigin - camOrigin), camForward) > 0;
                IsFacingCamera = IsInFront && Vector3D.Dot(nodeForward, camForward) > 0;

                if (Visible)
                {
                    MatrixD worldToPlane = MatrixD.Invert(PlaneToWorld);
                    LineD cursorLine = HudMain.Cursor.WorldLine;

                    PlaneD plane = new PlaneD(nodeOrigin, nodeForward);
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

            public override void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth)
            {
                fullZOffset = ParentUtils.GetFullZOffset(this, _parent);
                UpdateActions.EnsureCapacity(UpdateActions.Count + children.Count + 1);

                var accessors = new HudUpdateAccessors()
                {
                    Item1 = GetOrSetMemberFunc,
                    Item2 = new MyTuple<Func<ushort>, Func<Vector3D>>(GetZOffsetFunc, GetNodeOriginFunc),
                    Item3 = DepthTestAction,
                    Item4 = InputAction,
                    Item5 = LayoutAction,
                    Item6 = DrawAction
                };

                UpdateActions.Add(accessors);
                treeDepth++;

                for (int n = 0; n < children.Count; n++)
                    children[n].GetUpdateAccessors(UpdateActions, treeDepth);
            }
        }
    }
}
