using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI
{
    public abstract class HudNodeBase : IReadonlyHudNode
    {
        public virtual bool Visible { get; set; }
        public virtual HudNodeBase Parent { get; protected set; }
        public virtual ReadOnlyCollection<HudNodeBase> Children { get; }

        private readonly List<HudNodeBase> children;
        private int childIndex;

        public HudNodeBase(HudNodeBase parent)
        {
            Visible = true;
            children = new List<HudNodeBase>();
            Children = new ReadOnlyCollection<HudNodeBase>(children);

            if (parent != null)
                Register(parent);
        }

        public virtual void BeforeInput()
        {
            if (Visible)
            {
                HandleInput();

                for (int n = children.Count - 1; n >= 0; n--)
                {
                    if (children[n].Visible)
                        children[n].BeforeInput();
                }
            }
        }

        protected virtual void HandleInput() { }

        public virtual void BeforeDraw()
        {
            if (Visible)
            {
                Draw();

                foreach (HudNodeBase child in children)
                {
                    if (child.Visible)
                        child.BeforeDraw();
                }
            }
        }

        protected virtual void Draw() { }

        public void SetFocus()
        {
            if (Parent != null)
            {
                int last = Parent.children.Count - 1;

                Parent.children[last].childIndex = childIndex;
                Parent.children.Move(childIndex, last);
                childIndex = last;
            }
        }

        public void RegisterChildren(IEnumerable<HudNodeBase> newChildren)
        {
            foreach (HudNodeBase child in newChildren)
                child.Register(this);
        }

        public void Register(IReadonlyHudNode parent) =>
            Register(parent);

        public void Register(HudNodeBase parent)
        {
            if (Parent != null)
                Unregister();

            Parent = parent;
            childIndex = Parent.children.Count;
            Parent.children.Add(this);
        }

        public void Unregister()
        {
            if (Parent != null)
            {
                Parent.children.RemoveAt(childIndex);
                Parent = null;
            }
        }
    }

    public abstract class HudElementBase : HudNodeBase, IReadonlyHudElement
    {
        public override HudNodeBase Parent
        {
            get { return base.Parent; }
            protected set
            {
                base.Parent = value;
                parent = value as HudElementBase;
            }
        }
        /// <summary>
        /// Current scale of the element. Includes parent scale by default.
        /// </summary>
        public virtual float Scale
        {
            get { return (parent == null || ignoreParentScale) ? localScale : localScale * parent.Scale; }
            set { localScale = value; }
        }
        /// <summary>
        /// Size of the element in pixels.
        /// </summary>
        public virtual Vector2 Size { get; protected set; }
        public Vector2 NativeSize
        {
            get { return HudMain.GetNativeVector(Size); }
            protected set { Size = HudMain.GetPixelVector(value); }
        }
        /// <summary>
        /// Origin of the element, not including its offset, w/respect to the sum of the origin of its parent and that parent's offset, if one exists.
        /// </summary>
        public virtual Vector2 Origin
        {
            get { return GetAlignedOrigin(); }
            set { origin = value; }
        }
        public Vector2 NativeOrigin
        {
            get { return HudMain.GetNativeVector(Origin); }
            set { Origin = HudMain.GetPixelVector(value); }
        }
        public virtual Vector2 Offset { get; set; }
        public OriginAlignment OriginAlignment { get; set; }
        public ParentAlignment ParentAlignment { get; set; }

        public bool CaptureCursor { get; protected set; }
        public bool ShareCursor { get; protected set; }
        public bool IsMousedOver { get { return (isMousedOver && HudMain.Cursor.IsCapturing(this)); } }
        public bool ignoreParentScale;

        private const float minMouseBounds = 8f;
        private float lastScale;
        private bool isMousedOver;
        private Vector2 origin;

        protected float localScale;
        protected HudElementBase parent;

        public HudElementBase(HudNodeBase parent) : base(parent)
        {
            ShareCursor = true;
            OriginAlignment = OriginAlignment.Center;
            ParentAlignment = ParentAlignment.Center;
            localScale = 1f;
            lastScale = 1f;
        }

        public sealed override void BeforeInput()
        {
            if (Visible)
            {
                if (!ShareCursor)
                    HandleChildInput();

                if (CaptureCursor && HudMain.Cursor.Visible && !HudMain.Cursor.IsCaptured)
                {
                    isMousedOver = IsMouseInBounds();

                    if (isMousedOver)
                        HudMain.Cursor.Capture(this);                       
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

        public sealed override void BeforeDraw()
        {
            base.BeforeDraw();

            if (Visible && lastScale != Scale) // I sense some frustrating edge cases in my future
            {
                ScaleChanged(Scale / lastScale);
                lastScale = Scale;
            }
        }

        protected virtual void ScaleChanged(float change)
        {
            Size *= change;
        }

        private void ShareInput()
        {
            bool wasCapturing = IsMousedOver;
            HudMain.Cursor.TryRelease(this);

            HandleChildInput();

            if (!HudMain.Cursor.IsCaptured && wasCapturing)
                HudMain.Cursor.Capture(this);
        }

        private void HandleChildInput()
        {
            for (int n = Children.Count - 1; n >= 0; n--)
            {
                if (Children[n].Visible)
                    Children[n].BeforeInput();
            }
        }

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
            Vector2 origin = GetOriginWithOffset(), alignment;

            if (OriginAlignment == OriginAlignment.UpperLeft)
                alignment = new Vector2(Size.X / 2f, -Size.Y / 2f);
            else if (OriginAlignment == OriginAlignment.UpperRight)
                alignment = new Vector2(-Size.X / 2f, -Size.Y / 2f);
            else if (OriginAlignment == OriginAlignment.LowerRight)
                alignment = new Vector2(-Size.X / 2f, Size.Y / 2f);
            else if (OriginAlignment == OriginAlignment.LowerLeft)
                alignment = new Vector2(Size.X / 2f, Size.Y / 2f);
            else if (OriginAlignment == OriginAlignment.Auto)
            {
                alignment = Vector2.Zero;
                alignment.X = origin.X < 0 ? Size.X / 2f : -Size.X / 2f;
                alignment.Y = origin.Y < 0 ? Size.Y / 2f : -Size.Y / 2f;
            }
            else
                alignment = Vector2.Zero;

            return origin + alignment;
        }

        private Vector2 GetParentAlignment()
        {
            Vector2 alignment;

            if (ParentAlignment == ParentAlignment.Bottom)
                alignment = new Vector2(0, -(parent.Size.Y + Size.Y) / 2f);
            else if (ParentAlignment == ParentAlignment.Left)
                alignment = new Vector2(-(parent.Size.X + Size.X) / 2f, 0);
            else if (ParentAlignment == ParentAlignment.Right)
                alignment = new Vector2((parent.Size.X + Size.X) / 2f, 0);
            else if (ParentAlignment == ParentAlignment.Top)
                alignment = new Vector2(0, (parent.Size.Y + Size.Y) / 2f);
            else
                alignment = Vector2.Zero;

            return alignment;
        }
    }

    /// <summary>
    /// Base class for hud elements that can be manually resized.
    /// </summary>
    public abstract class ResizableElementBase : HudElementBase, IReadonlyResizableElement
    {
        public sealed override Vector2 Size
        {
            get { return new Vector2(Width, Height); }
            protected set { Width = value.X; Height = value.Y; }
        }
        /// <summary>
        /// With of the hud element in pixels.
        /// </summary>
        public virtual float Width { get; set; }
        /// <summary>
        /// Height of the hud element in pixels.
        /// </summary>
        public virtual float Height { get; set; }
        public virtual Vector2 MinimumSize { get; protected set; }
        public bool autoResize;

        public ResizableElementBase(HudNodeBase parent) : base(parent)
        { }

        public virtual void SetSize(Vector2 newSize) =>
            Size = newSize;

        protected sealed override void Draw()
        {
            Vector2 minSize = MinimumSize;

            if (autoResize)
                Size = minSize;
            else
            {
                if (Width < minSize.X)
                    Width = minSize.X;

                if (Height < minSize.Y)
                    Height = minSize.Y;
            }

            AfterDraw();
        }

        protected virtual void AfterDraw() { }
    }

    /// <summary>
    /// Base class for hud elements that have text elements and a <see cref="TexturedBox"/> background.
    /// </summary>
    public abstract class TextBoxBase : ResizableElementBase, IReadonlyTextBox
    {
        public override float Width { get { return background.Width; } set { background.Width = value; } }
        public override float Height { get { return background.Height; } set { background.Height = value; } }
        public override Vector2 MinimumSize => TextSize + Padding; 
        public Vector2 Padding { get { return padding; } set { padding = Utils.Math.Abs(value); } }
        public abstract Vector2 TextSize { get; }
        public abstract float TextScale { get; set; }
        public virtual Color BgColor { get { return background.Color; } set { background.Color = value; } }
        public IReadonlyResizableElement Background => background;

        private readonly TexturedBox background;
        private Vector2 padding;

        public TextBoxBase(HudNodeBase parent) : base(parent)
        {
            autoResize = true;
            background = new TexturedBox(this);
        }

        protected override void ScaleChanged(float change)
        {
            base.ScaleChanged(change);
            Padding *= change;
        }
    }
}