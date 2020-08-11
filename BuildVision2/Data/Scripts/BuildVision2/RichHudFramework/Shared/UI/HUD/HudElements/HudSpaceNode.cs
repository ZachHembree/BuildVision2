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

        /// <summary>
        /// HUD node used to replace the standard Pixel to World matrix with an arbitrary
        /// world matrix transform. Typically parented to HudMain.Root.
        /// </summary>
        public class HudSpaceNode : HudNodeBase
        {
            /// <summary>
            /// Returns the current draw matrix
            /// </summary>
            public MatrixD PlaneToWorld => _planeToWorld;

            /// <summary>
            /// Used to update the current draw matrix. If no delegate is set, the node will default
            /// to the matrix supplied by its parent.
            /// </summary>
            public Func<MatrixD> UpdateMatrixFunc;

            /// <summary>
            /// Cursor position on the XY plane defined by the HUD space. Z == dist from screen.
            /// </summary>
            public Vector3 CursorPos { get; protected set; }

            /// <summary>
            /// If set to true, then the cursor will be drawn in the node's HUD space when capturing.
            /// True by default.
            /// </summary>
            public bool DrawCursorInHudSpace { get; set; }

            protected MatrixD _planeToWorld;
            protected readonly HudSpaceDelegate GetHudSpaceFunc;

            public HudSpaceNode(HudParentBase parent = null) : base(parent)
            {
                GetHudSpaceFunc = () => new MyTuple<bool, float, MatrixD>(DrawCursorInHudSpace, Scale, _planeToWorld);
                DrawCursorInHudSpace = true;
            }

            protected override MyTuple<Vector3, HudSpaceDelegate> InputDepth(Vector3 cursorPos, HudSpaceDelegate GetHudSpaceFunc)
            {
                if (Visible)
                {
                    MatrixD worldToPlane = MatrixD.Invert(_planeToWorld),
                    pixelToWorld = HudMain.PixelToWorld;

                    Vector3D worldPos = HudMain.Cursor.WorldPos;
                    Vector3D.TransformNoProjection(ref worldPos, ref worldToPlane, out worldPos);

                    // I'm not interested in the Z coordinate. That only gives me the distance from the 
                    // XY plane of the node's matrix.
                    worldPos.Z = 0d;
                    cursorPos = new Vector3(worldPos.X, worldPos.Y, 0f);

                    // Project worldPos back into screen space to get distance from the screen.
                    Vector3D.TransformNoProjection(ref worldPos, ref pixelToWorld, out worldPos);

                    // X & Y == Cursor position on the XY plane of the node's matrix. Z == dist from 
                    // screen to facilitate depth testing.
                    cursorPos.Z = (float)Math.Abs(worldPos.Z);
                    GetHudSpaceFunc = this.GetHudSpaceFunc;

                    CursorPos = cursorPos;
                }

                return new MyTuple<Vector3, HudSpaceDelegate>(cursorPos, GetHudSpaceFunc);
            }

            protected override MyTuple<Vector3, HudSpaceDelegate> BeginInput(Vector3 cursorPos, HudSpaceDelegate GetHudSpaceFunc)
            {
                if (Visible)
                {
                    cursorPos = CursorPos;
                    GetHudSpaceFunc = this.GetHudSpaceFunc;

                    HandleInput();
                }

                return new MyTuple<Vector3, HudSpaceDelegate>(cursorPos, GetHudSpaceFunc);
            }

            protected override object BeginDraw(object oldMatrix)
            {
                if (UpdateMatrixFunc != null)
                    _planeToWorld = UpdateMatrixFunc();
                else
                    _planeToWorld = (MatrixD)oldMatrix;

                Draw(_planeToWorld);

                return _planeToWorld;
            }
        }
    }
}
