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
                get { return (parent == null || ignoreParentScale) ? localScale : localScale * parentScale; }
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
            /// Width of the hud element in pixels.
            /// </summary>
            public virtual float Width
            {
                get { return (width * Scale) + Padding.X; }
                set
                {
                    if (Padding.X < value)
                        width = (value - Padding.X) / Scale;
                    else
                        width = (value / Scale);
                }
            }

            /// <summary>
            /// Height of the hud element in pixels.
            /// </summary>
            public virtual float Height
            {
                get { return (height * Scale) + Padding.Y; }
                set
                {
                    if (Padding.Y < value)
                        height = (value - Padding.Y) / Scale;
                    else
                        height = (value / Scale);
                }
            }

            /// <summary>
            /// Border size. Included in total element size.
            /// </summary>
            public virtual Vector2 Padding { get { return padding * Scale; } set { padding = value / Scale; } }

            /// <summary>
            /// Starting position of the hud element on the screen in pixels.
            /// </summary>
            public virtual Vector2 Origin => (parent == null) ? Vector2.Zero : (parent.Origin + parent.Offset + offsetAlignment);

            /// <summary>
            /// Position of the element relative to its origin.
            /// </summary>
            public virtual Vector2 Offset { get { return offset * Scale; } set { offset = value / Scale; } }

            /// <summary>
            /// Absolute position of the hud element. Origin + Offset.
            /// </summary>
            public Vector2 Position => Origin + Offset;

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            public ParentAlignments ParentAlignment { get; set; }

            /// <summary>
            /// Determines how/if an element will copy its parent's dimensions. 
            /// </summary>
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
            /// If true, the hud element will cede cursor input to its children.
            /// </summary>
            public bool CedeCursor { get; set; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over the element. The element must
            /// be set to capture the cursor for this to work.
            /// </summary>
            public virtual bool IsMousedOver => Visible && isMousedOver;

            /// <summary>
            /// Determines whether or not the scale of the parent element should be used in the calculation
            /// of the final scale. True by default.
            /// </summary>
            public bool ignoreParentScale;

            private const float minMouseBounds = 8f;
            private float parentScale, localScale, width, height;
            private bool isMousedOver;
            private Vector2 offset, padding, offsetAlignment;

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
                parentScale = 1f;
            }

            public sealed override void HandleInputStart()
            {
                if (Visible)
                {
                    if (CedeCursor)
                        HandleChildInput();

                    if (CaptureCursor && HudMain.Cursor.Visible && !HudMain.Cursor.IsCaptured)
                    {
                        isMousedOver = IsMouseInBounds();

                        if (isMousedOver)
                            HudMain.Cursor.Capture(ID);
                    }
                    else
                        isMousedOver = false;

                    if (!CedeCursor)
                    {
                        if (ShareCursor)
                            ShareInput();
                        else
                            HandleChildInput();
                    }

                    HandleInput();
                }
                else
                    isMousedOver = false;
            }

            /// <summary>
            /// Temporarily releases the cursor and shares it with its child elements then attempts
            /// to recapture it.
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
            /// Updates input for child elements.
            /// </summary>
            private void HandleChildInput()
            {
                for (int n = children.Count - 1; n >= 0; n--)
                {
                    if (children[n].Visible)
                        children[n].HandleInputStart();
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

            public override void BeforeDrawStart()
            {             
                base.BeforeDrawStart();

                if (parent != null)
                {
                    if (parentScale != parent.Scale)
                        parentScale = parent.Scale;

                    GetDimAlignment();
                    offsetAlignment = GetParentAlignment();
                }
            }

            /// <summary>
            /// Updates element dimensions to match those of its parent in accordance
            /// with its DimAlignment.
            /// </summary>
            private void GetDimAlignment()
            {
                if (DimAlignment != DimAlignments.None)
                {
                    float width = Width, height = Height,
                        parentWidth = parent.Width, parentHeight = parent.Height;

                    if (DimAlignment.HasFlag(DimAlignments.IgnorePadding))
                    {
                        Vector2 parentPadding = parent.Padding;

                        if (DimAlignment.HasFlag(DimAlignments.Width))
                            width = parentWidth - parentPadding.X;

                        if (DimAlignment.HasFlag(DimAlignments.Height))
                            height = parentHeight - parentPadding.Y;
                    }
                    else
                    {
                        if (DimAlignment.HasFlag(DimAlignments.Width))
                            width = parentWidth;

                        if (DimAlignment.HasFlag(DimAlignments.Height))
                            height = parentHeight;
                    }

                    Width = width;
                    Height = height;
                }
            }

            /// <summary>
            /// Calculates the offset necessary to achieve the alignment specified by the
            /// ParentAlignment property.
            /// </summary>
            private Vector2 GetParentAlignment()
            {
                Vector2 size = new Vector2(Width, Height),
                    alignment = Vector2.Zero,
                    max = (parent.Size + Size) / 2f, 
                    min = -max;

                if (ParentAlignment.HasFlag(ParentAlignments.UsePadding))
                {
                    Vector2 parentPadding = parent.Padding;

                    min += parentPadding / 2f;
                    max -= parentPadding / 2f;
                }

                if (ParentAlignment.HasFlag(ParentAlignments.InnerV))
                {
                    min.Y += size.Y;
                    max.Y -= size.Y;
                }

                if (ParentAlignment.HasFlag(ParentAlignments.InnerH))
                {
                    min.X += size.X;
                    max.X -= size.X;
                }

                if (ParentAlignment.HasFlag(ParentAlignments.Bottom))
                    alignment.Y = min.Y;
                else if (ParentAlignment.HasFlag(ParentAlignments.Top))
                    alignment.Y = max.Y;

                if (ParentAlignment.HasFlag(ParentAlignments.Left))
                    alignment.X = min.X;
                else if (ParentAlignment.HasFlag(ParentAlignments.Right))
                    alignment.X = max.X;

                return alignment;
            }
        }
    }
}