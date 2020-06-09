using System.Collections.Generic;
using VRageMath;
using System;
using System.Collections;

namespace RichHudFramework.UI
{
    public class HudList<T> : HudChain<T> where T : class, IListBoxEntry
    {
        public HudList(IHudParent parent = null) : base(parent)
        { }

        public T AddReserved()
        {
            T element = null;

            if (Count < elements.Count)
            {
                element = elements[Count];
                element.Visible = true;
                Count++;
            }

            return element;
        }

        /// <summary>
        /// Resets all hud elements in the collection to allow for reuse.
        /// </summary>
        public void Reset()
        {
            for (int n = 0; n < Count; n++)
            {
                elements[n].Reset();
                elements[n].Visible = false;
            }

            Count = 0;
        }
    }
}
