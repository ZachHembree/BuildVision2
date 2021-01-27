using RichHudFramework;
using RichHudFramework.UI;
using System.Collections.Generic;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvScrollMenu
    {
        /// <summary>
        /// Object pool of BvPropertyBox instances wrapped in ScrollBoxEntry
        /// </summary>
        private class BvPropBoxPool : ObjectPool<ScrollBoxEntry<BvPropertyBox>>
        {
            public BvPropBoxPool() : base(new PropBoxPoolPolicy())
            { }

            private class PropBoxPoolPolicy : IPooledObjectPolicy<ScrollBoxEntry<BvPropertyBox>>
            {
                public ScrollBoxEntry<BvPropertyBox> GetNewObject()
                {
                    var container = new ScrollBoxEntry<BvPropertyBox>();
                    container.SetElement(new BvPropertyBox());

                    return container;
                }

                public void ResetObject(ScrollBoxEntry<BvPropertyBox> container)
                {
                    container.Enabled = false;
                    container.Element.Reset();
                }

                public void ResetRange(IReadOnlyList<ScrollBoxEntry<BvPropertyBox>> containers, int index, int count)
                {
                    for (int i = 0; (i + index) < containers.Count; i++)
                    {
                        containers[i].Enabled = false;
                        containers[i].Element.Reset();
                    }
                }

                public void ResetRange<T2>(IReadOnlyList<VRage.MyTuple<ScrollBoxEntry<BvPropertyBox>, T2>> containers, int index, int count)
                {
                    for (int i = 0; (i + index) < containers.Count; i++)
                    {
                        containers[i].Item1.Enabled = false;
                        containers[i].Item1.Element.Reset();
                    }
                }
            }
        }
    }
}