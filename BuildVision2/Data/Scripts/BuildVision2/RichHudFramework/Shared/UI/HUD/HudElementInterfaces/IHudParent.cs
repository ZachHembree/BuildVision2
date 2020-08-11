using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using HudLayoutDelegate = System.Func<bool, bool>;
using HudDrawDelegate = System.Func<object, object>;

namespace RichHudFramework
{
    using HudInputDelegate = Func<Vector3, HudSpaceDelegate, MyTuple<Vector3, HudSpaceDelegate>>;

    namespace UI
    {
        using HudUpdateAccessors = MyTuple<
            ushort, // ZOffset
            byte, // Depth
            HudInputDelegate, // DepthTest
            HudInputDelegate, // HandleInput
            HudLayoutDelegate, // BeforeLayout
            HudDrawDelegate // BeforeDraw
        >;

        public enum HudParentAccessors : int
        {
            Add = 1,
            RemoveChild = 2,
            SetFocus = 3,
        }

        /// <summary>
        /// Read-only interface for types capable of serving as parent objects to <see cref="HudNodeBase"/>s.
        /// </summary>
        public interface IReadOnlyHudParent
        {
            /// <summary>
            /// Determines whether or not the element will be drawn and/or accept
            /// input.
            /// </summary>
            bool Visible { get; }

            /// <summary>
            /// Scales the size and offset of an element. Any offset or size set at a given
            /// be increased or decreased with scale. Defaults to 1f. Includes parent scale.
            /// </summary>
            float Scale { get; }

            /// <summary>
            /// Used to change the draw order of the UI element. Lower offsets place the element
            /// further in the background. Higher offsets draw later and on top.
            /// </summary>
            sbyte ZOffset { get; set; }

            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            void GetUpdateAccessors(List<HudUpdateAccessors> DrawActions, byte treeDepth);
        }
    }
}