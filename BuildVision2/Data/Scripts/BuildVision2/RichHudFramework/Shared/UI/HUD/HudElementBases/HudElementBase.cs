using System;
using VRage;
using VRageMath;

namespace DarkHelmet
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
            public virtual float Width { get; set; }

            /// <summary>
            /// Height of the hud element in pixels.
            /// </summary>
            public virtual float Height { get; set; }

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
                get { return GetAlignedOrigin(); }
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

            [Obsolete]
            public OriginAlignment OriginAlignment { get; set; }

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            public ParentAlignment ParentAlignment { get; set; }

            /// <summary>
            /// If set to true, the element's size will be set to match that of its parent.
            /// </summary>
            public bool MatchParentSize { get; set; }

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
            private float lastScale;
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
                OriginAlignment = OriginAlignment.Center;
                ParentAlignment = ParentAlignment.Center;
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

                if (MatchParentSize && parent != null && Size != parent.Size)
                    Size = parent.Size;
            }

            protected virtual void ScaleChanged(float change)
            {
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

            private Vector2 GetAlignedOrigin()
            {
                Vector2 origin = GetOriginWithOffset(), alignment = Vector2.Zero;

                if (OriginAlignment.HasFlag(OriginAlignment.Auto))
                {
                    alignment.X = origin.X < 0 ? Size.X / 2f : -Size.X / 2f;
                    alignment.Y = origin.Y < 0 ? Size.Y / 2f : -Size.Y / 2f;
                }
                else 
                {
                    if (OriginAlignment.HasFlag(OriginAlignment.Top))
                        alignment.Y = -Size.Y / 2f;
                    else if (OriginAlignment.HasFlag(OriginAlignment.Bottom))
                        alignment.Y = Size.Y / 2f;

                    if (OriginAlignment.HasFlag(OriginAlignment.Left))
                        alignment.X = Size.X / 2f;
                    else if (OriginAlignment.HasFlag(OriginAlignment.Right))
                        alignment.X = -Size.X / 2f;
                }

                return origin + alignment;
            }

            /// <summary>
            /// Calculates the offset necessary to achieve the alignment specified by the
            /// ParentAlignment property.
            /// </summary>
            private Vector2 GetParentAlignment()
            {
                Vector2 alignment = Vector2.Zero;

                if (ParentAlignment.HasFlag(ParentAlignment.Bottom))
                {
                    if (ParentAlignment.HasFlag(ParentAlignment.InnerV))
                        alignment.Y = -(parent.Size.Y - Size.Y) / 2f;
                    else
                        alignment.Y = -(parent.Size.Y + Size.Y) / 2f;
                }
                else if (ParentAlignment.HasFlag(ParentAlignment.Top))
                {
                    if (ParentAlignment.HasFlag(ParentAlignment.InnerV))
                        alignment.Y = (parent.Size.Y - Size.Y) / 2f;
                    else
                        alignment.Y = (parent.Size.Y + Size.Y) / 2f;
                }

                if (ParentAlignment.HasFlag(ParentAlignment.Left))
                {
                    if (ParentAlignment.HasFlag(ParentAlignment.InnerH))
                        alignment.X = -(parent.Size.X - Size.X) / 2f;
                    else
                        alignment.X = -(parent.Size.X + Size.X) / 2f;
                }
                else if (ParentAlignment.HasFlag(ParentAlignment.Right))
                {
                    if (ParentAlignment.HasFlag(ParentAlignment.InnerH))
                        alignment.X = (parent.Size.X - Size.X) / 2f;
                    else
                        alignment.X = (parent.Size.X + Size.X) / 2f;
                }

                return alignment;
            }
        }
    }
}