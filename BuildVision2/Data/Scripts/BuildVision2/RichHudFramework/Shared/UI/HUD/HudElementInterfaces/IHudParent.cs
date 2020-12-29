using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        public enum HudElementAccessors : int
        {
            /// <summary>
            /// out: System.Type
            /// </summary>
            GetType = 1,
        }

        /// <summary>
        /// Read-only interface for types capable of serving as parent objects to <see cref="HudNodeBase"/>s.
        /// </summary>
        public interface IReadOnlyHudParent
        {
            /// <summary>
            /// Node defining the coordinate space used to render the UI element
            /// </summary>
            IReadOnlyHudSpaceNode HudSpace { get; }

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
            sbyte ZOffset { get; }

            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth);
        }
    }
}