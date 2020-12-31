using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        /// <summary>
        /// Base class for hud elements that can be parented to other elements.
        /// </summary>
        public abstract class HudNodeBase : HudParentBase, IReadOnlyHudNode
        {
            /// <summary>
            /// Read-only parent object of the node.
            /// </summary>
            IReadOnlyHudParent IReadOnlyHudNode.Parent => _parent;

            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public virtual HudParentBase Parent { get { return _parent; } protected set { _parent = value; } }

            /// <summary>
            /// Node defining the coordinate space used to render the UI element
            /// </summary>
            public override IReadOnlyHudSpaceNode HudSpace => _hudSpace;

            /// <summary>
            /// Determines whether or not an element will be drawn or process input. Visible by default.
            /// </summary>
            public override bool Visible
            {
                get { return _visible && parentVisible; }
                set { _visible = value; }
            }

            /// <summary>
            /// Determines whether the UI element will be drawn in the Back, Mid or Foreground
            /// </summary>
            public sealed override sbyte ZOffset
            {
                get { return (sbyte)(_zOffset + parentZOffset); }
                set { _zOffset = (sbyte)(value - parentZOffset); }
            }

            /// <summary>
            /// Scales the size and offset of an element. Any offset or size set at a given
            /// be increased or decreased with scale. Defaults to 1f. Includes parent scale.
            /// </summary>
            public sealed override float Scale => LocalScale * parentScale;

            /// <summary>
            /// Element scaling without parent scaling.
            /// </summary>
            public virtual float LocalScale { get; set; }

            /// <summary>
            /// Indicates whether or not the element has been registered to a parent.
            /// </summary>
            public bool Registered { get { return _registered; } private set { _registered = value; } }

            protected HudParentBase _parent;
            protected IReadOnlyHudSpaceNode _hudSpace;
            protected float parentScale;
            protected bool _visible, parentVisible;
            protected sbyte parentZOffset;

            public HudNodeBase(HudParentBase parent)
            {
                parentScale = 1f;
                LocalScale = 1f;
                parentVisible = true;
                _registered = false;

                Register(parent);
            }

            protected override void BeginLayout(bool refresh)
            {
                fullZOffset = GetFullZOffset(this, _parent);

                if (Visible || refresh)
                {
                    parentScale = _parent == null ? 1f : _parent.Scale;
                    Layout();
                }
            }

            protected override void BeginDraw()
            {
                if (Visible)
                    Draw();

                if (_parent == null)
                {
                    parentVisible = true;
                    parentZOffset = 0;
                }
                else
                {
                    parentVisible = _parent.Visible;
                    parentZOffset = _parent.ZOffset;
                }
            }


            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            public override void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth)
            {
                _hudSpace = _parent?.HudSpace;
                fullZOffset = GetFullZOffset(this, _parent);

                UpdateActions.EnsureCapacity(UpdateActions.Count + children.Count + 1);
                var accessors = new HudUpdateAccessors()
                {
                    Item1 = GetOrSetMemberFunc,
                    Item2 = new MyTuple<Func<ushort>, Func<Vector3D>>(GetZOffsetFunc, HudSpace.GetNodeOriginFunc),
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

            /// <summary>
            /// Registers the element to the given parent object.
            /// </summary>
            public void Register(HudParentBase parent)
            {
                if (parent != null && parent == this)
                    throw new Exception("Types of HudNodeBase cannot be parented to themselves!");

                if (parent != null && _parent == null)
                {
                    Parent = parent;
                    _parent.RegisterChild(this);

                    parentZOffset = _parent.ZOffset;
                    parentScale = _parent.Scale;
                    parentVisible = _parent.Visible;

                    Registered = true;
                }

                HudMain.RefreshDrawList = true;
            }

            /// <summary>
            /// Unregisters the element from its parent, if it has one.
            /// </summary>
            public void Unregister()
            {
                if (Parent != null)
                {
                    HudParentBase lastParent = _parent;

                    Parent = null;
                    lastParent.RemoveChild(this);

                    Registered = false;
                }

                parentZOffset = 0;
                parentScale = 1f;
                parentVisible = true;

                HudMain.RefreshDrawList = true;
            }
        }
    }
}