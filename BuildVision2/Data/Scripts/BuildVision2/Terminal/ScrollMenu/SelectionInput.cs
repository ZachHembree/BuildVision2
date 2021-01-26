using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvScrollMenu : HudElementBase
    {
        private void HandleSelectionInput()
        {
            if (!Selection.Visible)
                UpdateIndex(0);

            if (!Selection.InputOpen)
            {
                if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                    Scroll(-1);
                else if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                    Scroll(1);
            }
        }

        /// <summary>
        /// Interprets scrolling input. If a property is currently opened, scrolling will change the
        /// current property value, if applicable. Otherwise, it will change the current property selection.
        /// </summary>
        private void Scroll(int dir)
        {
            if (!PropOpen)
            {
                UpdateIndex(BvBinds.MultX.IsPressed ? dir * 4 : dir);
                listWrapTimer.Restart();
            }
            else
            {
                var scrollable = Selection.BlockMember as IBlockScrollable;

                if (scrollable != null)
                {
                    if (dir < 0)
                        scrollable.ScrollUp();
                    else if (dir > 0)
                        scrollable.ScrollDown();
                }
            }
        }

        /// <summary>
        /// Updates the selection index.
        /// </summary>
        private void UpdateIndex(int offset)
        {
            int min = GetFirstIndex(), max = GetLastIndex(), dir = (offset > 0) ? 1 : -1;
            offset = Math.Abs(offset);

            for (int x = 1; x <= offset; x++)
            {
                index += dir;

                for (int y = index; (y <= max && y >= min); y += dir)
                {
                    if (scrollBody.Collection[y].Enabled)
                    {
                        index = y;
                        break;
                    }
                }
            }

            if (listWrapTimer.ElapsedMilliseconds > 300 && (index > max || index < min) && !BvBinds.MultX.IsPressed)
            {
                if (index < min)
                {
                    index = max;
                    scrollBody.End = index;
                }
                else
                {
                    index = min;
                    scrollBody.Start = index;
                }
            }
            else
            {
                index = MathHelper.Clamp(index, min, max);

                if (index < scrollBody.Start)
                    scrollBody.Start = index;
                else if (index > scrollBody.End)
                    scrollBody.End = index;
            }
        }

        /// <summary>
        /// Returns the index of the first enabled property.
        /// </summary>
        /// <returns></returns>
        private int GetFirstIndex()
        {
            int first = 0;

            for (int n = 0; n < Count; n++)
            {
                if (scrollBody.Collection[n].Enabled)
                {
                    first = n;
                    break;
                }
            }

            return first;
        }

        /// <summary>
        /// Retrieves the index of the last enabled property.
        /// </summary>
        /// <returns></returns>
        private int GetLastIndex()
        {
            int last = 0;

            for (int n = Count - 1; n >= 0; n--)
            {
                if (scrollBody.Collection[n].Enabled)
                {
                    last = n;
                    break;
                }
            }

            return last;
        }
    }
}
