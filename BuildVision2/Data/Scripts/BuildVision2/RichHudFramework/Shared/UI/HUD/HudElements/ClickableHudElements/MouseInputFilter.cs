using System.Collections.Generic;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Filters cursor input to the elements behind it. If any bind in its list is pressed, then cursor
    /// input to those elements will be blocked.
    /// </summary>
    public class MouseInputFilter : HudElementBase
    {
        /// <summary>
        /// True if any bind in the bind list is pressed
        /// </summary>
        public bool IsControlPressed { get; private set; }

        /// <summary>
        /// List of binds used in filtering input
        /// </summary>
        public IList<IBind> Binds { get; set; }
        
        public MouseInputFilter(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            ShareCursor = true;
        }

        protected override void HandleInput()
        {
            IsControlPressed = false;

            if (IsMousedOver && Binds != null)
            {
                for (int n = 0; n < Binds.Count; n++)
                {
                    if (Binds[n].IsPressed)
                    {
                        IsControlPressed = true;
                        break;
                    }
                }
            }

            ShareCursor = !IsControlPressed;
        }
    }
}