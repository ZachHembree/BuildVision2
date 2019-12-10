using System.Collections.Generic;
using VRageMath;
using System;

namespace DarkHelmet.UI
{
    public class HudChain<T> : PaddedElementBase where T : IHudElement
    {
        public ReadOnlyCollection<T> ChainElements { get; }
        public new HudChain<T> ChildContainer => this;

        /// <summary>
        /// Determines whether or not chain elements will be resized to match the
        /// size of the element along the axis of alignment.
        /// </summary>
        public bool AutoResize { get; set; }

        /// <summary>
        /// Determines whether or not chain elements will be aligned vertically.
        /// </summary>
        public bool AlignVertical { get; set; }

        /// <summary>
        /// Distance between chain elements along their axis of alignment.
        /// </summary>
        public float Spacing { get { return spacing * Scale; } set { spacing = value / Scale; } }

        protected readonly List<T> elements;
        protected float spacing;

        public HudChain(IHudParent parent = null) : base(parent)
        {
            Spacing = 0f;
            elements = new List<T>();
            ChainElements = new ReadOnlyCollection<T>(elements);
        }

        /// <summary>
        /// Adds types of <see cref="HudElementBase"/> to the chain.
        /// </summary>
        public void Add(T element, bool addToChain)
        {
            Add(element);

            if (addToChain)
            {
                elements.Add(element);
            }
        }

        /// <summary>
        /// Removes types of <see cref="HudElementBase"/> from the chain.
        /// </summary>
        public void Remove(T element)
        {
            int index = elements.FindIndex(x => x.Equals(element));

            if (index != -1)
            {
                elements[index].Unregister();
                elements.RemoveAt(index);
            }
        }

        public void Remove(Func<T, bool> predicate)
        {
            int index = elements.FindIndex(x => predicate(x));

            if (index != -1)
            {
                elements[index].Unregister();
                elements.RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            elements.RemoveAt(index);
        }

        protected override void Draw()
        {
            if (elements != null && elements.Count > 0)
            {
                Vector2 offset = Vector2.Zero, size = GetSize();

                if (AlignVertical)
                    offset.Y = size.Y / 2f;
                else
                    offset.X = -size.X / 2f;

                UpdateOffsets(offset, size);
                Size = size + Padding;
            }
        }

        protected virtual Vector2 GetSize()
        {
            float width = 0f, height = 0f;

            if (AutoResize)
            {
                if (AlignVertical)
                    width = Width - Padding.X;
                else
                    height = Height - Padding.Y;
            }

            for (int n = 0; n < elements.Count; n++)
            {
                if (elements[n].Visible)
                {
                    if (AlignVertical)
                    {
                        height += elements[n].Height;

                        if (!AutoResize && elements[n].Width > width)
                            width = elements[n].Width;

                        if (n != elements.Count - 1)
                            height += Spacing * Scale;
                    }
                    else
                    {
                        width += elements[n].Width;

                        if (!AutoResize && elements[n].Height > height)
                            height = elements[n].Height;

                        if (n != elements.Count - 1)
                            width += Spacing * Scale;
                    }
                }
            }

            return new Vector2(width, height);
        }

        protected void UpdateOffsets(Vector2 offset, Vector2 memberArea)
        {
            for (int n = 0; n < elements.Count; n++)
            {
                if (elements[n].Visible)
                {
                    if (AlignVertical)
                    {                     
                        elements[n].Offset = new Vector2(0f, -(elements[n].Height / 2f)) + offset;

                        if (elements[n].ParentAlignment.HasFlag(ParentAlignment.Left))
                            elements[n].Offset += new Vector2(Padding.X / 2f, 0f);
                        else if (elements[n].ParentAlignment.HasFlag(ParentAlignment.Right))
                            elements[n].Offset += new Vector2(-Padding.X / 2f, 0f);

                        if (AutoResize)
                            elements[n].Width = memberArea.X;

                        offset.Y -= elements[n].Height;

                        if (n != elements.Count - 1)
                            offset.Y -= Spacing * Scale;
                    }
                    else
                    {
                        elements[n].Offset = new Vector2((elements[n].Width / 2f), 0f) + offset;

                        if (elements[n].ParentAlignment.HasFlag(ParentAlignment.Top))
                            elements[n].Offset += new Vector2(0f, -Padding.Y / 2f);
                        else if (elements[n].ParentAlignment.HasFlag(ParentAlignment.Bottom))
                            elements[n].Offset += new Vector2(0f, Padding.Y / 2f);

                        if (AutoResize)
                            elements[n].Height = memberArea.Y;

                        offset.X += elements[n].Width;

                        if (n != elements.Count - 1)
                            offset.X += Spacing * Scale;
                    }
                }
            }
        }
    }
}
