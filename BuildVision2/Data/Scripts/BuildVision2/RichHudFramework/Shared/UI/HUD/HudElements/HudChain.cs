using System.Collections.Generic;
using VRageMath;
using System;
using System.Collections;

namespace RichHudFramework.UI
{
    /// <summary>
    /// HUD element used to organize other elements into straight lines, either horizontally or vertically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HudChain<T> : HudElementBase, IEnumerable<T> where T : class, IHudElement
    {
        /// <summary>
        /// Used to allow the addition of child elements using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public HudChain<T> ChildContainer => this;

        /// <summary>
        /// List of elements registered to the chain. Does not include elements that are parented
        /// using normal methods.
        /// </summary>
        public ReadOnlyCollection<T> ChainMembers { get; }

        /// <summary>
        /// Determines whether or not chain elements will be resized to match the
        /// size of the element along the axis of alignment.
        /// </summary>
        public bool AutoResize { get; set; }

        /// <summary>
        /// Determines whether or not chain elements will be aligned vertically.
        /// </summary>
        public bool AlignVertical { get { return axis1 == 0; } set { axis1 = value ? 0 : 1; } }

        /// <summary>
        /// Distance between chain elements along their axis of alignment.
        /// </summary>
        public float Spacing { get { return spacing * Scale; } set { spacing = value / Scale; } }

        private readonly List<T> elements;
        private float spacing;
        private int axis1;

        public HudChain(IHudParent parent = null) : base(parent)
        {
            Spacing = 0f;
            elements = new List<T>();
            ChainMembers = new ReadOnlyCollection<T>(elements);
            AlignVertical = false;
        }

        public IEnumerator<T> GetEnumerator() =>
            elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Adds a <see cref="HudElementBase"/> to the chain.
        /// </summary>
        public void Add(T element)
        {
            RegisterChild(element);

            if (element.Parent == this)
            {
                elements.Add(element);
            }
        }

        /// <summary>
        /// Finds the chain member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public T Find(Func<T, bool> predicate)
        {
            return elements.Find(x => predicate(x));
        }

        /// <summary>
        /// Finds the index of the chain member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public int FindIndex(Func<T, bool> predicate)
        {
            return elements.FindIndex(x => predicate(x));
        }

        /// <summary>
        /// Removes types of <see cref="HudElementBase"/> from the chain.
        /// </summary>
        public override void RemoveChild(IHudNode element)
        {
            var member = element as T;

            if (member != null)
                elements.Remove(member);

            base.RemoveChild(element);
        }

        /// <summary>
        /// Removes the chain member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public void Remove(Func<T, bool> predicate) =>
            RemoveChild(elements.Find(x => predicate(x)));

        protected override void Layout()
        {
            if (elements != null && elements.Count > 0)
            {
                Vector2 offset = Vector2.Zero, size = Size - Padding;

                if (AlignVertical)
                {
                    offset.Y = size.Y / 2f;
                    UpdateOffsetsVertical(offset, size);
                }
                else
                {
                    offset.X = -size.X / 2f;
                    UpdateOffsetsHorizontal(offset, size);
                }

                Size = GetSize() + Padding;
            }
        }

        /// <summary>
        /// Calculates the size of the element
        /// </summary>
        private Vector2 GetSize()
        {
            Vector2 newSize = new Vector2();
            int axis2 = (axis1 == 0) ? 1 : 0;

            if (AutoResize)
                newSize[axis1] = Size[axis1] - Padding[axis1];

            for (int n = 0; n < elements.Count; n++)
            {
                if (elements[n].Visible)
                {
                    newSize[axis2] += elements[n].Size[axis2];

                    if (!AutoResize && elements[n].Size[axis1] > newSize[axis1])
                        newSize[axis1] = elements[n].Size[axis1];

                    if (n != elements.Count - 1)
                        newSize[axis2] += Spacing;
                }
            }

            return newSize;
        }

        /// <summary>
        /// Updates chain member offsets to ensure that they're in a straight, vertical line.
        /// </summary>
        private void UpdateOffsetsVertical(Vector2 offset, Vector2 memberArea)
        {
            for (int n = 0; n < elements.Count; n++)
            {
                if (elements[n].Visible)
                {
                    elements[n].Offset = new Vector2(0f, -(elements[n].Height / 2f)) + offset;

                    if (elements[n].ParentAlignment.HasFlag(ParentAlignments.Left))
                        elements[n].Offset += new Vector2(Padding.X / 2f, 0f);
                    else if (elements[n].ParentAlignment.HasFlag(ParentAlignments.Right))
                        elements[n].Offset += new Vector2(-Padding.X / 2f, 0f);

                    if (AutoResize)
                        elements[n].Width = memberArea.X;

                    offset.Y -= elements[n].Height;

                    if (n != elements.Count - 1)
                        offset.Y -= Spacing;
                }
            }           
        }

        /// <summary>
        /// Updates chain member offsets to ensure that they're in a straight, horizontal line.
        /// </summary>
        private void UpdateOffsetsHorizontal(Vector2 offset, Vector2 memberArea)
        {
            for (int n = 0; n < elements.Count; n++)
            {
                if (elements[n].Visible)
                {
                    elements[n].Offset = new Vector2((elements[n].Width / 2f), 0f) + offset;

                    if (elements[n].ParentAlignment.HasFlag(ParentAlignments.Top))
                        elements[n].Offset += new Vector2(0f, -Padding.Y / 2f);
                    else if (elements[n].ParentAlignment.HasFlag(ParentAlignments.Bottom))
                        elements[n].Offset += new Vector2(0f, Padding.Y / 2f);

                    if (AutoResize)
                        elements[n].Height = memberArea.Y;

                    offset.X += elements[n].Width;

                    if (n != elements.Count - 1)
                        offset.X += Spacing;
                }
            }   
        }
    }
}
