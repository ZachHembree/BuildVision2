using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        /// <summary>
        /// Base type for all hud elements with definite size and position. Inherits from HudParentBase and HudNodeBase.
        /// </summary>
        public abstract class HudElementBase : HudNodeBase, IHudElement
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            public override IHudParent Parent
            {
                get { return base.Parent; }
                protected set
                {
                    base.Parent = value;
                    parent = value as HudElementBase;
                }
            }

            /// <summary>
            /// Scales the size and offset of an element. Any offset or size set at a given
            /// be increased or decreased with scale. Defaults to 1f. Includes parent scale.
            /// </summary>
            public virtual float Scale
            {
                get { return (parent == null || ignoreParentScale) ? localScale : localScale * parent.Scale; }
                set { localScale = value; }
            }

            /// <summary>
            /// Size of the element in pixels.
            /// </summary>
            public virtual Vector2 Size
            {
                get { return new Vector2(Width, Height); }
                set { Width = value.X; Height = value.Y; }
            }

            /// <summary>
            /// With of the hud element in pixels.
            /// </summary>
            public virtual float Width
            {
                get { return width + Padding.X; }
                set
                {
                    if (Padding.X < value)
                        width = value - Padding.X;
                    else
                        width = value;
                }
            }

            /// <summary>
            /// Height of the hud element in pixels.
            /// </summary>
            public virtual float Height
            {
                get { return height + Padding.Y; }
                set
                {
                    if (Padding.Y < value)
                        height = value - Padding.Y;
                    else
                        height = value;
                }
            }

            public virtual Vector2 Padding { get; set; }

            public Vector2 NativeSize
            {
                get { return HudMain.GetNativeVector(Size); }
                set { Size = HudMain.GetPixelVector(value); }
            }

            /// <summary>
            /// Starting position of the hud element on the screen in pixels.
            /// </summary>
            public virtual Vector2 Origin
            {
                get { return GetOriginWithOffset(); }
                protected set { origin = value; }
            }

            public Vector2 NativeOrigin
            {
                get { return HudMain.GetNativeVector(Origin); }
                protected set { Origin = HudMain.GetPixelVector(value); }
            }

            /// <summary>
            /// Position of the element relative to its origin.
            /// </summary>
            public virtual Vector2 Offset { get; set; }

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            public ParentAlignments ParentAlignment { get; set; }

            public DimAlignments DimAlignment { get; set; }

            /// <summary>
            /// If set to true the hud element will be allowed to capture the cursor.
            /// </summary>
            public bool CaptureCursor { get; set; }

            /// <summary>
            /// If set to true the hud element will share the cursor with its child elements.
            /// </summary>
            public bool ShareCursor { get; set; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over the element. The element must
            /// be set to capture the cursor for this to work.
            /// </summary>
            public virtual bool IsMousedOver => Visible && isMousedOver;

            public bool ignoreParentScale;

            private const float minMouseBounds = 8f;
            private float lastScale, width, height;
            private bool isMousedOver;
            private Vector2 origin;

            protected float localScale;
            protected HudElementBase parent;

            /// <summary>
            /// Initializes a new hud element with cursor sharing enabled and scaling set to 1f.
            /// </summary>
            public HudElementBase(IHudParent parent) : base(parent)
            {
                ShareCursor = true;
                DimAlignment = DimAlignments.None;
                ParentAlignment = ParentAlignments.Center;
                localScale = 1f;
                lastScale = 1f;
            }

            /// <summary>
            /// If visible == true, it will update the input of the element before updating 
            /// the input of its child elements.
            /// </summary>
            public override void BeforeInput()
            {
                if (Visible)
                {
                    if (!ShareCursor)
                        HandleChildInput();

                    if (CaptureCursor && HudMain.Cursor.Visible && !HudMain.Cursor.IsCaptured)
                    {
                        isMousedOver = IsMouseInBounds();

                        if (isMousedOver)
                            HudMain.Cursor.Capture(ID);
                    }
                    else
                        isMousedOver = false;

                    HandleInput();

                    if (ShareCursor)
                        ShareInput();
                }
                else
                    isMousedOver = false;
            }

            /// <summary>
            /// If visible == true, the element will draw itself before updating its child
            /// elements.
            /// </summary>
            public override void BeforeDraw()
            {
                base.BeforeDraw();

                if (lastScale != Scale) // I sense some frustrating edge cases in my future
                {
                    ScaleChanged(Scale / lastScale);
                    lastScale = Scale;
                }

                if (parent != null && Size != parent.Size)
                {
                    if (DimAlignment.HasFlag(DimAlignments.IgnorePadding))
                    {
                        if (DimAlignment.HasFlag(DimAlignments.Width))
                            Width = parent.Width - parent.Padding.X;

                        if (DimAlignment.HasFlag(DimAlignments.Height))
                            Height = parent.Height - parent.Padding.Y;
                    }
                    else
                    {
                        if (DimAlignment.HasFlag(DimAlignments.Width))
                            Width = parent.Width;

                        if (DimAlignment.HasFlag(DimAlignments.Height))
                            Height = parent.Height;
                    }
                }
            }

            protected virtual void ScaleChanged(float change)
            {
                Padding *= change;
                Size *= change;
            }

            /// <summary>
            /// Temporarily releases the cursor and shares it with its child elements.
            /// </summary>
            private void ShareInput()
            {
                bool wasCapturing = isMousedOver && HudMain.Cursor.IsCapturing(ID);
                HudMain.Cursor.TryRelease(ID);
                HandleChildInput();

                if (!HudMain.Cursor.IsCaptured && wasCapturing)
                    HudMain.Cursor.Capture(ID);
            }

            /// <summary>
            /// Updates child element input.
            /// </summary>
            private void HandleChildInput()
            {
                for (int n = children.Count - 1; n >= 0; n--)
                {
                    if (children[n].Visible)
                        children[n].BeforeInput();
                }
            }

            /// <summary>
            /// Determines whether or not the cursor is within the bounds of the hud element.
            /// </summary>
            private bool IsMouseInBounds()
            {
                Vector2 pos = Origin + Offset, cursorPos = HudMain.Cursor.Origin;
                float
                    width = Math.Max(minMouseBounds, Size.X),
                    height = Math.Max(minMouseBounds, Size.Y),
                    leftBound = pos.X - width / 2f,
                    rightBound = pos.X + width / 2f,
                    upperBound = pos.Y + height / 2f,
                    lowerBound = pos.Y - height / 2f;

                return
                    (cursorPos.X >= leftBound && cursorPos.X < rightBound) &&
                    (cursorPos.Y >= lowerBound && cursorPos.Y < upperBound);
            }

            private Vector2 GetOriginWithOffset() =>
                    (parent == null) ? origin : (origin + parent.Origin + parent.Offset + GetParentAlignment());

            /// <summary>
            /// Calculates the offset necessary to achieve the alignment specified by the
            /// ParentAlignment property.
            /// </summary>
            private Vector2 GetParentAlignment()
            {
                Vector2 alignment = Vector2.Zero;

                if (ParentAlignment.HasFlag(ParentAlignments.Bottom))
                {
                    if (ParentAlignment.HasFlag(ParentAlignments.InnerV))
                        alignment.Y = -(parent.Size.Y - Size.Y) / 2f;
                    else
                        alignment.Y = -(parent.Size.Y + Size.Y) / 2f;
                }
                else if (ParentAlignment.HasFlag(ParentAlignments.Top))
                {
                    if (ParentAlignment.HasFlag(ParentAlignments.InnerV))
                        alignment.Y = (parent.Size.Y - Size.Y) / 2f;
                    else
                        alignment.Y = (parent.Size.Y + Size.Y) / 2f;
                }

                if (ParentAlignment.HasFlag(ParentAlignments.Left))
                {
                    if (ParentAlignment.HasFlag(ParentAlignments.InnerH))
                        alignment.X = -(parent.Size.X - Size.X) / 2f;
                    else
                        alignment.X = -(parent.Size.X + Size.X) / 2f;
                }
                else if (ParentAlignment.HasFlag(ParentAlignments.Right))
                {
                    if (ParentAlignment.HasFlag(ParentAlignments.InnerH))
                        alignment.X = (parent.Size.X - Size.X) / 2f;
                    else
                        alignment.X = (parent.Size.X + Size.X) / 2f;
                }

                return alignment;
            }
        }
    }
}