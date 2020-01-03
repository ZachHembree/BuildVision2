using System;
using VRage;

namespace RichHudFramework
{
    namespace UI
    {
        public interface IClickableElement : IHudElement
        {
            event Action OnCursorEnter;
            event Action OnCursorExit;
            event Action OnLeftClick;
            event Action OnLeftRelease;
            event Action OnRightClick;
            event Action OnRightRelease;
        }
    }
}